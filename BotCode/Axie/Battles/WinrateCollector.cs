using System;
using System.Threading;
using Discord;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using DiscordBot.Axie;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DiscordBot.Mongo;
using System.Text;
using MongoDB.Driver.Core;


namespace DiscordBot.Axie.Battles
{
    class WinrateCollector
    {
        public static bool IsDbSyncing = false;
        public static int apiPerc = 0;
        public static int dbPerc = 0;
        public static int lastUnixTimeCheck = 0;
        public static readonly int unixTimeBetweenUpdates = 21600;
        public static void GetAllData()
        {
            Dictionary<int, Winrate> winrateData = new Dictionary<int, Winrate>();
            List<AxieWinrate> winrateList = new List<AxieWinrate>();
            int battleCount = 29950;
            int axieIndex = 0;
            int safetyNet = 0;
            int perc = battleCount / 100;
            int currentPerc = 0;
            while (axieIndex < battleCount)
            {
                axieIndex++;
                if (axieIndex % perc == 0)
                {
                    currentPerc++;
                    Console.WriteLine(currentPerc.ToString() + "%");
                }
                string json = null;
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        json = wc.DownloadString("https://api.axieinfinity.com/v1/battle/history/matches/" + axieIndex.ToString());
                        safetyNet = 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        safetyNet++;
                    }
                }
                if (json != null)
                {
                    JObject axieJson = JObject.Parse(json);
                    JObject script = JObject.Parse((string)axieJson["script"]);
                    int[] team1 = new int[3];
                    int[] team2 = new int[3];
                    for (int i = 0; i < 3; i++)
                    {
                        team1[i] = (int)script["metadata"]["fighters"][i]["id"];
                        team2[i] = (int)script["metadata"]["fighters"][i + 3]["id"];
                    }
                    int winningAxie = (int)script["result"]["lastAlive"][0];
                    int[] winningTeam;
                    int[] losingTeam;
                    if (team1.Contains(winningAxie))
                    {
                        winningTeam = team1;
                        losingTeam = team2;
                    }
                    else
                    {
                        losingTeam = team1;
                        winningTeam = team2;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        var winner = winrateList.FirstOrDefault(a => a.id == winningTeam[i]);
                        if (winner != null)
                        {
                            winner.win++;
                            winner.battleHistory += "1";
                        }
                        else winrateList.Add(new AxieWinrate(winningTeam[i], 1, 0, "0x1", 0));

                        var loser = winrateList.FirstOrDefault(a => a.id == losingTeam[i]);
                        if (loser != null)
                        {
                            loser.loss++;
                            loser.battleHistory += "0";
                        }
                        else winrateList.Add(new AxieWinrate(losingTeam[i], 0, 1, "0x0", 0));
                    }
                }
            }
            Console.WriteLine("Data Fetched. Initialising DB write phase");

