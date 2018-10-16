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


namespace DiscordBot.Axie
{
    public class TraitInfo
    {
        public int Usage = 0;
        public int WinDiff = 0;
        public TraitInfo(int gain)
        {
            Usage = 1;
            WinDiff = gain;
        }
    }

    class FightDataHandler
    {


        public static void GetData()
        {
            Dictionary<string, TraitInfo> traitData = new Dictionary<string, TraitInfo>();
            bool dataAvailable = true;
            int battleCount = 12500;
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
                        json = wc.DownloadString("https://api.axieinfinity.com/v1/battle/history/matches/" + axieIndex.ToString() );
                        safetyNet = 0;
                    }
                    catch (Exception ex)
                    {
                        safetyNet++;
                        if (safetyNet > 10) dataAvailable = false;
                        //Console.WriteLine(ex.Message);
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
                    for (int j = 0; j < 6; j++)
                    {
                        int id = (int)script["metadata"]["fighters"][j]["id"];
                        int gain = 0;
                        if (winningTeam.Contains(id)) gain = 1;
                        else gain = -1;
                        for (int k = 0; k < 4; k++)
                        {
                            string partmove = (string)script["metadata"]["fighters"][j]["moveSet"][k]["part"]["name"];
                            if (traitData.ContainsKey(partmove))
                            {
                                traitData[partmove].Usage++;
                                traitData[partmove].WinDiff += gain;
                            }
                            else
                            {
                                traitData.Add(partmove, new TraitInfo(gain));
                            }
                        }
                    }
                }
            }
            List<KeyValuePair<string, TraitInfo>> transferList = traitData.ToList();
            transferList.Sort((pair1, pair2) => pair2.Value.Usage.CompareTo(pair1.Value.Usage));
            traitData = transferList.ToDictionary(name => name.Key, _data => _data.Value);
            string data = JsonConvert.SerializeObject(traitData, Formatting.Indented);
            string path = "TraitData.txt";
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

    }

    public class StatDataHandler
    {
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
                    AxieData axieData = axieJson.ToObject<AxieData>();
                    axieData.jsonData = axieJson;
                    if (axieData.stage <= 2)
                    {
                        Console.WriteLine("Axie still egg, moving on.");
                        continue;
                    }
                    int dpsScore = (int)Math.Floor(axieData.GetDPR() / AxieData.GetMaxDPR() * 100);
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
    }

    public class CollectionStatDataHandler
    {
        public void GetData(string address)
        {
            Rank[] dpsRankingList = new Rank[100];
            Rank[] tankRankingList = new Rank[100];
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
            int currentPerc = 0;
            while (axieIndex < axiePages && dataAvailable)
            {

                    Console.WriteLine($"Page {axieIndex} out of {axiePages}");

                string json = null;
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        json = wc.DownloadString("https://axieinfinity.com/api/addresses/"+ address +"/axies?offset=" + (12 * axieIndex).ToString());
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
                    if(!setupDone)
                    {
                        axiePages = (int)addressJson["totalPages"];
                        setupDone = true;
                    }
                    foreach(var axie in addressJson["axies"])
                    {
                        AxieDataOld axieData = axie.ToObject<AxieDataOld>();
                        axieData.jsonData = JObject.Parse(axie.ToString());
                        if (axieData.stage <= 2)
                        {
                            Console.WriteLine("Axie still egg, moving on.");
                            continue;
                        }
                        int dpsScore = axieData.GetDPRScore();
                        int tankScore = axieData.GetTNKScore();
                        if (dpsScore == 0) dpsScore++;
                        if (tankScore == 0) tankScore++;
                        if(dpsScore > tankScore)
                        {
                            dpsRankingList[100 - dpsScore].axies.Add(axieData.id);
                        }
                        else
                        {
                            tankRankingList[100 - tankScore].axies.Add(axieData.id);
                        }
                    }
                }
            }
            string dpsData = JsonConvert.SerializeObject(dpsRankingList, Formatting.Indented); 
            string tankData = JsonConvert.SerializeObject(tankRankingList, Formatting.Indented);


            string dpsPath = "CocoDpsRankingList.txt";
            string tankPath = "CocoTankRankingList.txt";
            if (!File.Exists(dpsPath))
            {
                File.Create(dpsPath);
                using (var tw = new StreamWriter(dpsPath))
                {
                    tw.Write(dpsData);
                }
            }
            else if (File.Exists(dpsPath))
            {
                using (var tw = new StreamWriter(dpsPath))
                {
                    tw.Write(dpsData);
                }
            }
            if (!File.Exists(tankPath))
            {
                File.Create(tankPath);
                using (var tw = new StreamWriter(tankPath))
                {
                    tw.Write(tankData);
                }
            }
            else if (File.Exists(tankPath))
            {
                using (var tw = new StreamWriter(tankPath))
                {
                    tw.Write(tankData);
                }
            }
        }


        public static List<AxieDataOld> GetAxieListFromAddress(string address)
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
                        json = wc.DownloadString("https://axieinfinity.com/api/addresses/" + address + "/axies?offset=" + (12 * axieIndex).ToString());
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
                        AxieDataOld axieData = axie.ToObject<AxieDataOld>();
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

    }



    public class Rank
    {
        public int rank;
        public List<int> axies;

        public Rank(int _rank)
        {
            rank = _rank;
            axies = new List<int>();
        }
    }
}



