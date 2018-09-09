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
        public AxieParts parts;

        public int exp;
        public int level;
        public int stage;
        public AxieStats stats;
        //public AxieFigure figure;
        public static EmbedBuilder EmbedAxieData(AxieData axieData, JObject axieJson)
        {
            string imageUrl = axieJson["figure"]["static"]["idle"].ToString();
            int pureness = GetPureness(axieData);
            


            var embed = new EmbedBuilder();
            embed.WithTitle(axieData.name);
            embed.AddInlineField("Class", axieData.Class);
            embed.AddInlineField("Title", axieData.title == null ? "None" : axieData.title);
            embed.AddInlineField("Exp", axieData.exp);
            embed.AddInlineField("Level", axieData.level);
            embed.AddField("Owner", axieData.owner);
            //embed.AddField("HP", axieData.stats.hp.ToString());
            embed.AddField("HP", $" ({axieData.stats.hp})".PadLeft( 5 + axieData.stats.hp, '|'));
            embed.AddField("Speed", $" ({axieData.stats.speed})".PadLeft(5 + axieData.stats.speed, '|'));
            embed.AddField("Skill", $" ({axieData.stats.skill})".PadLeft(5 + axieData.stats.skill, '|'));
            embed.AddField("Morale", $" ({axieData.stats.morale})".PadLeft(5 + axieData.stats.morale, '|'));

            //embed.AddField("Speed", axieData.stats.speed.ToString());
            //embed.AddField("Skill", axieData.stats.skill.ToString());
            //embed.AddField("Morale", axieData.stats.morale.ToString());
            //embed.AddInlineField("Eyes || " + axieData.parts.eyes.Clazz, axieData.parts.eyes.name + (axieData.parts.eyes.mystic ? " (M)" : ""));
            //embed.AddInlineField("Mouth || " + axieData.parts.mouth.Clazz, axieData.parts.mouth.name + (axieData.parts.mouth.mystic ? " (M)" : ""));
            //embed.AddInlineField("Ears || " + axieData.parts.ears.Clazz, axieData.parts.ears.name + (axieData.parts.ears.mystic ? " (M)" : ""));
            //embed.AddInlineField("Horn || " + axieData.parts.horn.Clazz, axieData.parts.horn.name + (axieData.parts.horn.mystic ? " (M)" : ""));
            //embed.AddInlineField("Back || " + axieData.parts.back.Clazz, axieData.parts.back.name + (axieData.parts.back.mystic ? " (M)" : ""));
            //embed.AddInlineField("Tail || " + axieData.parts.tail.Clazz, axieData.parts.tail.name + (axieData.parts.tail.mystic ? " (M)" : ""));
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
        public static int GetPureness(AxieData axie)
        {
            int pureness = 0;
            if (axie.parts.ears.Clazz == axie.Class) pureness++;
            if (axie.parts.tail.Clazz == axie.Class) pureness++;
            if (axie.parts.horn.Clazz == axie.Class) pureness++;
            if (axie.parts.back.Clazz == axie.Class) pureness++;
            if (axie.parts.eyes.Clazz == axie.Class) pureness++;
            if (axie.parts.mouth.Clazz == axie.Class) pureness++;
            return pureness++;
        }
    }

    public class AxieParts
    {
        public AxiePart tail;
        public AxiePart back;
        public AxiePart horn;
        public AxiePart ears;
        public AxiePart mouth;
        public AxiePart eyes;
    }

    public class AxiePart
    {
        public string id;
        public string name;
        public string Clazz;
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
