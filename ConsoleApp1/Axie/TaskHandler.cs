using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Discord.Commands;
using Discord;
namespace DiscordBot.Axie
{
    public enum TaskType
    {
        BreedQuery,
        WinrateQuery
    }
    public class TaskHandler
    {
        public static readonly object SyncObj = new object();
        public static bool FetchingDataFromApi = false;
        private static Queue<Tuple<IUserMessage, string>> taskList = new Queue<Tuple<IUserMessage, string>>();

    }
}
