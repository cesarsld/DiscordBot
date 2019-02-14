using System;
using System.Collections.Generic;
using System.Linq;
using DiscordBot.Axie.Battles;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Discord;
using DiscordBot.Axie.Web3Axie;
using System.Numerics;
using System.Text;

namespace DiscordBot
{
    public class AxieMapping
    {
        public int id;
        public string name;
        public AxieMapping(int _id, string _name)
        {
            id = _id;
            name = _name;
        }
    }

    public class AxieObject
    {
        public int id;
        public string name;
        public string owner;
        public string genes;
        public string Class;
        public string title;
        public int sireId;
        public int matronId;
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
        public JObject oldjsonData;

        public EmbedBuilder EmbedAxieData(string extra)
        {
            string imageUrl = jsonData["figure"]["static"]["idle"].ToString();
            int pureness = GetPureness();



            var embed = new EmbedBuilder();
            embed.WithTitle(name);

            embed.AddField("Class", Class.First().ToString().ToUpper() + Class.Substring(1) + $" ({pureness}/6)", true);
            embed.AddField("Title", title == null ? "None" : title, true);
            embed.AddField("Exp", exp + $" | Pending exp : {pendingExp}", true);
            embed.AddField("Level", level, true);
            embed.AddField("HP", $" ({stats.hp})".PadLeft(5 + stats.hp / 2, '|'), true);
            embed.AddField("Skill", $" ({stats.skill})".PadLeft(5 + stats.skill / 2, '|'), true);
            embed.AddField("Speed", $" ({stats.speed})".PadLeft(5 + stats.speed / 2, '|'), true);

            embed.AddField("Morale", $" ({stats.morale})".PadLeft(5 + stats.morale / 2, '|'), true);
            if (extra != null && extra == "m")
            {
                embed.WithDescription("Disclaimer : DPS and Tank ratings are not official nor endorsed by the Axie team.");
                embed.AddField("DPS Score", (int)Math.Floor(GetDPR() / GetMaxDPR() * 100), true);
                embed.AddField("Tank Score", GetTNKScore(), true);
            }
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
                    color = new Color(255, 182, 193);
                    break;
                case "reptile":
                    color = Color.Magenta;
                    break;
                case "hidden_1":
                    color = new Color(224, 209, 216);
                    break;
                case "hidden_2":
                    color = new Color(153, 204, 255);
                    break;
                case "hidden_3":
                    color = new Color(0, 102, 153);
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
            embed.AddField("Price", price.ToString() + " ether", true);
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
                case "hidden_1":
                    color = new Color(224, 209, 216);
                    break;
                case "hidden_2":
                    color = new Color(153, 204, 255);
                    break;
                case "hidden_3":
                    color = new Color(0, 102, 153);
                    break;
            }
            embed.WithColor(color);
            embed.WithThumbnailUrl(GetImageUrl());
            embed.WithUrl("https://axieinfinity.com/axie/" + id.ToString());
            return embed;
        }
        public EmbedBuilder EmbedBaseData(bool expAllowed)
        {
            string json = "";
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    json = wc.DownloadString("https://axieinfinity.com/api/axies/" + id.ToString()); //https://axieinfinity.com/api/axies/
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            JObject axieJson = JObject.Parse(json);
            oldjsonData = axieJson;
            var embed = new EmbedBuilder();
            embed.WithTitle($"Axie #{id}");
            if (expAllowed)
                embed.WithDescription($"Exp : " + axieJson["exp"] + $" | Pending exp : {pendingExp - (AxieDataGetter.GetSyncedExp(id).GetAwaiter().GetResult())}");
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
                    color = new Color(255, 182, 193);
                    break;
                case "reptile":
                    color = Color.Magenta;
                    break;
                case "hidden_1":
                    color = new Color(224, 209, 216);
                    break;
                case "hidden_2":
                    color = new Color(153, 204, 255);
                    break;
                case "hidden_3":
                    color = new Color(0, 102, 153);
                    break;
            }
            embed.WithColor(color);
            return embed;
        }
        public EmbedBuilder EmbedBaseDataLarge(bool expAllowed)
        {
            string json = "";
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    json = wc.DownloadString("https://axieinfinity.com/api/axies/" + id.ToString()); //https://axieinfinity.com/api/axies/
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            JObject axieJson = JObject.Parse(json);
            oldjsonData = axieJson;
            var embed = new EmbedBuilder();
            embed.WithTitle($"Axie #{id}");
            embed.WithImageUrl(GetImageUrl());
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
                    color = new Color(255, 182, 193);
                    break;
                case "reptile":
                    color = Color.Magenta;
                    break;
                case "hidden_1":
                    color = new Color(224, 209, 216);
                    break;
                case "hidden_2":
                    color = new Color(153, 204, 255);
                    break;
                case "hidden_3":
                    color = new Color(0, 102, 153);
                    break;
            }
            embed.WithColor(color);
            return embed;
        }

        public EmbedBuilder EmbedWinrate(AxieWinrate winRate)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle($"Axie #{id}");
            embed.WithDescription($"Total battles : {winRate.win + winRate.loss} | Win rate : {winRate.winrate}%");
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
                    color = new Color(255, 182, 193);
                    break;
                case "reptile":
                    color = Color.Magenta;
                    break;
                case "hidden_1":
                    color = new Color(224, 209, 216);
                    break;
                case "hidden_2":
                    color = new Color(153, 204, 255);
                    break;
                case "hidden_3":
                    color = new Color(0, 102, 153);
                    break;
            }
            embed.WithColor(color);
            return embed;
        }
        public EmbedBuilder EmbedGetQuickBattleLogs(AxieWinrate winRate)
        {
            StringBuilder sbWin = new StringBuilder();
            StringBuilder sbLoss = new StringBuilder();
            var counter = 0;
            foreach (var win in winRate.wonBattles.OrderByDescending(a => a))
            {
                sbWin.Append($"[Battle #{win}](https://axieinfinity.com/battle/" + win.ToString() + ") \n");
                counter++;
                if (counter > 15) break;
            }
            counter = 0;
            foreach (var loss in winRate.lostBattles.OrderByDescending(a => a))
            {
                sbLoss.Append($"[Battle #{loss}](https://axieinfinity.com/battle/" + loss.ToString() + ") \n");
                counter++;
                if (counter > 15) break;
            }

            var embed = new EmbedBuilder();
            embed.WithTitle($"Axie #{id}");
            embed.WithDescription("Battle logs");
            embed.AddField("Last won battles", sbWin, true);
            embed.AddField("Last lost battles", sbLoss, true);
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
                case "hidden_1":
                    color = new Color(224, 209, 216);
                    break;
                case "hidden_2":
                    color = new Color(153, 204, 255);
                    break;
                case "hidden_3":
                    color = new Color(0, 102, 153);
                    break;
            }
            embed.WithColor(color);
            embed.WithThumbnailUrl(GetImageUrl());
            embed.WithUrl("https://axieinfinity.com/axie/" + id.ToString());
            return embed;
        }
        public EmbedBuilder EmbedWinrateRecent(AxieWinrate winRate, int historyLength)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle($"Axie #{id}");
            var index = winRate.battleHistory.Length - 2 - historyLength;
            if (index < 0) index = 0;
            var history = winRate.battleHistory.Substring(2 + index);
            var perfWinrate = (float)history.Count(c => c=='1') / (history.Count(c => c == '1') + history.Count(c => c == '0')) * 100;
            embed.WithDescription($" win rate from the last {historyLength} battles: {perfWinrate}%");
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
                    color = new Color(255, 182, 193);
                    break;
                case "reptile":
                    color = Color.Magenta;
                    break;
                case "hidden_1":
                    color = new Color(224, 209, 216);
                    break;
                case "hidden_2":
                    color = new Color(153, 204, 255);
                    break;
                case "hidden_3":
                    color = new Color(0, 102, 153);
                    break;
            }
            embed.WithColor(color);
            return embed;
        }
        public EmbedBuilder EmbedBreedData(int breedCount)
        {
            var embed = EmbedBaseData(false);
            string qual = "";
            switch (breedCount)
            {
                case 0:
                    qual = "Virgin!";
                    break;
                case 1:
                    qual = "Experienced!";
                    break;
                case 2:
                    qual = "Bunny!";
                    break;
                case 3:
                    qual = "Escort!";
                    break;
                case 4:
                    qual = "Mature!";
                    break;
                case 5:
                    qual = "Granny!";
                    break;
                case 6:
                    qual = "Ancient!";
                    break;
                case 7:
                    qual = "Sterile!";
                    break;

            }
            embed.WithTitle(qual);
            embed.WithDescription($"Breed  count : {breedCount}");
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
        public static async Task<AxieObject> GetAxieFromApi(int axieId)
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
            try
            {
                JObject axieJson = JObject.Parse(json);
                AxieObject axieData = axieJson.ToObject<AxieObject>();
                axieData.jsonData = axieJson;
                return axieData;
            }
            catch (Exception e)
            {
                return new AxieObject { id = 0};
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
        public async Task<int> GetTrueBeedData()
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

                }
            }
            JObject axieJson = JObject.Parse(json);
            return (int)axieJson["expForBreeding"];
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
        public int sireId;
        public int matronId;
        public int? expForBreeding;
        public int stage;
        public AxieStats stats;
        public AxieAuction auction;
        public JObject jsonData;


        public bool hasMystic
        {
            get
            {
                foreach (var part in parts)
                    if (part.mystic)
                        return true;
                return false;
            }
        }
        public int mysticCount
        {
            get
            {
                int count = 0;
                foreach (var part in parts)
                    if (part.mystic)
                        count++;

                return count;
            }
        }

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

        public async Task<bool> CanBreed()
        {
            var syncedExp = await AxieDataGetter.GetSyncedExp(id);
            var v1Api = await AxieObject.GetAxieFromApi(id);
            int totalExp = exp + v1Api.pendingExp - syncedExp;
            return totalExp >= expForBreeding;
        }

        public int GetDPR(bool isMax)
        {
            int dpr = 0;
            foreach (var part in parts)
            {
                if (part.type == "back" || part.type == "mouth" || part.type == "horn" || part.type == "tail")
                    dpr += part.moves[0].attack * part.moves[0].accuracy / 100;
            }
            if(isMax)
                dpr = Convert.ToInt32(dpr * ( ((float)100 + ((float)stats.morale / 2 - (float)33 / 4)) / 100f));
            else
                dpr = Convert.ToInt32(dpr * (((float)100 + ((float)stats.morale / 2 - (float)61 / 4)) / 100f));
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
        public int GetDPRScore(bool isMax)
        {
            int dpr = GetDPR(isMax);
            return (int)GetDPR(isMax);
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
        public static async Task<AxieDataOld> GetAxieFromApi(int axieId)
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
                        json = await wc.DownloadStringTaskAsync("https://axieinfinity.com/api/axies/" + axieId.ToString()); //https://axieinfinity.com/api/axies/ || https://api.axieinfinity.com/v1/axies/
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
            AxieDataOld axieData = axieJson.ToObject<AxieDataOld>();
            axieData.jsonData = axieJson;
            return axieData;
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
