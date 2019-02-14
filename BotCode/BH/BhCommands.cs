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
                                                 + "- `>bh fam fam_name` : Returns the information of familiar. \n"
                                                 + "- `>bh mount mount_name` : Returns the information of mount. (COMING SOON) \n");
        }

        [Command("fam"), Summary("give BH commands")]
        public async Task GetFamInfo(string name)
        {
            if (lastCallFromGeneral == null || DateTime.Now.Subtract(lastCallFromGeneral).Seconds > 10 || Context.Channel.Id != 241550898998935552)
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

                        await Context.Channel.SendMessageAsync("", false, DataFetcher.GetFamData(obj).Build(), null);
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

                        await Context.Channel.SendMessageAsync("", false, DataFetcher.GetFusionData(obj).Build(), null);
                        await Context.Channel.SendFileAsync(image.Stream, "fam.png");
                    }
                }
            }
        }

        [Command("mount"), Summary("give BH commands")]
        public async Task GetMountInfo(string name)
        {
            if (lastCallFromGeneral == null || DateTime.Now.Subtract(lastCallFromGeneral).Seconds > 10 || Context.Channel.Id != 241550898998935552)
            {
                if (Context.Channel.Id == 241550898998935552) lastCallFromGeneral = DateTime.Now;
                JArray mountJson;
                using (StreamReader sr = new StreamReader("BHData/mounts.json"))
                {
                    string data = await sr.ReadToEndAsync();
                    mountJson = JArray.Parse(data);
                }
                foreach (var obj in mountJson.Children<JObject>())
                {
                    if (((string)obj["name"]).ToLower().Replace(" ", string.Empty).Replace("'", string.Empty) == name.ToLower().Replace(" ", string.Empty).Replace("'", string.Empty))
                    {
                        Image image;
                        //byte[] data = Convert.FromBase64String((string)obj["avatar"]);
                        var data64 = Regex.Match((string)obj["avatar"], @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
                        byte[] data = Convert.FromBase64String(data64);

                        image = new Image(new MemoryStream(data, 0, data.Length));

                        await Context.Channel.SendMessageAsync("", false, DataFetcher.GetFamData(obj).Build(), null);
                        await Context.Channel.SendFileAsync(image.Stream, "mount.png");
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
