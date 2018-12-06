using System;
using System.Threading;
using Discord;
using MongoDB.Driver;
using System.Threading.Tasks;
using DiscordBot.Mongo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using DiscordBot.Axie.Breeding;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;


namespace DiscordBot.Axie.Battles
{
    public class TraitInfo
    {
        public int Usage = 0;
        public int WinDiff = 0;
        public TraitInfo(int gain, int use)
        {
            Usage = use;
            WinDiff = gain;
        }
    }

    public class PartWinrate
    {

        public static async Task GetAllPartsData()
        {
            var db = DatabaseConnection.GetDb();
            var collec = db.GetCollection<AxieWinrate>("AxieWinrate");
            var axieList = (await collec.FindAsync(Builders<AxieWinrate>.Filter.Empty)).ToList();
            int tot = axieList.Count;
            int div = tot / 100;
            var i = 0;
            var perc = 0;
            foreach (var axie in axieList)
            {
                i++;
                if (i == div)
                {
                    perc++;
                    Console.WriteLine($"{perc}%");
                    i = 0;
                }
                var data = await AxieDataOld.GetAxieFromApi(axie.id);
                string[] moves = new string[4];
                var index = 0;
                foreach (var move in data.parts)
                {
                    switch (move.type)
                    {
                        case "mouth":
                        case "back":
                        case "horn":
                        case "tail":
                            moves[index] = move.name;
                            index++;
                            break;

                    }
                }
                var filterId = Builders<AxieWinrate>.Filter.Eq("_id", axie.id);
                var update = Builders<AxieWinrate>.Update.Set(a => a.moves, moves);
                await collec.UpdateOneAsync(filterId, update);
            }
        }

        public static async Task ConcatData()
        {
            Dictionary<string, TraitInfo> traitData = new Dictionary<string, TraitInfo>();

            var db = DatabaseConnection.GetDb();
            var collec = db.GetCollection<AxieWinrate>("AxieWinrate");
            var axieList = (await collec.FindAsync(Builders<AxieWinrate>.Filter.Empty)).ToList();
            foreach (var axie in axieList)
            {
                foreach (var move in axie.moves)
                {
                    if (traitData.ContainsKey(move))
                    {
                        traitData[move].Usage += axie.win + axie.loss;
                        traitData[move].WinDiff += axie.win - axie.loss;
                    }
                    else {
                        traitData.Add(move, new TraitInfo(axie.win - axie.loss, axie.win + axie.loss));
                    }
                }
            }
            List<KeyValuePair<string, TraitInfo>> transferList = traitData.ToList();
            transferList.Sort((pair1, pair2) => pair2.Value.WinDiff.CompareTo(pair1.Value.WinDiff));
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
}
