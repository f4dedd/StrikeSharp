using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using App.Bot.Database;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace App.Bot
{
    class Bot
    {
        // Intents for the bot for now just Guilds and AllUnprivileged until
        // i find the minimum intents needed to get the bot to work
        const DiscordIntents intents = DiscordIntents.Guilds
            | DiscordIntents.AllUnprivileged;

        // Client for the discord.
        public DiscordClient Client { get; private set; }

        // Configuration for the client and other settings
        private readonly Config config;
        public Updater Updater { get; private set; }
        public Data Database { get; private set; }

        public Dictionary<int, Strike> strikeSet;

        // List of owners for the bot
        // Allows use of commands and elevated commands for the bot
        public static Checks.InPermissionList<ulong>? ownerList { get; private set; }


        // List of roles that can execute base commands
        public static Checks.InPermissionList<ulong>? roleList { get; private set; }

        public Bot(Config config)
        {
            this.config = config;
            this.strikeSet = new Dictionary<int, Strike>();

            ConfigBot(config);

            this.Database = new Data(new DataConfig(config));

            foreach (Strike i in config.ServerConfig.StrikeRoles)
            {
                this.strikeSet.Add(i.Level, i);
            }

            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = config.BotConfig.Token,
                Intents = intents,
            });

            this.Updater = new Updater(config.DatabaseConfig.RefreshDelay,
                this.Database,
                this.strikeSet,
                config.StrikeReset,
                this.Client!,
                config.ServerConfig.GuildId,
                config.ServerConfig.LogChannel
                );
            this.Updater.Start();
        }

        // Configuration settings for the bot that can be accessed globally
        private static void ConfigBot(Config config)
        {
            roleList = new Checks.InPermissionList<ulong>(config.BotConfig.AllowedRoles);
            ownerList = new Checks.InPermissionList<ulong>(config.BotConfig.Owners);

        }

        // Starts the bot and runs a loop until program is terminated
        public async void Run()
        {
            if (Client == null)
            {
                throw new Exception("Client not found");
            }
            ServiceCollection services = new ServiceCollection();

            services.AddSingleton<Data>(this.Database);
            services.AddSingleton<Config>(this.config);
            services.AddSingleton<Updater>(this.Updater);

            // Registering commands
            var slash = Client.UseSlashCommands(new SlashCommandsConfiguration { Services = services.BuildServiceProvider() });

            slash.RegisterCommands<Command>();


            // Run the discord client and loop it
            await Client.ConnectAsync();
        }

        public Strike GetStrike(int level)
        {
            return this.strikeSet[level];
        }
    }
}
