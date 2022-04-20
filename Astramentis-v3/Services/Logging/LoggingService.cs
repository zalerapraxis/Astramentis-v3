using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Targets;

namespace Astramentis.Services.Logging
{
    public class LoggingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IConfigurationRoot _config;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private string _logDirectory { get; }
        private string _logFile => Path.Combine(_logDirectory, "log.txt");

        // DiscordSocketClient and CommandService are injected automatically from the IServiceProvider
        public LoggingService(DiscordSocketClient discord, IConfigurationRoot config)
        {
            _logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");

            _discord = discord;
            _config = config;

            // implement custom target to send messages to the bot admin via Discord DMs
            Target.Register<NLogDiscordTarget>("NLogDiscordTarget");

            var logConfig = new NLog.Config.LoggingConfiguration();

            // configure how NLog will display log messages
            var logLayout = "${longdate}|${level:uppercase=true}|${logger:shortName=true}|${message}";

            // configure targets for NLog to send messages to
            var logFile = new FileTarget("logFile") { FileName = _logFile, ArchiveEvery = FileArchivePeriod.Day, Layout = logLayout };
            var logDiscord = new NLogDiscordTarget { DiscordClient = _discord, DiscordBotOwnerId = ulong.Parse(_config["discordBotOwnerId"]), Layout = logLayout };
            var logConsole = new ConsoleTarget("logConsole") {Layout = logLayout };

            // tell NLog what range of LogLevels to send to each target
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logConsole);
            logConfig.AddRule(LogLevel.Error, LogLevel.Fatal, logDiscord);

            LogManager.Configuration = logConfig;
        }
    }
}
