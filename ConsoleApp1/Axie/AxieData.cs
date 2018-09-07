using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Discord;

namespace DiscordBot
{
    public class AxieData
    {
        public int id;
        public string name;
        public string owner;
        public string genes;
        public string Class;
        public string title;
        public AxiePart[] parts;
        public int exp;
        public int level;
        public int stage;
        public AxieStats stats;
        //public AxieFigure figure;
        public static EmbedBuilder EmbedAxieData(AxieData axieData, JObject axieJson)
        {
            string imageUrl = (string)axieJson["figure"]["images"][axieData.id.ToString() + ".png"];
            int pureness = axieData.parts.Count(p => p.Class == axieData.Class);
            var embed = new EmbedBuilder();
            embed.WithTitle(axieData.name);
            embed.AddInlineField("Class", axieData.Class);
            embed.AddInlineField("Title", axieData.title == null ? "None" : axieData.title);
            embed.AddInlineField("Exp", axieData.exp);
            embed.AddInlineField("Level", axieData.level);
            embed.AddField("Owner", axieData.owner);
            embed.AddField("HP", axieData.stats.hp.ToString());
            embed.AddInlineField("Speed", axieData.stats.speed.ToString());
            embed.AddInlineField("Skill", axieData.stats.skill.ToString());
            embed.AddField("Morale", axieData.stats.morale.ToString());
            embed.AddInlineField("Eyes || " + axieData.parts[0].Class, axieData.parts[0].name + (axieData.parts[0].mystic ? " (M)" : ""));
            embed.AddInlineField("Mouth || " + axieData.parts[1].Class, axieData.parts[1].name + (axieData.parts[1].mystic ? " (M)" : ""));
            embed.AddInlineField("Ears || " + axieData.parts[2].Class, axieData.parts[2].name + (axieData.parts[2].mystic ? " (M)" : ""));
            embed.AddInlineField("Horn || " + axieData.parts[3].Class, axieData.parts[3].name + (axieData.parts[3].mystic ? " (M)" : ""));
            embed.AddInlineField("Back || " + axieData.parts[4].Class, axieData.parts[4].name + (axieData.parts[4].mystic ? " (M)" : ""));
            embed.AddInlineField("Tail || " + axieData.parts[5].Class, axieData.parts[5].name + (axieData.parts[5].mystic ? " (M)" : ""));
            embed.AddField("Pureness", pureness);
            embed.WithImageUrl(imageUrl);
            Color color = Color.Default;
            switch (axieData.Class)
            {
                case "plant":
                    color = Color.Green;
                    break;
                case "beast":
                    color = Color.Gold;
                    break;
                case "aquatic":
                    color = Color.Blue;
                    break;
                case "bug":
                    color = Color.Red;
                    break;
                case "bird":
                    color = Color.Magenta;
                    break;
                case "reptile":
                    color = Color.DarkMagenta;
                    break;
            }
            embed.WithColor(color);
            return embed;
        }
    }
    public class AxiePart
    {
        public string id;
        public string name;
        public string Class;
        public string type;
        public bool mystic;
    }
    public class AxieStats
    {
        public int hp;
        public int speed;
        public int skill;
        public int morale;
    }
    public class AxieFigure
    {
        public string atlas;
        public AxieImage images;
        public string model;
    }

    public class AxieImage
    {
        [JsonProperty(PropertyName = "")]
        public string png ;
    }
}
