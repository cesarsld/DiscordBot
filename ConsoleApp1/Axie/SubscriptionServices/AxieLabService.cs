using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Axie.SubscriptionServices
{
    class AxieLabService : ISubscriptionService
    {
        public ServiceEnum name { get; set; }

        private float priceTrigger;

        public AxieLabService(ServiceEnum _service)
        {
            name = _service;
            priceTrigger = 0.15f;
        }

        public float GetPrice() => priceTrigger;
        public void SetPrice(float price) => priceTrigger = price;
    }
}
