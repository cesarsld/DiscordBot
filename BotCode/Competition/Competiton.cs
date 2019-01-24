using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Mongo;
using MongoDB.Driver;
using MongoDB.Bson;

namespace DiscordBot.Competition
{
    [Group("competition"), Alias("comp", "a")]
    public class Competition : ModuleBase
    {

        public static double bank = 0;
        public Competition()
        {
        }

        [Command("enter"), Summary("enters the user in the competition")]
        public async Task enterCompetition()
        {
            // Util.SetupMongoCollection("competition");
            var collec = DatabaseConnection.GetDb().GetCollection<UsersEntered>("Competition");
            var user = ((SocketGuildUser)Context.Message.Author);
            var userInCompetition = (await collec.FindAsync(ele => ele._id == user.Id)).FirstOrDefault();
           
            if (user.JoinedAt > DateTime.Today.AddDays(-7))
            {
                await ReplyAsync($"You have joined the server too recently to enter!");
            }
            else if (userInCompetition != null)
            {
                await ReplyAsync($"You have already joined the competition!");
            }
            else
            {
                //Util.createUserInCompetition(user);
                await collec.InsertOneAsync(new UsersEntered(user));
                await ReplyAsync($"You have joined the competition, Good Luck!");
            }
            //Util.SetupMongoCollection("userData");
        }

        [Command("bet")]
        public async Task rollUnder(int under, int bet)
        {
            var collec = DatabaseConnection.GetDb().GetCollection<UsersEntered>("Competition");
            var user = ((SocketGuildUser)Context.Message.Author);
            var embed = new EmbedBuilder();
            var userInCompetition = (await collec.FindAsync(ele => ele._id == user.Id)).FirstOrDefault();
            if (user != null)
            {
                if (under < 2 || under > 99)
                {
                    await ReplyAsync($"Undervalue must be between 2 and 99");
                }
                else if (userInCompetition.credits < bet)
                {
                    await ReplyAsync($"{user} has {userInCompetition.credits} credits! You cannot bet {bet}!");
                }
                else if (bet < 10)
                {
                    await ReplyAsync($"Bet must bet 10 or more credits");
                }
                else
                {
                    Random rand = new Random(DateTime.Now.Millisecond);
                    var value = rand.Next(100) + 1;
                    double factor = 100 / (double)(under - 1);
                    var message = await ReplyAsync("", embed: GetAwaitingRollEmbed(bet, factor, user));

                    await Task.Delay(2500);
                    if (under > value) //win
                    {
                        var winnings = bet * factor;
                        await collec.UpdateOneAsync(ele => ele._id == userInCompetition._id,
                            Builders<UsersEntered>.Update.Set(u => u.credits, userInCompetition.credits + Convert.ToInt64(winnings - bet)));

                        embed.WithColor(Color.Green);
                        embed.AddInlineField("Result", "Winner");
                        embed.AddInlineField("Random Value", value);
                        embed.AddInlineField("Winnings", (int)winnings);
                        embed.AddInlineField("Total points", userInCompetition.credits + Convert.ToInt64(winnings - bet));
                    }
                    else
                    {
                        await collec.UpdateOneAsync(ele => ele._id == userInCompetition._id,
                            Builders<UsersEntered>.Update.Set(u => u.credits, userInCompetition.credits - bet));
                        embed.WithColor(Color.Red);
                        embed.WithDescription($"Result for <@{user.Id}>");
                        embed.AddInlineField("Outcome", "Loser");
                        embed.AddInlineField("Random Value", value);
                        embed.AddInlineField("Losing", -bet);
                        embed.AddField("Total points", userInCompetition.credits - bet);
                    }
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
            }

            //Util.SetupMongoCollection("userData");
        }

        private EmbedBuilder GetAwaitingRollEmbed(int bet, double factor, SocketGuildUser user)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle($"Roll for {user.Username}");
            embed.WithColor(new Color(95, 211, 232));
            embed.AddInlineField("Result", "?");
            embed.AddInlineField("Random Value", "?");
            embed.AddInlineField("Potential gain", $"x{factor.ToString("F4")}");
            //embed.AddInlineField("Total points", userInCompetition.credits + Convert.ToInt64(winnings - bet));
            return embed;
        }

    }
}
