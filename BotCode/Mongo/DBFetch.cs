using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;
using DiscordBot.Axie.Web3Axie;

namespace DiscordBot.Mongo
{
    class DbFetch
    {
        public static async Task<List<string>> FetchUniqueBuyers()
        {
            var collec = DatabaseConnection.GetDb().GetCollection<UniqueBuyer>("UniqueBuyers");
            var data = (await collec.FindAsync(a => true)).ToList();
            List<string> list = new List<string>();
            foreach (var buyer in data) list.Add(buyer.id);
            return list;
        }

        public static async Task<List<string>> FetchUniqueLandHolders()
        {
            var collec = DatabaseConnection.GetDb().GetCollection<UniqueBuyer>("UniqueLandHolders");
            var data = (await collec.FindAsync(a => true)).ToList();
            List<string> list = new List<string>();
            foreach (var buyer in data) list.Add(buyer.id.ToLower());
            return list;
        }
    }
}