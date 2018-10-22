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
using Newtonsoft;
namespace DiscordBot

{
    [Group("mount")]
    public class MountCommands : ModuleBase
    {
        private static readonly object SyncObj;

        static MountCommands()
        {
            //ChannelCommandInstances = new Dictionary<ulong, RunningCommandInfo>();
            SyncObj = new object();
        }

        private void HandleNewGuild(IMongoDatabase db)
        {
            Guild newGuild = new Guild(Context.Guild);
            var guildCollec = db.GetCollection<BsonDocument>("Guilds");
            var filterId = Builders<BsonDocument>.Filter.Eq("_id", Context.Guild.Id);
            if (guildCollec.Find(filterId).Count() == 0)
            {
                guildCollec.InsertOne(newGuild.ToBsonDocument());
            }

        }

        [Command("buyMount"), Summary("register")]
        public async Task CreateAccount(int index = 0)
        {
            if (index == 0)
            {
                await ReplyAsync("");
            }
        }

            public async Task RegisterToMountGame()
        {
            DatabaseConnection1 dbConenction = new DatabaseConnection1();
            dbConenction.SetupConnection();

            IMongoDatabase discordDb = dbConenction.GetDb();
            var userCollection = discordDb.GetCollection<BsonDocument>("Users");
            var playerDataCollection = discordDb.GetCollection<BsonDocument>("PlayerData");

            HandleNewGuild(discordDb);

            User newuser = new User(Context.Message.Author);
            Trainer newTrainer = new Trainer(newuser);
            var filterId = Builders<BsonDocument>.Filter.Eq("_id", newuser._id);
            if (userCollection.Find(filterId).Count() > 0)
            {
                await ReplyAsync("User already registered.");
            }
            else
            {
                await userCollection.InsertOneAsync(newuser.ToBsonDocument());
                await playerDataCollection.InsertOneAsync(newTrainer.ToBsonDocument());
                await ReplyAsync($"User Registered.");
            }

        }

        [Command("createAccount"), Summary("register")]
        public async Task CreateAccount()
        {
            User newUser = LookupUser(Context.Message.Author);
            if (newUser != null)
            {
                await ReplyAsync("User already has an account!");
            }
            else
            {
                await RegisterToMountGame();
                await ReplyAsync("Account created.");
            }
        }

        [Command("GiveExp"), Summary("test")]
        public async Task GiveExp(int exp)
        {
            Trainer trainer = PlayerFacotry.GetTrainerFromDb(Context.Message.Author.Id);
            if (trainer != null)
            {
                trainer.Experience += exp;
                PlayerFacotry.UpdateExpToDb(trainer);
                await ReplyAsync("Added " + exp.ToString() + " exp.");
            }
            else
            {
                await ReplyAsync("Trainer does not exist.");
            }

        }

        public User LookupUser(IUser user)
        {
            DatabaseConnection1 dbConenction = new DatabaseConnection1();
            dbConenction.SetupConnection();

            IMongoDatabase discrdDB = dbConenction.GetDb();
            var userCollection = discrdDB.GetCollection<BsonDocument>("Users");

            var builder = Builders<BsonDocument>.Filter;
            var filterId = Builders<BsonDocument>.Filter.Eq("_id", user.Id);
            var doc = userCollection.Find(filterId).FirstOrDefault();
            if (doc != null)
            {
                var userData = BsonSerializer.Deserialize<User>(doc);
                return userData;
                //await ReplyAsync($"userId : {obj._id} \rUsername : {obj.Username}");
            }
            else
            {
                User nullUser = null;
                return nullUser;
            }
        }
    }
}
