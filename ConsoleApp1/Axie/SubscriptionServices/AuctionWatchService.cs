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
    public class AuctionWatchService : ISubscriptionService
    {

        public ServiceEnum name { get; set; }
        [JsonProperty]
        private List<AuctionFilter> auctionFilterList;

        [JsonConstructor]
        public AuctionWatchService(ServiceEnum _name, List<AuctionFilter> list)
        {
            name = _name;
            auctionFilterList = list != null ? list : new List<AuctionFilter>();
        }

        public AuctionWatchService(ServiceEnum _name)
        {
            name = _name;
            auctionFilterList = new List<AuctionFilter>();
        }

        public List<AuctionFilter> GetList() => auctionFilterList;
        public void RemoveElements(List<AuctionFilter> listToRemove) => auctionFilterList = auctionFilterList.Except(listToRemove).ToList();
        public void AddFilter(AuctionFilter filter) => auctionFilterList.Add(filter);
        public void RemoveFilter(AuctionFilter filter) => auctionFilterList.Remove(filter);
    }

    public class AuctionFilter
    {
        public bool isMystic;
        public int mysticCount;
        public string Class;
        public int purity;
        public BigInteger triggerPrice;

        public void Init()
        {
            isMystic = false;
            mysticCount = 0;
            Class = "any";
            purity = 0;
            triggerPrice = 0;
        }

        public async Task<EmbedBuilder> GetTriggerMessage(int axieId)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("TRIGGER ALERT!!!");
            embed.WithDescription($"Axie #{axieId} just got auctionned!");
            embed.AddField("Filters", isMystic? "mystic " : mysticCount > 0? $"mystic count : {mysticCount} ":"" + Class != "any"? Class:"" + (purity>0? $"pureness : {purity} ":"") + (triggerPrice > 10? $"price = {ConvertToEth().ToString("F4")} ":""));
            embed.WithUrl("https://axieinfinity.com/axie/" + axieId.ToString() + "?r=9SG7dDe-x3sLFShtw_Sah7mUZ3M");
            embed.AddField("Note", "Your auciton filter will now be removed.");
            embed.WithThumbnailUrl((await AxieData.GetAxieFromApi(axieId)).GetImageUrl());
            embed.WithColor(Color.Blue);

            return embed;
        }

        private float ConvertToEth()
        {
            return Convert.ToSingle(Nethereum.Util.UnitConversion.Convert.FromWei(triggerPrice));
        }
    }


}
