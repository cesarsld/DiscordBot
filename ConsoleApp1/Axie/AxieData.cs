﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Discord;
using System.Numerics;

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
        public AxiePart[] OldParts;
        public AxieParts parts;
        
        public bool hasMystic
        {
            get
            {
                return parts.ears.mystic || parts.mouth.mystic || parts.horn.mystic || parts.tail.mystic || parts.eyes.mystic || parts.back.mystic;
            }
        }
        public int mysticCount
        {
            get
            {
                int count = 0;
                if (parts.ears.mystic) count++;
                if (parts.mouth.mystic) count++;
                if (parts.tail.mystic) count++;
                if (parts.eyes.mystic) count++;
                if (parts.back.mystic) count++;
                if (parts.horn.mystic) count++;
                return count;
            }
        }
        public int exp;
        public int pendingExp;
        public int level;
        public int stage;
        public AxieStats stats;
        public AxieAuction auction;
        public JObject jsonData;

        public EmbedBuilder EmbedAxieData(string extra)
        {
            string imageUrl = jsonData["figure"]["static"]["idle"].ToString();
            int pureness = GetPureness();



            var embed = new EmbedBuilder();
            embed.WithTitle(name);

            embed.AddInlineField("Class", Class.First().ToString().ToUpper() + Class.Substring(1) + $" ({pureness}/6)");
            embed.AddInlineField("Title", title == null ? "None" : title);
            embed.AddInlineField("Exp", exp + $" | Pending exp : {pendingExp}");
            embed.AddInlineField("Level", level);
            //embed.AddField("Owner", axieData.owner);
            embed.AddInlineField("HP", $" ({stats.hp})".PadLeft(5 + stats.hp / 2, '|'));
            embed.AddInlineField("Skill", $" ({stats.skill})".PadLeft(5 + stats.skill / 2, '|'));
            embed.AddInlineField("Speed", $" ({stats.speed})".PadLeft(5 + stats.speed / 2, '|'));

            embed.AddInlineField("Morale", $" ({stats.morale})".PadLeft(5 + stats.morale / 2, '|'));
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
            switch (Class)
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
            
            var embed = new EmbedBuilder();
            embed.WithTitle(name);
            embed.WithDescription("Has been sold!");
            embed.AddInlineField("Price", price.ToString() + " ether");
            Color color = Color.Default;
            switch (Class)
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
            embed.WithThumbnailUrl(GetImageUrl());
            embed.WithUrl("https://axieinfinity.com/axie/" + id.ToString());
            return embed;
        }
        public EmbedBuilder EmbedQQData(bool expAllowed)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(name);
            if (expAllowed)
                embed.WithDescription($"Exp : {exp} | Pending exp : {pendingExp}");
            embed.WithThumbnailUrl(GetImageUrl());
            embed.WithUrl("https://axieinfinity.com/axie/" + id.ToString());
            Color color = Color.Default;
            switch (Class)
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

        public int GetPureness()
        {
            int pureness = 0;
            if (parts.ears.Class == Class) pureness++;
            if (parts.tail.Class == Class) pureness++;
            if (parts.horn.Class == Class) pureness++;
            if (parts.back.Class == Class) pureness++;
            if (parts.eyes.Class == Class) pureness++;
            if (parts.mouth.Class == Class) pureness++;
            return pureness;
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
        public int GetDPRScore()
        {
            int dpr = GetDPR();
            return (int)Math.Floor(GetDPR() / GetMaxDPR() * 100);
        }

        public static float GetMaxDPR() => 91.5f;
        public static float GetMaxTNK() => 129f;
        public static float GetMinTNK() => 33;

        public string GetImageUrl()
        {
            try
            {
                return jsonData["figure"]["static"]["idle"].ToString();
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public static async Task<AxieData> GetAxieFromApi(int axieId)
        {
            string json = "";
            int downloadTries = 0;
            bool hasFetched = false;
            while (downloadTries < 5 && !hasFetched)
            {
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        json = await wc.DownloadStringTaskAsync("https://api.axieinfinity.com/v1/axies/" + axieId.ToString()); //https://axieinfinity.com/api/axies/ || https://api.axieinfinity.com/v1/axies/
                        hasFetched = true;
                    }

                    catch (Exception ex)
                    {
                        if (downloadTries == 5)
                        {
                            try
                            {
                                json = await wc.DownloadStringTaskAsync("https://axieinfinity.com/api/axies/" + axieId.ToString());
                                hasFetched = true;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(ex.ToString());
                                return null;
                            }
                        }
                        downloadTries++;
                        hasFetched = false;
                        continue;
                    }
                }
            }
            JObject axieJson = JObject.Parse(json);
            AxieData axieData = axieJson.ToObject<AxieData>();
            axieData.jsonData = axieJson;
            //axieData.FillBetterParts();
            return axieData;
        }
        private void FillBetterParts()
        {
            parts = new AxieParts();
            foreach (var parts in OldParts)
            {
                switch (parts.type)
                {
                    case "eyes":
                        this.parts.eyes = parts;
                        break;
                    case "horn":
                        this.parts.horn = parts;
                        break;
                    case "back":
                        this.parts.back = parts;
                        break;
                    case "tail":
                        this.parts.tail = parts;
                        break;
                    case "mouth":
                        this.parts.mouth = parts;
                        break;
                    case "ears":
                        this.parts.ears = parts;
                        break;
                }
            }
        }
        public async Task GetTrueAuctionData()
        {
            string json = "";
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    json = await wc.DownloadStringTaskAsync("https://axieinfinity.com/api/axies/" + id.ToString()); //https://axieinfinity.com/api/axies/
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }
            JObject axieJson = JObject.Parse(json);
            auction = axieJson["auction"].ToObject<AxieAuction>();
        }
    }


    public class AxieDataOld
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
        public AxieAuction auction;
        public JObject jsonData;


        public int GetPureness()
        {
            int pureness = 0;
            foreach (var part in parts)
                if (part.Class == Class) pureness++;
            return pureness;
        }

        public int GetAbsolutePureness()
        {
            int[] pureness = new int[6];
            for (int i = 0; i < pureness.Length; i++) pureness[i] = 0;
            foreach (var part in parts)
            {
                switch (part.Class)
                {
                    case "bird":
                        pureness[0]++;
                        break;
                    case "plant":
                        pureness[1]++;
                        break;
                    case "aquatic":
                        pureness[2]++;
                        break;
                    case "bug":
                        pureness[3]++;
                        break;
                    case "beast":
                        pureness[4]++;
                        break;
                    case "reptile":
                        pureness[5]++;
                        break;

                }
            }
            return pureness.Max();
        }

        public int GetDPR()
        {
            int dpr = 0;
            foreach (var part in parts)
            {
                if (part.type == "back" || part.type == "mouth" || part.type == "horn" || part.type == "tail")
                    dpr += part.moves[0].attack * part.moves[0].accuracy / 100;
            }
            return dpr;
        }
        public float GetTNK()
        {
            float tnk = stats.hp;
            foreach (var part in parts)
            {
                if (part.type == "back" || part.type == "mouth" || part.type == "horn" || part.type == "tail")
                    tnk += part.moves[0].defense;
            }
            return tnk;
        }
        public int GetTNKScore()
        {
            float tnk = GetTNK();
            float minTnk = GetMinTNK();
            return (int)Math.Floor((tnk - minTnk) / (GetMaxTNK() - minTnk) * 100);
        }
        public int GetDPRScore()
        {
            int dpr = GetDPR();
            return (int)Math.Floor(GetDPR() / GetMaxDPR() * 100);
        }

        public static float GetMaxDPR() => 91.5f;
        public static float GetMaxTNK() => 129f;
        public static float GetMinTNK() => 33;

        public string GetImageUrl()
        {
            try
            {
                return jsonData["figure"]["static"]["idle"].ToString();
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public static async Task<AxieData> GetAxieFromApi(int axieId)
        {
            string json = "";
            int downloadTries = 0;
            bool hasFetched = false;
            while (downloadTries < 5 && !hasFetched)
            {
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        json = await wc.DownloadStringTaskAsync("https://api.axieinfinity.com/v1/axies/" + axieId.ToString()); //https://axieinfinity.com/api/axies/ || https://api.axieinfinity.com/v1/axies/
                        hasFetched = true;
                    }

                    catch (Exception ex)
                    {
                        if (downloadTries == 5)
                        {
                            try
                            {
                                json = await wc.DownloadStringTaskAsync("https://axieinfinity.com/api/axies/" + axieId.ToString());
                                hasFetched = true;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(ex.ToString());
                                return null;
                            }
                        }
                        downloadTries++;
                        hasFetched = false;
                        continue;
                    }
                }
            }
            JObject axieJson = JObject.Parse(json);
            AxieData axieData = axieJson.ToObject<AxieData>();
            axieData.jsonData = axieJson;
            //axieData.FillBetterParts();
            return axieData;
        }
        public async Task GetTrueAuctionData()
        {
            string json = "";
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    json = await wc.DownloadStringTaskAsync("https://axieinfinity.com/api/axies/" + id.ToString()); //https://axieinfinity.com/api/axies/
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }
            JObject axieJson = JObject.Parse(json);
            auction = axieJson["auction"].ToObject<AxieAuction>();
        }
    }


    //support classes


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
        public PartEffect[] effects;

    }
    public class PartEffect
    {
        public string name;
        public string type;
        public string description;

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
    public class AxieAuction
    {
        public string type;
        public BigInteger startingPrice;
        public BigInteger endingPrice;
        public BigInteger buyNowPrice;
        public BigInteger suggestedPrice;
        public long startingTime;
        public long duration;
        public long timeLeft;
    }
    public class AxieImage
    {
        [JsonProperty(PropertyName = "")]
        public string png;
    }
}
