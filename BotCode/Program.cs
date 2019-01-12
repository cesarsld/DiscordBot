using System;
using System.Threading;
using Discord;
using MongoDB.Bson;
using DiscordBot.Mongo;
using DiscordBot.Axie.ApiCalls;
using MongoDB.Driver;
using DiscordBot.Axie;
using System.Linq;
using DiscordBot.Axie.Battles;
using System.Collections.Generic;
using DiscordBot.Axie.Web3Axie;
using System.Diagnostics;

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
                    //StatDataHandler.GetTraitMapping("Zigzag");
                    //StatDataHandler.GetAxieListFromGlobal().GetAwaiter().GetResult();
                    //DatabaseConnection.UpdateMystic().GetAwaiter().GetResult();
                    //var sw = new Stopwatch();
                    //sw.Start();
                    //var axie = AxieDataOld.GetAxieFromApi(150).GetAwaiter().GetResult();
                    //sw.Stop();
                    //Console.WriteLine(sw.ElapsedMilliseconds);
                    //Console.ReadLine();
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
