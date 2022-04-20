using System;
using System.ComponentModel;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Astramentis.Attributes;
using Astramentis.Services;
using NLog;
using System.Collections.Generic;
using NLog.Targets;

namespace Astramentis.Modules
{
    [Name("Clear")]
    [Summary("Clearing chat messages")]
    public class ChatClearingModule : ModuleBase<SocketCommandContext>
    {
        public DiscordSocketClient DiscordSocketClient { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // clear chatlogs from server channels
        [Command("clear")]
        [Summary("Clears the last x messages in current channel - default 100")]
        [Alias("clean", "prune")]
        [Syntax("clear {optional: number of messages}")]
        [Example("clear 5")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearChatlogsAsync(int count = 100)
        {
            // delete the command message first, so we don't factor it in the message count. 
            // this ensures accuracy when a user needs to set a specific number of messages to be removed.
            await Context.Message.DeleteAsync();

            await Task.Delay(1000); // test

            // get messages from channel
            var channel = Context.Channel as SocketTextChannel;
            var messages = await channel.GetMessagesAsync(count).FlattenAsync();

            messages = messages.ToList();

            Logger.Log(LogLevel.Info, $"Deleting {messages.Count()} messages in the channel {channel.Name} in the server {channel.Guild.Name}.");

            // if the messages are older than two weeks, a bulk delete won't work.
            // so it'll catch the resulting error, and switch over to individual deletion.
            try
            {
                await channel.DeleteMessagesAsync(messages);
                Logger.Log(LogLevel.Info, "Bulk deleted messages successfully.");
            }
            catch
            {
                Logger.Log(LogLevel.Info, "Could not bulk delete all messages. Switching to individual deletion.");

                var oldMessages = messages.Where(msg => msg.Timestamp < DateTimeOffset.Now.AddDays(-14));
                var newMessages = messages.Where(msg => msg.Timestamp > DateTimeOffset.Now.AddDays(-14));

                if (oldMessages.Any())
                {
                    // notify the user of individual delete process
                    var responseMsgContents =
                        "Some of the messages you selected are older than two weeks, so we have to individually delete them. This will take some time.";
                    var responseMsgProgress = $"(0/{oldMessages.Count()})";
                    var responseMsg = await ReplyAsync($"{responseMsgContents} {responseMsgProgress}");

                    // don't delete the notification yet
                    newMessages = newMessages.Where(msg => msg.Id != responseMsg.Id);

                    // bulk delete whatever new messages we can
                    await channel.DeleteMessagesAsync(newMessages);

                    await Task.Delay(1000);

                    // provide a progress visual
                    var i = 0;
                    // individually delete old messages
                    foreach (var oldMessage in oldMessages)
                    {
                        await oldMessage.DeleteAsync();
                        i++;
                        if (i % 5 == 0) // every 10th message
                        {
                            responseMsgProgress = $"({i}/{oldMessages.Count()})";
                            await responseMsg.ModifyAsync(m => m.Content = $"{responseMsgContents} {responseMsgProgress}");
                        }
                        await Task.Delay(2000);
                    }

                    // done with deleting stuff, so delete the notification
                    await responseMsg.DeleteAsync();
                }
            }

            Logger.Log(LogLevel.Info, $"Finished deleting {messages.Count()} messages in the channel {channel.Name} in the server {channel.Guild.Name}.");
        }

        // clear chatlogs from DMs
        [Command("clear")]
        [Summary("Clears the last x messages in current channel - default 100")]
        [Alias("clean", "prune")]
        [Syntax("clear {optional: number of messages}")]
        [Example("clear 5")]
        [RequireContext(ContextType.DM)]
        public async Task ClearDMChatlogsAsync(int count = 100)
        {
            var channel = Context.Channel as SocketDMChannel;
            var messages = await channel.GetMessagesAsync().FlattenAsync();

            var botMessages = messages.Where(x => x.Author.Id == DiscordSocketClient.CurrentUser.Id).Take(count).ToList();

            Logger.Log(LogLevel.Info, $"Deleting {botMessages.Count} messages in a DM with {channel.Recipient}.");

            // individually delete old messages
            foreach (var message in botMessages)
            {
                await message.DeleteAsync();
                await Task.Delay(1000);
            }

            Logger.Log(LogLevel.Info, $"Finished deleting {botMessages.Count} messages in a DM with {channel.Recipient}.");
        }
    }
}
