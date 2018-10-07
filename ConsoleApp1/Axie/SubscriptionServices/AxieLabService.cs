using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;

namespace DiscordBot.Axie.SubscriptionServices
{
    class AxieLabService : ISubscriptionService
    {
        public ServiceEnum name { get; set; }
        [JsonProperty]
        private double priceTrigger;

        public AxieLabService(ServiceEnum _service)
        {
            name = _service;
            priceTrigger = 0;
        }

        public double GetPrice() => priceTrigger;
        public void SetPrice(double price) => priceTrigger = price;

        public EmbedBuilder GetTriggerEmbedMessage()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("TRIGGER ALERT!!!");
            embed.WithDescription($"The Egg pod price has dropped below your threshold of {priceTrigger} ether!");
            embed.WithUrl("https://axieinfinity.com/axie-lab?r=9SG7dDe-x3sLFShtw_Sah7mUZ3M");
            embed.AddField("Note", "Your price trigger will now be removed.");
            embed.WithColor(Color.Red);

            return embed;

        }


    }
}
