using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Nethereum;
using DiscordBot.AxieRace;
namespace DiscordBot
{
    [Group("axie")]
    public class AxieCommand : BaseCommands
    {

        public static bool CreateChannelCommandInstance(string commandName, ulong userId, ulong channelId, ulong guildId, AxieRacingInstance gameInstance, out RunningCommandInfo instance)
        {
            lock (SyncObj)
            {
                ChannelCommandInstances.TryGetValue(channelId, out instance);
                if (instance == null)
                {
                    instance = new RunningCommandInfo(commandName, userId, channelId, guildId, gameInstance);
                    ChannelCommandInstances[channelId] = instance;
                    return true;
                }
            }
            return false;
        }


        [Command("help"), Summary("register ETH wallet address to user ID")]
        public async Task PostHelp()
        {
            await Context.Channel.SendMessageAsync("Commands that you can use :\n"
                                                 + "- `>axie addaddress _input_address` : register an ETH wallet address to user ID. You can add several addresses if you own more than 1 . \n"
                                                 + "- `>axie removeaddress _input_address` : remove an ETH wallet address from user.\n"
                                                 + "- `>axie removeuser` : remove all data from user.\n"
                                                 + "- `>axie buy _input_id` : Ping the owner of this axie.\n"
                                                 + "- `>axie show` : Show user's addresses");
        }
        [Command("addaddress"), Summary("register ETH wallet address to user ID")]
        public async Task AddAddress(string address)
        {
            await AxieHolderListHandler.AddUserAddress(Context.Message.Author.Id, address, Context.Channel);
        }

        [Command("removeaddress"), Summary("remove ETH wallet address to user ID")]
        public async Task RemoveAddress(string address)
        {
            await AxieHolderListHandler.RemoveAddress(Context.Message.Author.Id, address, Context.Channel);
        }
        [Command("removeuser"), Summary("remove user from DB")]
        public async Task RemoveUser()
        {
            await AxieHolderListHandler.RemoveUser(Context.Message.Author.Id, Context.Channel);
        }

        [Command("buy"), Summary("notify buyer than you want to buy axie")]
        public async Task BuyAxie(int axieNumber)
        {
            string json = "";
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    json = await wc.DownloadStringTaskAsync("https://axieinfinity.com/api/axies/" + axieNumber.ToString());
                }
                catch (Exception ex)
                {
                    await Context.Channel.SendMessageAsync("Error. Axie could not be found.");
                    return;
                }
            }
            JObject axieJson = JObject.Parse(json);
            AxieData axieData = axieJson.ToObject<AxieData>();
            await AxieHolderListHandler.GetHolderId(axieData.owner, Context);
            //if (axieData.stage <= 2) await Context.Channel.SendMessageAsync("Axie is still an egg! I can't check what it's going to be >:O ");
        }

        [Command("show"), Summary("show you addresses")]
        public async Task ShowAddresses()
        {
            
            await AxieHolderListHandler.GetUserAddressList(Context.Message.Author.Id, Context.Channel);
        }

        [Command("SetRacerData"), Summary("Set racer data")]
        public async Task SetRacerData(ulong gameId, string axieClass, int pace, int awareness, int diet)
        {
            RunningCommandInfo commandInfo = GetRunningCommandInfo(gameId);
            if (commandInfo != null)
            {
                AxieClass _class = AxieClass.undefined;
                Enum.TryParse(axieClass.ToLower(), out _class);
                await commandInfo.RaceInstance.GetRacerDataFromDm(Context.Message.Author.Id, _class, pace, awareness, diet, Context);
            }
            else await Context.Channel.SendMessageAsync("I'm sorry I can't find the game instance. It may have ended or the ID you gave me is wrong :(");          
        }
        [Command("RaceGame", RunMode = RunMode.Async), Summary("Launch raceing game")]
        public async Task AxieRacing([Summary("Max minutes to wait for players")]string strMaxMinutesToWait = null)
        {

            AxieRacingInstance gameInstance = new AxieRacingInstance(Context.Channel.Id);
            await StartGameInternal(gameInstance, strMaxMinutesToWait);
            
        }
        private async Task StartGameInternal(AxieRacingInstance raceInstance, string strSecondsToWait)
        {
            bool cleanupCommandInstance = false;
            try
            {
                Logger.LogInternal($"G:{Context.Guild.Name}  Command " + $"'giveAway' executed by '{Context.Message.Author.Username}'");
                RunningCommandInfo commandInfo;
                if (CreateChannelCommandInstance("GiveAway", Context.User.Id, Context.Channel.Id, Context.Guild.Id, raceInstance, out commandInfo))
                {
                    cleanupCommandInstance = true;
                    int maxSecsToWait;
                    int testUsers = 0;

                    if (Int32.TryParse(strSecondsToWait, out maxSecsToWait) == false) maxSecsToWait = 5;
                    if (maxSecsToWait <= 0) maxSecsToWait = 1;


                    SocketGuildUser user = Context.Message.Author as SocketGuildUser;
                    string userThatStartedGame = user?.Nickname ?? Context.Message.Author.Username;
                    await raceInstance.RunRace(maxSecsToWait, Context, userThatStartedGame, testUsers);
                    cleanupCommandInstance = false;
                    //await Context.Channel.SendMessageAsync($"MaxUsers: {maxUsers}  MaxMinutesToWait: {maxMinutesToWait} SecondsDelayBetweenDays: {secondsDelayBetweenDays} NumWinners: {numWinners}");
                }
                else
                {
                    try
                    {
                        await LogAndReplyAsync($"The '{commandInfo.CommandName}' command is currently running!.  Can't run this command until that finishes");
                    }
                    catch (Exception ex)
                    {
                        await Logger.Log(new LogMessage(LogSeverity.Error, "StartGameInternal", "Unexpected Exception", ex));
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.Log(new LogMessage(LogSeverity.Error, "StartGame", "Unexpected Exception", ex));
            }
            finally
            {
                try
                {
                    if (cleanupCommandInstance)
                        RemoveChannelCommandInstance(Context.Channel.Id);
                }
                catch (Exception ex)
                {
                    await Logger.Log(new LogMessage(LogSeverity.Error, "StartGame", "Unexpected Exception in Finally", ex));
                }
            }
        }
    }
}
