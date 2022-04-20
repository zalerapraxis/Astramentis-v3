using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Astramentis.Attributes;
using Astramentis.Datasets;
using Astramentis.Models;
using Astramentis.Services;
using Astramentis.Services.MarketServices;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Astramentis.Enums;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Astramentis.Modules
{
    [Name("Market")]
    [Summary("Get data from FFXIV markets")]
    public class MarketModule : InteractiveBase
    {
        public MarketService MarketService { get; set; }
        public APIRequestService APIRequestService { get; set; }
        public APIHeartbeatService APIHeartbeatService { get; set; }
        public IConfigurationRoot Config { get; set; }

        

        [Command("market price")]
        [Alias("mbp")]
        [Summary("Get prices for an item")]
        [Syntax("market price {optional: server} {name/ID}")]
        [Example("mbp grade 3 tincture of strength cactuar")]
        public async Task MarketGetItemPrice([Remainder] string input)
        {
            // check if companion api is down - func will inform if not
            if (await IsCompanionAPIUsable() == false)
                return;

            // clean up inputs & parse them out
            MarketCommandInputsModel parsedInput = (await SplitCommandInputs(input, InteractiveCommandReturnOptions.Price)).FirstOrDefault();

            if (parsedInput == null)
                // splitcommandinputs passed data off to InteractiveUserSelectItem, which will
                // call this command again once the user has selected the item they want
                return;

            // get market data
            var marketQueryResults = await MarketService.GetMarketListings(parsedInput.ItemName, parsedInput.ItemID, parsedInput.ItemHq, parsedInput.WorldsToSearch);

            // if no listings found, notify and end
            if (marketQueryResults.Count == 0)
            {
                await ReplyAsync(
                    "No listings found for that item. Either nobody's selling it, or it's not tradable.");
                return;
            }

            // format market data & display
            var pages = new List<EmbedFieldBuilder>();

            var i = 0;
            var itemsPerPage = 11;

            // iterate through the market results, making a page for every (up to) itemsPerPage listings
            while (i < marketQueryResults.Count)
            {
                // pull up to itemsPerPage entries from the list, skipping any from previous iterations
                var currentPageMarketList = marketQueryResults.Skip(i).Take(itemsPerPage);

                StringBuilder sbListing = new StringBuilder();

                // build data for this page
                foreach (var listing in currentPageMarketList)
                {
                    sbListing.Append($"• **{listing.Quantity}** ");

                    if (listing.IsHq)
                        sbListing.Append("**HQ** ");

                    if (listing.Quantity > 1)
                        // multiple units
                        sbListing.Append($"for {listing.CurrentPrice * listing.Quantity:N0} (**{listing.CurrentPrice:N0}** per) ");
                    else // single units
                        sbListing.Append($"for **{listing.CurrentPrice:N0}** ");
                    sbListing.Append($"- **{listing.Server}** ");
                    sbListing.Append($"- **{listing.RetainerName}** ");
                    sbListing.AppendLine();
                }

                var page = new EmbedFieldBuilder()
                {
                    Name = $"{parsedInput.ItemName} ({parsedInput.ItemID})",
                    Value = sbListing
                };

                pages.Add(page);

                i = i + itemsPerPage;
            }

            var pager = new PaginatedMessage()
            {
                Pages = pages,
                Author = new EmbedAuthorBuilder()
                {
                    Name = $"{marketQueryResults.Count} Market listing(s) for {parsedInput.ItemName}",
                    IconUrl = parsedInput.ItemIconUrl,
                },
                Color = Color.Blue,
                Options = new PaginatedAppearanceOptions()
                {
                    InformationText = "This is an interactive message. Use the reaction emotes to change pages. Use the :1234: emote and then type a number in chat to go to that page.",
                    Info = new Emoji("ℹ️"),
                    FieldsPerPage = 1
                }
            };

            await PagedReplyAsync(pager);
        }

        [Command("market history")]
        [Alias("mbh")]
        [Summary("Get history for an item")]
        [Syntax("market history {optional: server} {name/ID}")]
        [Example("mbh gold ore")]
        public async Task MarketGetItemHistory([Remainder] string input)
        {
            // check if companion api is down - func will inform if not
            if (await IsCompanionAPIUsable() == false)
                return;

            // clean up inputs & parse them out
            MarketCommandInputsModel parsedInput = (await SplitCommandInputs(input, InteractiveCommandReturnOptions.History)).FirstOrDefault();

            if (parsedInput == null)
                // splitcommandinputs passed data off to InteractiveUserSelectItem, which will
                // call this command again once the user has selected the item they want
                return;

            // get history data
            var historyQueryResults = await MarketService.GetHistoryListings(parsedInput.ItemName, parsedInput.ItemID, parsedInput.WorldsToSearch);

            // if no listings found, notify and end
            if (historyQueryResults.Count == 0)
            {
                await ReplyAsync(
                    "No listings found for that item. Either nobody's selling it, or it's not tradable.");
                return;
            }

            // format history data & display
            var pages = new List<EmbedFieldBuilder>();

            var i = 0;
            var itemsPerPage = 10;

            // iterate through the history results, making a page for every (up to) itemsPerPage listings
            while (i < historyQueryResults.Count)
            {
                // pull up to itemsPerPage entries from the list, skipping any from previous iterations
                var currentPageHistoryList = historyQueryResults.Skip(i).Take(itemsPerPage);

                StringBuilder sbListing = new StringBuilder();

                // build data for this page
                foreach (var listing in currentPageHistoryList)
                {
                    sbListing.Append($"• **{listing.Quantity}** ");

                    if (listing.IsHq)
                        sbListing.Append("**HQ** ");

                    if (listing.Quantity > 1)
                        // multiple units
                        sbListing.Append($"for {listing.SoldPrice * listing.Quantity:N0} (**{listing.SoldPrice:N0}** per) ");
                    else // single units
                        sbListing.Append($"for **{listing.SoldPrice:N0}** ");
                    sbListing.AppendLine();
                    sbListing.Append("››› Sold ");
                    sbListing.Append($"- **{listing.Server}** ");
                    sbListing.Append($"- {listing.SaleDate}");
                    sbListing.AppendLine();
                }

                var page = new EmbedFieldBuilder()
                {
                    Name = $"{parsedInput.ItemName} ({parsedInput.ItemID})",
                    Value = sbListing
                };

                pages.Add(page);

                i = i + itemsPerPage;
            }

            var pager = new PaginatedMessage()
            {
                Pages = pages,
                Author = new EmbedAuthorBuilder()
                {
                    Name = $"{historyQueryResults.Count} History listing(s) for {parsedInput.ItemName}",
                    IconUrl = parsedInput.ItemIconUrl,
                },
                Color = Color.Blue,
                Options = new PaginatedAppearanceOptions()
                {
                    InformationText = "This is an interactive message. Use the reaction emotes to change pages. Use the :1234: emote and then type a number in chat to go to that page.",
                    Info = new Emoji("ℹ️"),
                    FieldsPerPage = 1
                }
            };

            await PagedReplyAsync(pager);
        }


        [Command("market analyze")]
        [Alias("mba")]
        [Summary("Get market analysis for an item")]
        [Syntax("market analyze {name/ID} {optional: server, defaults to Adamantoise}")]
        [Example("mba 27770")]
        public async Task MarketAnalyzeItem([Remainder] string input)
        {
            // check if companion api is down - func will inform if not
            if (await IsCompanionAPIUsable() == false)
                return;

            // clean up inputs & parse them out
            MarketCommandInputsModel parsedInput = (await SplitCommandInputs(input, InteractiveCommandReturnOptions.Analyze)).FirstOrDefault();

            if (parsedInput == null)
                // splitcommandinputs passed data off to InteractiveUserSelectItem, which will
                // call this command again once the user has selected the item they want
                return;

            // get analyses
            var marketAnalysis = await MarketService.CreateMarketAnalysis(parsedInput.ItemName, parsedInput.ItemID, parsedInput.WorldsToSearch);
            var hqMarketAnalysis = marketAnalysis[0];
            var nqMarketAnalysis = marketAnalysis[1];

            // format history data & display

            EmbedBuilder analysisEmbedBuilder = new EmbedBuilder();

            // hq stuff if hq analysis exists
            if (hqMarketAnalysis.NumRecentSales != 0)
            {
                StringBuilder hqFieldBuilder = new StringBuilder();
                hqFieldBuilder.AppendLine($"Avg Listed Price: {hqMarketAnalysis.AvgMarketPrice}");
                hqFieldBuilder.AppendLine($"Avg Sale Price: {hqMarketAnalysis.AvgSalePrice}");
                hqFieldBuilder.AppendLine($"Differential: {hqMarketAnalysis.Differential}%");
                hqFieldBuilder.AppendLine($"Lowest diff: {hqMarketAnalysis.DifferentialLowest}%");
                hqFieldBuilder.AppendLine($"Lowest Price: {hqMarketAnalysis.LowestPrices.FirstOrDefault().Price}");
                hqFieldBuilder.Append("Active:");
                if (hqMarketAnalysis.NumRecentSales >= 5)
                    hqFieldBuilder.AppendLine(" Yes");
                else
                    hqFieldBuilder.AppendLine("No");
                hqFieldBuilder.Append($"Number of sales: {hqMarketAnalysis.NumRecentSales}");
                if (hqMarketAnalysis.NumRecentSales >= 20)
                    hqFieldBuilder.AppendLine("+");
                else
                    hqFieldBuilder.AppendLine("");

                hqFieldBuilder.Append($"Total items avail: {hqMarketAnalysis.NumTotalItems}");

                analysisEmbedBuilder.AddField("HQ", hqFieldBuilder.ToString());
            }

            // nq stuff - first line inline=true in case we had hq values
            StringBuilder nqFieldBuilder = new StringBuilder();
            nqFieldBuilder.AppendLine($"Avg Listed Price: {nqMarketAnalysis.AvgMarketPrice:N0}");
            nqFieldBuilder.AppendLine($"Avg Sale Price: {nqMarketAnalysis.AvgSalePrice:N0}");
            nqFieldBuilder.AppendLine($"Differential: {nqMarketAnalysis.Differential}%");
            nqFieldBuilder.AppendLine($"Lowest diff: {nqMarketAnalysis.DifferentialLowest}%");
            nqFieldBuilder.AppendLine($"Lowest Price: {nqMarketAnalysis.LowestPrices.FirstOrDefault().Price:N0}");
            nqFieldBuilder.Append("Active:");
            if (nqMarketAnalysis.NumRecentSales >= 5)
                nqFieldBuilder.AppendLine(" Yes");
            else
                nqFieldBuilder.AppendLine("No");
            nqFieldBuilder.Append($"Number of sales: {nqMarketAnalysis.NumRecentSales}");
            if (nqMarketAnalysis.NumRecentSales >= 20)
                nqFieldBuilder.AppendLine("+");
            else
                nqFieldBuilder.AppendLine("");

            nqFieldBuilder.Append($"Total items avail: {nqMarketAnalysis.NumTotalItems}");

            analysisEmbedBuilder.AddField("NQ", nqFieldBuilder.ToString());

            StringBuilder embedNameBuilder = new StringBuilder();
            embedNameBuilder.Append($"Market analysis for {parsedInput.ItemName}");
            if (parsedInput.WorldsToSearch.Count == 1)
                embedNameBuilder.Append($" on {parsedInput.WorldsToSearch[0]}");

            analysisEmbedBuilder.Author = new EmbedAuthorBuilder()
            {
                Name = embedNameBuilder.ToString()
            };
            analysisEmbedBuilder.ThumbnailUrl = parsedInput.ItemIconUrl;
            analysisEmbedBuilder.Color = Color.Blue;

            await ReplyAsync(null, false, analysisEmbedBuilder.Build());
        }

        // should be able to accept inputs in any order - if two values are provided, they will be treated as minilvl and maxilvl respectively
        [Command("market exchange")]
        [Alias("mbe")]
        [Summary("Find the best items to spend your tomes/seals on - run without any options to view currency types")]
        [Syntax("market exchange {currency} {optional: server, defaults to Adamantoise}")]
        [Example("mbe ycs sargatanas")]
        public async Task MarketGetBestCurrencyExchangesAsync([Remainder] string input = null)
        {
            if (input == null || !input.Any())
            {
                StringBuilder categoryListBuilder = new StringBuilder();
                categoryListBuilder.AppendLine("These are the categories you can check:");

                categoryListBuilder.AppendLine("gc - grand company seals");
                categoryListBuilder.AppendLine("poetics - i380 crafter mats, more later maybe");
                categoryListBuilder.AppendLine("gems/gemstones - bicolor gemstones from fates");
                categoryListBuilder.AppendLine("nuts - sacks of nuts from hunts :peanut:");
                categoryListBuilder.AppendLine("wgs - White Gatherer Scrip items");
                categoryListBuilder.AppendLine("wcs - White Crafter Scrip items");
                categoryListBuilder.AppendLine("pgs - Purple Gatherer Scrip items");
                categoryListBuilder.AppendLine("pcs - Purple Crafter Scrip items");
                categoryListBuilder.AppendLine("tome/tomes - tome mats (phantasmagoria)");
                categoryListBuilder.AppendLine("sky/skybuilders - skybuilders' scrips");

                await ReplyAsync(categoryListBuilder.ToString());
                return;
            }

            // check if companion api is down - func will inform if not
            if (await IsCompanionAPIUsable() == false)
                return;

            // convert to lowercase so that if user specified server in capitals,
            // it doesn't break our text matching in serverlist and with api request
            input = input.ToLower();

            // clean up input
            var worldsToSearch = GetServer(input, true);
            input = CleanCommandInput(input);

            string category = input;

            // grab data from api
            var currencyDeals = await MarketService.GetBestCurrencyExchange(category, worldsToSearch);

            // catch if the user didn't send a good category
            if (currencyDeals.Count == 0 || !currencyDeals.Any())
            {
                await ReplyAsync("You didn't input an existing category. Run the command by itself to get the categories this command can take.");
                return;
            }

            EmbedBuilder dealsEmbedBuilder = new EmbedBuilder();

            // keep items that are actively selling, and order by value ratio to put the best stuff to sell on top
            currencyDeals = currencyDeals.OrderByDescending(x => x.NumRecentSales)
                .ThenByDescending(y => y.ValueRatio)
                .ToList();

            foreach (var item in currencyDeals.Take(8))
            {
                StringBuilder dealFieldNameBuilder = new StringBuilder();
                dealFieldNameBuilder.Append($"{item.Name}");

                StringBuilder dealFieldContentsBuilder = new StringBuilder();
                dealFieldContentsBuilder.AppendLine($"Avg Listed Price: {item.AvgMarketPrice:N0}");
                dealFieldContentsBuilder.AppendLine($"Avg Sale Price: {item.AvgSalePrice:N0}");
                dealFieldContentsBuilder.AppendLine($"Currency cost: {item.CurrencyCost}");
                dealFieldContentsBuilder.AppendLine($"Value ratio: {item.ValueRatio:N3} gil/c");

                dealFieldContentsBuilder.Append($"Recent sales: {item.NumRecentSales}");
                if (item.NumRecentSales >= 20)
                    dealFieldContentsBuilder.AppendLine("+");
                else
                    dealFieldContentsBuilder.AppendLine("");

                if (item.VendorLocation != null)
                    dealFieldContentsBuilder.AppendLine($"Location: {item.VendorLocation}");

                dealsEmbedBuilder.AddField(dealFieldNameBuilder.ToString(), dealFieldContentsBuilder.ToString(), true);
            }


            // build author stuff 
            StringBuilder embedNameBuilder = new StringBuilder();
            embedNameBuilder.Append($"{category}");
            if (worldsToSearch.Count == 1)
                embedNameBuilder.Append($" on {worldsToSearch[0]}");

            var authorurl = "";

            switch (category)
            {
                case "gc":
                    authorurl = "https://xivapi.com/i/065000/065004.png";
                    break;
                case "poetics":
                    authorurl = "https://xivapi.com/i/065000/065023.png";
                    break;
                case "gemstones":
                case "gems":
                    authorurl = "https://xivapi.com/i/065000/065071.png";
                    break;
                case "nuts":
                    authorurl = "https://xivapi.com/i/065000/065068.png";
                    break;
                case "wgs":
                    authorurl = "https://xivapi.com/i/065000/065069.png";
                    break;
                case "wcs":
                    authorurl = "https://xivapi.com/i/065000/065070.png";
                    break;
                case "pgs":
                    authorurl = "https://xivapi.com/i/065000/065087.png";
                    break;
                case "pcs":
                    authorurl = "https://xivapi.com/i/065000/065088.png";
                    break;
                case "tome":
                case "tomes":
                    authorurl = "https://xivapi.com/i/065000/065085.png";
                    break;
                case "sky":
                case "skybuilder":
                case "skybuilders":
                case "skybuilders'":
                case "skybuilder's":
                    authorurl = "https://xivapi.com/i/065000/065073.png";
                    break;
            }

            dealsEmbedBuilder.Author = new EmbedAuthorBuilder()
            {
                Name = embedNameBuilder.ToString(),
                IconUrl = authorurl
            };
            dealsEmbedBuilder.Color = Color.Blue;

            await ReplyAsync("Items are sorted in descending order by their value ratio - items that are better to sell are at the top.", false, dealsEmbedBuilder.Build());
        }


        [Command("market order")]
        [Alias("mbo")]
        [Summary("Build a list of the lowest market prices for items, ordered by server")]
        [Syntax("market order {itemname/ID:count, itemname/ID:count, etc} or marker order {fending, crafting, etc} - add hq to an item name to require high quality")]
        [Example("mbo zonure skin:122, caprice fleece:20, 5:1000")]
        public async Task MarketCrossWorldPurchaseOrderAsync([Remainder] string commandInput = null)
        {
            // check if companion api is down - func will inform if not
            if (await IsCompanionAPIUsable() == false)
                return;

            // check if user input a gearset request instead of item lists - no 'of' so we avoid catching things like
            // 'facet coat of casting' under the 'casting' gearset
            if (!commandInput.Contains("of") && MarketOrderGearsetDataset.Gearsets.Any(x => commandInput.Contains(x.Key)))
            {
                bool hq = false;
                if (commandInput.Contains("hq"))
                {
                    commandInput = commandInput.Replace("hq", "").Trim();
                    hq = true;
                }
                
                var gearset = MarketOrderGearsetDataset.Gearsets.FirstOrDefault(x => x.Key.Equals(commandInput)).Value;

                var i = 1;
                var gearsetInputSb = new StringBuilder();
                foreach (var item in gearset)
                {
                    var count = 1;
                    if (item.Contains(" Ring"))
                        count = 2;

                    gearsetInputSb.Append($"{item}:{count}");

                    if (hq)
                        gearsetInputSb.Append($" hq");

                    if (i < gearset.Count)
                        gearsetInputSb.Append(", ");

                    i++;
                }

                commandInput = gearsetInputSb.ToString();
            }

            // regarding interactivecommandreturn, we can't actually take advantage of the interactiveuserselect stuff since this command typically
            // takes multiple items and it's not designed for that - we pass this param so it knows to NOT try to handle this interactively
            List<MarketCommandInputsModel> parsedInputs = await SplitCommandInputs(commandInput, InteractiveCommandReturnOptions.Order);

            if (!parsedInputs.Any())
            {
                await ReplyAsync("I couldn't get info for one or more items entered. This command can only take exact item names.");
                return;
            }

            var itemsList = new List<MarketItemCrossWorldOrderModel>();
            foreach (var item in parsedInputs)
            {
                itemsList.Add(new MarketItemCrossWorldOrderModel() { Name = item.ItemName, ItemID = item.ItemID, NeededQuantity = item.NeededQuantity, ShouldBeHQ = item.ItemHq});
            }
            
            var results = await MarketService.GetMarketCrossworldPurchaseOrder(itemsList, parsedInputs[0].WorldsToSearch);

            // sort the results into different lists, grouped by server
            var purchaseOrder = results.GroupBy(x => x.Server).ToList();

            // check the list to verify if we got back all of the items we requested
            // if any are missing, add them to the stringbuilder
            string itemsMissingFromOrder = null;
            var itemsMissingFromOrderStringBuilder = new StringBuilder();
            foreach (var input in parsedInputs)
            {
                if (!results.Any(x => x.Name == input.ItemName))
                {
                    itemsMissingFromOrderStringBuilder.Append($"{input.ItemName}");
                    if (input != parsedInputs.Last())
                    {
                        itemsMissingFromOrderStringBuilder.Append(", ");
                    }
                }
            }
            if (itemsMissingFromOrderStringBuilder.Length > 0)
            {
                itemsMissingFromOrder = $"The following orders could not be fulfilled due to lack of supply: {itemsMissingFromOrderStringBuilder}";
            }

            // to get the overall cost of the order
            var totalCost = 0;

            // build the embed
            var purchaseOrderEmbed = new EmbedBuilder();
            foreach (var server in purchaseOrder)
            {
                List<StringBuilder> purchaseOrderFields = new List<StringBuilder>();

                StringBuilder purchaseOrderSb = new StringBuilder();
                // convert this server IGrouping to a list so we can access its values easily
                var serverList = server.ToList();

                // build this server's item list 
                foreach (var item in serverList)
                {
                    var quality = item.IsHQ ? "(HQ)" : "";
                    var entry = $"{item.Name} {quality} - {item.Quantity} for {item.Price:N0} (total: {item.Quantity * item.Price:N0})";

                    // handle field overflow by copying field contents to a list and re-initializing a new one to continue with
                    if (purchaseOrderSb.Length + entry.Length > 1024)
                    {
                        purchaseOrderFields.Add(purchaseOrderSb);
                        purchaseOrderSb = new StringBuilder();
                    }

                    purchaseOrderSb.AppendLine(entry);

                    totalCost += item.Price * item.Quantity;
                }

                // add current purchaseOrder stringbuilder to the fields list
                purchaseOrderFields.Add(purchaseOrderSb);

                foreach (var purchaseOrderField in purchaseOrderFields)
                {
                    var field = new EmbedFieldBuilder();

                    // regular field
                    field.Name = $"{serverList[0].Server}";
                    field.Value = purchaseOrderField.ToString();
                    purchaseOrderEmbed.AddField(field);
                }

                // embed title
                purchaseOrderEmbed.Title = $"Total cost: {totalCost}";

            }

            // sending a null embed will cause an exception, so check for it before we send out
            if (purchaseOrderEmbed.Title != null)
                await ReplyAsync(null, false, purchaseOrderEmbed.Build());
            else
                await ReplyAsync(itemsMissingFromOrder, false);

        }

        [Command("market collectable")]
        [Alias("mbc")]
        [Summary("Determine the best collectable to make, based on estimated times & prices")]
        [Syntax(
            "market collectible {number of collectables to make}")]
        [Example("mbc 25")]
        [RequireOwner]
        public async Task MarketGetBestCollectable(int numOfCollectables = 1)
        {
            // rough average of the time it takes to gather a single item
            // includes travel time
            double secondsPerGatheredItem = 2.2;
            // based on lemonette node, since lemonette is the only timed node used in collectables
            var itemsPerTimedNode = 42;
            // seconds per item when quicksynthing subcrafts
            var secondsPerSubcraft = 3.5;

            // we specifically want all worlds enabled
            var worlds = GetServer(" ", false);

            var yellowCrafterScripCollectableList = CollectableRecipesDataset.YellowCrafterScripTurnInsList;

            foreach (var collectable in yellowCrafterScripCollectableList)
            {
                // === figure out how long it'll take to gather this recipe's materials
                double totalTimeToGather = 0.0;
                foreach (var recipeMat in collectable.RecipeMaterials.Where(x => x.Material.IsGatherable))
                {
                    // how many timed nodes will we need to hit
                    if (recipeMat.Material.IsTimedNode)
                    {
                        // how many nodes we'll need to hit to gather the materials - round down
                        collectable.TotalTimedNodeCycles = Math.Ceiling((double)(recipeMat.Quantity * numOfCollectables) / itemsPerTimedNode);
                    }
                    
                    // regular node
                    totalTimeToGather += (recipeMat.Quantity * numOfCollectables) * secondsPerGatheredItem;
                }
                collectable.TotalGatherTime = Math.Round(totalTimeToGather / 60, 1); // to minutes, round 


                // === figure out how much the non-gatherable mats will cost to buy
                var itemsList = new List<MarketItemCrossWorldOrderModel>();
                foreach (var recipeMat in collectable.RecipeMaterials.Where(x => x.Material.IsGatherable == false))
                {
                    itemsList.Add(new MarketItemCrossWorldOrderModel() { Name = recipeMat.Material.Name, ItemID = recipeMat.Material.ItemID, NeededQuantity = (recipeMat.Quantity * numOfCollectables), ShouldBeHQ = false });
                }

                // get back purchase orders for the recipe mats
                var results = await MarketService.GetMarketCrossworldPurchaseOrder(itemsList, worlds);

                // add up their costs
                var totalValue = 0;
                foreach (var order in results)
                {
                    totalValue += order.Price * order.Quantity;
                }
                collectable.TotalPrice = totalValue;

                // === figure out how long it'll take to do the subcrafts, minutes
                collectable.TotalSubcraftTime = Math.Round((collectable.SubcraftCount * numOfCollectables * secondsPerSubcraft) / 60, 1);
            }

            // sort from lowest to highest total price 
            var parsedCollectablesList = yellowCrafterScripCollectableList.OrderBy(x => x.TotalGatherTime);


            var collectableListEmbed = new EmbedBuilder();

            foreach (var collectable in parsedCollectablesList)
            {
                var collectableField = new EmbedFieldBuilder()
                {
                    Name = collectable.Name,
                    IsInline = true
                };

                var collectableFieldSb = new StringBuilder();

                if (collectable.TotalGatherTime > 0)
                    collectableFieldSb.AppendLine($" - Est. gather time: {collectable.TotalGatherTime}m");

                if (collectable.SubcraftCount > 0)
                    collectableFieldSb.AppendLine($" - Est. subcraft time: {collectable.TotalSubcraftTime}m");

                if (collectable.TotalPrice > 0)
                    collectableFieldSb.AppendLine($" - Price: {collectable.TotalPrice}");

                if (collectable.TotalTimedNodeCycles > 0)
                    collectableFieldSb.AppendLine($" - Timed node visits: {collectable.TotalTimedNodeCycles}");

                collectableField.Value = collectableFieldSb.ToString();

                collectableListEmbed.AddField(collectableField);
            }

            await ReplyAsync(null, false, collectableListEmbed.Build());
        }

        [Command("market status")]
        [Alias("mbs")]
        [Summary("Display login status of servers & display number of API requests performed")]
        public async Task MarketStatus([Remainder] string input = null)
        {
            var marketStatusSb = new StringBuilder();
            marketStatusSb.AppendLine("**Server login status**");

            foreach (var entry in APIHeartbeatService.serverLoginStatusTracker)
            {
                string serverStatus;

                if (entry.Value)
                    serverStatus = "✅";
                else
                    serverStatus = "❌";

                marketStatusSb.AppendLine($"{entry.Key}: {serverStatus}");
            }

            marketStatusSb.AppendLine();
            marketStatusSb.AppendLine("**API status**");
            // TODO: add timezone conversion function somewhere and use it instead of this addhours(-4) crap
            marketStatusSb.Append(
                $"{APIRequestService.totalCustomAPIRequestsMade} requests since {System.Diagnostics.Process.GetCurrentProcess().StartTime.AddHours(-4)} ({APIRequestService.totalCustomAPIRequestsMadeSinceHeartbeat} since last heartbeat check).");

            await ReplyAsync(marketStatusSb.ToString());
        }

        /*
        [Command("market watchlist add")]
        [Alias("mwa")]
        [RequireOwner]
        [Summary("Add item to market watchlist")]
        [Syntax("market watchlist add {name/id}")]
        [Example("mwa heavens' eye materia viii")]
        public async Task MarketWatchlistAdd([Remainder] string input)
        {
            // check if companion api is down - func will inform if not
            if (await IsCompanionAPIUsable() == false)
                return;

            // clean inputs
            input = input.ToLower();
            input = CleanCommandInput(input);

            // replace hq text in input var if it exists, and set shouldbehq flag to true
            bool inputShouldBeHq = false;
            if (input.Contains("hq"))
            {
                inputShouldBeHq = true;
                input = input.Replace("hq", "").Trim();
            }

            // grab item id 
            var inputId = await MarketService.SearchForItemByNameExact(input);

            // build entry
            var watchlistEntry = new DbWatchlistEntry()
            {
                ItemName = input,
                ItemID = inputId[0].ID,
                HQOnly = inputShouldBeHq
            };

            // add to database
            await DatabaseMarketWatchlist.AddToWatchlist(watchlistEntry);

            await ReplyAsync($"Added item {watchlistEntry.ItemName} ({watchlistEntry.ItemID}) to watchlist.");
        }

        [Command("market watchlist remove")]
        [Alias("mwr")]
        [RequireOwner]
        [Summary("Remove item from market watchlist")]
        [Syntax("market watchlist remove {name/id}")]
        [Example("mwr heavens' eye materia viii")]
        public async Task MarketWatchlistRemove([Remainder] string input)
        {
            // check if companion api is down - func will inform if not
            if (await IsCompanionAPIUsable() == false)
                return;

            // clean inputs
            input = input.ToLower();
            input = CleanCommandInput(input);

            // grab item id 
            var inputId = await MarketService.SearchForItemByNameExact(input);

            // build entry
            var watchlistEntry = new DbWatchlistEntry()
            {
                ItemName = input,
                ItemID = inputId[0].ID,
            };

            // add to database
            await DatabaseMarketWatchlist.RemoveFromWatchlist(watchlistEntry);

            await ReplyAsync($"Removed item {watchlistEntry.ItemName} ({watchlistEntry.ItemID}) from watchlist.");
        }

        [Command("market watchlist toggle")]
        [Alias("mwt", "mwm")]
        [RequireOwner]
        [Summary("Toggle watchlist checking & reporting")]
        public async Task MarketWatchlistMute()
        {
            if (MarketWatcherService.WatchlistMuted)
                MarketWatcherService.WatchlistMuted = false;
            else
                MarketWatcherService.WatchlistMuted = true;

            await ReplyAsync($"Watchlist is now {(MarketWatcherService.WatchlistMuted ? "muted" : "unmuted")}.");
        }

        [Command("market watchlist cutoff")]
        [Alias("mwc")]
        [RequireOwner]
        [Summary("Adjust watchlist analysis differential cutoff")]
        [Syntax("market watchlist cutoff {percent}")]
        [Example("mwc 40")]
        public async Task MarketWatchlistSetDifferentialCutoff(int cutoff)
        {
            MarketWatcherService.DifferentialCutoff = cutoff;
            await ReplyAsync($"Watchlist report cutoff set to {cutoff}%");
        }

        [Command("market watchlist run")]
        [Alias("mwrun")]
        [RequireOwner]
        [Summary("Force-run watchlist")]
        [Example("mwrun")]
        public async Task MarketWatchlistForceRun()
        {
            await MarketWatcherService.WatchlistTimerTick();
        }

        [Command("market watchlist list")]
        [Alias("mwl")]
        [RequireOwner]
        [Summary("Show the contents of the watchlist")]
        public async Task MarketWatchlistShowList()
        {
            var watchlist = await MarketWatcherService.GetMarketWatchlist();
            var embed = new EmbedBuilder();
            var watchlistSb = new StringBuilder();

            foreach (var item in watchlist)
            {
                watchlistSb.AppendLine(item);
            }

            embed.WithTitle($"Watchlist - diff cutoff: {MarketWatcherService.DifferentialCutoff}%");
            embed.WithDescription(watchlistSb.ToString());

            await ReplyAsync(null, false, embed.Build());
        }
        */


        /// <summary>
        /// Clean up & parse command inputs
        /// For use with commands that take item names & potentially server as inputs
        /// </summary>
        /// <param name="input">Command input</param>
        /// <param name="function">The corresponding <c>InteractiveCommandReturn</c> for the command calling this function</param>
        /// <returns>List of <c>MarketCommandInputsModel</c>, containing item name, ID and other input bits</returns>
        private async Task<List<MarketCommandInputsModel>> SplitCommandInputs(string input, InteractiveCommandReturnOptions function)
        {
            List <MarketCommandInputsModel> inputsSplit = new List<MarketCommandInputsModel>();

            // convert to lowercase so that if user specified server in capitals,
            // it doesn't break our text matching in serverlist and with api request
            input = input.ToLower();

            // if multiple item inputs are given, split them by comma delimiter
            // won't break anything if no delimiter is found
            var individualInputs = input.Split(',');

            foreach (var individualInput in individualInputs)
            {
                // clean up input
                var worldsToSearch = GetServer(individualInput, false);
                var itemShouldBeHq = CheckIfUserRequestedHq(individualInput);
                var quantity = GetUserRequestedQuantity(individualInput); // used for market order command
                var inputName = CleanCommandInput(individualInput);
                

                // try to get an itemid from input - returns null if failure
                var itemIdResponse = await GetItemIdFromInput(inputName, function, worldsToSearch);

                if (itemIdResponse == null)
                    return inputsSplit;

                var itemID = itemIdResponse.Value;

                // TODO: do we need to check the success of this function afterwards?
                // we can test by running various commands with incorrect spellings and see how commands react, if at all
                var itemDetailsQueryResult = await APIRequestService.QueryXivapiWithItemId(itemID);

                var itemName = itemDetailsQueryResult.Name;
                var itemIconUrl = $"https://xivapi.com/{itemDetailsQueryResult.Icon}";

                var inputModel = new MarketCommandInputsModel()
                {
                    ItemName = itemName,
                    ItemID = itemID,
                    ItemIconUrl = itemIconUrl,
                    ItemHq = itemShouldBeHq,
                    NeededQuantity = quantity,
                    WorldsToSearch = worldsToSearch
                };

                inputsSplit.Add(inputModel);
            }

            return inputsSplit;
        }


        // provides an item id for market commands - 
        private async Task<int?> GetItemIdFromInput(string input, InteractiveCommandReturnOptions function, List<string> worldsToSearch)
        {
            int itemId;

            int maxNumberOfResults = 100;

            // try to see if the given text is an item ID
            var searchTermIsItemId = int.TryParse(input, out itemId);

            // if user passed a itemname, get corresponding itemid.
            if (!searchTermIsItemId)
            {
                List<ItemSearchResultModel> itemIdQueryResult;

                if (function == InteractiveCommandReturnOptions.Order)
                    itemIdQueryResult = await MarketService.SearchForItemByNameExact(input);
                else
                    itemIdQueryResult = await MarketService.SearchForItemByName(input);

                // something is wrong with xivapi
                if (itemIdQueryResult == null)
                {
                    await Context.Channel.SendMessageAsync(
                        "Something is wrong with XIVAPI. Try using Garlandtools to get the item's ID and use that instead.");
                    return null;
                }

                // no results
                if (itemIdQueryResult.Count == 0)
                {
                    await Context.Channel.SendMessageAsync($"No tradable items found for the search term \"{input}\". Check for typos.");
                    return null;
                }

                // too many results
                if (itemIdQueryResult.Count > maxNumberOfResults)
                {
                    await Context.Channel.SendMessageAsync($"Too many results found ({itemIdQueryResult.Count}). Try narrowing down your search terms.");
                    return null;
                }

                

                // if more than one result was found, send the results to the selection function to narrow it down to one
                // terminate this function, as the selection function will eventually re-call this method with a single result item
                // 10 is the max number of items we can use interactiveuserselectitem with
                if (itemIdQueryResult.Count > 1 && itemIdQueryResult.Count < maxNumberOfResults && function != InteractiveCommandReturnOptions.Order)
                {
                    await InteractiveUserSelectItem(itemIdQueryResult, function, worldsToSearch);
                    return null;
                }

                // if we can't find a singular item to return, and interactiveuserselection would kick in, but this was called from
                // the market order command, then end the function and return null
                if (itemIdQueryResult.Count > 1 && function == InteractiveCommandReturnOptions.Order)
                    return null;

                // if only one result was found, select it and continue without any prompts
                if (itemIdQueryResult.Count == 1)
                    itemId = itemIdQueryResult[0].ID;

            }

            return itemId;
        }



        private async Task InteractiveUserSelectItem(List<ItemSearchResultModel> itemsList, InteractiveCommandReturnOptions function, List<string> worldsToSearch)
        {
            var totalItems = itemsList.Count;

            EmbedBuilder embedBuilder = new EmbedBuilder();

            List<StringBuilder> potentialItemsListCollection = new List<StringBuilder>();
            StringBuilder potentialItemsList = new StringBuilder();

            
            for (int i = 0; i < totalItems; i++)
            {
                var itemsDictionaryName = itemsList[i].Name;
                var entry = $"{i} - {itemsDictionaryName}";

                // handle field overflow by copying field contents to a list and re-initializing a new one to continue with
                if (potentialItemsList.Length + entry.Length > 1024)
                {
                    potentialItemsListCollection.Add(potentialItemsList);
                    potentialItemsList = new StringBuilder();
                }

                potentialItemsList.AppendLine(entry);
            }

            // add current potentialItemsList stringbuilder to the fields list
            potentialItemsListCollection.Add(potentialItemsList);

            foreach (var itemlist in potentialItemsListCollection)
            {
                var field = new EmbedFieldBuilder();
                field.Name = "Test";
                field.Value = itemlist.ToString();
                field.IsInline = true;
                embedBuilder.AddField(field);
            }

            //embedBuilder.WithDescription("Test");
            embedBuilder.WithColor(Color.Blue);

            await ReplyAsync("Please type in the number corresponding to the item you're looking for.", false, embedBuilder.Build());
            var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(20));
            if (response != null)
            {
                var result = int.TryParse(response.Content, out int responseID);
                if (result == false)
                {
                    await ReplyAsync($"Your input \"{result}\" was not valid. Please enter a numeral.");
                }

                // show that the bot's processing
                await Context.Channel.TriggerTypingAsync();

                await HandleInteractiveUserSelectCallback(itemsList[responseID].ID, function, worldsToSearch);
            }
            else
                await ReplyAsync($"Didn't get a response from {Context.User} within 20 seconds - please try again once you've made your decision.");
        }


        // this might get modified to accept a 'function' param that will run in a switch:case to
        // select what calling function this callback handler should re-run with the user-selected data
        // optional server parameter to preserve server filter option
        private async Task HandleInteractiveUserSelectCallback(int itemId, InteractiveCommandReturnOptions function, List<string> worldsToSearch)
        {
            string searchLocation = "";

            // if user originally specified a world for the command, set it as the searchLocation to pass back to the command
            if (worldsToSearch.Intersect(MarketService.DefaultDatacenter).Count() == 1)
            {
                searchLocation = worldsToSearch.FirstOrDefault(x => MarketService.DefaultDatacenter.Contains(x));
            }

            switch (function)
            {
                case InteractiveCommandReturnOptions.Price:
                    await MarketGetItemPrice($"{searchLocation} {itemId}");
                    break;
                case InteractiveCommandReturnOptions.History:
                    await MarketGetItemHistory($"{searchLocation} {itemId}");
                    break;
                case InteractiveCommandReturnOptions.Analyze:
                    await MarketAnalyzeItem($"{searchLocation} {itemId}");
                    break;
            }
        }

        // returns a list of strings representing the servers that should be parsed
        // can be either one server, in the case of the user requesting a specific server, or all servers in a datacenter
        private List<string> GetServer(string input, bool useDefaultWorld)
        {

            //TODO: somewhere in here, we need to add a check to our database that queries for a user's preferred datacenter
            // we'll also need to add a command to allow the user to *set* that preferred datacenter

            var resultsList = new List<string>();

            string server = null;

            // look to see if the input contains one of the server names
            foreach (var world in MarketService.DefaultDatacenter)
            {
                if (input.Contains(world.ToString()))
                {
                    server = world.ToString();
                }
            }

            // if user supplied a server, return that in the list
            if (server != null)
            {
                resultsList.Add(server);
                return resultsList;
            }

            // if we didn't find a server, but calling function requested we use the default world instead of the default datacenter,
            // return the default world instead of the default datacenter
            if (useDefaultWorld && server == null)
            {
                resultsList.Add(MarketService.DefaultWorld.ToString());
                return resultsList;
            }

            // otherwise, add every server in the datacenter
            foreach (var world in MarketService.DefaultDatacenter)
                resultsList.Add(world.ToString());

            return resultsList;
        }

        //
        private string CleanCommandInput(string input)
        {
            var wordsToRemove = new List<string>();
            string result = input;

            // add each possible input into a list of words to look for
            foreach (var world in MarketService.DefaultDatacenter)
                wordsToRemove.Add(world.ToString());
            wordsToRemove.Add("hq");

            foreach (var word in wordsToRemove)
            {
                if (Regex.Match(input, $@"\b{word}\b", RegexOptions.IgnoreCase).Success)
                {
                    result = ReplaceWholeWord(result, word, "");
                }
            }

            // if input contains a quantity request, get rid of it (quantity should be checked before cleaning)
            var resultSplit = result.Split(':');
            result = resultSplit[0].Trim(); // clean up any whitespace

            return result;
        }

        //
        private bool CheckIfUserRequestedHq(string input)
        {
            if (input.Contains("hq"))
                return true;
            return false;
        }

        // eg market order where request is marked by a colon followed by desired quantity
        private int GetUserRequestedQuantity(string input)
        {
            var inputSplit = input.Split(':');
            
            // if so, try to get the quantity and return it
            if (inputSplit.Count() > 1)
            {
                // regex to match numbers in case we get a scenario like 'mbo grade 3 tincture of strength hq:300 gilgamesh'
                // which would put 'gilgamesh' in the number side of the delimiter and cause it to fail
                var success = int.TryParse(Regex.Match(inputSplit[1], @"\d+").Value, out var quantity);

                if (success)
                    return quantity;
                else
                    return 0; // failure case, if someone inputs a non-number after a colon
            }

            // if no quantity was requested, assume 1 - though nothing should actually use this response
            return 1;
        }

        //
        private async Task<bool> IsCompanionAPIUsable()
        {
            var isApiUp = await APIHeartbeatService.IsCompanionAPIUsable();

            if (isApiUp)
                return true;
            
            await InformUserOfAPIFailure();
            return false;
            
        }

        //
        private async Task InformUserOfAPIFailure()
        {
            var apiStatus = APIHeartbeatService.ApiStatus;

            var humanReadableStatus = GetCustomAPIStatusFailureReason(apiStatus);

            await Context.Channel.SendMessageAsync(humanReadableStatus);
        }

        //
        private string GetCustomAPIStatusFailureReason(CustomApiStatus status)
        {
            string apiStatusHumanResponse = "";

            var discordBotOwnerId = ulong.Parse(Config["discordBotOwnerId"]);

            if (status == CustomApiStatus.NotLoggedIn)
                apiStatusHumanResponse = $"Not logged in to Companion API. I'll be trying to log in soon.";
            if (status == CustomApiStatus.UnderMaintenance)
                apiStatusHumanResponse = "SE's API is down for maintenance. Check back later.";
            if (status == CustomApiStatus.AccessDenied)
                apiStatusHumanResponse = $"Access denied. I'm working on it.";
            if (status == CustomApiStatus.ServiceUnavailable || status == CustomApiStatus.APIFailure)
                apiStatusHumanResponse = $"Something broke somewhere. I'm working on it.";

            return apiStatusHumanResponse;
        }

        //
        private string ReplaceWholeWord(string original, string wordToFind, string replacement, RegexOptions regexOptions = RegexOptions.None)
        {
            string pattern = $@"\b{wordToFind}\b";
            string replaced = Regex.Replace(original, pattern, replacement, regexOptions).Trim(); // remove the unwanted word
            string ret = Regex.Replace(replaced, @"\s+", " "); // clear out any excess whitespace
            return ret;
        }
    }
}
