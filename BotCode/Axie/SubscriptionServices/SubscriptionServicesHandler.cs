using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Numerics;
using Discord.Commands;
using Discord;
using DiscordBot.Axie.SubscriptionServices.PremiumServices;

namespace DiscordBot.Axie.SubscriptionServices
{
    public class SubscriptionServicesHandler
    {
        private static string subFileName = "SubList.txt";
        private static readonly object SyncObj = new object();
        private static List<SubUser> subUserList;
        public static async Task<List<SubUser>> GetSubList()
        {
            if (subUserList == null) subUserList = await GetSubListFromFile();
            return subUserList;
        }
        public static SubUser GetUserFromId(ulong userId) => subUserList.FirstOrDefault(u => u.GetId() == userId);


        public static async Task<List<SubUser>> GetSubListFromFile()
        {
            List<SubUser> list = new List<SubUser>();
            if (File.Exists(subFileName))
            {
                //var settings = new JsonSerializerSettings();
                //settings.Converters.Add(new SubServiceConverter());
                string fileData = "";
                using (StreamReader sr = new StreamReader(subFileName))
                {
                    fileData = await sr.ReadToEndAsync();
                }
                string[] jsonFiles = Regex.Split(fileData, "\r\n|\r|\n");
                foreach (var json in jsonFiles)
                {
                    JObject userJson = JObject.Parse(json);
                    SubUser user = new SubUser((ulong)userJson["userId"]);
                    foreach (var service in userJson["subServiceList"])
                    {
                        switch ((int)service["name"])
                        {
                            case 0:
                                user.AddService(service.ToObject<AxieLabService>());
                                break;
                            case 1:
                                var marketService = service.ToObject<MarketplaceService>();
                                user.AddService(marketService);
                                break;
                            case 2:
                                user.AddService(service.ToObject<AuctionWatchService>());
                                break;
                        }
                    }
                    list.Add(user);
                }
            }
            return list;
        }

        #region Axie Lab
        public static async Task SubscribeToLabNotif(ulong newUserId)
        {
            subUserList = await GetSubListFromFile();
            //subUserList = new List<SubUser>();
            var existingUser = GetUserFromId(newUserId);
            if (existingUser == null)
            {
                var newUser = new SubUser(newUserId);
                newUser.AddService(new AxieLabService(ServiceEnum.AxieLab));
                subUserList.Add(newUser);
            }
            else if (!existingUser.GetServiceList().Exists(_service => _service.name == ServiceEnum.AxieLab))
            {
                existingUser.AddService(new AxieLabService(ServiceEnum.AxieLab));
            }
            await SetSubList();
        }
        public static async Task SetLabPriceTrigger(ulong userId, float priceTrigger, ICommandContext context)
        {
            subUserList = await GetSubListFromFile();
            var existingUser = GetUserFromId(userId);
            if (existingUser != null)
            {
                var axieLabService = existingUser.GetServiceList().FirstOrDefault(_service => _service.name == ServiceEnum.AxieLab) as AxieLabService;
                if (axieLabService != null)
                {
                    if (priceTrigger >= 0.13f)
                    {
                        axieLabService.SetPrice(priceTrigger);
                        await SetSubList();
                        await context.Message.AddReactionAsync(new Emoji("✅"));
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync("Price trigger is lower than lowest egg price :/");
                    }
                }
                else await context.Channel.SendMessageAsync("User is not not subscribed to service. Please subscribe using command `>axie axieLabSub` .");
            }
        }
        public static async Task RemoveLabPodPriceTrigger(ulong userId, ICommandContext context)
        {
            if (subUserList == null) subUserList = await GetSubListFromFile();
            var existingUser = GetUserFromId(userId);
            if (existingUser != null)
            {
                var axieLabService = existingUser.GetServiceList().FirstOrDefault(_service => _service.name == ServiceEnum.AxieLab) as AxieLabService;
                if (axieLabService != null)
                {
                    axieLabService.SetPrice(0);
                    await SetSubList();
                    await context.Message.AddReactionAsync(new Emoji("✅"));
                }
            }
        }
        #endregion