            foreach (var axie in winrateList) axie.GetWinrate();
            var db = DatabaseConnection.GetDb();
            var collection = db.GetCollection<BsonDocument>("AxieWinrate");
            float percDB = (float)winrateList.Count / 100f;
            int counter = 0;
            int currentperc = 0;
            foreach (var axie in winrateList)
            {
                counter++;
                if (counter > perc)
                {
                    currentperc++;
                    counter = 0;
                    Console.WriteLine($"{currentperc}%");
                }
                collection.InsertOne(axie.ToBsonDocument());
            }
        }

        public static async Task FetchDataFromAddress(string address, IUserMessage message)
        {
            var listFromApi = await CollectionStatDataHandler.GetAxieListFromAddress(address);
            var db = DatabaseConnection.GetDb();
            var collection = db.GetCollection<BsonDocument>("AxieWinrate");
            string winrateAddressPath = "WinrateFromAddress.txt";
            var winrateList = new List<AxieWinrate>();
            foreach(var axie in listFromApi)
            {
                var filterId = Builders<BsonDocument>.Filter.Eq("_id", axie.id);
                var doc = collection.Find(filterId).FirstOrDefault();
                if (doc != null)
                {
                    winrateList.Add(BsonSerializer.Deserialize<AxieWinrate>(doc));
                }
            }
            winrateList = winrateList.OrderBy(ax => ax.id).ToList();
            string winrateData = JsonConvert.SerializeObject(winrateList, Formatting.Indented);  
            using (var tw = new StreamWriter(winrateAddressPath))
            {
                await tw.WriteAsync(winrateData);
            }
            await message.Channel.SendFileAsync(winrateAddressPath);
        }
        public static void UpdateUnixLastCheck()
        {
            using (StreamReader sr = new StreamReader("AxieData/LastTimeCheck.txt", Encoding.UTF8))
            {
                lastUnixTimeCheck = Convert.ToInt32(sr.ReadToEnd());
            }
        }
        public static async Task GetDataSinceLastChack()
        {
            IsDbSyncing = true;
            lastUnixTimeCheck = Convert.ToInt32(((DateTimeOffset)(DateTime.UtcNow)).ToUnixTimeSeconds());
            using (var tw = new StreamWriter("AxieData/LastTimeCheck.txt"))
            {
                tw.Write(lastUnixTimeCheck.ToString());
            }
            string dataCountUrl = "https://api.axieinfinity.com/v1/battle/history/matches-count";
            string battleNumberPath = "AxieData/LastCheck.txt";
            int lastChecked = 0;
            int lastBattle = 0;
            apiPerc = 0;
            int counter = 0;
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                lastBattle = Convert.ToInt32((await wc.DownloadStringTaskAsync(dataCountUrl)));
            }
            using (StreamReader sr = new StreamReader(battleNumberPath, Encoding.UTF8))
            {
                lastChecked = Convert.ToInt32(sr.ReadToEnd());
            }
            List<AxieWinrate> winrateList = new List<AxieWinrate>();
            int total = lastBattle - lastChecked;
            float perc = (float)total / 100;
            while (lastChecked < lastBattle)
            {
                lastChecked++;
                counter++;
                if(counter > perc)
                {
                    apiPerc++;
                    counter = 0;
                    Console.WriteLine($"{apiPerc}%");
                }
                string json = null;
                try
                {
                    using (System.Net.WebClient wc = new System.Net.WebClient())
                    {
                        json = wc.DownloadString("https://api.axieinfinity.com/v1/battle/history/matches/" + lastChecked.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                if (json != null)
                {
                    JObject axieJson = JObject.Parse(json);
                    JObject script = JObject.Parse((string)axieJson["script"]);
                    int[] team1 = new int[3];
                    int[] team2 = new int[3];
                    for (int i = 0; i < 3; i++)
                    {
                        team1[i] = (int)script["metadata"]["fighters"][i]["id"];
                        team2[i] = (int)script["metadata"]["fighters"][i + 3]["id"];
                    }
                    int winningAxie = (int)script["result"]["lastAlive"][0];
                    int[] winningTeam;
                    int[] losingTeam;
                    if (team1.Contains(winningAxie))
                    {
                        winningTeam = team1;
                        losingTeam = team2;
                    }
                    else
                    {
                        losingTeam = team1;
                        winningTeam = team2;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        var winner = winrateList.FirstOrDefault(a => a.id == winningTeam[i]);
                        if (winner != null)
                        {
                            winner.win++;
                            winner.battleHistory += "1";
                        }
                        else winrateList.Add(new AxieWinrate(winningTeam[i], 1, 0, "0x1", lastUnixTimeCheck));

                        var loser = winrateList.FirstOrDefault(a => a.id == losingTeam[i]);
                        if (loser != null)
                        {
                            loser.loss++;
                            loser.battleHistory += "0";
                        }
                        else winrateList.Add(new AxieWinrate(losingTeam[i], 0, 1, "0x0", lastUnixTimeCheck));
                    }
                }
            }
            foreach (var axie in winrateList) axie.GetWinrate();
            var db = DatabaseConnection.GetDb();
            var collection = db.GetCollection<BsonDocument>("AxieWinrate");
            Console.WriteLine("Initialising DB write phase");
            dbPerc = 0;
            perc = (float)winrateList.Count / 100;
            counter = 0;
            foreach (var axie in winrateList)
            {
                counter++;
                if (counter > perc)
                {
                    dbPerc++;
                    counter = 0;
                    Console.WriteLine($"{dbPerc}%");
                }
                var filterId = Builders<BsonDocument>.Filter.Eq("_id", axie.id);
                var doc = collection.Find(filterId).FirstOrDefault();
                if (doc != null)
                {
                    var axieData = BsonSerializer.Deserialize<AxieWinrate>(doc);
                    axieData.AddLatestResults(axie);
                    var update = Builders<BsonDocument>.Update
                                                       .Set("win", axieData.win).Set("loss", axieData.loss)
                                                       .Set("winrate", axieData.winrate)
                                                       .Set("battleHistory", axieData.battleHistory)
                                                       .Set("lastBattleDate", axieData.lastBattleDate);
                    collection.UpdateOne(filterId, update);
                }
                else collection.InsertOne(axie.ToBsonDocument());             
            }
            using (var tw = new StreamWriter(battleNumberPath))
            {
                tw.Write((lastBattle - 1).ToString());
            }
            IsDbSyncing = false;
        }
    
        public static EmbedBuilder GetTop10LeaderBoard(List<AxieWinrate> list, int mysticCount)
        {
            var embed = new EmbedBuilder();
            string mult = "";
            switch(mysticCount)
            {
                case 1:
                    mult = "Single";
                    break;
                case 2:
                    mult = "Double";
                    break;
                case 3:
                    mult = "Triple";
                    break;
                case 4:
                    mult = "Quad";
                    break;
            }
            embed.WithTitle(mult + " mystic leaderboard");
            embed.WithDescription("Axies that haven't fought within the last 4 days will be removed from the leaderboard.");
            int size = 0;
            if (mysticCount == 4) size = list.Count;
            else size = 10;
            for (int i = 0; i < size; i++)
            {
                embed.AddField($"#{i + 1}", $" [Axie#{list[i].id}](https://axieinfinity.com/axie/{list[i].id})  Total battles : {list[i].win + list[i].loss} | Win rate : {list[i].winrate}% ");
            }

            embed.WithColor(Color.DarkPurple);
            return embed;

        }
    }
}
