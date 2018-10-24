using System;
using System.Threading;
using Discord;
using MongoDB.Bson;
using DiscordBot.Mongo;
using MongoDB.Driver;
using DiscordBot.Axie.Battles;
using System.Linq;
using System.Collections.Generic;

namespace DiscordBot
{
    public class Peperoni
    {
        public int id;
        public string slice;
        public Peperoni(int _id, string ee)
        {
            id = _id;
            slice = ee;
        }
    }
    class Program
    {
        static void RunBot()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
                    new Bot().RunAsync().GetAwaiter().GetResult();
                    //WinrateCollector.GetAllData();
                    //var db = DatabaseConnection.GetDb();
                    //var collection = db.GetCollection<BsonDocument>("AxieWinrate");
                    //collection.UpdateMany(Builders<BsonDocument>.Filter.Empty, Builders<BsonDocument>.Update.Set("mysticCount", "0" ));
                    //var db = DatabaseConnection.GetDb();
                    //var collection = db.GetCollection<Peperoni>("test");
                    //List<int> list = new List<int>();
                    //for (int i = 0; i < 6; i++)
                    //    list.Add(i);
                    //var option = new UpdateOptions();
                    //option.IsUpsert = true;
                    //collection.UpdateMany(ele => list.Contains(ele.id), Builders<Peperoni>.Update.Set("slice", "an OIU"), option);
                    //var pizza = collection.Find(x => x.id == 150);

                    //var real = pizza.First();
                    //int e = 3 + 5;


                }
                catch (Exception ex)
                {
                    Logger.Log(new LogMessage(LogSeverity.Error, ex.ToString(), "Unexpected Exception", ex));
                    Console.WriteLine(ex.ToString());
                }
                Thread.Sleep(1000);
                break;
            }

        }
        static void Main()
        {
            RunBot();
        }



    }
}
