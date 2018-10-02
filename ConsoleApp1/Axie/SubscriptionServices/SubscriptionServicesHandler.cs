using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Discord.Commands;
using Discord;

namespace DiscordBot.Axie.SubscriptionServices
{
    public class SubscriptionServicesHandler
    {
        private static string subFileName = "SubList.txt";
        private static readonly object SyncObj = new object();
        private static List<SubUser> subUserList;
        public static List<SubUser> GetSubList() => subUserList;

        public static async Task<List<SubUser>> GetSubListFromFile()
        {
            List<SubUser> list = new List<SubUser>();
            if (File.Exists(subFileName))
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new SubServiceConverter());
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
            if (subUserList == null) subUserList = await GetSubListFromFile();
            //subUserList = new List<SubUser>();
            var existingUser = subUserList.FirstOrDefault(user => user.GetId() == newUserId);
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
            if (subUserList == null) subUserList = await GetSubListFromFile();
            var existingUser = subUserList.FirstOrDefault(user => user.GetId() == userId);
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
        #endregion

        #region Markeplace

        public static async Task SubscribeToMarketPlace(ulong newUserId)
        {
            if (subUserList == null) subUserList = await GetSubListFromFile();
            //subUserList = new List<SubUser>();
            var existingUser = subUserList.FirstOrDefault(user => user.GetId() == newUserId);
            if (existingUser == null)
            {
                var newUser = new SubUser(newUserId);
                newUser.AddService(new MarketplaceService(ServiceEnum.MarketPlace));
                subUserList.Add(newUser);
            }
            else if (!existingUser.GetServiceList().Exists(_service => _service.name == ServiceEnum.MarketPlace))
            {
                existingUser.AddService(new MarketplaceService(ServiceEnum.AxieLab));
            }
            await SetSubList();
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

    }
}
