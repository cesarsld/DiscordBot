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