        #region Markeplace

        public static async Task SubscribeToMarketPlace(ulong newUserId)
        {
            subUserList = await GetSubList();
            //subUserList = new List<SubUser>();
            var existingUser = GetUserFromId(newUserId);
            if (existingUser == null)
            {
                var newUser = new SubUser(newUserId);
                newUser.AddService(new MarketplaceService(ServiceEnum.MarketPlace));
                subUserList.Add(newUser);
                await SetSubList();
            }
            else if (!existingUser.GetServiceList().Exists(_service => _service.name == ServiceEnum.MarketPlace))
            {
                existingUser.AddService(new MarketplaceService(ServiceEnum.MarketPlace));
                await SetSubList();
            }
        }

        public static async Task SetMarketPriceTrigger(ulong userId, int axieId, double priceTrigger, ICommandContext context)
        {
            subUserList = await GetSubList();
            var existingUser = GetUserFromId(userId);
            if (existingUser != null)
            {
                BigInteger convertedPrice = new BigInteger(priceTrigger * Math.Pow(10, 18));
                var axieData = await AxieObject.GetAxieFromApi(axieId);
                //Use this function while API v1 is down
                await axieData.GetTrueAuctionData();
                if (axieData.auction != null)
                {
                    var newTrigger = new AxieTrigger();
                    newTrigger.SetupAxieTrigger(
                        axieId,
                        MarketPlaceTriggerTypeEnum.OnPriceTrigger,
                        (long)axieData.auction.duration,
                        (long)axieData.auction.startingTime,
                        axieData.auction.startingPrice,
                        axieData.auction.endingPrice,
                        convertedPrice,
                        axieData.GetImageUrl());

                    var marketplaceService = existingUser.GetServiceList().FirstOrDefault(_service => _service.name == ServiceEnum.MarketPlace) as MarketplaceService;
                    if (marketplaceService != null)
                    {
                        if (!marketplaceService.GetTriggerList().Exists(t => t.axieId == axieId)) marketplaceService.AddTrigger(newTrigger);
                        else
                        {
                            var existingTrigger = marketplaceService.GetTriggerList().FirstOrDefault(t => t.axieId == axieId);
                            marketplaceService.ReplaceTrigger(existingTrigger, newTrigger);
                        }
                        await SetSubList();
                        await context.Message.AddReactionAsync(new Emoji("✅"));
                    }
                    else await context.Channel.SendMessageAsync("Error. You are not subscribed to Marketplace service.");
                }
                else await context.Channel.SendMessageAsync("Error. Specified axie is not on sale :/");
            }
        }

        public static async Task SetSaleNotifier(ulong userId, bool _bool, ICommandContext context)
        {
            subUserList = await GetSubList();
            var existingUser = GetUserFromId(userId);
            if (existingUser != null)
            {
                var marketplaceService = existingUser.GetServiceList().FirstOrDefault(_service => _service.name == ServiceEnum.MarketPlace) as MarketplaceService;
                if (marketplaceService != null)
                {
                    marketplaceService.SetNotifStatus(_bool);
                    await context.Message.AddReactionAsync(new Emoji("✅"));
                }
                else await context.Channel.SendMessageAsync("Error. You are not subscribed to Marketplace service.");
            }
        }

        public static async Task RemoveMarketPriceTrigger(ulong userId, int axieId, ICommandContext context)
        {
            if (subUserList == null) subUserList = await GetSubListFromFile();
            var existingUser = GetUserFromId(userId);
            if (existingUser != null)
            {
                var marketplaceService = existingUser.GetServiceList().FirstOrDefault(_service => _service.name == ServiceEnum.MarketPlace) as MarketplaceService;
                if (marketplaceService != null)
                {
                    if (marketplaceService.GetTriggerList().Exists(t => t.axieId == axieId))
                    {
                        marketplaceService.RemoveTriggerFromId(axieId);
                        await SetSubList();
                        await context.Message.AddReactionAsync(new Emoji("✅"));
                    }
                }
            }
        }

        #endregion

