using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Astramentis.Attributes;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Astramentis.Services;

namespace Astramentis.Modules
{
    [Name("Item")]
    [Summary("Getting data on FFXIV items")]
    public class ItemModule : InteractiveBase
    {
        public MarketService MarketService { get; set; }


        [Command("item search")]
        [Summary("Search for items by name - requires a search term")]
        [Alias("isearch")]
        [Syntax("item search {name}")]
        [Example("item search dried hi-ether")]
        public async Task ItemSearchAsync([Remainder] string searchTerm)
        {
            // show that the bot's processing
            await Context.Channel.TriggerTypingAsync();

            // response is either a ordereddictionary of keyvaluepairs, or null
            var itemSearchResults = await MarketService.SearchForItemByName(searchTerm);

            if (itemSearchResults == null)
            {
                await ReplyAsync("Something is wrong with XIVAPI. Try using Garlandtools to get the item's ID and use that instead.");
                return;
            }

            // no results
            if (itemSearchResults.Count == 0)
            {
                await ReplyAsync("No results found. Try to expand your search terms, or check for typos.");
                return;
            }


            var pages = new List<EmbedFieldBuilder>();

            var i = 0;
            var itemsPerPage = 12;

            // iterate through the market results, making a page for every (up to) itemsPerPage listings
            while (i < itemSearchResults.Count)
            {
                // pull up to itemsPerPage entries from the list, skipping any from previous iterations
                var currentPageItemSearchResultsList = itemSearchResults.Skip(i).Take(itemsPerPage);

                StringBuilder sbListingName = new StringBuilder();
                StringBuilder sbListingId = new StringBuilder();

                // build data for this page
                foreach (var item in currentPageItemSearchResultsList)
                {
                    sbListingName.AppendLine(item.Name);
                    sbListingId.AppendLine(item.ID.ToString());
                }

                var one = new EmbedFieldBuilder()
                {
                    Name = "Name",
                    Value = sbListingName,
                    IsInline = true
                };
                var two = new EmbedFieldBuilder()
                {
                    Name = "ID",
                    Value = sbListingId,
                    IsInline = true
                };

                pages.Add(one);
                pages.Add(two);

                i = i + itemsPerPage;
            }

            PaginatedMessage paginatedMessage = new PaginatedMessage()
            {
                Pages = pages,
                Options = new PaginatedAppearanceOptions()
                {
                    InformationText = "This is an interactive message. Use the reaction emotes to change pages. Use the :1234: emote and then type a number in chat to go to that page.",
                    Info = new Emoji("ℹ️"),
                    FieldsPerPage = 2
                },
                Color = new Color(33, 176, 252),
                Title = $"{itemSearchResults.Count} result(s) for \"{searchTerm}\""
            };

            await PagedReplyAsync(paginatedMessage);
        }
    }
}
