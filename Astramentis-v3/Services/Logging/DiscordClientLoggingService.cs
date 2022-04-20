using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NLog;

namespace Astramentis.Services.Logging
{
    public class DiscordClientLoggingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly LoggingService _loggingService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DiscordClientLoggingService(DiscordSocketClient discord, CommandService commands, LoggingService loggingService)
        {
            _discord = discord;
            _commands = commands;
            _loggingService = loggingService;

            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;

        }

        private async Task OnLogAsync(LogMessage msg)
        {
            Logger.Log(ConvertLogSeverityToLogLevel(msg.Severity), $"{msg.Message} {msg.Exception}");
        }

        // convert discord's logseverity to nlog's loglevel
        private LogLevel ConvertLogSeverityToLogLevel(LogSeverity logSeverity)
        {
            switch (logSeverity)
            {
                case LogSeverity.Critical:  // total failure
                    return LogLevel.Fatal;
                case LogSeverity.Error:     // recoverable failure
                    return LogLevel.Error;
                case LogSeverity.Warning:   // may cause problems
                    return LogLevel.Warn;
                case LogSeverity.Debug:     // debug info without discord.net stuff
                    return LogLevel.Debug;
                case LogSeverity.Info:      // usual level of info
                    return LogLevel.Info;
                case LogSeverity.Verbose:   // everything including discord.net stuff
                    return LogLevel.Trace;
            }

            return LogLevel.Off;
        }
    }
}
