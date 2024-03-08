using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace App.Bot
{
    internal class Config
    {
        [JsonProperty("bot-config")]
        public BotConfig BotConfig { get; set; }

        [JsonProperty("server-config")]
        public ServerConfig ServerConfig { get; set; }

        [JsonProperty("database-config")]
        public DatabaseConfig DatabaseConfig { get; set; }

        [JsonProperty("strike-reset")]
        public int StrikeReset { get; set; }
    }

    internal class BotConfig
    {
        [JsonProperty("bot-token")]
        public string Token {  get; set; }

        [JsonProperty("owner-ids")]
        public List<ulong> Owners { get; set; }

        [JsonProperty("owner-roles")]
        public List<ulong> OwnerRoles { get; set; }

        [JsonProperty("allowed-roles")]
        public List<ulong> AllowedRoles { get; set; }
    }

    internal class ServerConfig
    {
        // The guild id for the bot to execute commands in
        [JsonProperty("guild-id")]
        public ulong GuildId { get; set; }

        // The channels that are used for logs
        [JsonProperty("log-channel")]
        public ulong LogChannel { get; set; }


        // The channel where commands may be issued
        [JsonProperty("command-channel")]
        public ulong CommandChannel { get; set; }

        [JsonProperty("strike-roles")]
        public List<Strike> StrikeRoles { get; set; }
    }

    internal class DatabaseConfig
    {

        // Delay before database refresh/lookup
        [JsonProperty("refresh-delay")]
        public int RefreshDelay { get; set; }

        // Host of the databse
        [JsonProperty("data-host")]
        public string Host {  get; set; }

        // Port that database is connected on
        [JsonProperty("data-port")]
        public string Port { get; set; }

        // Name of the database to connect to
        [JsonProperty("data-database")]
        public string Database {  get; set; }

        // Username for database
        [JsonProperty("data-username")]
        public string Username { get; set; }

        // Password for databse
        [JsonProperty("data-password")]
        public string Password { get; set; }
    }
}
