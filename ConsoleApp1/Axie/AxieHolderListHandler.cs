using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace DiscordBot
{
    class AxieHolderListHandler
    {
        private static string strFilename = "AxieHolderList.txt";
        private static readonly object SyncObj = new object();

        public AxieHolderListHandler()
        {

        }

        public static async Task<List<AxieHolder>> GetAxieHolders()
        {
            List<AxieHolder> list = new List<AxieHolder>();
            if(File.Exists(strFilename))
            {
                string fileData = "";
                using (StreamReader sr = new StreamReader(strFilename))
                {
                    fileData = await sr.ReadToEndAsync();
                }
                string[] jsonFiles = Regex.Split(fileData, "\r\n|\r|\n");
                foreach (var json in jsonFiles)
                {
                    AxieHolder obj = JObject.Parse(json).ToObject<AxieHolder>();
                    if (obj != null) list.Add(obj);
                }
            }
            return list;
        }

        public static async Task AddUserAddress(ulong userId, string address, IMessageChannel channel = null)
        {
            List<AxieHolder> axieHolders = await GetAxieHolders();
            if (IsAddressValid(address))
            {
                address = address.ToLower();
                foreach (var holder in axieHolders)
                {
                    if (holder.GetAddressList().Contains(address))
                    {
                        await channel.SendMessageAsync("Error. Address already used.");
                        return;
                    }
                }
                AxieHolder axieHolder = axieHolders.FirstOrDefault(holder => holder.GetUserId() == userId);
                if (axieHolder == null)
                {
                    List<string> addressList = new List<string>() { address };
                    axieHolders.Add(new AxieHolder(userId, addressList));
                }
                else
                {
                    axieHolder.AddAddress(address);
                }
                await SetAddressList(axieHolders);
                await channel.SendMessageAsync("Address added :)");
            }
            else await channel.SendMessageAsync("Error. Invalid address >:(");
        }

        public static async Task RemoveUser(ulong userId, IMessageChannel channel)
        {
            List<AxieHolder> axieHolders = await GetAxieHolders();

            AxieHolder axieHolder = axieHolders.FirstOrDefault(holder => holder.GetUserId() == userId);
            if (axieHolder == null)
            {
                await channel.SendMessageAsync("User does not exist, fool!");
            }
            else
            {
                axieHolders.Remove(axieHolder);
                await SetAddressList(axieHolders);
                await channel.SendMessageAsync("User removed");
            }
        }

        public static async Task RemoveAddress(ulong userId, string address, IMessageChannel channel)
        {
            if (IsAddressValid(address))
            {
                List<AxieHolder> axieHolders = await GetAxieHolders();
                AxieHolder axieHolder = axieHolders.First(holder => holder.GetUserId() == userId);
                if (axieHolder == null)
                {
                    await channel.SendMessageAsync("This address doesn't belong to you :(");
                }
                else
                {
                    axieHolder.RemoveAddress(address);
                    await channel.SendMessageAsync("Address removed!");

                }
                await SetAddressList(axieHolders);
            }
            else await channel.SendMessageAsync("Error. Invalid address >:(");
        }

        private static async Task SetAddressList(List<AxieHolder> axieHolderList)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < axieHolderList.Count; i ++)
            {
                stringBuilder.Append( JsonConvert.SerializeObject(axieHolderList[i]));
                if (i != axieHolderList.Count - 1)
                stringBuilder.AppendLine();
            }
            
            using (StreamWriter sw = new StreamWriter(strFilename))
            {
                await sw.WriteAsync(stringBuilder.ToString());
            }
        }

        public static async Task GetHolderId(string address, ICommandContext context)
        {
            List<AxieHolder> axieHolders = await GetAxieHolders();
            foreach (var holder in axieHolders)
            {
                if (holder.GetAddressList().Contains(address))
                {
                    await context.Channel.SendMessageAsync($"<@{holder.GetUserId()}> , one of your axies has caught the interest of <@{context.Message.Author.Id}> ! Take it to DM for further discussions ;)");
                    return;
                }
            }
            await context.Channel.SendMessageAsync("Uhoh... It looks like this axie doesn't belong to anyone in my Database :(");
        }

        public static async Task GetUserAddressList(ulong userId, IMessageChannel channel)
        {
            List<AxieHolder> axieHolders = await GetAxieHolders();
            AxieHolder axieHolder = axieHolders.FirstOrDefault(holder => holder.GetUserId() == userId);
            if (axieHolder == null)
            {
                await channel.SendMessageAsync("User does not exist :/");
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var address in axieHolder.GetAddressList()) sb.Append($"- `{address}`\n");
                await channel.SendMessageAsync("User's addresses:\n" + sb.ToString());
            }
        }

        private static bool IsAddressValid(string address)
        {
            if (address.StartsWith("0x"))
            {
                char[] cleanAddress = address.ToLower().ToCharArray();
                if (cleanAddress.Length == 42)
                {
                    for (int i = 2; i < cleanAddress.Length; i++)
                    {
                        if (!(cleanAddress[i] >= '0' && cleanAddress[i] <= '9' ||
                              cleanAddress[i] >= 'a' && cleanAddress[i] <= 'f')) return false;
                    }
                }
                else return false;
                return true;
            }
            return false;
        }
    }
}
