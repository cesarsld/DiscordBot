using System;
using Discord;
using Discord.WebSocket;

namespace BHungerGaemsBot
{
    public class Player : IEquatable<Player>
    {
        public ulong UserId { get; }
        public string UserName { get; }
        public string UserNameWithDiscriminator { get; }
        public string NickName { get; }
        public string FullUserName => $"{UserNameWithDiscriminator}##{NickName}";
        public string ContestantName => NickName;

        public Player(IUser userParm)
        {
            SocketGuildUser user = userParm as SocketGuildUser;
            UserId = userParm.Id;
            UserName = userParm.Username;
            NickName = user?.Nickname ?? UserName;
            UserNameWithDiscriminator = userParm.ToString(); // tostring has usersnamd and discriminator in it, which is unique.
        }

        public Player(int index)
        {
            UserId = (ulong)index;
            UserName = "P";
            NickName = "P" + index;
            UserNameWithDiscriminator = $"P#{index}"; // tostring has usersnamd and discriminator in it, which is unique.
        }

        public override int GetHashCode()
        {
            return (int)UserId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Player);
        }

        public bool Equals(Player other)
        {
            return other != null && other.UserId == UserId;
        }
    }
}
