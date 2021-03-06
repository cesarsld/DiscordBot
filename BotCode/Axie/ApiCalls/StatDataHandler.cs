﻿using System;
using System.Threading;
using Discord;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using DiscordBot.Axie.Breeding;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using DiscordBot.Mongo;
using System.IO;
using System.Text;

namespace DiscordBot.Axie.ApiCalls
{
    public class TraitMap
    {
        public int partGroup;
        public string traitCode;
        public string classCode;

        public TraitMap()
        { }
        public TraitMap(int group, string trait, string _class)
        {
            partGroup = group;
            traitCode = trait;
            classCode = _class;
        }
    }

    public class ShapeMap
    {
        public int partGroup;
        public string shapeCode;

        public ShapeMap()
        { }
        public ShapeMap(int group, string shape)
        {
            partGroup = group;
            shapeCode = shape;
        }
    }

    public class StatDataHandler
    {

        private static async Task<int> GetAxieCount()
        {
            var json = "";
            //https://axieinfinity.com/api/axies
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    json = await wc.DownloadStringTaskAsync("https://axieinfinity.com/api/axies"); //https://axieinfinity.com/api/axies/ || https://api.axieinfinity.com/v1/axies/

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            JObject axieObj = JObject.Parse(json);
            return (int)axieObj["totalAxies"];
        }

