﻿using System;
using System.Threading;
using Discord;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using DiscordBot.Axie.ApiCalls;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DiscordBot.Mongo;
using System.Text;
using MongoDB.Driver.Core;
using System.Data;

namespace DiscordBot.Axie.Battles
{
    //https://api.axieinfinity.com/v1/battle/teams/?address=0x4ce15b37851a4448a28899062906a02e51dee267&offset=0&count=10

    class WinrateCollector
    {
        public static bool IsDbSyncing = false;
        public static int apiPerc = 0;
        public static int dbPerc = 0;
        public static int lastUnixTimeCheck = 0;
        public static readonly int unixTimeBetweenUpdates = 86400;
        private static int updateCount = 0;

        public static void GetUniquePlayers()
        {
            Dictionary<int, Winrate> winrateData = new Dictionary<int, Winrate>();
            List<AxieWinrate> winrateList = new List<AxieWinrate>();
            List<string> uniqueUsers = new List<string>();
            int timeCheck = 0;
            int battleCount = 78634;
            int axieIndex = 0;
            int safetyNet = 0;
            int perc = battleCount / 100;
            int currentPerc = 0;
            var db1 = DatabaseConnection.GetDb();
            var collection1 = db1.GetCollection<DailyUsers>("DailyBattleDAU");
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
                    int time = Convert.ToInt32(((string)axieJson["createdAt"]).Remove(((string)axieJson["createdAt"]).Length - 3, 3));
                    if (timeCheck == 0) timeCheck = time;
                    if (time - timeCheck > 86400)
                    {
                        Console.WriteLine("Day passed");
                        var dailyData = new DailyUsers(time, uniqueUsers.Count);
                        collection1.InsertOne(dailyData);
                        timeCheck += 86400;
                        uniqueUsers.Clear();
                    }
                    if (!uniqueUsers.Contains((string)axieJson["winner"])) uniqueUsers.Add((string)axieJson["winner"]);
                    if (!uniqueUsers.Contains((string)axieJson["loser"])) uniqueUsers.Add((string)axieJson["loser"]);
                }
            }
            
        }


        public static DataTable jsonStringToTable(string jsonContent)
        {
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(jsonContent);
            return dt;
        }
        //public static string jsonToCsv(string jsonContent, string delimiter)
        //{
        //    StringWriter csvString = new StringWriter();
        //    //using (var csv = new CsvWriter(csvString))
        //    //{
            
        //        csv.Configuration.SkipEmptyRecords = true;
        //        csv.Configuration.WillThrowOnMissingField = false;
        //        csv.Configuration.Delimiter = delimiter;

        //        using (var dt = jsonStringToTable(jsonContent))
        //        {
        //            foreach (DataColumn column in dt.Columns)
        //            {
        //                csv.WriteField(column.ColumnName);
        //            }
        //            csv.NextRecord();

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                for (var i = 0; i < dt.Columns.Count; i++)
        //                {
        //                    csv.WriteField(row[i]);
        //                }
        //                csv.NextRecord();
        //            }
        //        }
        //    //}
        //    return csvString.ToString();
        //}

        public static async Task FetchDataFromAddress(string address, IUserMessage message)
        {
            var listFromApi = await StatDataHandler.GetAxieListFromAddress(address);
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
            //var dddd = CsvSerializer.SerializeToCsv<AxieWinrate>(winrateList);
            //var euh = winrateList.tocs
            using (var tw = new StreamWriter(winrateAddressPath))
            {
                await tw.WriteAsync(winrateData);
            }
            await message.Channel.SendFileAsync(winrateAddressPath);
        }
        public static async Task FetchTeamDataFromAddress(string address, IUserMessage message)
        {
            var teamList = await StatDataHandler.GetTeamListFromAddress(address);
            var db = DatabaseConnection.GetDb();
            var collection = db.GetCollection<BsonDocument>("AxieWinrate");
            string winrateAddressPath = "TeamWinrateFromAddress.txt";
            foreach (var team in teamList)
            {
                for(int i = 0; i < 3; i++)
                {
                    var member = team.GetAxieByIndex(i);
                    var filterId = Builders<BsonDocument>.Filter.Eq("_id", member.id);
                    var doc = collection.Find(filterId).FirstOrDefault();
                    if (doc != null)
                    {
                        team.teamMembers[i] = BsonSerializer.Deserialize<AxieWinrate>(doc);
                    }
                }
            }
            var reducedList = new List<AxieTeamReduced>();
            foreach (var team in teamList)
            {
                reducedList.Add(new AxieTeamReduced(team));
            }
            string teamData = JsonConvert.SerializeObject(reducedList, Formatting.Indented);
            using (var tw = new StreamWriter(winrateAddressPath))
            {
                await tw.WriteAsync(teamData);
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

        private static async Task GetDailyPlayers(List<AxieWinrate> list)
        {
            List<string> uniquePlayers = new List<string>();
            foreach (var axie in list)
            {
                var data = await AxieObject.GetAxieFromApi(axie.id);
                if (!uniquePlayers.Contains(data.owner)) uniquePlayers.Add(data.owner);
            }
            var dailyData = new DailyUsers(Convert.ToInt32(((DateTimeOffset)(DateTime.UtcNow)).ToUnixTimeSeconds()), uniquePlayers.Count);
            var db = DatabaseConnection.GetDb();
            var collection = db.GetCollection<DailyUsers>("DailyBattleDAU");
            collection.InsertOne(dailyData);
        }

        public static EmbedBuilder GetTop10LeaderBoard(List<AxieWinrate> list, int mysticCount)
        {
            var embed = new EmbedBuilder();
            string mult = "";
            switch(mysticCount)
            {
                case 0:
                    mult = "Non";
                    break;
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
            if (list.Count < 10) size = list.Count;
            else size = 10;
            for (int i = 0; i < size; i++)
            {
                embed.AddField($"#{i + 1}", $" [Axie#{list[i].id}](https://axieinfinity.com/axie/{list[i].id})  Total battles : {list[i].win + list[i].loss} | Win rate : {list[i].winrate}% ");
            }

            embed.WithColor(Color.DarkPurple);
            return embed;

        }

        public static EmbedBuilder GetTop10LeaderBoardLatest(List<AxieWinrate> list, int mysticCount)
        {
            var embed = new EmbedBuilder();
            string mult = "";
            switch (mysticCount)
            {
                case 0:
                    mult = "Non";
                    break;
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
            embed.WithTitle(mult + " mystic leaderboard - Last 100 battles");
            embed.WithDescription("Axies that haven't fought within the last 4 days will be removed from the leaderboard.");
            int size = 0;
            if (list.Count < 10) size = list.Count;
            else size = 10;
            for (int i = 0; i < size; i++)
            {
                embed.AddField($"#{i + 1}", $" [Axie#{list[i].id}](https://axieinfinity.com/axie/{list[i].id})  Total battles : {list[i].GetRecentWins() + list[i].GetRecentLosses()} | Win rate : {list[i].GetRecentWinrate()}% ");
            }

            embed.WithColor(Color.DarkPurple);
            return embed;

        }

    }
}
