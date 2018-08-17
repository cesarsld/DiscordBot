using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace BHungerGaemsBot
{
    class BotV3GameInstance : BotGameInstance
    {
        private BHungerGamesV3 _gameInstance;

        protected override Player CreatePlayer(IUser user)
        {
            return new PlayerRPG(user);
        }

        public Task FetchPlayerInput(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            try
            {
                if (reaction != null && reaction.User.IsSpecified && reaction.UserId != msg.Value.Author.Id) // for now except all reactions && reaction.Emote.Name == ReactionToUse)
                {
                    lock (SyncObj)
                    {
                        if (msg.Value?.Id == MessageId)
                        {
                            _gameInstance.HandlePlayerInput(reaction.UserId, reaction.Emote.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(new LogMessage(LogSeverity.Error, "FethPlayerInput", "Unexpected Exception", ex));
            }
            return Task.CompletedTask;
        }

        protected override string GetRunGameMessage(string bhgRoleMention, string userName, int maxMinutesToWait, int maxTurns, int maxScore)
        {
            return $"{bhgRoleMention} Preparing to start a Bit Heroes BattleGround Game for ```Markdown\r\n<{userName}> in {maxMinutesToWait} seconds. "
                   + $"The Game will last {maxTurns} turns" + (maxScore == 1000000000 ? "." : $" OR when a player reaches a Score of <{maxScore} points>.")
                   + "```\r\n"
                   + "React to this message with any emoji to enter!  Multiple Reactions(emojis) will NOT enter you more than once.\r\nPlayer entered: ";
        }

        protected override void RunGameInternal(int maxScore, int maxTurns, List<Player> players)
        {
            Bot.DiscordClient.ReactionAdded += FetchPlayerInput;
            try
            {
                _gameInstance = new BHungerGamesV3();
                _gameInstance.Run(players, maxScore, maxTurns,  LogToChannel, GetCancelGame, SendImageFile, SendMsg);

            }
            finally
            {
                Bot.DiscordClient.ReactionAdded -= FetchPlayerInput;
            }
        }


    }
}
