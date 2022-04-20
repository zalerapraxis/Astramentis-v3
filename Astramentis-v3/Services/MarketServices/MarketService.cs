using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Astramentis.Datasets;
using Astramentis.Models;
using Astramentis.Services.MarketServices;
using Astramentis.Enums;

namespace Astramentis.Services
{
    public class MarketService
    {
        public string DefaultWorld = Worlds.gilgamesh.ToString();
        public List<string> DefaultDatacenter = Datacenter.Aether;

        private readonly APIRequestService _apiRequestService;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // Set to 1 to (hopefully) force parallel.foreach loops to run one at a time (synchronously)
        // set to -1 for default behavior
        private static ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = -1 };

        public MarketService(APIRequestService apiRequestService)
        {
            _apiRequestService = apiRequestService;
        }

        public async Task<List<MarketItemListingModel>> GetMarketListings(string itemName, int itemId, bool itemShouldBeHq, List<string> worldsToSearch)
        {
            var marketListings = new List<MarketItemListingModel>();

            // get all market entries for specified item across all servers, bundle results into tempMarketList
            var tasks = Task.Run(() => Parallel.ForEach(worldsToSearch, parallelOptions, async server =>
            {
                var apiResponse = _apiRequestService.QueryCustomApiForPrices(itemId, server).Result;

                // handle no listings for item
                if (apiResponse.GetType() == typeof(CustomApiStatus) &&
                    apiResponse == CustomApiStatus.NoResults)
                    return;

                // handle API failure 
                // TODO - move handling into the querycustomapiforprices function
                if (apiResponse.GetType() == typeof(CustomApiStatus) &&
                    apiResponse == CustomApiStatus.APIFailure)
                    return;

                foreach (var listing in apiResponse.Prices)
                {
                    // build a marketlisting with the info we get from method parameters and the api call
                    var marketListing = new MarketItemListingModel()
                    {
                        Name = itemName,
                        ItemId = itemId,
                        CurrentPrice = (int)listing.PricePerUnit,
                        Quantity = (int)listing.Quantity,
                        IsHq = (bool)listing.IsHQ,
                        RetainerName = listing.RetainerName,
                        Server = server
                    };

                    marketListings.Add(marketListing);
                }
            }));

            await Task.WhenAll(tasks);

            if (itemShouldBeHq)
                marketListings = marketListings.Where(x => x.IsHq).ToList();

            // sort the list by the item price
            marketListings = marketListings.OrderBy(x => x.CurrentPrice).ToList();

            return marketListings;
        }

        // take an item id and get the lowest history listings from across all servers, return list of historylist of HistoryItemListings
        public async Task<List<HistoryItemListingModel>> GetHistoryListings(string itemName, int itemId, List<string> worldsToSearch)
        {
            List<HistoryItemListingModel> historyListings = new List<HistoryItemListingModel>();

            // get all market entries for specified item across all servers, bundle results into tempMarketList
            var tasks = Task.Run(() => Parallel.ForEach(worldsToSearch, parallelOptions, server =>
            {
                var apiResponse = _apiRequestService.QueryCustomApiForHistory(itemId, server).Result;

                // handle no listings for item
                if (apiResponse.GetType() == typeof(CustomApiStatus) &&
                    apiResponse == CustomApiStatus.NoResults)
                    return;

                // handle API failure 
                // TODO - move handling into the QueryCustomApiForHistory function
                if (apiResponse.GetType() == typeof(CustomApiStatus) &&
                    apiResponse == CustomApiStatus.APIFailure)
                    return;

                foreach (var listing in apiResponse.Sales)
                {
                    // convert companionapi's buyRealData from epoch time (milliseconds) to a normal datetime
                    // set this for converting from epoch time to normal people time
                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var saleDate = epoch.AddMilliseconds(listing.PurchaseDateMs);

                    // build a marketlisting with the info we get from method parameters and the api call
                    var historyListing = new HistoryItemListingModel()
                    {
                        Name = itemName,
                        ItemId = itemId,
                        SoldPrice = (int)listing.PricePerUnit,
                        IsHq = listing.IsHQ,
                        Quantity = (int)listing.Quantity,
                        SaleDate = saleDate,
                        Server = listing.Server,
                };
                    

                    historyListings.Add(historyListing);
                }
            }));

            await Task.WhenAll(tasks);

            // sort the list by the item price
            historyListings = historyListings.OrderByDescending(item => item.SaleDate).ToList();

            return historyListings;
        }

