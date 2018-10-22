using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Axie.SubscriptionServices
{
    public interface ISubscriptionService
    {
        ServiceEnum name { get; set; }
    }
}
