using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DiscordBot.Axie.SubscriptionServices
{
    public class SubscriptionServicesHandler
    {
        private static string subFileName = "SubList.txt";
        private static readonly object SyncObj = new object();


        public static async Task<List<SubUser>> GetSubUserList()
        {
            List<SubUser> list = new List<SubUser>();
            if (File.Exists(subFileName))
            {
                string fileData = "";
                using (StreamReader sr = new StreamReader(subFileName))
                {
                    fileData = await sr.ReadToEndAsync();
                }
                string[] jsonFiles = Regex.Split(fileData, "\r\n|\r|\n");
                foreach (var json in jsonFiles)
                {
                    SubUser obj = JObject.Parse(json).ToObject<SubUser>();
                    if (obj != null) list.Add(obj);
                }
            }
            return list;
        }

        #region Axie Lab
        public static async Task SubscribeToLabNotif(ulong newUserId)
        {
            var subUserList = await GetSubUserList();

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

        public static async Task SetLabPriceTrigger(ulong userId, float priceTrigger)
        {
            var subUserList = await GetSubUserList();
            var existingUser = subUserList.FirstOrDefault(user => user.GetId() == userId);
            if (existingUser != null)
            {
                var axieLabService = existingUser.GetServiceList().FirstOrDefault(_service => _service.name == ServiceEnum.AxieLab) as AxieLabService;
                if(axieLabService != null)
                {
                    if (priceTrigger >= 0.13f)
                    {
                        axieLabService.SetPrice(priceTrigger);
                        await SetSubList(subUserList);
                    }
                    else
                    {
                        //price too low
                    }
                }

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
