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
using DiscordBot.Axie.Battles;
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
        private static Queue<Tuple<IUserMessage, string, TaskType>> taskList = new Queue<Tuple<IUserMessage, string, TaskType>>();

        public static async Task RunTasks()
        {
            while (taskList.Count != 0)
            {
                Tuple<IUserMessage, string, TaskType> query;
                lock (SyncObj)
                {
                    query = taskList.Dequeue();
                }
                switch(query.Item3)
                {
                    case TaskType.BreedQuery:
                        await PureBreeder.GetPureBreedingChancesFromAddress(query.Item2, query.Item1);
                        break;
                    case TaskType.WinrateQuery:
                        await WinrateCollector.FetchDataFromAddress(query.Item2, query.Item1);
                        break;
                }

            }
            FetchingDataFromApi = false;
        }

        public static async Task AddTask(ICommandContext context, string address, TaskType taskType)
        {
            var message = await context.Message.Author.SendMessageAsync($"Added to task queue at position #{taskList.Count + 1}. Please wait.");
            lock (SyncObj)
            {
                taskList.Enqueue(new Tuple<IUserMessage, string, TaskType>(message, address, taskType));
            }
        }

    }
}
