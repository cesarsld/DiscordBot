using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;
using DiscordBot.Axie.Battles;
namespace DiscordBot.Mongo
{
    class DatabaseConnection
    {
        private static MongoClient Client;
        private static IMongoDatabase AxieDatabase;
        private static IMongoCollection<BsonDocument> Collection;


        public static void SetupConnection()
        {
            var connectionString = DiscordKeyGetter.GetDBUrl();

            Client = new MongoClient(connectionString);
            AxieDatabase = Client.GetDatabase("AxieData");
        }

        public static void UpdateIpAddress(string ip)
        {
            var connectionString = DiscordKeyGetter.GetDBUrl();
            var newString = connectionString.Substring(0, 26);
            newString += ip + ":27017";
            DiscordKeyGetter.SetDBUrl(newString);
            
        }

        public static IMongoDatabase GetDb()
        {

            if (Client == null)
            {
                SetupConnection();
            }
            return AxieDatabase;
        }

        public static async Task UpdateMystic()
        {
            var db = DatabaseConnection.GetDb();
            var collection = db.GetCollection<AxieWinrate>("AxieWinrate");
            var list = collection.Find(Builders<AxieWinrate>.Filter.Empty).ToList();
            //collection.UpdateMany(Builders<BsonDocument>.Filter.Empty, Builders<BsonDocument>.Update.Set("mysticCount", 0 ));
            //collection.UpdateMany(Builders<BsonDocument>.Filter.Empty, Builders<BsonDocument>.Update.Set("lastBattleDate", time ));
            var perc = (float)list.Count / 100;
            var currentPerc = 0;
            var counter = 0;

            foreach (var axie in list)
            {
                counter++;
                if (counter > perc)
                {
                    counter = 0;
                    currentPerc++;
                    Console.WriteLine($"{currentPerc}%");
                }
                var apiData = await AxieObject.GetAxieFromApi(axie.id);
                axie.mysticCount = apiData.mysticCount;
                collection.UpdateOne(a => a.id == axie.id, Builders<AxieWinrate>.Update.Set("mysticCount", axie.mysticCount));
            }
        }

    }
}