        // returns three analysis objects: hq, nq, and overall
        public async Task<List<MarketItemAnalysisModel>> CreateMarketAnalysis(string itemName, int itemID, List<string> worldsToSearch)
        {
            var analysisHQ = new MarketItemAnalysisModel();
            var analysisNQ = new MarketItemAnalysisModel();
            var analysisOverall = new MarketItemAnalysisModel(); // items regardless of quality - used for exchange command

            analysisHQ.Name = itemName;
            analysisNQ.Name = itemName;
            analysisOverall.Name = itemName;
            analysisHQ.ID = itemID;
            analysisNQ.ID = itemID;
            analysisOverall.ID = itemID;
            analysisHQ.IsHQ = true;
            analysisNQ.IsHQ = false;
            analysisOverall.IsHQ = false;

            // make API requests for data
            var apiMarketResponse = await GetMarketListings(itemName, itemID, false, worldsToSearch);
            var apiHistoryResponse = await GetHistoryListings(itemName, itemID, worldsToSearch);

            // split market results by quality
            var marketHQ = apiMarketResponse.Where(x => x.IsHq == true).ToList();
            var marketNQ = apiMarketResponse.Where(x => x.IsHq == false).ToList();
            var marketOverall = apiMarketResponse.ToList();

            // split history results by quality
            var salesHQ = apiHistoryResponse.Where(x => x.IsHq == true).ToList();
            var salesNQ = apiHistoryResponse.Where(x => x.IsHq == false).ToList();
            var salesOverall = apiHistoryResponse.ToList();

            // handle HQ items if they exist
            if (salesHQ.Any() && marketHQ.Any())
            {
                // assign recent sale count
                analysisHQ.NumRecentSales = CountRecentSales(salesHQ);

                // assign total items avail
                analysisHQ.NumTotalItems = CountTotalItemsAvailable(marketHQ);

                // assign average price of last 5 sales
                analysisHQ.AvgSalePrice = GetAveragePricePerUnit(salesHQ.Take(10).ToList());

                // assign average price of lowest ten listings
                analysisHQ.AvgMarketPrice = GetAveragePricePerUnit(marketHQ.Take(10).ToList());

                // build lowest values list
                analysisHQ.LowestPrices = new List<MarketItemAnalysisLowestPricesModel>();
                foreach (var listing in apiMarketResponse)
                {
                    if (!analysisHQ.LowestPrices.Any(x => x.Server == listing.Server))
                        analysisHQ.LowestPrices.Add(new MarketItemAnalysisLowestPricesModel() { Server = listing.Server, Price = listing.CurrentPrice });
                }

                // set checks for if item's sold or has listings
                if (analysisHQ.AvgMarketPrice == 0)
                    analysisHQ.ItemHasListings = false;
                else
                    analysisHQ.ItemHasListings = true;

                if (analysisHQ.AvgSalePrice == 0)
                    analysisHQ.ItemHasHistory = false;
                else
                    analysisHQ.ItemHasHistory = true;

                // assign differential of sale price to market price
                if (analysisHQ.ItemHasHistory == false || analysisHQ.ItemHasListings == false)
                    analysisHQ.Differential = 0;
                else
                {
                    analysisHQ.Differential = Math.Round(((decimal)analysisHQ.AvgSalePrice - analysisHQ.AvgMarketPrice) / analysisHQ.AvgSalePrice * 100, 2);
                    analysisHQ.DifferentialLowest = Math.Round(((decimal)analysisHQ.AvgSalePrice - analysisHQ.LowestPrices.FirstOrDefault().Price) / analysisHQ.AvgSalePrice * 100, 2);
                }
            }


            // handle NQ items
            // assign recent sale count
            analysisNQ.NumRecentSales = CountRecentSales(salesNQ);

            // assign total items avail
            analysisNQ.NumTotalItems = CountTotalItemsAvailable(marketNQ);

            // assign average price of last few sales
            analysisNQ.AvgSalePrice = GetAveragePricePerUnit(salesNQ.Take(10).ToList());

            // assign average price of lowest listings
            analysisNQ.AvgMarketPrice = GetAveragePricePerUnit(marketNQ.Take(5).ToList());

            // build lowest values list
            analysisNQ.LowestPrices = new List<MarketItemAnalysisLowestPricesModel>();
            foreach (var listing in apiMarketResponse)
            {
                if (!analysisNQ.LowestPrices.Any(x => x.Server == listing.Server))
                    analysisNQ.LowestPrices.Add(new MarketItemAnalysisLowestPricesModel() { Server = listing.Server, Price = listing.CurrentPrice });
            }

            // set checks for if item's sold or has listings
            if (analysisNQ.AvgMarketPrice == 0)
                analysisNQ.ItemHasListings = false;
            else
                analysisNQ.ItemHasListings = true;

            if (analysisNQ.AvgSalePrice == 0)
                analysisNQ.ItemHasHistory = false;
            else
                analysisNQ.ItemHasHistory = true;

            // assign differential of sale price to market price
            if (analysisNQ.ItemHasHistory == false || analysisNQ.ItemHasListings == false)
                analysisNQ.Differential = 0;
            else
            {
                analysisNQ.Differential = Math.Round(((decimal)analysisNQ.AvgSalePrice - analysisNQ.AvgMarketPrice) / analysisNQ.AvgSalePrice * 100, 2);
                analysisNQ.DifferentialLowest = Math.Round(((decimal)analysisNQ.AvgSalePrice - analysisNQ.LowestPrices.FirstOrDefault().Price) / analysisNQ.AvgSalePrice * 100, 2);
            }

            
            // handle overall items list
            // assign recent sale count
            analysisOverall.NumRecentSales = CountRecentSales(salesOverall);

            // assign total items available count
            analysisOverall.NumTotalItems = CountTotalItemsAvailable(marketOverall);

            // assign average price of last few sales
            analysisOverall.AvgSalePrice = GetAveragePricePerUnit(salesOverall.Take(10).ToList());

            // assign average price of lowest listings
            analysisOverall.AvgMarketPrice = GetAveragePricePerUnit(marketOverall.Take(5).ToList());

            // build lowest values list
            analysisOverall.LowestPrices = new List<MarketItemAnalysisLowestPricesModel>();
            foreach (var listing in apiMarketResponse)
            {
                if (!analysisOverall.LowestPrices.Any(x => x.Server == listing.Server))
                    analysisOverall.LowestPrices.Add(new MarketItemAnalysisLowestPricesModel() { Server = listing.Server, Price = listing.CurrentPrice });
            }

            // set checks for if item's sold or has listings
            if (analysisOverall.AvgMarketPrice == 0)
                analysisOverall.ItemHasListings = false;
            else
                analysisOverall.ItemHasListings = true;

            if (analysisOverall.AvgSalePrice == 0)
                analysisOverall.ItemHasHistory = false;
            else
                analysisOverall.ItemHasHistory = true;

            // assign differential of sale price to market price
            if (analysisOverall.ItemHasHistory == false || analysisOverall.ItemHasListings == false)
                analysisOverall.Differential = 0;
            else
            {
                analysisOverall.Differential = Math.Round(((decimal)analysisOverall.AvgSalePrice - analysisOverall.AvgMarketPrice) / analysisOverall.AvgSalePrice * 100, 2);
                analysisOverall.DifferentialLowest = Math.Round(((decimal)analysisOverall.AvgSalePrice - analysisOverall.LowestPrices.FirstOrDefault().Price) / analysisOverall.AvgSalePrice * 100, 2);
            }

            
            List<MarketItemAnalysisModel> response = new List<MarketItemAnalysisModel>();
            response.Add(analysisHQ);
            response.Add(analysisNQ);
            response.Add(analysisOverall);

            return response;
        }

