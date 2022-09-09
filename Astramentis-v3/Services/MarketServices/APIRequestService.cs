using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Astramentis.Enums;
using Discord;
using Discord.WebSocket;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using NLog;

namespace Astramentis.Services.MarketServices
{
    public class APIRequestService
    {
        private readonly DiscordSocketClient _discord;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private string _customMarketApiUrl;
        private string _xivApiKey;

        private int exceptionRetryCount = 5; // number of times to retry api requests
        private int exceptionRetryDelay = 1000; // ms delay between retries

        // XIVAPI concurrent/max request values for ratelimiting
        private int concurrentXIVAPIRequests = 0;
        private int concurrentXIVAPIRequestsMax = 20;

        // custom API request values for tracking overall Companion API load
        public int totalCustomAPIRequestsMade = 0;
        public int totalCustomAPIRequestsMadeSinceHeartbeat = 0;

        // custom API concurrent/max request values for tracking realtime Companion API load
        public int concurrentCustomAPIRequestsCompleted = 0; // number of requests completed
        public int concurrentCustomAPIRequestsTotal = 0; // number of requests to do

        private Timer botStatusUpdateTimer;

        public APIRequestService(DiscordSocketClient discord, IConfigurationRoot config)
        {
            _discord = discord;

            _customMarketApiUrl = config["customMarketApiUrl"];
            _xivApiKey = config["xivApiKey"];
        }

        public async Task<dynamic> QueryXivapiWithItemId(int itemId)
        {
            return await PerformXivapiRequest($"https://xivapi.com/item/{itemId}");
        }

        // search XIVAPI using item name - generally used to get an item's ID or to search for items
        public async Task<dynamic> QueryXivapiWithString(string itemName)
        {
            return await PerformXivapiRequest(
                $"https://xivapi.com/search?string={itemName}&indexes=Item&filters=IsUntradable=0&private_key={_xivApiKey}");
        }

        // search XIVAPI using EXACT item name - generally used to get an item's ID
        public async Task<dynamic> QueryXivapiWithStringExact(string itemName)
        {
            return await PerformXivapiRequest(
                $"https://xivapi.com/search?string={itemName}&indexes=Item&string_algo=match&filters=IsUntradable=0&private_key={_xivApiKey}");
        }

        // get current market listings from custom api
        public async Task<dynamic> QueryCustomApiForPrices(int itemId, string server)
        {
            // TODO: catch when api fails with code 202 (waiting on se) and retry
            // TODO: handle failure cases with retries

            var apiResponse = await PerformCustomApiRequest($"{_customMarketApiUrl}/market/?id={itemId}&server={server}");                

            // log any important errors
            if (apiResponse.GetType() == typeof(CustomApiStatus))
                if (apiResponse != CustomApiStatus.NoResults)
                    Logger.Log(LogLevel.Error, $"Custom API listing query for {itemId} on {server} gave {apiResponse.ToString()}");

            return apiResponse;
        }

        // get history data from custom api
        public async Task<dynamic> QueryCustomApiForHistory(int itemId, string server)
        {
            // todo: catch when api fails with code 202 (waiting on se) and retry

            var apiResponse = await PerformCustomApiRequest($"{_customMarketApiUrl}/market/history.php?id={itemId}&server={server}");

            // log any important errors
            if (apiResponse.GetType() == typeof(CustomApiStatus))
                if (apiResponse != CustomApiStatus.NoResults)
                    Logger.Log(LogLevel.Error, $"Custom API history query for {itemId} on {server} gave {apiResponse.ToString()}");

            return apiResponse;
        }

        public async Task<bool> LoginToCompanionApi(string server)
        {
            var response =
                await
                    $"{_customMarketApiUrl}/market/scripts/logincmd.php?server={server}"
                        .GetStringAsync();

            if (response.Equals("1"))
                return true;
            else
                return false;
        }

        private async Task<dynamic> PerformXivapiRequest(string url)
        {
            // number of retries attempted
            var i = 0;

            while (i < exceptionRetryCount)
            {
                while (concurrentXIVAPIRequests >= concurrentXIVAPIRequestsMax)
                    await Task.Delay(1000);
                Interlocked.Increment(ref concurrentXIVAPIRequests);

                try
                {
                    dynamic apiResponse = await url.GetJsonAsync();

                    Interlocked.Decrement(ref concurrentXIVAPIRequests);

                    // if our request was a search, this will cover any no results errors
                    if (((IDictionary<String, object>) apiResponse).ContainsKey("Results") &&
                        apiResponse.Results.Count == 0)
                        return CustomApiStatus.NoResults;

                    return apiResponse;
                }
                catch (FlurlHttpException exception)
                {
                    Logger.Log(LogLevel.Error, $"Performing URL request ({url}) resulted in an exception: {exception.Message}");
                    await Task.Delay(exceptionRetryDelay);

                    // slow down further if we're being given a rate limit error
                    if (exception.Call.HttpResponseMessage.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        Logger.Log(LogLevel.Warn, $"Performing URL request ({url}) resulted in a rate limit error, delaying 5 seconds.");
                        await Task.Delay(5000);
                    }
                }
                i++;
            }

            // return generic api failure code
            return CustomApiStatus.APIFailure;
        }

