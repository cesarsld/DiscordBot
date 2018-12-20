using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.AxieRace;
using DiscordBot.Axie;
using DiscordBot.Axie.Breeding;
using DiscordBot.Axie.SubscriptionServices.PremiumServices;
using DiscordBot.Axie.QoLListHandler;
using DiscordBot.Axie.SubscriptionServices;
using DiscordBot.Axie.Web3Axie;
using DiscordBot.Axie.Battles;
using DiscordBot.Axie.ApiCalls;
using DiscordBot.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
namespace DiscordBot
{
    [Group("axie")]
    [Alias("a")]
    public class AxieCommand : BaseCommands
    {

        #region Modifier methods
        private bool IsDev(ICommandContext context)
        {
            return context.Message.Author.Id == 195567858133106697;
        }
        private bool IsMarketPlace(ICommandContext context)
        {
            CommandContext ctxt = new CommandContext(context.Client, context.Message);
            if (context.Guild != null && context.Guild.Id != 410537146672349205) return true;
            return ctxt.IsPrivate || context.Channel.Id == 423343101428498435 || context.Guild.Id == 329959863545364480;
        }

        private bool IsSuggestion(ICommandContext context)
        {
            CommandContext ctxt = new CommandContext(context.Client, context.Message);
            if (context.Guild != null && context.Guild.Id != 410537146672349205) return true;
            return ctxt.IsPrivate || context.Channel.Id == 416268771510976513 || context.Guild.Id == 329959863545364480;
        }

        private bool IsArena(ICommandContext context)
        {
            CommandContext ctxt = new CommandContext(context.Client, context.Message);
            if (context.Guild != null && context.Guild.Id != 410537146672349205) return true;
            return ctxt.IsPrivate || context.Channel.Id == 498508595206422543 || context.Guild.Id == 329959863545364480;
        }

        private bool IsBreeding(ICommandContext context)
        {
            CommandContext ctxt = new CommandContext(context.Client, context.Message);
            if (context.Guild != null && context.Guild.Id != 410537146672349205) return true;
            return ctxt.IsPrivate || context.Channel.Id == 442695028188381185 || context.Guild.Id == 329959863545364480;
        }


        private bool IsBotCommand(ICommandContext context)
        {
            CommandContext ctxt = new CommandContext(context.Client, context.Message);
            if (context.Guild != null && context.Guild.Id != 410537146672349205) return true;
            return ctxt.IsPrivate || context.Channel.Id == 487932149354463232 || context.Guild.Id == 329959863545364480 || context.Guild.Id != 410537146672349205; //487932149354463232
        }

        private bool IsCouncilOrHigherMember()
        {
            if (Context.Message.Author.Id == 195567858133106697) return true; //testing purposes
            var roleList = new List<ulong>();
            foreach (var role in Context.Guild.Roles)
            {
                if (role.Name == "Core" || role.Name == "Staff" || role.Name == "Moderator" || role.Name == "Player Council") roleList.Add(role.Id);
            }
            foreach (var role in ((IGuildUser)Context.Message.Author).RoleIds)
            {
                if (roleList.Contains(role)) return true;
            }
            return false;
        }

        private bool IsCoreOrStaffMember()
        {
            if (Context.Message.Author.Id == 195567858133106697) return true; //testing purposes
            var roleList = new List<ulong>();
            foreach (var role in Context.Guild.Roles)
            {
                if (role.Name == "Core" || role.Name == "Staff") roleList.Add(role.Id);
            }
            foreach (var role in ((IGuildUser)Context.Message.Author).RoleIds)
            {
                if (roleList.Contains(role)) return true;
            }
            return false;
        }

