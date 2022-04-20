using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Astramentis
{
    public class StartupService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public StartupService(
            IServiceProvider provider,
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config)
        {
            _provider = provider;
            _config = config;
            _discord = discord;
            _commands = commands;
        }

        public async Task StartAsync()
        {
            // Get the discord token from the config file
            string discordToken = _config["tokens:discord"];
            if (string.IsNullOrWhiteSpace(discordToken))
                throw new Exception("Please enter your bot's token into the `_config.yml` file found in the application's root directory.");

            // login to discord and connect
            await _discord.LoginAsync(TokenType.Bot, discordToken);
            await _discord.StartAsync();

            _discord.Ready += OnReady;
        }

        private async Task OnReady()
        {
            // this check breaks because getuser always returns null now for some reason, despite
            // the discord client being marked as ready. going to test not having this check for now. 
            /*
            // check if the discordBotOwnerId entry in the config file is correct - check if it exists, and then check if it's valid
            var discordBotOwnerIdSet = ulong.TryParse(_config["discordBotOwnerId"], out var discordBotOwnerId);
            if (discordBotOwnerIdSet)
            {
                var test = _discord.GetUser(discordBotOwnerId);
                if (_discord.GetUser(discordBotOwnerId) == null)
                    throw new Exception("Please verify that your Discord user ID in the `_config.yml` file is correct.");
            }
            else
                throw new Exception("Please enter your Discord user ID into the `_config.yml` file found in the application's root directory.");
            */

            // Load commands and modules into the command service
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }
    }
}