        private async Task<dynamic> PerformCustomApiRequest(string url)
        {
            // number of retries attempted
            var i = 0;

            // thread-safe tracking of custom API requests made
            Interlocked.Increment(ref totalCustomAPIRequestsMade); // total overall 
            Interlocked.Increment(ref totalCustomAPIRequestsMadeSinceHeartbeat); // total since last heartbeat check
            Interlocked.Increment(ref concurrentCustomAPIRequestsTotal); // concurrent requests active

            // start the timer - function has a null check so it will only do anything the first time we run it
            await Task.Run(InitiateTimer);

            while (i < exceptionRetryCount)
            {
                try
                {
                    var apiResponse = await url.GetJsonAsync();

                    if ((object)apiResponse != null)
                    {
                        // request has been fulfilled in some way, increment the completed requests count
                        Interlocked.Increment(ref concurrentCustomAPIRequestsCompleted); // concurrent requests completed

                        // check if custom API handled error - get apiResponse as dict of keyvalue pairs
                        // if the dict contains 'Error' key, it's a handled error
                        if (((IDictionary<String, object>)apiResponse).ContainsKey("Error"))
                        {
                            if (apiResponse.Error == null)
                                return CustomApiStatus.APIFailure;
                            if (apiResponse.Error == "Not logged in")
                                return CustomApiStatus.NotLoggedIn;
                            if (apiResponse.Error == "Under maintenance")
                                return CustomApiStatus.UnderMaintenance;
                            if (apiResponse.Error == "Access denied")
                                return CustomApiStatus.AccessDenied;
                            if (apiResponse.Error == "Service unavailable")
                                return CustomApiStatus.ServiceUnavailable;
                        }

                        // check if prices or history key exists
                        if (((IDictionary<String, object>)apiResponse).ContainsKey("Prices"))
                        {
                            if (apiResponse.Prices == null || apiResponse.Prices.Count == 0)
                                return CustomApiStatus.NoResults;

                            // success, return response
                            return apiResponse;
                        }

                        if (((IDictionary<String, object>) apiResponse).ContainsKey("Sales"))
                        {
                            if (apiResponse.Sales == null || apiResponse.Sales.Count == 0)
                                return CustomApiStatus.NoResults;

                            // success, return response
                            return apiResponse;
                        }

                        // key didn't exist, retry
                    }
                }
                catch (FlurlHttpException exception)
                {
                    Logger.Log(LogLevel.Error, $"{exception.Message}");
                    await Task.Delay(exceptionRetryDelay);
                }
                i++;
            }

            // return generic api failure code
            return CustomApiStatus.APIFailure;
        }

        private void InitiateTimer()
        {
            // this function is called every time a custom api call is made, but we only want it to initiate a timer once
            if (botStatusUpdateTimer == null)
                botStatusUpdateTimer = new Timer(delegate { BotStatusUpdateTimer(); }, null, 0, 4000);
        }

        private void BotStatusUpdateTimer()
        {
            // if requests are completed: remove the bot's status msg, reset the request counts, and get rid of the timer
            if (concurrentCustomAPIRequestsCompleted == concurrentCustomAPIRequestsTotal)
            {
                // await _discord.SetGameAsync(null);

                // reset concurrent api requests values
                Interlocked.Add(ref concurrentCustomAPIRequestsTotal, concurrentCustomAPIRequestsTotal * -1);
                Interlocked.Add(ref concurrentCustomAPIRequestsCompleted, concurrentCustomAPIRequestsCompleted * -1);

                if (botStatusUpdateTimer != null)
                {
                    botStatusUpdateTimer.Dispose();
                    botStatusUpdateTimer = null;
                }
            }
            else
            {
                // await _discord.SetGameAsync($"{concurrentCustomAPIRequestsTotal} API requests...", type: ActivityType.Watching); // "watching x requests
            }
        }
    }
}
