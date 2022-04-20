using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Astramentis.Enums;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;

namespace Astramentis.Services.MarketServices
{
    public class APIHeartbeatService
    {
        private readonly DiscordSocketClient _discord;
        private readonly APIRequestService _apiRequestService;
        private readonly IConfigurationRoot _config;
        private readonly Random _rng;

        public ConcurrentDictionary<Worlds, bool> serverLoginStatusTracker = new ConcurrentDictionary<Worlds, bool>();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Timer _heartbeatTimer;

        public CustomApiStatus ApiStatus;

        // Set to 1 to (hopefully) force parallel.foreach loops to run one at a time (synchronously)
        // set to -1 for default behavior
        private static ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = -1 };

        public APIHeartbeatService(DiscordSocketClient discord, 
            APIRequestService apiRequestService, 
            IConfigurationRoot config,
            Random rng)
        {
            _discord = discord;
            _apiRequestService = apiRequestService;
            _config = config;
            _rng = rng;

            Logger.Log(LogLevel.Debug, $"Heartbeat timer started!");
            _heartbeatTimer = new Timer(async delegate { await HeartbeatCheck(); }, null, 0, Timeout.Infinite);
        }

        private async Task HeartbeatCheck()
        {
            Logger.Log(LogLevel.Info, $"Heartbeat check - {_apiRequestService.totalCustomAPIRequestsMade} API requests made ({_apiRequestService.totalCustomAPIRequestsMadeSinceHeartbeat} requests in the last hour) - starting server checks.");
            // thread-safe reset 30m counter
            Interlocked.Exchange(ref _apiRequestService.totalCustomAPIRequestsMadeSinceHeartbeat, 0);

            // do a global companion status check to see if the API is down or if something else is wrong
            CustomApiStatus globalCompanionStatusRequestResult = GetCompanionApiStatus().Result;

            // if companion is in maintenance, reset timer and don't do anything else
            if (globalCompanionStatusRequestResult == CustomApiStatus.UnderMaintenance) 
            {
                AdjustHeartbeatTimer();
                Logger.Log(LogLevel.Info, $"The SE API is down for maintenance at the moment. Trying again later.");
                return;
            }

            // log any error responses that aren't logged-out or maintenance issues & send DM to bot owner
            // this is deprecated now as any errors from the APIRequestService will give us logs and DMs already
            /*
            if (globalCompanionStatusRequestResult != CustomApiStatus.OK && globalCompanionStatusRequestResult != CustomApiStatus.NotLoggedIn && globalCompanionStatusRequestResult != CustomApiStatus.UnderMaintenance)
            {
                Logger.Log(LogLevel.Info, $"SE API error - received error: {globalCompanionStatusRequestResult}.");

                var dm = await _discord.GetUser(ulong.Parse(_config["discordBotOwnerId"])).GetOrCreateDMChannelAsync();
                await dm.SendMessageAsync($"Something's wrong with the API - received error: {globalCompanionStatusRequestResult}.");
            } */

            // figure out each server's login status
            serverLoginStatusTracker.Clear();
            var tasks = Task.Run(() => Parallel.ForEach((Worlds[])Enum.GetValues(typeof(Worlds)), parallelOptions, async server =>
            {
                CustomApiStatus serverStatusRequestResult = GetCompanionApiStatus(server.ToString()).Result;
                bool serverLoggedIn = false;

                if (serverStatusRequestResult == CustomApiStatus.OK) // logged in, continue processing
                    serverLoggedIn = true;
                else if (serverStatusRequestResult == CustomApiStatus.NotLoggedIn) // not logged in, continue processing
                    serverLoggedIn = false;

                // runs if servers are logged in or not logged in, will not run if api error or maintenance
                serverLoginStatusTracker.TryAdd(server, serverLoggedIn);
            }));
            await Task.WhenAll(tasks);

            // get how many servers are logged in currently
            var serverLoggedInCount = serverLoginStatusTracker.Count(x => x.Value == true);
            Logger.Log(LogLevel.Info, $"{serverLoggedInCount} of {serverLoginStatusTracker.Count} game servers are logged in. ");

            // if any servers are down, try to log them in
            // use a random delay to avoid potential bans
            if (serverLoggedInCount != serverLoginStatusTracker.Count)
            {
                int errorCount = 0;
                foreach (var serverStatus in serverLoginStatusTracker.Where(x => x.Value == false))
                {
                    var server = serverStatus.Key;
                    Logger.Log(LogLevel.Info, $"Logging into {server}...");
                    var loginResult = await _apiRequestService.LoginToCompanionApi(server.ToString());

                    Logger.Log(LogLevel.Info, $"Login to {server} was {(loginResult ? "successful" : "unsuccessful")}.");
                    if (!loginResult) errorCount++; // increment on unsuccessful logins

                    await Task.Delay(_rng.Next(5000, 10000)); // random delay between 5-10 seconds
                }
                Logger.Log(LogLevel.Info, $"Login process completed with {errorCount} error(s).");
            }

            AdjustHeartbeatTimer();
        }

        private void AdjustHeartbeatTimer()
        {
            // adjust the timer tick to 30m +- 1m interval & start it again
            var _heartbeatTimerInterval = Convert.ToInt32(TimeSpan.FromMinutes(30).TotalMilliseconds) + _rng.Next(-60000, 60000);
            Logger.Log(LogLevel.Debug, $"Next tick at {DateTime.Now.AddMilliseconds(_heartbeatTimerInterval):hh:mm:ss tt}");
            _heartbeatTimer.Change(_heartbeatTimerInterval, Timeout.Infinite);
        }

        public async Task<bool> IsCompanionAPIUsable(string server = null)
        {
            var apiResponse = await GetCompanionApiStatus(server);            

            if (apiResponse != CustomApiStatus.OK)
                return false;

            return true;
        }

        // returns custom api status 
        private async Task<CustomApiStatus> GetCompanionApiStatus(string server = null)
        {
            if (server == null)
                server = "gilgamesh";

            // run test query
            var apiResponse = await _apiRequestService.QueryCustomApiForHistory(_rng.Next(2, 19), server);

            // if apiresponse does not return a status type, then it should be running fine
            if (apiResponse.GetType() != typeof(CustomApiStatus))
                apiResponse = CustomApiStatus.OK;

            // assign publicly available status var 
            ApiStatus = apiResponse;

            // if it does return a status type, pass that back to calling function
            return apiResponse;
        }
    }
}
