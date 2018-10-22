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
namespace DiscordBot.Mongo
{
    class DatabaseConnection
    {
        private static MongoClient Client;
        private static IMongoDatabase AxieDatabase;
        private static IMongoCollection<BsonDocument> Collection;


        public static void SetupConnection()
        {
            var connectionString = DiscordKeyGetter.GetDBUrl(); ;

            Client = new MongoClient(connectionString);
            AxieDatabase = Client.GetDatabase("AxieData");
        }

        public static IMongoDatabase GetDb()
        {

            if (Client == null)
            {
                SetupConnection();
            }
            return AxieDatabase;
        }
    }
}
