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
        public string clazz;
        public string title;
        public AxieParts parts;
        public bool hasMystic
        {
            get
            {
                return parts.ears.mystic || parts.mouth.mystic || parts.horn.mystic || parts.tail.mystic || parts.eyes.mystic || parts.back.mystic;
            }
        }
        public int exp;
        public int level;
        public int stage;
        public AxieStats stats;
        //public AxieFigure figure;
        public JObject jsonData;
        public  EmbedBuilder EmbedAxieData(string extra)
        {
            string imageUrl = jsonData["figure"]["static"]["idle"].ToString();
            int pureness = GetPureness();
            


            var embed = new EmbedBuilder();
            embed.WithTitle(name);
            
            embed.AddInlineField("Class", clazz.First().ToString().ToUpper() + clazz.Substring(1) + $" ({pureness}/6)");
            embed.AddInlineField("Title", title == null ? "None" : title);
            embed.AddInlineField("Exp", exp);
            embed.AddInlineField("Level", level);
            //embed.AddField("Owner", axieData.owner);
            embed.AddInlineField("HP", $" ({stats.hp})".PadLeft( 5 + stats.hp/2, '|'));
            embed.AddInlineField("Skill", $" ({stats.skill})".PadLeft(5 + stats.skill/2, '|'));
            embed.AddInlineField("Speed", $" ({stats.speed})".PadLeft(5 + stats.speed/2, '|'));
            
            embed.AddInlineField("Morale", $" ({stats.morale})".PadLeft(5 + stats.morale/2, '|'));
            if (extra != null && extra == "m")
            { 
                embed.WithDescription("Disclaimer : DPS and Tank ratings are not official nor endorsed by the Axie team.");
                embed.AddInlineField("DPS Score", (int)Math.Floor(GetDPR() / GetMaxDPR() * 100));
                embed.AddInlineField("Tank Score", GetTNKScore());
            }
            //embed.AddField("Pureness", pureness);
            embed.WithThumbnailUrl(imageUrl);
            embed.WithUrl("https://axieinfinity.com/axie/" + id.ToString());
            Color color = Color.Default;
            switch (clazz)
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
        public EmbedBuilder EmbedAxieSaleData(float price)
        {
            string imageUrl = jsonData["figure"]["static"]["idle"].ToString();
            var embed = new EmbedBuilder();
            embed.WithTitle(name);
            embed.WithDescription("Has been sold!");
            embed.AddInlineField("Price", price.ToString() + " ether");
            Color color = Color.Default;
            switch (clazz)
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
                    color = new Color(255, 182, 193);
                    break;
                case "reptile":
                    color = Color.Magenta;
                    break;
            }
            embed.WithColor(color);
            embed.WithThumbnailUrl(imageUrl);
            embed.WithUrl("https://axieinfinity.com/axie/" + id.ToString());
            return embed;
        }
        public int GetPureness()
        {
            int pureness = 0;
            if (parts.ears.Class == clazz) pureness++;
            if (parts.tail.Class == clazz) pureness++;
            if (parts.horn.Class == clazz) pureness++;
            if (parts.back.Class == clazz) pureness++;
            if (parts.eyes.Class == clazz) pureness++;
            if (parts.mouth.Class == clazz) pureness++;
            return pureness++;
        }
        public int GetDPR()
        {
            int dpr = 0;
            dpr += parts.back.moves[0].attack * parts.back.moves[0].accuracy / 100;
            dpr += parts.mouth.moves[0].attack * parts.mouth.moves[0].accuracy / 100;
            dpr += parts.horn.moves[0].attack * parts.horn.moves[0].accuracy / 100;
            dpr += parts.tail.moves[0].attack * parts.tail.moves[0].accuracy / 100;
            return dpr;
        }


        public float GetTNK()
        {
            float tnk = stats.hp;
            tnk += parts.back.moves[0].defense;
            tnk += parts.mouth.moves[0].defense;
            tnk += parts.horn.moves[0].defense;
            tnk += parts.tail.moves[0].defense;
            return tnk;
        }

        public int GetTNKScore()
        {
            float tnk = GetTNK();
            float minTnk = GetMinTNK();
            return (int)Math.Floor((tnk - minTnk) / (GetMaxTNK() - minTnk) * 100);
        }

        public static float GetMaxDPR() => 91.5f;
        public static float GetMaxTNK() => 129f;
        public static float GetMinTNK() => 33;
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
        public string Class;
        public string type;
        public bool mystic;
        public PartMove[] moves;
    }

    public class PartMove
    {
        public string id;
        public string name;
        public string type;
        public int attack;
        public int defense;
        public int accuracy;
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
