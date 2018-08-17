
namespace BHungerGaemsBot
{
    public class RunningCommandInfo
    {
        public string CommandName { get; }
        public ulong UserId { get; }
        public ulong ChannelId { get; }
        public ulong GuildId { get;  }
        public BotGameInstance GameInstance { get; }

        public RunningCommandInfo(string commandName, ulong userId, ulong channelId, ulong guildId, BotGameInstance gameInstance)
        {
            CommandName = commandName;
            UserId = userId;
            ChannelId = channelId;
            GuildId = guildId;
            GameInstance = gameInstance;
        }
    }
}
