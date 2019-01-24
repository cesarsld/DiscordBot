using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
namespace DiscordBot.Competition
{
    public class UsersEntered
    {
        public ulong _id { get; set; }
        public string name { get; set; }
        public long credits { get; set; }
        public UsersEntered(SocketGuildUser user)
        {
            _id = user.Id;
            name = user.Username;
            credits = 10000;
        }
    }
}
