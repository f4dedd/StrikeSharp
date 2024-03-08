using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Bot.Database
{
    internal class DataConfig
    {
		
		public string Host {  get;  private set; }

		public string Port { get; private set; }

		public string Database {  get; private set; }

		public string Username { get; private set; }

		public string Password { get; private set; }

        public readonly string userTableCreate = @"CREATE TABLE IF NOT EXISTS public.profile (
	id serial4 NOT NULL,
	user_id varchar NOT NULL,
	total_strikes int4 DEFAULT 0 NOT NULL,
	current_strikes int4 DEFAULT 0 NOT NULL,
	strike_reset timestamptz NULL,
	CONSTRAINT profile_pk PRIMARY KEY (id),
	CONSTRAINT profile_unique UNIQUE (user_id)
);";
        public readonly string strikeTableCreate = @"CREATE TABLE IF NOT EXISTS public.strikes (
	id serial4 NOT NULL,
	user_id varchar NOT NULL,
	reason varchar NOT NULL,
	issue_time timestamptz NOT NULL,
	issuer_id varchar NOT NULL,
	CONSTRAINT strike_pk PRIMARY KEY (id)
);";

		public DataConfig(Config config)
		{
			this.Host = config.DatabaseConfig.Host;
			this.Port = config.DatabaseConfig.Port;
			this.Database = config.DatabaseConfig.Database;
			this.Username = config.DatabaseConfig.Username;
			this.Password = config.DatabaseConfig.Password;
		}

    }
}
