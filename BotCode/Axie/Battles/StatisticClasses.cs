using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Axie.Battles
{
    class DailyUsers
    {
        public int id;
        public int Count;

        public DailyUsers(int _id, int _count)
        {
            id = _id;
            Count = _count;
        }
    }
}
