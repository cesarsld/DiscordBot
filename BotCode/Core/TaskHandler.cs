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
using DiscordBot.Axie.SubscriptionServices;
namespace DiscordBot.Axie
{
    public enum TaskType
    {
        BreedQuery,
        WinrateQuery
    }
    public class TaskHandler
    {
        public static bool IsOn = false;
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

        public static async Task UpdateServiceCheckLoop()
        {
            while (IsOn)
            {
                bool hasTriggered = false;
                //eggLabPrice *= 0.999;
                int unixTime = Convert.ToInt32(((DateTimeOffset)(DateTime.UtcNow)).ToUnixTimeSeconds());
                //if (eggLabPrice < 0.133) eggLabPrice = 0.133;
                if (WinrateCollector.lastUnixTimeCheck == 0) WinrateCollector.UpdateUnixLastCheck();
                if (unixTime - WinrateCollector.lastUnixTimeCheck >= WinrateCollector.unixTimeBetweenUpdates) _=  WinrateCollector.GetDataSinceLastChack();
                var subList = await SubscriptionServicesHandler.GetSubList();

                foreach (var sub in subList)
                {
                    //Axie lab service check
                    //var axieLabSub = sub.GetServiceList().FirstOrDefault(service => service.name == ServiceEnum.AxieLab) as AxieLabService;
                    //if (axieLabSub != null)
                    //{
                    //    if (axieLabSub.GetPrice() >= eggLabPrice)
                    //    {
                    //        hasTriggered = true;
                    //        _ = Bot.GetUser(sub.GetId()).SendMessageAsync("", false, axieLabSub.GetTriggerEmbedMessage());
                    //        axieLabSub.SetPrice(0);
                    //    }
                    //}
                    //MarketPlace trigger check
                    var marketplaceSub = sub.GetServiceList().FirstOrDefault(service => service.name == ServiceEnum.MarketPlace) as MarketplaceService;
                    if (marketplaceSub != null)
                    {
                        var triggersToRemove = new List<AxieTrigger>();
                        foreach (var trigger in marketplaceSub.GetTriggerList())
                        {
                            if (unixTime > trigger.triggerTime)
                            {
                                hasTriggered = true;
                                _ = Bot.GetUser(sub.GetId()).SendMessageAsync("", false, trigger.GetTriggerMessage());
                                triggersToRemove.Add(trigger);
                            }
                        }
                        marketplaceSub.RemoveTriggers(triggersToRemove);
                    }
                }
                if (hasTriggered)
                {
                    _ = SubscriptionServicesHandler.SetSubList();
                    hasTriggered = false;
                }
                await Task.Delay(60000);
            }
        }


    }
}