        #region Auction filter service
        public static async Task SubscribeToAuctionWatcher(ulong newUserId, ICommandContext context)
        { 
            subUserList = await GetSubList();
            //subUserList = new List<SubUser>();
            var existingUser = GetUserFromId(newUserId);
            if (existingUser == null)
            {
                var newUser = new SubUser(newUserId);
                newUser.AddService(new MarketplaceService(ServiceEnum.MarketPlace));
                subUserList.Add(newUser);
                await SetSubList();
                await context.Message.AddReactionAsync(new Emoji("✅"));
            }
            else if (!existingUser.GetServiceList().Exists(_service => _service.name == ServiceEnum.AuctionWatch))
            {
                existingUser.AddService(new MarketplaceService(ServiceEnum.MarketPlace));
                await SetSubList();
                await context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }

        public static async Task CreateAuctionFilter(ulong userId, string filterInput, ICommandContext context)
        {
            var existingUser = GetUserFromId(userId);
            if (existingUser != null)
            {
                var auctionService = existingUser.GetServiceList().FirstOrDefault(_service => _service.name == ServiceEnum.AuctionWatch) as AuctionWatchService;
                if (auctionService != null)
                {
                    filterInput = filterInput.ToLower();
                    var auctionFilter = AuctionFilter.ReturnNewAuctionFilter(filterInput);
                    if (auctionFilter != null)
                    {
                        if (auctionService.GetList().Count > 0) auctionFilter.triggerId = auctionService.GetList().Max(f => f.triggerId) + 1;
                        else auctionFilter.triggerId = 0;
                        auctionService.AddFilter(auctionFilter);
                        await SetSubList();
                        await context.Message.AddReactionAsync(new Emoji("✅"));
                    }
                }
            }
            else await context.Channel.SendMessageAsync("Error. You are not subscribed to Auction watch service.");
        }

        #endregion
        public static async Task SetSubList()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < subUserList.Count; i++)
            {
                stringBuilder.Append(JsonConvert.SerializeObject(subUserList[i]));
                if (i != subUserList.Count - 1)
                    stringBuilder.AppendLine();
            }

            using (StreamWriter sw = new StreamWriter(subFileName))
            {
                await sw.WriteAsync(stringBuilder.ToString());
            }
        }
        public static async Task SubscribeToService(ulong newUserId, ICommandContext context, ServiceEnum service)
        {
            subUserList = await GetSubList();

            var existingUser = GetUserFromId(newUserId);
            if (existingUser == null)
            {
                var newUser = new SubUser(newUserId);
                switch (service)
                {
                    case ServiceEnum.AuctionWatch:
                        newUser.AddService(new AuctionWatchService(service));
                        break;
                    case ServiceEnum.MarketPlace:
                        newUser.AddService(new MarketplaceService(service));
                        break;
                    case ServiceEnum.AxieLab:
                        newUser.AddService(new AxieLabService(service));
                        break;
                }
                subUserList.Add(newUser);
                await SetSubList();
                await context.Message.AddReactionAsync(new Emoji("✅"));
            }
            else if (!existingUser.GetServiceList().Exists(_service => _service.name == service))
            {
                switch (service)
                {
                    case ServiceEnum.AuctionWatch:
                        existingUser.AddService(new AuctionWatchService(service));
                        break;
                    case ServiceEnum.MarketPlace:
                        existingUser.AddService(new MarketplaceService(service));
                        break;
                    case ServiceEnum.AxieLab:
                        existingUser.AddService(new AxieLabService(service));
                        break;
                }
                await SetSubList();
                await context.Message.AddReactionAsync(new Emoji("✅"));
            }
            
        }
        public static async Task UnSubToService(ulong userId, ICommandContext context, ServiceEnum service)
        {
            if (subUserList == null) subUserList = await GetSubListFromFile();
            var existingUser = subUserList.FirstOrDefault(user => user.GetId() == userId);
            if (existingUser != null)
            {
                if (existingUser.GetServiceList().Exists(s => s.name == service)) existingUser.RemoveService(service);
                await SetSubList();
                await context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }
    }
}
