using System;
using System.Threading;
using Discord;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using DiscordBot.Axie;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace DiscordBot.Axie
{
    public class TraitInfo
    {
        public int Usage = 0;
        public int WinDiff = 0;
        public TraitInfo(int gain)
        {
            Usage = 1;
            WinDiff = gain;
        }
    }



    public class Rank
    {
        public int rank;
        public List<int> axies;

        public Rank(int _rank)
        {
            rank = _rank;
            axies = new List<int>();
        }
    }
}



