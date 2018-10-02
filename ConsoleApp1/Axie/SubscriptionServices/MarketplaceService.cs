using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using System.Numerics;
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
        public int axieId;
        public MarketPlaceTriggerTypeEnum triggerTypeEnum;
        public int auctionStartTime;
        public int duration;
        public BigInteger startprice;
        public BigInteger endPrice;

        public AxieTrigger(int id, MarketPlaceTriggerTypeEnum type, int startTime, int _duration, BigInteger _startPrice, BigInteger _endPrice)
        {
            axieId = id;
            triggerTypeEnum = type;
            duration = _duration;
            auctionStartTime = startTime;
            startprice = _startPrice;
            endPrice = _endPrice;

        }
    }


}