        public static void GetData()
        {
            Rank[] rankingList = new Rank[100];
            for (int i = 0; i < rankingList.Length; i++)
            {
                rankingList[i] = new Rank(i + 1);
            }
            bool dataAvailable = true;
            int axieCount = 5664;
            int axieIndex = 0;
            int safetyNet = 0;
            int perc = axieCount / 100;
            int currentPerc = 0;
            while (axieIndex < axieCount && dataAvailable)
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
                        json = wc.DownloadString("https://api.axieinfinity.com/v1/axies/" + axieIndex.ToString());
                        safetyNet = 0;
                    }
                    catch (Exception ex)
                    {
                        safetyNet++;
                        if (safetyNet > 25)
                        {
                            //dataAvailable = false;
                            axieIndex++;
                        }
                        axieIndex--;
                        //Console.WriteLine(ex.Message);
                    }
                }
                if (json != null)
                {
                    JObject axieJson = JObject.Parse(json);
                    AxieObject axieData = axieJson.ToObject<AxieObject>();
                    axieData.jsonData = axieJson;
                    if (axieData.stage <= 2)
                    {
                        Console.WriteLine("Axie still egg, moving on.");
                        continue;
                    }
                    int dpsScore = (int)Math.Floor(axieData.GetDPR(true) / AxieObject.GetMaxDPR() * 100);
                    if (dpsScore == 0) dpsScore++;
                    try
                    {
                        rankingList[100 - dpsScore].axies.Add(axieData.id);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("oops");
                    }

                }
            }
            string data = JsonConvert.SerializeObject(rankingList, Formatting.Indented);
            string path = "DpsRankingList.txt";
            if (!File.Exists(path))
            {
                File.Create(path);
                using (var tw = new StreamWriter(path))
                {
                    tw.Write(data);
                }
            }
            else if (File.Exists(path))
            {
                using (var tw = new StreamWriter(path))
                {
                    tw.Write(data);
                }
            }
        }

        public static void TankAndDpsLists(string address)
        {
            Rank[] dpsRankingList = new Rank[100];
            Rank[] tankRankingList = new Rank[100];
            var maxDict = new Dictionary<int, List<int>>();
            var minDict = new Dictionary<int, List<int>>();
            for (int i = 0; i < dpsRankingList.Length; i++)
            {
                dpsRankingList[i] = new Rank(i + 1);
                tankRankingList[i] = new Rank(i + 1);
            }
            bool dataAvailable = true;
            bool setupDone = false;
            int axiePages = 9999;
            int axieIndex = 0;
            int safetyNet = 0;
            int perc = axiePages / 100;
            while (axieIndex < axiePages && dataAvailable)
            {

                Console.WriteLine($"Page {axieIndex} out of {axiePages}");

                string json = null;
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        json = wc.DownloadString("https://axieinfinity.com/api/addresses/" + address + "/axies?offset=" + (12 * axieIndex).ToString());
                        safetyNet = 0;
                    }
                    catch (Exception ex)
                    {
                        safetyNet++;
                        if (safetyNet > 25)
                        {
                            //dataAvailable = false;
                            axieIndex++;
                        }
                        axieIndex--;
                        //Console.WriteLine(ex.Message);
                    }
                }
                axieIndex++;
                if (json != null)
                {
                    JObject addressJson = JObject.Parse(json);
                    if (!setupDone)
                    {
                        axiePages = (int)addressJson["totalPages"];
                        setupDone = true;
                    }
                    foreach (var axie in addressJson["axies"])
                    {
                        AxieDataOld axieData = axie.ToObject<AxieDataOld>();
                        axieData.jsonData = JObject.Parse(axie.ToString());
                        if (axieData.stage <= 2)
                        {
                            Console.WriteLine("Axie still egg, moving on.");
                            continue;
                        }
                        int maxDpsScore = axieData.GetDPRScore(true);
                        int mindDpsScore = axieData.GetDPRScore(false);
                        if (maxDict.ContainsKey(maxDpsScore))
                            maxDict[maxDpsScore].Add(axieData.id);
                        else
                            maxDict.Add(maxDpsScore, new List<int> { axieData.id });

                        if (minDict.ContainsKey(mindDpsScore))
                            minDict[mindDpsScore].Add(axieData.id);
                        else
                            minDict.Add(mindDpsScore, new List<int> { axieData.id });


                    }
                }
            }
            var list = maxDict.ToList().OrderByDescending(a => a.Key).ToList();
            var minList = minDict.ToList().OrderByDescending(a => a.Key).ToList();
            string dpsData = JsonConvert.SerializeObject(list, Formatting.Indented);
            string tankData = JsonConvert.SerializeObject(minList, Formatting.Indented);


            string dpsPath = "CocoDpsRankingList.txt";
            string tankPath = "CocoTankRankingList.txt";
            if (File.Exists(dpsPath))
            {
                using (var tw = new StreamWriter(dpsPath))
                {
                    tw.Write(dpsData);
                }
            }
            if (File.Exists(tankPath))
            {
                using (var tw = new StreamWriter(tankPath))
                {
                    tw.Write(tankData);
                }
            }
        }

        public static async Task<List<AxieDataOld>> GetAxieListFromAddress(string address)
        {
            var axieList = new List<AxieDataOld>();
            bool dataAvailable = true;
            bool setupDone = false;
            int axiePages = 9999;
            int axieIndex = 0;
            int safetyNet = 0;
            int perc = axiePages / 100;
            while (axieIndex < axiePages && dataAvailable)
            {

                Console.WriteLine($"Page {axieIndex} out of {axiePages}");

                string json = null;
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        //var uri = new Uri("https://axieinfinity.com/api/addresses/" + address + "/axies?offset=" + (12 * axieIndex).ToString());
                        json = await wc.DownloadStringTaskAsync("http://axieinfinity.com/api/v2/addresses/" + address + "/axies?offset=" + (12 * axieIndex).ToString());
                        safetyNet = 0;
                    }
                    catch (Exception ex)
                    {
                        safetyNet++;
                        if (safetyNet > 25) axieIndex++;
                        axieIndex--;
                    }
                }
                axieIndex++;
                if (json != null)
                {
                    JObject addressJson = JObject.Parse(json);
                    if (!setupDone)
                    {
                        var axies = (int)addressJson["totalAxies"];
                        axiePages = (axies / 12) + (axies % 12 == 0 ? 0 : 1);
                        setupDone = true;
                    }
                    foreach (var axie in addressJson["axies"])
                    {
                        AxieDataOld axieData = new AxieDataOld();
                        if ((int)axie["stage"] < 4)
                        {
                            Console.WriteLine("Axie still egg, moving on.");
                            continue;
                        }
                        //try
                        //{
                            axieData = axie.ToObject<AxieDataOld>();
                        //}
                        //catch (Exception e)
                        //{ Console.WriteLine(e.Message); }
                        axieData.jsonData = JObject.Parse(axie.ToString());

                        axieList.Add(axieData);
                    }
                }
            }
            return axieList;
        }

        public static async Task<List<AxieDataOld>> GetAxieListFromMarketplace()
        {
            var axieList = new List<AxieDataOld>();
            bool dataAvailable = true;
            bool setupDone = false;
            int axiePages = 9999;
            int axieIndex = 0;
            int safetyNet = 0;
            int perc = axiePages / 100;
            while (axieIndex < axiePages && dataAvailable)
            {

                Console.WriteLine($"Page {axieIndex} out of {axiePages}");

                string json = null;
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        //var uri = new Uri("https://axieinfinity.com/api/addresses/" + address + "/axies?offset=" + (12 * axieIndex).ToString());
                        json = await wc.DownloadStringTaskAsync("https://axieinfinity.com/api/axies?breedable&language=en&offset=" + (12 * axieIndex).ToString() + "&sale=1&sorting=lowest_price");
                        safetyNet = 0;
                    }
                    catch (Exception ex)
                    {
                        safetyNet++;
                        if (safetyNet > 25) axieIndex++;
                        axieIndex--;
                    }
                }
                axieIndex++;
                if (json != null)
                {
                    JObject addressJson = JObject.Parse(json);
                    if (!setupDone)
                    {
                        axiePages = (int)addressJson["totalPages"];
                        setupDone = true;
                    }
                    foreach (var axie in addressJson["axies"])
                    {
                        AxieDataOld axieData = new AxieDataOld();
                        //try
                        //{
                        axieData = axie.ToObject<AxieDataOld>();
                        //}
                        //catch (Exception e)
                        //{ Console.WriteLine(e.Message); }
                        axieData.jsonData = JObject.Parse(axie.ToString());
                        if (axieData.stage <= 2)
                        {
                            Console.WriteLine("Axie still egg, moving on.");
                            continue;
                        }
                        axieList.Add(axieData);
                    }
                }
            }
            return axieList;
        }


        public static async Task<List<AxieMapping>> GetAxieListFromGlobal()
        {
            var axieList = new List<AxieMapping>();
            var axieCount = await GetAxieCount();
            int axiePages = axieCount / 12 + (axieCount % 12 == 0 ? 0 : 1);
            int total = axiePages;
            int perc = axiePages / 100;
            while (axiePages >= 0)
            {

                Console.WriteLine($"Page {total - axiePages} out of {total}");

                string json = null;
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        axiePages--;
                        //var uri = new Uri("https://axieinfinity.com/api/addresses/" + address + "/axies?offset=" + (12 * axieIndex).ToString());
                        json = await wc.DownloadStringTaskAsync("https://axieinfinity.com/api/axies?offset=" + (12 * axiePages).ToString());
                    }
                    catch (Exception ex)
                    {
                    }
                }
                if (json != null)
                {
                    JObject addressJson = JObject.Parse(json);

                    foreach (var axie in addressJson["axies"])
                    {
                        AxieDataOld axieData = new AxieDataOld();
                        //try
                        //{
                        axieData = axie.ToObject<AxieDataOld>();
                        //}

                        //catch (Exception e)
                        //{ Console.WriteLine(e.Message); }
                        axieData.jsonData = JObject.Parse(axie.ToString());
                        if (axieList.Exists(obj => obj.name == (string)axie["name"]))
                            axieList.Add(new AxieMapping((int)axie["id"], (string)axie["id"]));
                        else
                            axieList.Add(new AxieMapping((int)axie["id"], (string)axie["name"]));
                    }
                }
            }

            var collec = DatabaseConnection.GetDb().GetCollection<AxieMapping>("IdNameMapping");
            await collec.InsertManyAsync(axieList);

            return axieList;
        }

        public static async Task<List<AxieTeam>> GetTeamListFromAddress(string address)
        {
            var teamList = new List<AxieTeam>();
            bool dataAvailable = true;
            bool setupDone = false;
            int pages = 9999;
            int axieIndex = 0;
            int safetyNet = 0;

            while (axieIndex < pages && dataAvailable)
            {
                Console.WriteLine($"Page {axieIndex + 1} out of {pages}");
                string json = null;
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        //https://api.axieinfinity.com/v1/battle/teams?address=0xf521Bb7437bEc77b0B15286dC3f49A87b9946773&offset=0&count=500&no_limit=1
                        json = await wc.DownloadStringTaskAsync("https://api.axieinfinity.com/v1/battle/teams/?address=" + address + "&offset=" + (10 * axieIndex).ToString() + "&count=10");
                        safetyNet = 0;
                    }
                    catch (Exception ex)
                    {
                        safetyNet++;
                        if (safetyNet > 25) axieIndex++;
                        axieIndex--;
                    }
                }
                axieIndex++;
                if (json != null)
                {
                    JObject teamJson = JObject.Parse(json);
                    if (!setupDone)
                    {
                        int axieTeams = (int)teamJson["total"];
                        int remainder = axieTeams % 10;
                        if (remainder == 0) pages = axieTeams / 10;
                        else pages = axieTeams / 10 + 1;
                        setupDone = true;
                    }
                    foreach (var team in teamJson["teams"])
                    {
                        AxieTeam axieTeam = new AxieTeam();

                        axieTeam.name = (string)team["name"];
                        for (int i = 0; i < 3; i++)
                        {
                            var member = axieTeam.GetAxieByIndex(i);
                            member.id = (int)team["teamMembers"][i]["axieId"];
                        }
                        teamList.Add(axieTeam);
                    }
                }

            }
            return teamList;
        }

        public static async Task GetAxiesWithTraits(string trait, string address, IUserMessage message)
        {
            var axieList = await GetAxieListFromAddress(address);
            var traitMap = GetTraitMapping(trait);
            List<int> matchList = new List<int>();
            foreach (var axie in axieList)
            {
                var genome = PureBreeder.calcBinary(axie.genes);
                var subGroup = PureBreeder.GetSubGroups(genome);
                if (new AxieGeneData(subGroup).ContainsTrait(traitMap)) matchList.Add(axie.id);
            }
            Console.WriteLine("data ready");
            string data = JsonConvert.SerializeObject(matchList, Formatting.Indented);
            using (var tw = new StreamWriter("TraitMatchList.txt"))
            {
                tw.Write(data);
            }
            Console.WriteLine("Sending data");
            await message.Channel.SendFileAsync("TraitMatchList.txt");
        }

        public static async Task GetAxiesWithShape(string trait, IUserMessage message)
        {
            var axieList = await GetAxieListFromMarketplace();
            //var one = await AxieDataOld.GetAxieFromApi(17696);
            var shapeMap = GetShapeMapping(trait);
            List<int> matchList = new List<int>();
            //var genome = PureBreeder.calcBinary(one.genes);
            //var subGroup = PureBreeder.GetSubGroups(genome);
            //if (new AxieGeneData(subGroup).ContainsShape(shapeMap)) matchList.Add(one.id);
            foreach (var axie in axieList)
            {
                var genome = PureBreeder.calcBinary(axie.genes);
                var subGroup = PureBreeder.GetSubGroups(genome);
                if (new AxieGeneData(subGroup).ContainsShape(shapeMap)) matchList.Add(axie.id);
            }
            Console.WriteLine("data ready");
            string data = JsonConvert.SerializeObject(matchList, Formatting.Indented);
            using (var tw = new StreamWriter("TraitMatchList.txt"))
            {
                tw.Write(data);
            }
            Console.WriteLine("Sending data");
            await message.Channel.SendFileAsync("TraitMatchList.txt");
        }

        public static ShapeMap GetShapeMapping(string shape)
        {
            var json = "";
            using (StreamReader sr = new StreamReader("AxieData/ShapeMapping.txt", Encoding.UTF8))
            {
                json = sr.ReadToEnd();
            }
            JToken shapeMap = JToken.Parse(json);
            //var test = (string)shapeMap[shape.ToLower()];
            try
            {
                return new ShapeMap(1, (string)shapeMap[shape.ToLower()]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static TraitMap GetTraitMapping(string trait)
        {
            var json = "";
            using (StreamReader sr = new StreamReader("AxieData/TraitMapping.txt", Encoding.UTF8))
            {
                json = sr.ReadToEnd();
            }
            JToken traitsMap = JToken.Parse(json);
            bool traitFound = false;
            TraitMap traitMap = new TraitMap();
            foreach (var Class in traitsMap)
            {
                foreach (var part in Class.First)
                {
                    foreach (var codeName in part.First)
                    {
                        var nameObject = codeName.First["global"];
                        if (trait.ToLower() == ((string)nameObject).ToLower())
                        {
                            traitFound = true;
                            var jProps = codeName.ToObject<JProperty>();
                            traitMap.traitCode = jProps.Name;
                            break;
                        }
                    }
                    if (traitFound)
                    {
                        var jProps = part.ToObject<JProperty>();
                        string partTypeName = jProps.Name;
                        switch (partTypeName)
                        {
                            case "eyes":
                                traitMap.partGroup += 2;
                                break;
                            case "mouth":
                                traitMap.partGroup += 3;
                                break;
                            case "ears":
                                traitMap.partGroup += 4;
                                break;
                            case "horn":
                                traitMap.partGroup += 5;
                                break;
                            case "back":
                                traitMap.partGroup += 6;
                                break;
                            case "tail":
                                traitMap.partGroup += 7;
                                break;
                        }
                        break;
                    }
                }
                if (traitFound)
                {
                    var jProps = Class.ToObject<JProperty>();
                    string classTypeName = jProps.Name;
                    switch (classTypeName)
                    {
                        case "beast":
                            traitMap.classCode = "0000";
                            break;
                        case "bug":
                            traitMap.classCode = "0001";
                            break;
                        case "bird":
                            traitMap.classCode = "0010";
                            break;
                        case "plant":
                            traitMap.classCode = "0011";
                            break;
                        case "aquatic":
                            traitMap.classCode = "0100";
                            break;
                        case "reptile":
                            traitMap.classCode = "0101";
                            break;
                    }
                    break;
                }
            }
            return traitMap;
        }
    }

}