        public async Task<List<CurrencyTradeableItem>> GetBestCurrencyExchange(string category, List<string> worldsToSearch)
        {
            List<CurrencyTradeableItem> itemsList = new List<CurrencyTradeableItem>();

            switch (category)
            {
                case "gc":
                    itemsList = CurrencyTradeableItemsDataset.GrandCompanySealItemsList;
                    break;
                case "poetics":
                    itemsList = CurrencyTradeableItemsDataset.PoeticsItemsList;
                    break;
                case "gemstones":
                case "gems":
                    itemsList = CurrencyTradeableItemsDataset.GemstonesItemsList;
                    break;
                case "nuts":
                    itemsList = CurrencyTradeableItemsDataset.NutsItemsList;
                    break;
                case "wgs":
                    itemsList = CurrencyTradeableItemsDataset.WhiteGathererScripsItemsList;
                    break;
                case "wcs":
                    itemsList = CurrencyTradeableItemsDataset.WhiteCrafterScripsItemsList;
                    break;
                case "pgs":
                    itemsList = CurrencyTradeableItemsDataset.PurpleGathererScripsItemsList;
                    break;
                case "pcs":
                    itemsList = CurrencyTradeableItemsDataset.PurpleCrafterScripsItemsList;
                    break;
                case "tome":
                case "tomes":
                    itemsList = CurrencyTradeableItemsDataset.TomeItemsList;
                    break;
                case "sky":
                case "skybuilder":
                case "skybuilders":
                    itemsList = CurrencyTradeableItemsDataset.SkybuildersScripItemsList;
                    break;
                default:
                    return itemsList;
            }

            var tasks = Task.Run(() => Parallel.ForEach(itemsList, parallelOptions, item =>
            {
                var analysisResponse = CreateMarketAnalysis(item.Name, item.ItemID, worldsToSearch).Result;
                // index 2 is the 'overall' analysis that includes both NQ and HQ items
                var analysis = analysisResponse[2];

                item.AvgMarketPrice = analysis.AvgMarketPrice;
                item.AvgSalePrice = analysis.AvgSalePrice;
                item.ValueRatio = item.AvgMarketPrice / item.CurrencyCost;
                item.NumRecentSales = analysis.NumRecentSales;
            }));

            await Task.WhenAll(tasks);

            return itemsList;
        }

