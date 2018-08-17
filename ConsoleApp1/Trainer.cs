using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;

namespace BHungerGaemsBot
{
    public class Trainer
    {
        [BsonId]
        public ulong _id;

        public List<ulong> MountIdList;

        public int Coins;

        public long Experience;

        public Trainer(User user)
        {
            _id = user._id;
            Coins = 50;
            Experience = 0;
            MountIdList = new List<ulong>();
        }
    }

    public class Mount
    {
        public DateTime BirthDate;

        public int Level;

        public int Experience;

        public int Speed;

        public int Stamina;

        public Gender gender;
    }

    public enum Gender
    {
        Male,
        Female
    }

    public enum MountRarity
    {
        Trash,
        Common,
        Rare,
        Epic,
        Legendary,
        Mythic
    }

    public class Inventory
    {
        public Item[] itemList;
    }

    public class ItemMount
    {
        public string itemName;
        public int amount;
    }

    public class User
    {
        [BsonId]
        public ulong _id;
        public string Username;

        public User(IUser user)
        {
            _id = user.Id;
            Username = user.Username;
        }
    }

    public class GuildUser : User
    {
        public ulong GuildId;
        public string NickName;
        public GuildUser(IUser user) : base(user)
        { }
    }

    public class Guild
    {    
        [BsonId]
        public ulong _id;
        public string Name;
        public Guild(IGuild guild)
        {
            _id = guild.Id;
            Name = guild.Name;
        }
    }

    public class DatabaseConnection
    {
        private MongoClient Client;
        private IMongoDatabase DiscordDatabase;

        public void SetupConnection()
        {
            var connectionString = "mongodb://localhost:27017";

            Client = new MongoClient(connectionString);
            DiscordDatabase = Client.GetDatabase("DiscordDB");
        }

        public IMongoDatabase GetDb()
        {
            return DiscordDatabase;
        }

    }

    public class CharacterFactory
    {
        public void CreateNewPlayer()
        {

        }

        public void LoadPlayerFromDb()
        {

        }
    }

}
