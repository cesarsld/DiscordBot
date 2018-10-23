using System;
using System.Threading;
using Discord;
using MongoDB.Bson;
using DiscordBot.Mongo;
using MongoDB.Driver;

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
                    new Bot().RunAsync().GetAwaiter().GetResult();
                    //var db = DatabaseConnection.GetDb();
                    //var collection = db.GetCollection<BsonDocument>("AxieWinrate");
                    //collection.UpdateMany(Builders<BsonDocument>.Filter.Empty, Builders<BsonDocument>.Update.Set("battleHistory", "0x" ));
;
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
