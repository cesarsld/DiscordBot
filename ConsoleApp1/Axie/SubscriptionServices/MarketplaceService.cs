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
        public int triggerTime;
        public BigInteger startPrice;
        public BigInteger endPrice;
        public BigInteger triggerPrice;

        public AxieTrigger(int id, MarketPlaceTriggerTypeEnum type, int startTime, int _duration, BigInteger _startPrice, BigInteger _endPrice, BigInteger _trigger)
        {
            axieId = id;
            triggerTypeEnum = type;
            duration = _duration;
            auctionStartTime = startTime;
            startPrice = _startPrice;
            endPrice = _endPrice;
            triggerPrice = _trigger;
            triggerTime = GetTriggerTime();

        }

        private int GetTriggerTime()
        {
            BigInteger time = (triggerPrice * duration) / BigInteger.Abs(startPrice - endPrice) + auctionStartTime;
            return (int)(time);
        }
    }


}
