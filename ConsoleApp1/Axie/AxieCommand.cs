using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using DiscordBot.AxieRace;
using DiscordBot.Axie;
namespace DiscordBot
{
    [Group("axie")]
    public class AxieCommand : BaseCommands
    { 
        private bool IsMarketPlace(ICommandContext context)
        {
            CommandContext ctxt = new CommandContext(context.Client, context.Message);
            return ctxt.IsPrivate || context.Channel.Id == 423343101428498435 ||context.Guild.Id == 329959863545364480;
        }

        private bool CanSendMessages(ICommandContext context)
        {
            CommandContext ctxt = new CommandContext(context.Client, context.Message);
            return ctxt.IsPrivate|| context.Channel.Id == 487932149354463232 || context.Channel.Id == 414794784448970752 || context.Guild.Id == 329959863545364480;
        }

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
            if(CanSendMessages(Context))
            await Context.Channel.SendMessageAsync("Commands that you can use :\n"
                                                 + "- `>axie addaddress input_address` : register an ETH wallet address to user ID. You can add several addresses if you own more than 1 . \n"
                                                 + "- `>axie removeaddress input_address` : remove an ETH wallet address from user.\n"
                                                 + "- `>axie removeuser` : remove all data from user.\n"
                                                 + "- `>axie ping` : Enable/disable ping status of user.\n"
                                                 + "- `>axie nonbuyable axie_id` : Remove an axie from being buyable.\n"
                                                 + "- `>axie buyable axie_id` : Add an axie to be buyable again.\n"
                                                 + "- `>axie buy input_id` : Ping the owner of this axie.\n"
                                                 + "- `>axie show` : Show user's addresses.\n"
                                                 + "- `>axie purechance axie_id_1 axie_id_2 (optional)beta` : Show user's chance to breed a pure axie from 2 preset axies. Write beta at the end if you want to access axies in beta.");
        }
        [Command("addaddress"), Summary("register ETH wallet address to user ID")]
        public async Task AddAddress(string address)
        {
            if(IsMarketPlace(Context))
                await AxieHolderListHandler.AddUserAddress(Context.Message.Author.Id, address, Context);
        }

        [Command("removeaddress"), Summary("remove ETH wallet address to user ID")]
        public async Task RemoveAddress(string address)
        {
            if (IsMarketPlace(Context))
                await AxieHolderListHandler.RemoveAddress(Context.Message.Author.Id, address, Context);
        }
        [Command("removeuser"), Summary("remove user from DB")]
        public async Task RemoveUser()
        {
            if (IsMarketPlace(Context))
                await AxieHolderListHandler.RemoveUser(Context.Message.Author.Id, Context);
        }

        [Command("nonbuyable"), Summary("change ping status")]
        public async Task AddNonBuyable(int axieNumber)
        {
            if (IsMarketPlace(Context))
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
                await AxieHolderListHandler.AddNonBuyableAxie(axieData.id, Context.Message.Author.Id, axieData.owner, Context);
            }
        }

        [Command("buyable"), Summary("change ping status")]
        public async Task RemoveNonBuyable(int axieNumber)
        {
            if (IsMarketPlace(Context))
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
                await AxieHolderListHandler.RemoveNonBuyableAxie(axieData.id, Context.Message.Author.Id, axieData.owner, Context);
            }
        }

        [Command("Ping"), Summary("change ping status")]
        public async Task SetPingStatus()
        {
            if (IsMarketPlace(Context))
                await AxieHolderListHandler.SetPingStatus(Context.Message.Author.Id, Context);
        }

        [Command("buy"), Summary("notify buyer than you want to buy axie")]
        public async Task BuyAxie(int axieNumber)
        {
            if (IsMarketPlace(Context))
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
                await AxieHolderListHandler.GetHolderId(axieData.owner, axieData.id, Context);
                //if (axieData.stage <= 2) await Context.Channel.SendMessageAsync("Axie is still an egg! I can't check what it's going to be >:O ");
            }
        }

        [Command("show"), Summary("show you addresses")]
        public async Task ShowAddresses()
        {
            if (IsMarketPlace(Context))
                await AxieHolderListHandler.GetUserAddressList(Context.Message.Author.Id, Context.Channel);
        }
        [Command("find"), Summary("find an axie from API")]
        public async Task FindAxie(int index)
        {
            if (CanSendMessages(Context))
            {
                string json = "";
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        json = await wc.DownloadStringTaskAsync("https://api.axieinfinity.com/v1/axies/" + index.ToString()); //https://axieinfinity.com/api/axies/
                    }

                    catch (Exception ex)
                    {
                        await Context.Channel.SendMessageAsync("Error. Axie could not be found.");
                        return;
                    }
                }
                JObject axieJson = JObject.Parse(json);
                AxieData axieData = axieJson.ToObject<AxieData>();

                if (axieData.stage <= 2) await Context.Channel.SendMessageAsync("Axie is still an egg! I can't check what it's going to be >:O ");
                else await Context.Channel.SendMessageAsync("", false, AxieData.EmbedAxieData(axieData, axieJson));
                string imageUrl = axieJson["figure"]["static"]["idle"].ToString();


                byte[] imageData = null;
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        imageData = wc.DownloadData(imageUrl); //https://axieinfinity.com/api/axies/
                    }

                    catch (Exception ex)
                    {
                    }
                }
                var image = new Image(new MemoryStream(imageData));

                await Context.Channel.SendFileAsync(image.Stream, "axieIdle.png");
            }
        }

        [Command("purechance"), Summary("show you addresses")]
        public async Task GetBreedingChance(int axie1, int axie2, string isBeta = null)
        {
            if (CanSendMessages(Context))
            {
                string url = isBeta == "beta" ? "http://beta.axieinfinity.com/api/axies/" : "https://axieinfinity.com/api/axies/";
                string json1 = "";
                string json2 = "";
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        json1 = await wc.DownloadStringTaskAsync(url + axie1.ToString());
                        json2 = await wc.DownloadStringTaskAsync(url + axie2.ToString());
                    }
                    catch (Exception ex)
                    {
                        await Context.Channel.SendMessageAsync("Error. Axies could not be found.");
                        return;
                    }
                }
                JObject axieJson1 = JObject.Parse(json1);
                AxieData axieData1 = axieJson1.ToObject<AxieData>();
                JObject axieJson2 = JObject.Parse(json2);
                AxieData axieData2 = axieJson2.ToObject<AxieData>();
                float chance = PureBreeder.GetBreedingChance(axieData1.genes, axieData2.genes);
                await Context.Channel.SendMessageAsync($"The chance to breed a pure axie with axies #{axieData1.id} and #{axieData2.id} is = {chance}%");
            }
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
            await Context.Channel.SendMessageAsync("Not finished yet :/");
            //AxieRacingInstance gameInstance = new AxieRacingInstance(Context.Channel.Id);
            //await StartGameInternal(gameInstance, strMaxMinutesToWait);

        }

        [Command("Migration", RunMode = RunMode.Async), Summary("Launch raceing game")]
        public async Task Migration()
        {
            if (Context.Message.Author.Id == 195567858133106697)
            {
                await AxieHolderListHandler.Migration();
                await Context.Channel.SendMessageAsync("Migration compeleted");
            }

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
