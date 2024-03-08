using App.Bot.Database;
using DSharpPlus;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Microsoft.VisualBasic;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace App.Bot
{
    internal class Updater
    {
        private bool isStopped = false;

        private int delay;

        private Data db;

        private Dictionary<int, Strike> strikeSet;

        private int reset;

        private DiscordClient client;

        private ulong guildId;

        private ulong logChannel;

        public Updater(int delay, Data db, Dictionary<int, Strike> strikeSet, int reset, DiscordClient client, ulong guildId, ulong logChannel)
        {
            this.delay = delay;
            this.db = db;
            this.strikeSet = strikeSet;
            this.reset = reset;
            this.client = client;
            this.guildId = guildId;
            this.logChannel = logChannel;
         }

        public async void Start()
        {
            while (!isStopped)
            {
                await Task.Delay(delay);
                await ResetUsers();
            }
        }

        public async Task<StrikeInfo> AddStrike(StrikeInfo strike)
        {
            if (!await UserExists((ulong)strike.UserId!))
            {
                var user = new UserInfo()
                {
                    Id = strike.UserId,
                };
                await db.AddUser(user);
            }
            await db.AddUserStrikeInfo((ulong)strike.UserId!, 1, 1, GetStrikeResetTime());
            var info = await db.AddStrike(strike);
            await StrikeUpdateUser(strike);
            return info!;
        }

        public async Task<bool> UserExists(ulong userId)
        {
            try
            {
                await db.GetUserInfo(userId);
                return true;
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<UserInfo> GetUserInfo(ulong userId)
        {
            return await db.GetUserInfo(userId);
        }

        public DateTime GetStrikeResetTime()
        {
            return DateTime.UtcNow.AddSeconds(reset);
        }

        public int GetMuteTime(int level)
        {
            try
            {
                return strikeSet[level].MuteTime;
            } catch (Exception)
            {
                return strikeSet.Last().Value.MuteTime;
            }
            
        }

        public async Task StrikeUpdateUser(StrikeInfo strike)
        {
            try
            {
                var info = await db.GetUserInfo((ulong)strike.UserId!);
                var guild = await client.GetGuildAsync(guildId);

                var member = await guild.GetMemberAsync((ulong)strike.UserId);
                await member.TimeoutAsync(DateTime.UtcNow.AddSeconds(GetMuteTime((int)info.CurrentStrikes!)));
                await SetMemberRole((int)info.CurrentStrikes!, member);
            } catch (Exception e)
            {
                Console.WriteLine(e.Message + "Error on StrikeUpdate");
            }
        }

        private async Task ClearRoles(DiscordMember member)
        {
            foreach (var item in strikeSet)
            {
                try
                {
                    await member.RevokeRoleAsync(member.Guild.GetRole((ulong)item.Value.RoleId));
                } catch (Exception)
                {

                }
            }
        }

        private async Task SetMemberRole(int level, DiscordMember member)
        {
            if (level > strikeSet.Count)
            {
                return;
            }
            try
            {
                var item = strikeSet[level];
                await member.GrantRoleAsync(member.Guild.GetRole((ulong)item.RoleId));
                foreach (var i in strikeSet)
                {
                    if (item.RoleId == i.Value.RoleId)
                        continue;

                    try
                    {
                        await member.RevokeRoleAsync(member.Guild.GetRole((ulong)i.Value.RoleId));
                    } catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            } catch (Exception e)
            {
               Console.WriteLine($"Error: {e.Message}");
            }
            
        }

        public async Task ClearUserStrikes(ulong userId)
        {
            if (!await UserExists(userId))
            {
                throw new Exception("User does not exists");
            }

            var info = await GetUserInfo(userId);
            await db.AddUserStrikeInfo(userId, 0, -info.CurrentStrikes, DateTime.UtcNow);
            await db.DeleteStrikeRecord(userId);
        }

        public int UserStrikeAmount(ulong userId)
        {
            return db.GetUserInfo(userId).Result.TotalStrikes;
        }
            

        private async Task ResetUsers()
        {
            var userReader = await db.GetStrikeResetUsers();

            while (userReader.Read())
            {
                var info = ReadUserInfo(userReader);
                await db.AddUserStrikeInfo(info.Id, 0, -info.CurrentStrikes, null);
                try
                {
                    var user = await client.GetGuildAsync(guildId).Result.GetMemberAsync(info.Id);
                    var discordUser = await client.GetUserAsync(info.Id);
                    await ClearRoles(user);

                    var embed = new DiscordEmbedBuilder();
                    embed.WithTitle("User strikes have been reset");
                    embed.WithColor(DiscordColor.Green);
                    embed.WithThumbnail(discordUser.AvatarUrl);
                    embed.AddField("User", discordUser.Mention);

                    await client.GetChannelAsync(logChannel).Result.SendMessageAsync(embed.Build());
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }

            await userReader.CloseAsync();
        }

        private static UserInfo ReadUserInfo(NpgsqlDataReader reader)
        {
            var userInfo = new UserInfo();

            userInfo.Id = Convert.ToUInt64(reader["user_id"] as string);
            userInfo.TotalStrikes = (int)reader["total_strikes"];
            userInfo.StrikeReset = reader["strike_reset"] as DateTime?;
            userInfo.CurrentStrikes = (int)reader["current_strikes"];

            return userInfo;
        }
    }
}
