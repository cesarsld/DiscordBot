﻿using System;
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

        public static void GetAllData()
        {
            Dictionary<int, Winrate> winrateData = new Dictionary<int, Winrate>();
            List<AxieWinrate> winrateList = new List<AxieWinrate>();
            int battleCount = 25000;
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
                            winner.win++;
                        else winrateList.Add(new AxieWinrate(winningTeam[i], 1, 0));

                        var loser = winrateList.FirstOrDefault(a => a.id == losingTeam[i]);
                        if (loser != null)
                            loser.loss++;
                        else winrateList.Add(new AxieWinrate(losingTeam[i], 0, 1));
                    }
                }
            }
            Console.WriteLine("Data Fetched. Initialising DB write phase");

            foreach (var axie in winrateList) axie.GetWinrate();
            var db = DatabaseConnection.GetDb();
            var collection = db.GetCollection<BsonDocument>("AxieWinrate");
            foreach (var axie in winrateList)
            {
                collection.InsertOne(axie.ToBsonDocument());
            }
        }

        public static void GetDataSinceLastChack()
        {
            string dataCountUrl = "https://api.axieinfinity.com/v1/battle/history/matches-count";
            string battleNumberPath = "AxieData/LastBattleNumberCheck.text";
            int lastChecked = 0;
            int lastBattle = 0;
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                lastBattle = Convert.ToInt32(wc.DownloadString(dataCountUrl));
            }
            using (StreamReader sr = new StreamReader(battleNumberPath, Encoding.UTF8))
            {
                lastChecked = Convert.ToInt32(sr.ReadToEnd());
            }
            List<AxieWinrate> winrateList = new List<AxieWinrate>();

            while (lastChecked < lastBattle)
            {
                lastChecked++;
                string json = null;
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                     json = wc.DownloadString("https://api.axieinfinity.com/v1/battle/history/matches/" + lastChecked.ToString());
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
                            winner.win++;
                        else winrateList.Add(new AxieWinrate(winningTeam[i], 1, 0));

                        var loser = winrateList.FirstOrDefault(a => a.id == losingTeam[i]);
                        if (loser != null)
                            loser.loss++;
                        else winrateList.Add(new AxieWinrate(losingTeam[i], 0, 1));

                    }
                }
            }
            foreach (var axie in winrateList) axie.GetWinrate();
            var db = DatabaseConnection.GetDb();
            var collection = db.GetCollection<BsonDocument>("AxieWinrate");
            foreach (var axie in winrateList)
            {
                var filterId = Builders<BsonDocument>.Filter.Eq("_id", axie.id);
                var doc = collection.Find(filterId).FirstOrDefault();
                var axieData = BsonSerializer.Deserialize<AxieWinrate>(doc);
                axieData.AddLatestResults(axie);
                var update = Builders<BsonDocument>.Update.Set("win", axieData.win).Set("loss", axieData.loss).Set("winrate", axieData.winrate);
                collection.UpdateOne(filterId, update);
            }
        }
    }
}