using System;
using System.Threading.Tasks;
using Astramentis.Services;
using Astramentis.Services.Logging;
using Astramentis.Services.MarketServices;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Astramentis
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(string[] args)
        {
            var builder = new ConfigurationBuilder()        // Create a new instance of the config builder
                .SetBasePath(AppContext.BaseDirectory)      // Specify the default location for the config file
                .AddYamlFile("_config.yml");                // Add this (yaml encoded) file to the configuration
            Configuration = builder.Build();                // Build the configuration

        }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            var services = new ServiceCollection();             // Create a new instance of a service collection
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();     // Build the service provider

            provider.GetRequiredService<LoggingService>();      // Start the logging service
            provider.GetRequiredService<DiscordClientLoggingService>();
            provider.GetRequiredService<CommandHandler>(); 		// Start the command handler service

            await provider.GetRequiredService<StartupService>().StartAsync();       // Start the startup service

            provider.GetRequiredService<APIRequestService>();   // start api reuest service
            provider.GetRequiredService<APIHeartbeatService>(); // start api heartbeat timer

            //provider.GetRequiredService<LiamWatcherService>();

            await Task.Delay(-1);                               // Keep the program alive
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {                                       // Add discord to the collection
                LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                MessageCacheSize = 1000,             // Cache 1,000 messages per channel
                AlwaysDownloadUsers = true
            }))
            .AddSingleton(new CommandService(new CommandServiceConfig
            {                                       // Add the command service to the collection
                LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                DefaultRunMode = RunMode.Async,     // Force all commands to run async by default
            }))
            .AddSingleton<CommandHandler>()
            .AddSingleton<StartupService>()
            .AddSingleton<LoggingService>()
            .AddSingleton<DiscordClientLoggingService>()
            .AddSingleton<InteractiveService>()
            .AddSingleton<Random>()

            // database service and supporting subservices
            // database service and supporting subservices
            /* 
            .AddSingleton<DatabaseService>()
            .AddSingleton<DatabaseSudo>()
            .AddSingleton<DatabaseTags>()
            .AddSingleton<DatabaseServers>()
            .AddSingleton<DatabaseMarketWatchlist>()
            .AddSingleton<DatabaseSupport>()
            */

            // market backend
            .AddSingleton<APIRequestService>() // api request functions
            .AddSingleton<APIHeartbeatService>() // api health checks
            .AddSingleton<MarketService>() // market data processing
            //.AddSingleton<MarketWatcherService>() // market watchlist 

            .AddSingleton(Configuration);           // Add the configuration to the collection
        }
    }
}
