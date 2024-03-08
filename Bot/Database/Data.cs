using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using Newtonsoft.Json.Converters;
using Npgsql;
using NpgsqlTypes;


namespace App.Bot.Database
{
    internal class Data
    {

        private NpgsqlDataSource? dataSource;
        private DataConfig databaseConfig;

        public Data(DataConfig config)
        {
            this.databaseConfig = config;

            initConnection();
        }

        private async void initConnection()
        {
            var connectionString = @$"host={databaseConfig.Host}:{databaseConfig.Port};
Username={databaseConfig.Username};
Password={databaseConfig.Password};
Database={databaseConfig.Database}";

            dataSource = NpgsqlDataSource.Create(connectionString);

            try
            {
                await dataSource.CreateCommand(databaseConfig.userTableCreate).ExecuteNonQueryAsync();
                await dataSource.CreateCommand(databaseConfig.strikeTableCreate).ExecuteNonQueryAsync();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task<UserInfo> GetUserInfo(ulong id)
        {
            var dbConnection = await dataSource!.OpenConnectionAsync();
            var transaction = await dbConnection!.BeginTransactionAsync();
            await using var cmd = new NpgsqlCommand("SELECT * FROM public.profile WHERE user_id = @id", dbConnection, transaction)
            {
                Parameters =
                {
                    new("id", id.ToString())
                }
            };

            var info = await cmd.ExecuteReaderAsync();

            await Task.Factory.StartNew(async () =>
            {
                await Task.Delay(10000);
                await dbConnection.CloseAsync();
            });

            return ReadUserInfo(info!);
        }

        public async Task<StrikeInfo> GetStrikeInfo(ulong id)
        {
            var dbConnection = await dataSource!.OpenConnectionAsync();
            var transaction = await dbConnection!.BeginTransactionAsync();
            await using var cmd = new NpgsqlCommand($"SELECT * FROM public.strikes WHERE id = {id.ToString()}", dbConnection, transaction);

            var info = await cmd.ExecuteReaderAsync();

            await Task.Factory.StartNew(async () =>
            {
                await Task.Delay(3000);
                await dbConnection.CloseAsync();
            });

            return ReadStrikeInfo(info);
        }

        public async Task<StrikeInfo> AddStrike(StrikeInfo strike)
        {
            var dbConnection = await dataSource!.OpenConnectionAsync();
            var transaction = await dbConnection!.BeginTransactionAsync();
            await using var cmd = new NpgsqlCommand("INSERT INTO public.strikes (user_id, reason, issuer_id, issue_time) VALUES (@user_id, @reason, @issuer_id, @issue_time)",
                dbConnection, 
                transaction)
            {
                Parameters =
                {
                    new("user_id", Convert.ToInt64(strike.UserId)),
                    new("reason", strike.Reason),
                    new("issuer_id", strike.IssuerId.ToString()),
                    new("issue_time", strike.Timestamp)
                }
            };

            await using var cmd2 = new NpgsqlCommand("SELECT * FROM public.strikes WHERE issue_time = @issue_time",
                dbConnection,
                transaction)
            {
                Parameters =
                {
                    new("issue_time", strike.Timestamp)
                }
            };

            await cmd.ExecuteNonQueryAsync();
            

            await transaction.CommitAsync();

            var info = ReadStrikeInfo(await cmd2.ExecuteReaderAsync());
            transaction.Dispose();
            await dbConnection.CloseAsync();

            return info;
        }

        public async void RemoveStrike(long strike)
        {
            var dbConnection = await dataSource!.OpenConnectionAsync();
            var transaction = await dbConnection!.BeginTransactionAsync();
            await using var cmd = new NpgsqlCommand("DELETE FROM public.strikes WHERE id = @id",
                dbConnection,
                transaction)
            {
                Parameters =
                {
                    new("id", strike)
                }
            };

            await cmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            transaction.Dispose();
            await dbConnection.CloseAsync();
        }

        public async Task AddUser(UserInfo user)
        {
            var dbConnection = await dataSource!.OpenConnectionAsync();
            var transaction = await dbConnection!.BeginTransactionAsync();
            await using (var cmd = new NpgsqlCommand("INSERT INTO public.profile (user_id, total_strikes, strike_reset, current_strikes) VALUES (@user_id, @total_strikes, @strike_reset, @current_strikes)",
                dbConnection, transaction))
            {
                cmd.Parameters.Add(new NpgsqlParameter<string>("user_id", NpgsqlDbType.Varchar));
                cmd.Parameters.Add(new NpgsqlParameter<int>("total_strikes", NpgsqlDbType.Integer));
                cmd.Parameters.Add(new NpgsqlParameter<int>("current_strikes", NpgsqlDbType.Integer));
                cmd.Parameters.Add(new NpgsqlParameter("strike_reset", NpgsqlDbType.Timestamp));
                cmd.Parameters[0].Value = user.Id.ToString();
                cmd.Parameters[1].Value = user.TotalStrikes;
                cmd.Parameters[2].Value = user.CurrentStrikes;
                cmd.Parameters[3].Value = user.StrikeReset == null ? DBNull.Value : user.StrikeReset;

                await cmd.ExecuteNonQueryAsync();
            };

            

            await transaction.CommitAsync();
            transaction.Dispose();
            await dbConnection.CloseAsync();
        }

        public async Task AddUserStrikeInfo(ulong userId, int total, int current, DateTime? reset)
        {
            var dbConnection = await dataSource!.OpenConnectionAsync();
            var transaction = await dbConnection!.BeginTransactionAsync();
            await using var cmd = new NpgsqlCommand("UPDATE public.profile SET total_strikes = total_strikes + @total, current_strikes = current_strikes + @current, strike_reset = @reset WHERE user_id = @user", dbConnection, transaction)
            {
                Parameters =
                {
                    new("total", total),
                    new("current", current),
                    new("reset", reset == null ? DBNull.Value : reset),
                    new("user", userId.ToString())
                }
            };

            await cmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            transaction.Dispose();
            await dbConnection.CloseAsync();
        }

        public async Task<NpgsqlDataReader> GetStrikeResetUsers()
        {
            var dbConnection = await dataSource!.OpenConnectionAsync();
            var transaction = await dbConnection!.BeginTransactionAsync();
            await using var cmd = new NpgsqlCommand("SELECT * FROM public.profile WHERE strike_reset < @reset", dbConnection, transaction)
            {
                Parameters =
                {
                    new("reset", DateTime.UtcNow),
                }
            };

            var i = await cmd.ExecuteReaderAsync();

            await Task.Factory.StartNew(async () =>
            {
                await Task.Delay(3000);
                await dbConnection.CloseAsync();
            });

            return i;
        }

        public async Task DeleteStrikeRecord(ulong userId)
        {
            var dbConnection = await dataSource!.OpenConnectionAsync();
            var transaction = await dbConnection!.BeginTransactionAsync();
            await using var cmd = new NpgsqlCommand("DELETE FROM public.strikes WHERE user_id = @userId", dbConnection, transaction)
            {
                Parameters =
                {
                    new("userId", userId.ToString()),
                }
            };
            await cmd.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            await dbConnection.CloseAsync();
        }

        public async Task ExecuteSql(string sql)
        {
            var dbConnection = await dataSource!.OpenConnectionAsync();
            var transaction = await dbConnection!.BeginTransactionAsync();
            await using var cmd = new NpgsqlCommand(sql, dbConnection, transaction);
            await cmd.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            await dbConnection.CloseAsync();
        }

        public static UserInfo ReadUserInfo(NpgsqlDataReader reader)
        {
            var userInfo = new UserInfo();

            reader.Read();

            userInfo.Id = Convert.ToUInt64(reader["user_id"] as string);
            userInfo.TotalStrikes = (int)reader["total_strikes"];
            userInfo.StrikeReset = reader["strike_reset"] != DBNull.Value ? reader["strike_reset"] as DateTime? : null;
            userInfo.CurrentStrikes = (int)reader["current_strikes"];

            reader.Close();
            return userInfo;
        }

        public static StrikeInfo ReadStrikeInfo(NpgsqlDataReader reader)
        {
            var info = new StrikeInfo();

            reader.Read();

            info.Id = Convert.ToInt64(reader["id"]);
            info.UserId = Convert.ToUInt64(reader["user_id"] as string);
            info.Reason = (string)reader["reason"];
            info.IssuerId  = Convert.ToUInt64(reader["issuer_id"] as string);
            info.Timestamp = (DateTime)reader["issue_time"];

            reader.Close();
            return info;
        }
    }
}
