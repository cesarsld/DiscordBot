﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiscordBot.Axie.SubscriptionServices
{
    public class SubUser
    {
        [JsonProperty]
        private ulong userId;
        [JsonProperty]
        private List<ISubscriptionService> subServiceList;

        public ulong GetId() => userId;
        public List<ISubscriptionService> GetServiceList() => subServiceList;

        [JsonConstructor]
        public SubUser(ulong id, List<ISubscriptionService> list)
        {
            userId = id;
            subServiceList = list;
        }

        public SubUser(ulong id)
        {
            userId = id;
            subServiceList = new List<ISubscriptionService>();
        }

        public void AddService(ISubscriptionService service) => subServiceList.Add(service);
        public void RemoveService(ServiceEnum service) => subServiceList.RemoveAll(s => s.name == service);

    }
}