        public async Task<List<MarketItemCrossWorldOrderModel>> GetMarketCrossworldPurchaseOrder(List<MarketItemCrossWorldOrderModel> inputs, List<string> worldsToSearch)
        {
            List<MarketItemCrossWorldOrderModel> PurchaseOrderList = new List<MarketItemCrossWorldOrderModel>();

            // iterate through each item
            var tasks = Task.Run(() => Parallel.ForEach(inputs, parallelOptions, input =>
            {
                // getting first response for now, but we should find a way to make this more flexible later
                var itemName = input.Name;
                var itemId = input.ItemID;
                // paramer values only for use in this function
                var neededQuantity = input.NeededQuantity;
                var shouldBeHq = input.ShouldBeHQ;

                var numOfListingsToTake = 20;

                var listings = GetMarketListings(itemName, itemId, shouldBeHq, worldsToSearch).Result;

                // put together a list of each market listing
                var multiPartOrderList = new List<MarketItemCrossWorldOrderModel>();

                foreach (var listing in listings.Take(numOfListingsToTake))
                {
                    // make a new item each iteration
                    var item = new MarketItemCrossWorldOrderModel()
                    {
                        Name = itemName,
                        ItemID = itemId,
                        NeededQuantity = neededQuantity,
                        Price = listing.CurrentPrice,
                        Server = listing.Server,
                        Quantity = listing.Quantity,
                        IsHQ = listing.IsHq
                    };

                    multiPartOrderList.Add(item);
                }

                // send our listings off to find what the most efficient set of listings to buy are
                var efficientListings = GetMostEfficientPurchases(multiPartOrderList, input.NeededQuantity);

                if (efficientListings != null)
                    if (efficientListings.Any())
                        PurchaseOrderList.AddRange(efficientListings);

                multiPartOrderList.Clear();
            }));

            await Task.WhenAll(tasks);

            PurchaseOrderList = PurchaseOrderList.ToList();

            return PurchaseOrderList;
        }

