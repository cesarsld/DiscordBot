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

    public class Winrate
    {
        public int win;
        public int loss;
        public float winrate;
        public Winrate(int _win, int _loss)
        {
            win = _win;
            loss = _loss;
        }
        public void GetWinrate()
        {
            winrate = (float)win / (win + loss) * 100;
        }
    }

    class FightDataHandler
    {


        public static void GetData()
        {
            Dictionary<string, TraitInfo> traitData = new Dictionary<string, TraitInfo>();
            Dictionary<int, Winrate> winrateData = new Dictionary<int, Winrate>();
            bool dataAvailable = true;
            int battleCount = 24875;
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
                    for (int i = 0; i < 3; i++)
                    {
                        if (winrateData.ContainsKey(winningTeam[i])) winrateData[winningTeam[i]].win++;
                        else winrateData.Add(winningTeam[i], new Winrate(1, 0));

                        if (winrateData.ContainsKey(losingTeam[i])) winrateData[losingTeam[i]].loss++;
                        else winrateData.Add(losingTeam[i], new Winrate(0, 1));
                    }
                    for (int j = 0; j < 6; j++)
                    {
                        break;
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
            foreach (var axie in winrateData) axie.Value.GetWinrate();
            
            List<KeyValuePair<string, TraitInfo>> transferList = traitData.ToList();
            transferList.Sort((pair1, pair2) => pair2.Value.Usage.CompareTo(pair1.Value.Usage));
            traitData = transferList.ToDictionary(name => name.Key, _data => _data.Value);

            List <KeyValuePair< int, Winrate >> winrateTransfer = winrateData.ToList();
            //winrateTransfer.Sort((pair1, pair2) => pair2.Value.winrate.CompareTo(pair1.Value.winrate));
            winrateTransfer = winrateTransfer.OrderByDescending(wr => wr.Value.winrate).ThenByDescending(wr => wr.Value.win).ThenBy(wr => wr.Key).ToList();
            winrateData = winrateTransfer.ToDictionary(name => name.Key, _data => _data.Value);
            string data = JsonConvert.SerializeObject(traitData, Formatting.Indented);
            string winrateJson = JsonConvert.SerializeObject(winrateData, Formatting.Indented);
            string path = "TraitData.txt";
            string winratePath = "WinrateData.txt";
            if (!File.Exists(path))
            {
                File.Create(path);
                using (var tw = new StreamWriter(path))
                {
                    //tw.Write(data);
                }
            }
            else if (File.Exists(path))
            {
                using (var tw = new StreamWriter(path))
                {
                    //tw.Write(data);
                }
            }
            if (File.Exists(winratePath))
            {
                using (var tw = new StreamWriter(winratePath))
                {
                    tw.Write(winrateJson);
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
                        json = await wc.DownloadStringTaskAsync("https://axieinfinity.com/api/addresses/" + address + "/axies?offset=" + (12 * axieIndex).ToString());
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

    public class BreedingData
    {
        public static async Task GetData()
        {
            int mixMatronClass = 0;
            int mixSireClass = 0;
            int pureMatronClass = 0;
            int pureSireClass = 0;
            int pureBreeding = 0;
            int mixBreeding = 0;

            bool dataAvailable = true;
            int axieCount = 7040;
            int axieIndex = 3530;
            int perc = axieCount / 100;
            int currentPerc = axieIndex / perc;
            while (axieIndex < axieCount && dataAvailable)
            {
                axieIndex++;
                if (axieIndex % perc == 0)
                {
                    currentPerc++;
                    Console.WriteLine(currentPerc.ToString() + "%");
                }
                var axieData = await AxieData.GetAxieFromApi(axieIndex);
                if (axieData != null && axieData.sireId != 0)
                {
                    var matronData = await AxieData.GetAxieFromApi(axieData.matronId);
                    var sireData = await AxieData.GetAxieFromApi(axieData.sireId);
                    if (matronData != null)
                    {
                        if (sireData.Class == matronData.Class)
                        {
                            pureBreeding++;
                            if (axieData.Class == matronData.Class) pureMatronClass++;
                            else pureSireClass++;
                        }
                        else
                        {
                            mixBreeding++;
                            if (axieData.Class == matronData.Class) mixMatronClass++;
                            else mixSireClass++;
                        }
                    }

                }
            }
            float matronPerc = (float)mixMatronClass / mixBreeding;
            string data = $"Out of all {mixBreeding} mix breeds, {matronPerc * 100}% took the class of the matron and {(1f - matronPerc) * 100}% toold the class of the sire.";
            string path = "BreedData.txt";
            if (File.Exists(path))
            {
                using (var tw = new StreamWriter(path))
                {
                    tw.Write(data);
                }
            }
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



