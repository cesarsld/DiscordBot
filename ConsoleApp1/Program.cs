using System;
using System.Threading;
using Discord;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using DiscordBot.Axie;
using DiscordBot.Axie.Battles;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Numerics;
using System.Collections.Generic;

namespace DiscordBot
{

    class Program
    {
        static void RunBot()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
                    //new CollectionStatDataHandler().GetData("0xe0c71f6C2FeA87F2155DE000E090B09f65D361dF");
                    //WinrateCollector.GetAllData();
                    //PureBreeder.GetPureBreedingChancesFromAddress("0x721931508DF2764fD4F70C53Da646Cb8aEd16acE");
                    //BreedingData.GetData().GetAwaiter().GetResult();
                    new Bot().RunAsync().GetAwaiter().GetResult();
                    //BanListHandler bl = new BanListHandler();
                    //bl.UnbanUserFromBannedList(111);
                    //BHungerGames.Test();
                }
                catch (Exception ex)
                {
                    Logger.Log(new LogMessage(LogSeverity.Error, ex.ToString(), "Unexpected Exception", ex));
                    Console.WriteLine(ex.ToString());
                }
                Thread.Sleep(1000);
                break;
            }
            //BHungerGamesV3 bh = new BHungerGamesV3();
            //bh.Run(1, new System.Collections.Generic.List<Player>(),);
        }
        static void Main()
        {
            RunBot();
            //Stuff();
            //MainAsync().Wait();
            //Console.ReadLine();
        }

        static async Task MainAsync()
        {
            var connectionString = "mongodb://localhost:27017";

            MongoClient client = new MongoClient(connectionString);
            IMongoDatabase db = client.GetDatabase("DiscordDB");
            var collection = db.GetCollection<BsonDocument>("Users");

            var document = new BsonDocument
            {
                { "name", "ShadBase" },
                { "type", "Database" },
                { "count", 1 },
                { "info", new BsonDocument
                    {
                        { "x", 203 },
                        { "y", 102 }
                    }}
            };

            await collection.InsertOneAsync(document);
        }
        private static int GetTriggerTime(BigInteger triggerPrice, BigInteger startPrice, BigInteger endPrice, int duration, int auctionStartTime)
        {
            BigInteger time = (triggerPrice * duration) / BigInteger.Abs(startPrice - endPrice) + auctionStartTime;
            return (int)(time);
        }
        static void Stuff()
        {
            var connectionString = "mongodb://localhost:27017";

            MongoClient client = new MongoClient(connectionString);
            IMongoDatabase db = client.GetDatabase("DiscordDB");
            var userCollection = db.GetCollection<BsonDocument>("Users");

            var filterId = Builders<BsonDocument>.Filter.Eq("_id", 195567858133106697);
            var doc = userCollection.Find(filterId).FirstOrDefault();
            if (doc != null)
            {
                var obj = BsonSerializer.Deserialize<User>(doc);
                Console.WriteLine($" Hello userId : {obj._id} and Username : {obj.Username}");
            }
            else
            {
                Console.WriteLine("No user found.");
            }
        }
    }
}
