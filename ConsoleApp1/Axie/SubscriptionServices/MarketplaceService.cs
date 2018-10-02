using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
namespace DiscordBot.Axie.SubscriptionServices
{
    public class MarketplaceService : ISubscriptionService
    {
        public ServiceEnum name { get; set; }
        [JsonProperty]
        private bool notifyOnSale;

        private List<AxieTrigger> triggerList;

        [JsonConstructor]
        public MarketplaceService(ServiceEnum _name, bool _notifyOnSale, List<AxieTrigger> list)
        {
            name = _name;
            notifyOnSale = _notifyOnSale;
            triggerList = list;
        }

        public MarketplaceService(ServiceEnum _name)
        {
            name = _name;
            triggerList = new List<AxieTrigger>();
        }

        public bool GetNotifStatus() => notifyOnSale;
        public bool SetNotifStatus(bool status) => notifyOnSale = status;

        public List<AxieTrigger> GetList() => triggerList;
        public void AddTrigger(AxieTrigger trigger) => triggerList.Add(trigger);
        public void RemoveTrigger(AxieTrigger trigger) => triggerList.Remove(trigger);

    }

    public class AxieTrigger
    {
        public int id;
        public MarketPlaceTriggerTypeEnum triggerTypeEnum;
        public double price;
    }


}