        private bool IsGeneral(ICommandContext context)
        {
            CommandContext ctxt = new CommandContext(context.Client, context.Message);
            if (context.Guild != null && context.Guild.Id != 410537146672349205) return true;
            return ctxt.IsPrivate || context.Channel.Id == 410537147116814348 || context.Channel.Id == 414794784448970752 || context.Guild.Id == 329959863545364480;
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
        #endregion

        #region Help Commands
        [Command("help"), Summary("register ETH wallet address to user ID")]
        public async Task PostHelp()
        {
            if (IsBotCommand(Context))
                await Context.Channel.SendMessageAsync("Commands that you can use :\n"
                                                     + "- `>axie addaddress input_address` : register an ETH wallet address to user ID. You can add several addresses if you own more than 1 . \n"
                                                     + "- `>axie removeaddress input_address` : remove an ETH wallet address from user.\n"
                                                     + "- `>axie removeuser` : remove all data from user.\n"
                                                     + "- `>axie ping` : Enable/disable ping status of user.\n"
                                                     + "- `>axie nonbuyable axie_id` : Remove an axie from being buyable.\n"
                                                     + "- `>axie buyable axie_id` : Add an axie to be buyable again.\n"
                                                     + "- `>axie buy input_id` : Ping the owner of this axie.\n"
                                                     + "- `>axie show` : Show user's addresses.\n\n"
                                                     + "- `>axie find/f axie_id` : Show axie data.\n"
                                                     + "- `>axie peef axie_id/name` : Show axie image.\n"
                                                     + "- `>axie winrate/wr axie_id/name` : Show axie winrate.\n"
                                                     + "- `>axie updateNickname/IdNick axie_id` : Update axie nickname in Database.\n"
                                                     + "- `>axie UpdateAddressNickname/AddNick 0xADR355` : Update axie nickname from address in Database.\n"
                                                     + "- `>axie breedCount/bc axie_id` : Show axie's breed count.\n"
                                                     + "- `>axie purechance/pure/p axie_id_1 axie_id_2 (optional)beta` : Show user's chance to breed a pure axie from 2 preset axies. Write beta at the end if you want to access axies in beta.\n"
                                                     + "- `>axie subs` : Show services user can subscribe to.\n"
                                                     + "- `>axie qolhelp/qhelp` : Show QoL list help.\n"
                                                     + "- `>axie dbhelp` : Show database list help.\n"
                                                     + "- `>axie breedList 0xADRE55` : Returns a list of the best axie pairs to breed to obtain a pure axie.\n"
                                                     + "- `>axie traitList/tl 0xADRE55 trait` : Returns a list of the axies that contains that trait within their genes.\n"
                                                     + "- `>axie TeamList/teamL 0xADRE55` : Returns a list of winrates of each team of an address.\n"
                                                     + "NOTE : You may use the prefix `>a` instead of >axie");
        }

        [Command("qolhelp"), Summary("register ETH wallet address to user ID")]
        [Alias("qhelp")]
        public async Task PostQoLHelp()
        {
            if (IsBotCommand(Context))
                await Context.Channel.SendMessageAsync("Commands that you can use :\n"
                                                     + "- `>axie qol` : Show suggestion list. \n"
                                                     + "- `>axie qoltype` : Show what suggestion types you can submit.\n"
                                                     + "- `>axie qoladd/qadd _type description` : Submit a suggestion of type `_type`. (Player council or higher)\n"
                                                     + "- `>axie qolremove/qrem`_index : Remove suggestion at index `_iindex`. (Core or staff member)\n"
                                                     + "NOTE : You may use the prefix `>a` instead of >axie");
        }

        [Command("dbhelp"), Summary("return db help")]
        public async Task PostDbHelp()
        {
            if (IsBotCommand(Context))
                await Context.Channel.SendMessageAsync("Commands that you can use :\n"
                                                     + "- `>axie winrate/wr axieId` : Show axie's winrate. \n"
                                                     + "- `>axie winlist 0xADDRE55` : Show winrates of all axies on an address.\n"
                                                     + "- `>axie lapseWr axieId lapseTime` : Returns the winrate of an axie of lapseTime battles (up to 42).\n"
                                                     + "NOTE : You may use the prefix `>a` instead of >axie");
        }

        [Command("subscription"), Summary("register ETH wallet address to user ID")]
        [Alias("subs")]
        public async Task PostSubs()
        {
            if (IsBotCommand(Context))
                await Context.Channel.SendMessageAsync("Subscriptions that you can use :\n"
                                                     + "- Axie Lab service. For more info input `>a sub lab` \n"
                                                     + "- Marketplace service. For more info input `>a sub market`\n"
                                                     + "- Auction Watch service. For more info input `>a sub market` [PREMIUM]\n"
                                                     + "");
        }

        [Command("sub"), Summary("register ETH wallet address to user ID")]
        public async Task PostSub(string service)
        {
            if (IsBotCommand(Context))
            {
                string message = "";
                switch (service)
                {
                    case "lab":
                        message = "Commands: \n"
                                + "- `>a labsub` : subscribe to Axie Lab services\n"
                                + "- `>a labunsub` : subscribe to Axie Lab services\n"
                                + "- `>a labtrigger trigger_price` : change trigger price so that the bot notifies you when the pod price reaches your trigger price.\n"
                                + "- `>a removetrigger` : remove trigger price.";
                        break;
                    case "watch":
                        message = "Commands: \n"
                                + "- `>a watchsub` : subscribe to Market Watch services\n"
                                + "- `>a watchunsub` : subscribe to Market Watch services\n"
                                + "- `>a watchfilter/wf filter_Input` : Add an axie filter. Example :\n `>a wf mystic2 beast bug pure4 tail-the-last-one horn-strawberry-shortcake 2.345e `\n"
                                + "";
                        break;
                    case "market":
                        message = "Commands: \n"
                                + "- `>a marketsub` : subscribe to Marketplace services\n"
                                + "- `>a marketunsub/munsub` : unsubscribe to Marketplace services\n"
                                + "- `>a marketNotify true/false` : Sets trigger to notify when you sell an axie\n"
                                + "- `>a markettrigger/mtrigger axie_id trigger_price` : Bot will notify you when indicated axie reaches the trigger price.\n"
                                + "- `>a removetrigger/rtrigger axie_id` : Remove axie trigger from list.";
                        break;
                }
                await Context.Channel.SendMessageAsync(message);
            }
        }
        #endregion

        #region Generic commands
        [Command("addaddress"), Summary("register ETH wallet address to user ID")]
        public async Task AddAddress(string address)
        {
            if (IsMarketPlace(Context) || IsBotCommand(Context))
                await AxieHolderListHandler.AddUserAddress(Context.Message.Author.Id, address, Context);
        }

        [Command("removeaddress"), Summary("remove ETH wallet address to user ID")]
        public async Task RemoveAddress(string address)
        {
            if (IsMarketPlace(Context) || IsBotCommand(Context))
                await AxieHolderListHandler.RemoveAddress(Context.Message.Author.Id, address, Context);
        }
        [Command("removeuser"), Summary("remove user from DB")]
        public async Task RemoveUser()
        {
            if (IsMarketPlace(Context) || IsBotCommand(Context))
                await AxieHolderListHandler.RemoveUser(Context.Message.Author.Id, Context);
        }

        [Command("nonbuyable"), Summary("change ping status")]
        public async Task AddNonBuyable(int axieNumber)
        {
            if (IsMarketPlace(Context) || IsBotCommand(Context))
            {
                var axieData = await AxieObject.GetAxieFromApi(axieNumber);
                string owner = (string)axieData.jsonData["owner"];
                int axieId = (int)axieData.jsonData["id"];
                await AxieHolderListHandler.AddNonBuyableAxie(axieId, Context.Message.Author.Id, owner, Context);
            }
        }

        [Command("buyable"), Summary("change ping status")]
        public async Task RemoveNonBuyable(int axieNumber)
        {
            if (IsMarketPlace(Context) || IsBotCommand(Context))
            {
                var axieData = await AxieObject.GetAxieFromApi(axieNumber);
                string owner = (string)axieData.jsonData["owner"];
                int axieId = (int)axieData.jsonData["id"];
                await AxieHolderListHandler.RemoveNonBuyableAxie(axieId, Context.Message.Author.Id, owner, Context);
            }
        }

        [Command("Ping"), Summary("change ping status")]
        public async Task SetPingStatus()
        {
            if (IsMarketPlace(Context) || IsBotCommand(Context))
                await AxieHolderListHandler.SetPingStatus(Context.Message.Author.Id, Context);
        }

        [Command("buy"), Summary("notify buyer than you want to buy axie")]
        public async Task BuyAxie(int axieNumber)
        {
            if (IsMarketPlace(Context) || IsBotCommand(Context))
            {
                var axieData = await AxieObject.GetAxieFromApi(axieNumber);
                string owner = (string)axieData.jsonData["owner"];
                int axieId = (int)axieData.jsonData["id"];
                await AxieHolderListHandler.GetHolderId(owner, axieId, Context);
            }
        }

        [Command("show"), Summary("show you addresses")]
        public async Task ShowAddresses()
        {
            if (IsMarketPlace(Context) || IsBotCommand(Context))
                await AxieHolderListHandler.GetUserAddressList(Context.Message.Author.Id, Context.Channel);
        }
        [Command("find"), Summary("find an axie from API")]
        [Alias("f")]
        public async Task FindAxie(int index, string extra = null)
        {
            if (IsBotCommand(Context))
            {
                AxieObject axieData = await AxieObject.GetAxieFromApi(index);
                axieData.id = index;

                if (axieData.stage <= 2) await Context.Channel.SendMessageAsync("Axie is still an egg! I can't check what it's going to be >:O ");
                else await Context.Channel.SendMessageAsync("", false, axieData.EmbedAxieData(extra));
            }
        }

        [Command("peekexp"), Summary("find an axie from API")]
        [Alias("pxp")]
        public async Task FindAxieQQexp(int index, string extra = null)
        {
            if (IsArena(Context) || IsBotCommand(Context))
            {
                AxieObject data = await AxieObject.GetAxieFromApi(index);
                data.id = index;
                await Context.Channel.SendMessageAsync("", false, data.EmbedBaseData(true));

            }
        }

        //[Command("peek"), Summary("find an axie from API")]
        //[Alias("pk")]
        //public async Task FindAxieQQ(int index, string extra = null)
        //{
        //    if (IsGeneral(Context) || IsBotCommand(Context) || IsBreeding(Context) || IsArena(Context))
        //    {
        //        AxieObject data = await AxieObject.GetAxieFromApi(index);
        //        data.id = index;
        //        await Context.Channel.SendMessageAsync("", false, data.EmbedBaseData(false));

        //    }
        //}

        [Command("peek"), Summary("find an axie from API")]
        [Alias("pk")]
        public async Task FindAxieQQ([Remainder]string data)
        {
            if (IsGeneral(Context) || IsBotCommand(Context) || IsBreeding(Context) || IsArena(Context))
            {
                try
                {
                    var id = Convert.ToInt32(data);
                    AxieObject axie = await AxieObject.GetAxieFromApi(id);
                    await Context.Channel.SendMessageAsync("", false, axie.EmbedBaseData(false));

                }
                catch
                {
                    var collec = DatabaseConnection.GetDb().GetCollection<AxieMapping>("IdNameMapping");
                    //var axieMap = (await collec.FindAsync(a => a.name.ToLower() == data.ToLower())).ToList().FirstOrDefault();
                    var axieMap = (await collec.FindAsync(a => a.name.ToLower().Contains(data.ToLower()))).ToList().FirstOrDefault();
                    if (axieMap != null)
                    {
                        AxieObject axie = await AxieObject.GetAxieFromApi(axieMap.id);
                        await Context.Channel.SendMessageAsync("", false, axie.EmbedBaseData(false));

                    }
                }

            }
        }

        [Command("BreedCount"), Summary("find an axie from API")]
        [Alias("bc")]
        public async Task FindBreedCount(int index)
        {
            if (IsMarketPlace(Context) || IsBotCommand(Context))
            {
                var web3Data = await AxieDataGetter.GetExtraData(index);
                if (web3Data != null)
                {
                    AxieObject data = await AxieObject.GetAxieFromApi(index);
                    data.id = index;
                    await Context.Channel.SendMessageAsync("", false, data.EmbedBreedData(Convert.ToInt32(web3Data.numBreeding.ToString())));
                }
                else await Context.Message.AddReactionAsync(new Emoji("❌"));
            }
        }


        [Command("purechance"), Summary("show you addresses")]
        [Alias("pure", "p")]
        public async Task GetBreedingChance(int axie1, int axie2, string isBeta = null)
        {
            if (IsBotCommand(Context))
            {
                //string url = isBeta == "beta" ? "http://beta.axieinfinity.com/api/axies/" : "https://axieinfinity.com/api/axies/";
                var axieData1 = await AxieObject.GetAxieFromApi(axie1);
                var axieData2 = await AxieObject.GetAxieFromApi(axie2);

                string genes1 = (string)axieData1.jsonData["genes"];
                string genes2 = (string)axieData2.jsonData["genes"];

                int id1 = (int)axieData1.jsonData["id"];
                int id2 = (int)axieData2.jsonData["id"];

                float chance = PureBreeder.GetBreedingChance(genes1, genes2);
                await Context.Channel.SendMessageAsync($"The chance to breed a pure axie with axies #{id1} and #{id2} is = {chance}%");
            }
        }
        #endregion

        #region Lab services
        [Command("labprice"), Summary("Launch raceing game")]
        public async Task ChangeLapPrice(double newPrice)
        {
            AxieDataGetter.eggLabPrice = newPrice;
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("labsub"), Summary("subscribe to lab service")]
        public async Task SubscribeToLabService()
        {
            await SubscriptionServicesHandler.SubscribeToService(Context.Message.Author.Id, Context, ServiceEnum.AxieLab);
        }

        [Command("labunsub"), Summary("subscribe to lab service")]
        public async Task UnsubscribeToLabService()
        {
            await SubscriptionServicesHandler.UnSubToService(Context.Message.Author.Id, Context, ServiceEnum.AxieLab);
        }

        [Command("labtrigger"), Summary("set pod trigger")]
        public async Task SetLabPodPriceTrigger(float priceTrigger)
        {
            await SubscriptionServicesHandler.SetLabPriceTrigger(Context.Message.Author.Id, priceTrigger, Context);
        }

        [Command("removetrigger"), Summary("set pod trigger")]
        public async Task RemoveLabPodPriceTrigger()
        {
            await SubscriptionServicesHandler.RemoveLabPodPriceTrigger(Context.Message.Author.Id, Context);
        }

        #endregion

        #region Marketplace Services
        [Command("marketsub"), Summary("subscribe to market service")]
        public async Task SubscribeToMarketService()
        {
            await SubscriptionServicesHandler.SubscribeToService(Context.Message.Author.Id, Context, ServiceEnum.MarketPlace);
        }

        [Command("marketunsub"), Summary("subscribe to market service")]
        [Alias("munsub")]
        public async Task UnsubscribeToMarketService()
        {
            await SubscriptionServicesHandler.UnSubToService(Context.Message.Author.Id, Context, ServiceEnum.MarketPlace);
        }

        [Command("markettrigger"), Summary("set market trigger")]
        [Alias("mtrigger")]
        public async Task SetMarketplaceTrigger(int axieId, float priceTrigger)
        {
            await SubscriptionServicesHandler.SetMarketPriceTrigger(Context.Message.Author.Id, axieId, priceTrigger, Context);
        }

        [Command("marketNotify"), Summary("subscribe to market service")]
        [Alias("mnotif")]
        public async Task NotifySales(bool _bool)
        {
            await SubscriptionServicesHandler.UnSubToService(Context.Message.Author.Id, Context, ServiceEnum.MarketPlace);
        }

        [Command("removetrigger"), Summary("set market trigger")]
        [Alias("rtrigger")]
        public async Task RemoveMarketplaceTrigger(int axieId)
        {
            await SubscriptionServicesHandler.RemoveMarketPriceTrigger(Context.Message.Author.Id, axieId, Context);
        }

        #endregion

        #region Auction creation commands

        [Command("watchsub"), Summary("Create auction filter")]
        [Alias("as")]
        public async Task SubscribeToAuctionWatcher()
        {
            await SubscriptionServicesHandler.SubscribeToService(Context.Message.Author.Id, Context, ServiceEnum.AuctionWatch);
        }

        [Command("watchfilter"), Summary("Create auction filter")]
        [Alias("wf")]
        public async Task CreateAuctionFilter([Remainder]string filterInput)
        {
            await Context.Channel.SendMessageAsync("Under construction...");
            return;
            if (filterInput != null)
            {
                await SubscriptionServicesHandler.CreateAuctionFilter(Context.Message.Author.Id, filterInput, Context);
            }
        }

        [Command("testF"), Summary("Create auction filter")]
        [Alias("tf")]
        public async Task TestFilter(int axieId)
        {
            await SubscriptionServicesHandler.GetSubList();
            var existingUser = SubscriptionServicesHandler.GetUserFromId(Context.Message.Author.Id);
            var auctionService = existingUser.GetServiceList().FirstOrDefault(_service => _service.name == ServiceEnum.AuctionWatch) as AuctionWatchService;
            foreach (var filter in auctionService.GetList())
            {
                if (filter.Match(await AxieObject.GetAxieFromApi(axieId), 50))
                {
                    await Context.Channel.SendMessageAsync("", embed: await filter.GetTriggerMessage(axieId));
                }
            }

        }

        #endregion

        #region QoL commands

        [Command("qol"), Summary("Get QoLList")]
        public async Task GetQoLData()
        {
            if (IsSuggestion(Context) || IsBotCommand(Context))
            {
                var qol = new QolListHandler("QoLList.txt");
                await Context.Channel.SendMessageAsync("", embed: await qol.GetEmbedQoLData());
            }
        }

        [Command("qolType"), Summary("GetQoLList")]
        public async Task GetQoLTypes()
        {
            if ((IsSuggestion(Context) || IsBotCommand(Context)) && IsCouncilOrHigherMember())
            {
                await Context.Channel.SendMessageAsync("", embed: QolListHandler.GetTypeEmbed());
            }
        }

        [Command("qolAdd"), Summary("GetQoLList")]
        [Alias("qadd")]
        public async Task AddQoLData(string type, [Remainder]string description)
        {
            if (IsCouncilOrHigherMember())
            {
                var qol = new QolListHandler("QoLList.txt");
                await qol.AddSuggestion(type, description, Context);
            }
        }

        [Command("qolremove"), Summary("GetQoLList")]
        [Alias("qrem")]
        public async Task AddQoLData(int index)
        {
            if (IsCoreOrStaffMember())
            {
                var qol = new QolListHandler("QoLList.txt");
                await qol.RemoveSuggestion(index - 1, Context);
            }
        }


        #endregion

        #region DB Calls

        //[Command("winrate"), Summary("GetAxieWinrate")]
        //[Alias("wr")]
        //public async Task GetAxieWinrate(int id, int length = 0)
        //{
        //    if (IsArena(Context) || IsBotCommand(Context) )
        //    {
        //        if (!WinrateCollector.IsDbSyncing)
        //        {
        //            var db = DatabaseConnection.GetDb();
        //            var collection = db.GetCollection<BsonDocument>("AxieWinrate");
        //            var filterId = Builders<BsonDocument>.Filter.Eq("_id", id);
        //            var doc = (await collection.FindAsync(filterId)).FirstOrDefault();
        //            var axieWinrate = BsonSerializer.Deserialize<AxieWinrate>(doc);
        //            var axieData = await AxieObject.GetAxieFromApi(id);
        //            axieData.id = id;
        //            if (length < 42 && length > 0)
        //                await Context.Channel.SendMessageAsync("", embed: axieData.EmbedWinrateRecent(axieWinrate, length));
        //            else
        //                await Context.Channel.SendMessageAsync("", embed: axieData.EmbedWinrate(axieWinrate));
        //        }
        //        else await Context.Channel.SendMessageAsync($"The battle database is currently being updated. Please try again later. \nData fetched from API : **{WinrateCollector.apiPerc}%**  \nData synced to DB : **{WinrateCollector.dbPerc}%**");
        //    }
        //}

        [Command("winrate"), Summary("GetAxieWinrate")]
        [Alias("wr")]
        public async Task GetAxieWinrate(string data)
        {
            if (IsArena(Context) || IsBotCommand(Context))
            {
                try
                {
                    var id = Convert.ToInt32(data);
                    var db = DatabaseConnection.GetDb();
                    var collection = db.GetCollection<BsonDocument>("AxieWinrate");
                    var filterId = Builders<BsonDocument>.Filter.Eq("_id", id);
                    var doc = (await collection.FindAsync(filterId)).FirstOrDefault();
                    var axieWinrate = BsonSerializer.Deserialize<AxieWinrate>(doc);
                    var axieData = await AxieObject.GetAxieFromApi(id);
                    axieData.id = id;
                    await Context.Channel.SendMessageAsync("", embed: axieData.EmbedWinrate(axieWinrate));

                }
                catch
                {
                    var collec = DatabaseConnection.GetDb().GetCollection<AxieMapping>("IdNameMapping");
                    var axieMap = (await collec.FindAsync(a => a.name.ToLower().Contains(data.ToLower()))).ToList().FirstOrDefault();
                    if (axieMap != null)
                    {
                        AxieObject axie = await AxieObject.GetAxieFromApi(axieMap.id);
                        var db = DatabaseConnection.GetDb();
                        var collection = db.GetCollection<BsonDocument>("AxieWinrate");
                        var filterId = Builders<BsonDocument>.Filter.Eq("_id", axieMap.id);
                        var doc = (await collection.FindAsync(filterId)).FirstOrDefault();
                        var axieWinrate = BsonSerializer.Deserialize<AxieWinrate>(doc);
                        var axieData = await AxieObject.GetAxieFromApi(axieMap.id);
                        axieData.id = axieMap.id;
                        await Context.Channel.SendMessageAsync("", embed: axieData.EmbedWinrate(axieWinrate));

                    }
                }
            }
        }

        [Command("mysticleaderboard"), Summary("Get mystic winrate leaderboard")]
        [Alias("mysticlb")]
        public async Task GetMysticLeaderBoard(int mysticCount)
        {
            if (IsArena(Context) || IsBotCommand(Context)|| Context.Message.Author.Id == 195567858133106697)
            {
                if (!WinrateCollector.IsDbSyncing)
                {
                    var time = Convert.ToInt32(((DateTimeOffset)(DateTime.UtcNow)).ToUnixTimeSeconds());
                    var db = DatabaseConnection.GetDb();
                    var collection = db.GetCollection<AxieWinrate>("AxieWinrate");
                    var dbList = await collection.FindAsync(a => a.mysticCount == mysticCount && a.lastBattleDate > time - 345600);
                    var axieList = dbList.ToList().Where(a => (a.win + a.loss) >= 100).OrderByDescending(a => a.winrate).ToList();
                    await Context.Channel.SendMessageAsync("", embed: WinrateCollector.GetTop10LeaderBoard(axieList, mysticCount));
                }
                else await Context.Channel.SendMessageAsync($"The battle database is currently being updated. Please try again later. \nData fetched from API : **{WinrateCollector.apiPerc}%** \nData synced to DB : **{WinrateCollector.dbPerc}%**");
            }
        }

        [Command("latestleaderboard"), Summary("Get mystic winrate leaderboard")]
        [Alias("latelb")]
        public async Task GetLatestLeaderboard(int mysticCount)
        {
            if (IsArena(Context) || IsBotCommand(Context) || Context.Message.Author.Id == 195567858133106697)
            {
                if (!WinrateCollector.IsDbSyncing)
                {
                    var time = Convert.ToInt32(((DateTimeOffset)(DateTime.UtcNow)).ToUnixTimeSeconds());
                    var db = DatabaseConnection.GetDb();
                    var collection = db.GetCollection<AxieWinrate>("AxieWinrate");
                    var dbList = await collection.FindAsync(a => a.mysticCount == mysticCount && a.lastBattleDate > time - 345600);
                    var axieList = dbList.ToList().Where(a => (a.battleHistory.Length) >= 90).OrderByDescending(a => a.GetRecentWins()).ToList();
                    await Context.Channel.SendMessageAsync("", embed: WinrateCollector.GetTop10LeaderBoard(axieList, mysticCount));
                }
                else await Context.Channel.SendMessageAsync($"The battle database is currently being updated. Please try again later. \nData fetched from API : **{WinrateCollector.apiPerc}%** \nData synced to DB : **{WinrateCollector.dbPerc}%**");
            }
        }

        [Command("lapse"), Summary("GetAxieWinrate on a lapse of battles")]
        public async Task GetDailylyWinrate(int id, int length = 100)
        {
            if (IsArena(Context) || IsBotCommand(Context))
            {
                var db = DatabaseConnection.GetDb();
                var collection = db.GetCollection<BsonDocument>("AxieWinrate");
                var filterId = Builders<BsonDocument>.Filter.Eq("_id", id);
                var doc = (await collection.FindAsync(filterId)).FirstOrDefault();
                var axieWinrate = BsonSerializer.Deserialize<AxieWinrate>(doc);
                var axieData = await AxieObject.GetAxieFromApi(id);
                axieData.id = id;
                if (length >= 100) length = 100;
                if (length < 1) length = 1;
                await Context.Channel.SendMessageAsync("", embed: axieData.EmbedWinrateRecent(axieWinrate, length));

            }
        }

        [Command("UpdateNickname"), Summary("Update nickname")]
        [Alias("IdNick")]
        public async Task UpdateNickname(int id)
        {
            var collec = DatabaseConnection.GetDb().GetCollection<AxieMapping>("IdNameMapping");
            var data = await AxieDataOld.GetAxieFromApi(id);
            var axie = (await collec.FindAsync(a => a.id == id)).FirstOrDefault();
            if (axie != null)
            {
                await collec.UpdateOneAsync(a => a.id == id, Builders<AxieMapping>.Update.Set(b => b.name, data.name));
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
            else
                await collec.InsertOneAsync(new AxieMapping(data.id, data.name));
        }

        [Command("UpdateAddressNickname"), Summary("Update nickname")]
        [Alias("AddNick")]
        public async Task UpdateAddressNickname(string address)
        {
            var list = await StatDataHandler.GetAxieListFromAddress(address);
            var collec = DatabaseConnection.GetDb().GetCollection<AxieMapping>("IdNameMapping");
            foreach (var ax in list)
            {
                var axie = (await collec.FindAsync(a => a.id == ax.id)).FirstOrDefault();
                if (axie != null)
                {
                    var data = await AxieDataOld.GetAxieFromApi(ax.id);
                    await collec.UpdateOneAsync(a => a.id == ax.id, Builders<AxieMapping>.Update.Set(b => b.name, data.name));
                }
                else
                    await collec.InsertOneAsync(new AxieMapping(ax.id, ax.name));
            }
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        #endregion

        #region miscellaneous
        [Command("rebootSales"), Summary("show you addresses")]
        public async Task RebootSales()
        {
            if (!AxieDataGetter.IsServiceOn)
            {
                _ = AxieDataGetter.GetData();
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }

        [Command("startServices"), Summary("show you addresses")]
        public async Task StartServices()
        {
            if (IsDev(Context))
            {
                if (!TaskHandler.IsOn)
                {
                    TaskHandler.IsOn = true;
                    TaskHandler.FetchingDataFromApi = true;
                    _ = TaskHandler.RunTasks();
                    _ = TaskHandler.UpdateServiceCheckLoop();
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
            }
        }

        [Command("updatedb"), Summary("show you addresses")]
        public async Task StopServices(string ip)
        {
            if (IsDev(Context))
            {
                DatabaseConnection.UpdateIpAddress(ip);
                DatabaseConnection.SetupConnection();
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }

        [Command("stopServices"), Summary("show you addresses")]
        public async Task StopServices()
        {
            if (IsDev(Context))
            {
                if (TaskHandler.IsOn)
                {
                    TaskHandler.IsOn = false;
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
            }
        }


        [Command("breedlist"), Summary("Returns best breed pairs for pure axie")]
        [Alias("bl")]
        public async Task GetBreedList([Remainder]string address)
        {
            string[] inputData = address.Split(' ');
            List<string> classFilters = new List<string>();
            List<string> addressList = new List<string>();
            foreach (var input in inputData)
            {
                switch (input)
                {
                    case "bird":
                    case "beast":
                    case "reptile":
                    case "aquatic":
                    case "plant":
                    case "bug":
                        classFilters.Add(input);
                        break;
                    default:
                        addressList.Add(input);
                        break;

                }
            }
            string addresses = "";
            for (int i = 0; i < addressList.Count; i++)
            {
                if (i != 0) addresses += " ";
                addresses += addressList[i];
            }
            

            await TaskHandler.AddTask(Context, addresses, TaskType.BreedQuery, classFilters);
            await Context.Message.AddReactionAsync(new Emoji("✅"));
            if (!TaskHandler.FetchingDataFromApi)
            {
                TaskHandler.FetchingDataFromApi = true;
                _ = TaskHandler.RunTasks();
            }
        }

        [Command("traitList"), Summary("Returns best breed pairs for pure axie")]
        [Alias("tl")]
        public async Task GetBreedList(string address, [Remainder]string trait)
        {
            await TaskHandler.AddTask(Context, address, TaskType.TraitQuery, trait);
            await Context.Message.AddReactionAsync(new Emoji("✅"));
            if (!TaskHandler.FetchingDataFromApi)
            {
                TaskHandler.FetchingDataFromApi = true;
                _ = TaskHandler.RunTasks();
            }
        }

        [Command("winratelist"), Summary("Returns winrates of an address")]
        [Alias("winlist")]
        public async Task GetWinrateList(string address)
        {
            await TaskHandler.AddTask(Context, address, TaskType.WinrateQuery, null);
            await Context.Message.AddReactionAsync(new Emoji("✅"));
            if (!TaskHandler.FetchingDataFromApi)
            {
                TaskHandler.FetchingDataFromApi = true;
                _ = TaskHandler.RunTasks();
            }
        }

        [Command("teamList"), Summary("Returns winrates of an address")]
        [Alias("teamL")]
        public async Task GetTeamList(string address)
        {
            await TaskHandler.AddTask(Context, address, TaskType.TeamQuery, null);
            await Context.Message.AddReactionAsync(new Emoji("✅"));
            if (!TaskHandler.FetchingDataFromApi)
            {
                TaskHandler.FetchingDataFromApi = true;
                _ = TaskHandler.RunTasks();
            }
        }
        [Command("test")]
        public async Task Test()
        {
            await AxieDataGetter.AlertSeller(150, 4.5f, "0xf521Bb7437bEc77b0B15286dC3f49A87b9946773");
        }
        [Command("burn"), Summary("buuuuuuurn that axie >:D")]
        public async Task BurnJoke(int index)
        {
            await Context.Message.AddReactionAsync(new Emoji("🔥"));
        }
        [Command("ShutDownSales"), Summary("show you addresses")]
        public async Task ShutDownSales()
        {
            if (AxieDataGetter.IsServiceOn)
            {
                AxieDataGetter.IsServiceOn = false;
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }

        [Command("GiveAway", RunMode = RunMode.Async), Summary("hi")]
        public async Task LaunchGiveAway([Summary("Max minutes to wait for players")]string strMaxSecToWait = null,
            [Summary("Target number")]string strTargetNumber = null,
            [Summary("Number of Winners")]string strNumWinners = null,
            [Summary("Number of Winners")]string strTestUsers = null)
        {
            GiveawayInstance gameInstance = new GiveawayInstance();
            await StartGiveAway(gameInstance, strMaxSecToWait, strTargetNumber, strNumWinners, strTestUsers);
        }

        [Command("ping2")]
        public async Task pong() => await Context.Channel.SendMessageAsync("pong");
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

        [Command("cleanup", RunMode = RunMode.Async), Summary("Provides Help with commands.")]
        [Alias("clean", "delete", "remove")]
        public async Task CleanUpAxie()
        {
            await CleanUp();
        }
        #endregion
    }
}

