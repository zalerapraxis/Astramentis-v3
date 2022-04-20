using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Astramentis.Services.Logging
{
    [Target("NLogDiscordTarget")]
    public sealed class NLogDiscordTarget : AsyncTaskTarget
    {
        [RequiredParameter]
        public DiscordSocketClient DiscordClient { get; set; }

        [RequiredParameter]
        public ulong DiscordBotOwnerId { get; set; }

        protected override Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken token)
        {
            string logMessage = this.RenderLogEvent(Layout, logEvent);
            return SendMessageToBotAdministrator(logMessage);
        }

        private async Task SendMessageToBotAdministrator(string message)
        {
            //await DiscordClient.GetUser(DiscordBotOwnerId).SendMessageAsync(message);
        }
    }
}