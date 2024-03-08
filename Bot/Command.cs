using App.Bot.Database;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace App.Bot
{
    internal class Command : ApplicationCommandModule
    {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Updater Updater { private get; set; }
        public Config Config { private get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [SlashCommand("strike", "Add a strike to a member")]
        public async Task StrikeCommand(InteractionContext ctx,
            [Option("user", "The target user")] DiscordUser user,
            [Option("reason", "Reason of the strike")] string reason,
            [Option("empheral", "Make response invisible to others")] bool empheral = false
            )
        {
            var autherId = ctx.User.Id;
            UserInfo info;

            if (!Bot.ownerList!.Check(autherId) && !Bot.roleList!.Check(autherId))
            {
                await ctx.CreateResponseAsync("You cannot issue that command");
                return;
            }

            var strike = new StrikeInfo()
            {
                    UserId = user.Id,
                    Reason = reason,
                    Timestamp = DateTime.UtcNow,
                    IssuerId = autherId,
            };

            await ctx.DeferAsync();

            var strikeResult = await Updater.AddStrike(strike);

            info = await Updater.GetUserInfo(user.Id);

            var embed = new DiscordEmbedBuilder();

            embed.Title = $"Strike has been given to a user";
            embed.AddField("User", user.Mention);
            embed.Color = DiscordColor.Red;
            embed.WithThumbnail(user.AvatarUrl);
            embed.Description = $"Reason: {reason}";
            var msg = embed.Build();
            embed.WithAuthor(name: ctx.User.Username, iconUrl: ctx.User.AvatarUrl);
            embed.AddField("Current Strikes", info.CurrentStrikes.ToString() + " Strikes");
            embed.WithFooter($"Id: {strikeResult.Id}");
            var logMsg = embed.Build();

            await ctx.CreateResponseAsync(msg, empheral);

            await ctx.Guild.GetChannel(Config.ServerConfig.LogChannel).SendMessageAsync(logMsg);
        }

        [SlashCommand("user_info", "View strike information on a user")]
        public async Task UserInfoCommand(InteractionContext ctx,
            [Option("user", "The target user")] DiscordUser user,
            [Option("empheral", "Make response invisible to others")] bool empheral = false
            )
        {
            var userId = ctx.User.Id;

            if (!Bot.ownerList!.Check(userId))
            {
                await ctx.CreateResponseAsync("You cannot issue the command");
                return;
            }

            if (!await Updater.UserExists(user.Id) || Updater.UserStrikeAmount(user.Id) == 0)
            {
                await ctx.CreateResponseAsync($"{user.Mention} does not have any strikes", true);
                return;
            }

            UserInfo info = await Updater.GetUserInfo(user.Id);
            TimeSpan time = (DateTime)info.StrikeReset! - DateTime.UtcNow;
            var resetTime = info.StrikeReset! < DateTime.UtcNow ? "None" : $"[{time.Days} days] [{time.Hours} hours] [{time.Minutes} minutes]";

            var embed = new DiscordEmbedBuilder();

            embed.Title = "User Information";
            embed.Color = DiscordColor.CornflowerBlue;
            embed.AddField("Information:", $"Current Strikes: {info.CurrentStrikes}\nTotal Strikes: {info.TotalStrikes}\nReset At: {resetTime}");
            embed.WithThumbnail(user.AvatarUrl);
            var msg = embed.Build();
            await ctx.CreateResponseAsync(msg, empheral);
        }

        /*[SlashCommand("strike-logs", "View strike logs of a user")]
        public async Task StrikeLogsCommand(InteractionContext ctx,
            [Option("amount", "The amount of logs to view")]  amount,
            [Option("offset", "The offset of where to start lookiing at")] int offset
            )
        {
            if (amount < 0 || amount > 100)
            {
                await ctx.CreateResponseAsync("You can only issue an amount beetwen 1-100", true);
                return;
            }
        }*/

        [SlashCommand("clear_strikes", "Clears total and current strikes of a user")]
        public async Task ClearStrikeCommand(InteractionContext ctx,
            [Option("user", "The target user")] DiscordUser user
            )
        {
            var userId = ctx.User.Id;

            if (!Bot.ownerList!.Check(userId))
            {
                await ctx.CreateResponseAsync("You can not execute this command");
                return;
            }

            await Updater.ClearUserStrikes(user.Id);

            await ctx.CreateResponseAsync("Cleared user strike info");
        }

        [SlashCommand("remove_strike", "Remove a strike from a user")]
        public async Task RemoveStrikeCommand(InteractionContext ctx,
            [Option("user", "The target user")] DiscordUser user,
            [Option("strike-id", "The id of the strike")] long id
            )
        {
            var userId = ctx.User.Id;

            if (!Bot.ownerList!.Check(userId))
            {
                await ctx.CreateResponseAsync("You can not execute this command");
                return;
            }

            // TODO: send request to remove strike and return boolean

            await ctx.CreateResponseAsync("User Strike Remove: TODO");
        }
    }
}