        // for use with analysis commands
        private int CountRecentSales(List<HistoryItemListingModel> listings)
        {
            var twoDaysAgo = DateTime.Now.Subtract(TimeSpan.FromDays(2));

            var sales = 0;
            foreach (var item in listings)
            {
                if (item.SaleDate > twoDaysAgo)
                    sales += 1;
            }
            if (sales == 0 || listings.Count == 0)
                return 0;
            return sales;
        }

        private int CountTotalItemsAvailable(List<MarketItemListingModel> listings)
        {
            var itemsAvailable = 0;
            foreach (var item in listings)
            {
                itemsAvailable += item.Quantity;
            }
            if (itemsAvailable == 0)
                return 0;
            return itemsAvailable;
        }

        // for market
        private int GetAveragePricePerUnit(List<MarketItemListingModel> listings)
        {
            var sumOfPrices = 0;
            foreach (var item in listings)
            {
                sumOfPrices += item.CurrentPrice;
            }

            if (sumOfPrices == 0 || listings.Count == 0)
                return 0;
            return sumOfPrices / listings.Count;
        }

        // for history
        private int GetAveragePricePerUnit(List<HistoryItemListingModel> listings)
        {
            var sumOfPrices = 0;
            foreach (var item in listings)
            {
                sumOfPrices += item.SoldPrice;
            }

            if (sumOfPrices == 0 || listings.Count == 0)
                return 0;
            return sumOfPrices / listings.Count;
        }

        // this is used to determine the most efficient order of buying items cross-world
        private static List<MarketItemCrossWorldOrderModel> GetMostEfficientPurchases(List<MarketItemCrossWorldOrderModel> listings, int needed)
        {
            var helper = new MarketOrderHelper();
            var results = helper.SumUp(listings, needed);
            var resultsOrdered = results.OrderBy(x => x.Sum(y => y.Price * y.Quantity)).ToList().FirstOrDefault();

            return resultsOrdered;
        }

        // gets list of items, loads them into an list, returns list of items or empty list if request failed
        public async Task<List<ItemSearchResultModel>> SearchForItemByName(string searchTerm)
        {
            var apiResponse = await _apiRequestService.QueryXivapiWithString(searchTerm);

            var tempItemList = new List<ItemSearchResultModel>();

            // if api failure, return empty list
            if (apiResponse.GetType() == typeof(CustomApiStatus) &&
                apiResponse == CustomApiStatus.APIFailure)
                return null; // empty list

            // if no results, return empty list
            if (apiResponse.GetType() == typeof(CustomApiStatus) &&
                apiResponse == CustomApiStatus.NoResults)
                return tempItemList;

            foreach (var item in apiResponse.Results)
                tempItemList.Add(new ItemSearchResultModel()
                {
                    Name = item.Name,
                    ID = (int)item.ID
                });

            return tempItemList;
        }

        // gets list of items, loads them into an list, returns list of items or empty list if request failed
        public async Task<List<ItemSearchResultModel>> SearchForItemByNameExact(string searchTerm)
        {
            var apiResponse = await _apiRequestService.QueryXivapiWithStringExact(searchTerm);

            var tempItemList = new List<ItemSearchResultModel>();

            // if api failure, return empty list
            if (apiResponse.GetType() == typeof(CustomApiStatus) &&
                apiResponse == CustomApiStatus.APIFailure)
                return null; // empty list

            // if no results, return empty list
            if (apiResponse.GetType() == typeof(CustomApiStatus) &&
                apiResponse == CustomApiStatus.NoResults)
                return tempItemList;

            foreach (var item in apiResponse.Results)
                tempItemList.Add(new ItemSearchResultModel()
                {
                    Name = item.Name,
                    ID = (int)item.ID
                });

            return tempItemList;
        }
    }
}
