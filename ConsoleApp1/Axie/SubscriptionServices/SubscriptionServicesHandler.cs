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

        public static async Task<List<SubUser>> SetSubListFromFile()
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
            if (subUserList == null) subUserList = await SetSubListFromFile();
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
            await SetSubList(subUserList);
        }

        public static async Task SetLabPriceTrigger(ulong userId, float priceTrigger, ICommandContext context)
        {
            if (subUserList == null) subUserList = await SetSubListFromFile();
            var existingUser = subUserList.FirstOrDefault(user => user.GetId() == userId);
            if (existingUser != null)
            {
                var axieLabService = existingUser.GetServiceList().FirstOrDefault(_service => _service.name == ServiceEnum.AxieLab) as AxieLabService;
                if (axieLabService != null)
                {
                    if (priceTrigger >= 0.13f)
                    {
                        axieLabService.SetPrice(priceTrigger);
                        await SetSubList(subUserList);
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
        private static async Task SetSubList(List<SubUser> subList)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < subList.Count; i++)
            {
                stringBuilder.Append(JsonConvert.SerializeObject(subList[i]));
                if (i != subList.Count - 1)
                    stringBuilder.AppendLine();
            }
           
            using (StreamWriter sw = new StreamWriter(subFileName))
            {
                await sw.WriteAsync(stringBuilder.ToString());
                
            }
        }

    }
}
