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
        [JsonProperty]
        private List<AxieTrigger> triggerList;

        [JsonConstructor]
        public MarketplaceService(ServiceEnum _name, bool _notifyOnSale, List<AxieTrigger> list)
        {
            name = _name;
            notifyOnSale = _notifyOnSale;
            triggerList = list!= null? list : new List<AxieTrigger>();
        }

        public MarketplaceService(ServiceEnum _name)
        {
            name = _name;
            triggerList = new List<AxieTrigger>();
            notifyOnSale = true;
        }

        public bool GetNotifStatus() => notifyOnSale;
        public bool SetNotifStatus(bool status) => notifyOnSale = status;

        public List<AxieTrigger> GetList() => triggerList;
        public void RemoveElements(List<AxieTrigger> listToRemove) => triggerList = triggerList.Except(listToRemove).ToList();
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
        public string imageUrl;
        //[JsonConstructor]
        public void SetupAxieTrigger(int id, MarketPlaceTriggerTypeEnum type, int _duration, int startTime, BigInteger _startPrice, BigInteger _endPrice, BigInteger _trigger, string url)
        {
            axieId = id;
            triggerTypeEnum = type;
            duration = _duration;
            auctionStartTime = startTime;
            startPrice = _startPrice;
            endPrice = _endPrice;
            triggerPrice = _trigger;
            GetTriggerTime();
            imageUrl = url;

        }

        private void GetTriggerTime()
        {
            BigInteger time = (triggerPrice * duration) / BigInteger.Abs(startPrice - endPrice) + auctionStartTime;
            triggerTime = (int)(time);
        }

        public EmbedBuilder GetTriggerMessage()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("TRIGGER ALERT!!!");
            embed.WithDescription($"Axie #{axieId} price has dropped below your threshold of {ConvertToEth().ToString("F4")} ether!");
            embed.WithUrl("https://axieinfinity.com/axie/"+ axieId.ToString() +"?r=9SG7dDe-x3sLFShtw_Sah7mUZ3M");
            embed.AddField("Note", "Your price trigger will now be removed.");
            embed.WithThumbnailUrl(imageUrl);
            embed.WithColor(Color.Red);

            return embed;
        }

        private float ConvertToEth()
        {
            return Convert.ToSingle(Nethereum.Util.UnitConversion.Convert.FromWei(triggerPrice));
        }
    }


}
