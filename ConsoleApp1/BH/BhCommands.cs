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
using System.Text.RegularExpressions;
namespace DiscordBot.BH
{
    [Group("bh")]
    public class BhCommand : BaseCommands
    {
        private static DateTime lastCallFromGeneral;
        [Command("help"), Summary("give BH commands")]
        public async Task PostHelp()
        {
            //if (IsBotCommand(Context))
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

        [Command("fam"), Summary("give BH commands")]
        public async Task GetFamInfo(string name)
        {
            if (lastCallFromGeneral == null || DateTime.Now.Subtract(lastCallFromGeneral).Seconds > 10)
            {
                if(Context.Channel.Id == 241550898998935552) lastCallFromGeneral = DateTime.Now;
                JArray famJson;
                JArray fusionJson;
                using (StreamReader sr = new StreamReader("BHData/familiar.json"))
                {
                    string data = await sr.ReadToEndAsync();
                    famJson = JArray.Parse(data);
                }
                using (StreamReader sr = new StreamReader("BHData/fusions.json"))
                {
                    fusionJson = JArray.Parse(await sr.ReadToEndAsync());
                }
                foreach (var obj in famJson.Children<JObject>())
                {
                    if (((string)obj["name"]).ToLower().Replace(" ", string.Empty).Replace("'", string.Empty) == name.ToLower().Replace(" ", string.Empty).Replace("'", string.Empty))
                    {
                        Image image;
                        //byte[] data = Convert.FromBase64String((string)obj["avatar"]);
                        var data64 = Regex.Match((string)obj["avatar"], @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
                        byte[] data = Convert.FromBase64String(data64);

                        image = new Image(new MemoryStream(data, 0, data.Length));

                        await Context.Channel.SendMessageAsync("", false, DataFetcher.GetFamData(obj), null);
                        await Context.Channel.SendFileAsync(image.Stream, "fam.png");
                    }
                }

                foreach (var obj in fusionJson.Children<JObject>())
                {
                    if (((string)obj["name"]).ToLower().Replace(" ", string.Empty).Replace("'", string.Empty) == name.ToLower().Replace(" ", string.Empty).Replace("'", string.Empty))
                    {
                        Image image;
                        //byte[] data = Convert.FromBase64String((string)obj["avatar"]);
                        var data64 = Regex.Match((string)obj["avatar"], @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
                        byte[] data = Convert.FromBase64String(data64);

                        image = new Image(new MemoryStream(data, 0, data.Length));

                        await Context.Channel.SendMessageAsync("", false, DataFetcher.GetFusionData(obj), null);
                        await Context.Channel.SendFileAsync(image.Stream, "fam.png");
                    }
                }
            }
        }


        [Command("updateJson"), Summary("give BH commands")]
        public async Task upateJson(string dataType)
        {
            string path = "https://raw.githubusercontent.com/NerOcrO/bit-heroes/master/data/";
            string ext = "";
            switch (dataType)
            {
                case "familiar":
                    ext = "familiar.json";
                    break;
                case "mounts":
                    ext = "mounts.json";
                    break;
                case "fusions":
                    ext = "fusions.json";
                    break;
            }
            JArray data = await DataFetcher.FetchData(path + ext);
            if (File.Exists("BHData/" + ext))
            {
                using (var tw = new StreamWriter("BHData/" + ext))
                {
                    tw.Write(data);
                }
            }
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

    }
}
