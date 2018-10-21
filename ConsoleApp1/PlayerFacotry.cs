using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;

namespace DiscordBot
{
    class PlayerFacotry
    {
        public static Trainer GetTrainerFromDb(ulong _id)
        {
            DatabaseConnection1 dbConenction = new DatabaseConnection1();
            dbConenction.SetupConnection();

            IMongoDatabase discrdDB = dbConenction.GetDb();
            var userCollection = discrdDB.GetCollection<BsonDocument>("PlayerData");


            var filterId = Builders<BsonDocument>.Filter.Eq("_id", _id);
            var doc = userCollection.Find(filterId).FirstOrDefault();
            if (doc != null)
            {
                var userData = BsonSerializer.Deserialize<Trainer>(doc);
                return userData;
                //await ReplyAsync($"userId : {obj._id} \rUsername : {obj.Username}");
            }
            else
            {
                return null;
            }
        }

        public static void UpdateExpToDb(Trainer trainer)
        {
            DatabaseConnection1 dbConenction = new DatabaseConnection1();
            dbConenction.SetupConnection();

            IMongoDatabase discrdDB = dbConenction.GetDb();
            var filterId = Builders<BsonDocument>.Filter.Eq("_id", trainer._id);
            var elementToUpdate = Builders<BsonDocument>.Update.Set("Experience", trainer.Experience);
            var userCollection = discrdDB.GetCollection<BsonDocument>("PlayerData");
            userCollection.UpdateOne(filterId, elementToUpdate);
        }
    }
}
