using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Discord;
using System.IO;

namespace DiscordBot.BH
{
    class DataFetcher
    {
        public static async Task<JArray> FetchData(string url)
        {
            string json = "";
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    json = await wc.DownloadStringTaskAsync(url);

                }
                catch (Exception ex)
                {
                }
            }
            return JArray.Parse(json);
        }
        public static EmbedBuilder GetFamData(JObject obj)
        {
            var embed = new EmbedBuilder();

            embed.WithTitle((string)obj["name"]);
            embed.AddField("zone", (string)obj["zone"]);
            embed.AddInlineField("Power", (string)obj["power"] + "%");
            embed.AddInlineField("Stamina", (string)obj["stamina"] + "%");
            embed.AddInlineField("Agility", (string)obj["agility"] + "%");
            foreach (var skill in obj["skills"])
            {
                string info = $"{(string)skill["action"]} | SP = {(int)skill["skillPoint"]} | Range = {(string)skill["pourcentage"]}";
                embed.AddField((string)skill["name"], info);
            }
            Color color = Color.Orange;
            switch ((string)obj["type"])
            {
                case "Common":
                    color = Color.Green;
                    break;
                case "Rare":
                    color = Color.Blue;
                    break;
                case "Epic":
                    color = Color.DarkRed;
                    break;
                case "Legendary":
                    color = Color.Gold;
                    break;
                case "Mythic":
                    color = Color.Magenta;
                    break;
            }
            embed.WithColor(color);
            return embed;
        }


        public static EmbedBuilder GetFusionData(JObject obj)
        {
            var embed = new EmbedBuilder();

            embed.WithTitle((string)obj["name"]);
            embed.AddInlineField("Power", (string)obj["power"] + "%");
            embed.AddInlineField("Stamina", (string)obj["stamina"] + "%");
            embed.AddInlineField("Agility", (string)obj["agility"] + "%");
            foreach (var skill in obj["skills"])
            {
                string info = $"{(string)skill["action"]} | SP = {(int)skill["skillPoint"]} | Range = {(string)skill["pourcentage"]}";
                embed.AddField((string)skill["name"], info);
            }
            foreach (var skill in obj["passiveAbility"])
            {
                embed.AddField((string)skill["ability"], (string)skill["pourcentage"]);
            }
            Color color = Color.Orange;
            switch ((string)obj["type"])
            {
                case "Common":
                    color = Color.Green;
                    break;
                case "Rare":
                    color = Color.Blue;
                    break;
                case "Epic":
                    color = Color.DarkRed;
                    break;
                case "Legendary":
                    color = Color.Gold;
                    break;
                case "Mythic":
                    color = Color.Magenta;
                    break;
            }
            embed.WithColor(color);
            return embed;
        }

        public static EmbedBuilder GetMountData(JObject obj)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle((string)obj["name"]);
            embed.AddField("Movement speed", (string)obj["moveSpeed"] + "%");
            embed.AddField("Skill", (string)obj["skill"]);
            string coststr = "";
            foreach (var cost in obj["purchase"][0])
            {
                coststr += $"{(int)cost["count"]} {(string)cost["material"]}";
            }
            embed.AddField("Cost", coststr);
            Color color = Color.Orange;
            switch ((string)obj["type"])
            {
                case "Common":
                    color = Color.Green;
                    break;
                case "Rare":
                    color = Color.Blue;
                    break;
                case "Epic":
                    color = Color.DarkRed;
                    break;
                case "Legendary":
                    color = Color.Gold;
                    break;
                case "Mythic":
                    color = Color.Magenta;
                    break;
            }
            embed.WithColor(color);
            return embed;
        }
    }
}
