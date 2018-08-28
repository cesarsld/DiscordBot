using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot
{
    public class BotGameInstance
    {
        public delegate void ShowMessageDelegate(string msg, string logMsgOnly = null, IReadOnlyList<IEmote> emotes = null);
        public delegate Task ShowMessageDelegate1(string msg);

        public const string Smiley = "😃"; // :smiley:
        public const string Smile = "😄"; // :smile:
        public const string ReactionToUse = Smiley;
        public const string ReactionToUseText = ":smiley:(Smiley)";

        protected readonly object SyncObj = new object();
        protected Dictionary<Player, Player> _players;
        protected Dictionary<Player, List<string>> _cheatingPlayers;
        protected List<string> _firstPlayersToReact;
        protected bool CancelGame;
        protected bool StartSooner;

        protected ulong MessageId;
        protected IMessageChannel _channel;
        protected IGuild _guild;
        protected int testUsers;
        protected IUser gameAuthor;

        protected async Task SendWinnerToAuthor(string winnerMsg)
        {
            if (_guild.Name == "Bit Heroes")
            {
                await gameAuthor.SendMessageAsync(winnerMsg);
            }
                
        }

        protected void LogAndReply(string message)
        {
            Logger.LogInternal(message);
            _channel.SendMessageAsync(message);
        }

        protected void SendImageFile(string path, string logMsgOnly = null, IReadOnlyList<IEmote> emotes = null)
        {
            Logger.LogInternal("Uploading file from: " + path);
            _channel.SendFileAsync(path);
        }

        protected void LogAndReplyError(string message, string method)
        {
            Logger.Log(new LogMessage(LogSeverity.Error, method, message));
            _channel.SendMessageAsync(message);
        }

        protected virtual Player CreatePlayer(IUser user)
        {
            return new Player(user);
        }

        protected Task HandleReactionAdded(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            try
            {
                if (reaction != null && reaction.User.IsSpecified && reaction.UserId != msg.Value.Author.Id) // for now except all reactions && reaction.Emote.Name == ReactionToUse)
                {
                    Player player = CreatePlayer(reaction.User.Value);
                    lock (SyncObj)
                    {
                        if (msg.Value?.Id == MessageId)
                        {
                            // check for changed NickName
                            List<string> fullUserNameList;
                            if (_cheatingPlayers.TryGetValue(player, out fullUserNameList) && fullUserNameList != null && fullUserNameList.Count > 1)
                            {
                                if (fullUserNameList.Contains(player.FullUserName) == false)
                                {
                                    fullUserNameList.Add(player.FullUserName);
                                }
                            }
                            else
                            {
                                Player existingPlayer;
                                if (_players.TryGetValue(player, out existingPlayer))
                                {
                                    if (player.FullUserName.Equals(existingPlayer.FullUserName) == false)
                                    {
                                        _players.Remove(player);
                                        _cheatingPlayers[player] = new List<string> { existingPlayer.FullUserName, player.FullUserName };
                                    }
                                }
                                else
                                {
                                    if (_firstPlayersToReact.Count < 20)
                                    {
                                        _firstPlayersToReact.Add(player.FullUserName);
                                    }
                                    _players[player] = player;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(new LogMessage(LogSeverity.Error, "HandleReactionAdded", "Unexpected Exception", ex));
            }
            return Task.CompletedTask;
        }
        /*
                protected Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
                {
                    try
                    {
                        if (reaction != null && reaction.User.IsSpecified)
                        {
                            SocketGuildUser user = reaction.User.Value as SocketGuildUser;
                            string playerName = user?.Nickname ?? reaction.User.Value.Username;
                            lock (_syncObj)
                            {
                                if (msg.Value?.Id == _messageId)
                                {
                                    _players.Remove(playerName);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(new LogMessage(LogSeverity.Error, "HandleReactionRemoved", "Unexpected Exception", ex));
                    }
                    return Task.CompletedTask;
                }
        */
        public void AbortGame()
        {
            lock (SyncObj)
            {
                CancelGame = true;
            }
        }

        public void StartGameSooner()
        {
            lock (SyncObj)
            {
                StartSooner = true;
            }
        }

        protected bool GetCancelGame()
        {
            lock (SyncObj)
            {
                return CancelGame;
            }
        }

        public async Task StartGame(int numWinners, int maxUsers, int maxMinutesToWait, int secondsDelayBetweenDays, ICommandContext context, string userName, ulong userId, int testUsers)
        {
            this.testUsers = testUsers;

            await RunGame(numWinners, maxUsers, maxMinutesToWait, secondsDelayBetweenDays, context, userName, userId, false);
            //return true;
        }
        public bool StartGame(int maxTurn, int maxMinutesToWait, int maxScore, ICommandContext context, string userName, int testUsers)
        {
            this.testUsers = testUsers;

            Task.Run(() => RunGame( maxMinutesToWait, maxScore, maxTurn, context, userName));
            return true;
        }

        protected void CheckReactionUsers(IUserMessage gameMessage, Dictionary<string, string> newPlayersNickNameLookup)
        {
            int eventReactionsCount = newPlayersNickNameLookup.Count;
            int badGetReactions = 0;
            int addedGetReactions = 0;
            int existingGetReactions = 0;
            int getReactionsCount = 0;
            var result = gameMessage.GetReactionUsersAsync(ReactionToUse, DiscordConfig.MaxUsersPerBatch).GetAwaiter().GetResult();
            if (result != null)
            {
                Dictionary<string, string> playersUserNameLookup = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> keyValuePair in newPlayersNickNameLookup)
                {
                    playersUserNameLookup[keyValuePair.Value] = keyValuePair.Key;
                }

                getReactionsCount = result.Count;
                foreach (IUser user in result)
                {
                    if (playersUserNameLookup.ContainsKey(user.Username) == false)
                    {
                        SocketGuildUser userLookup = _channel.GetUserAsync(user.Id).GetAwaiter().GetResult() as SocketGuildUser;
                        if (userLookup != null)
                        {
                            newPlayersNickNameLookup[userLookup.Nickname ?? userLookup.Username] = userLookup.Username;
                            addedGetReactions++;
                        }
                        else
                        {
                            badGetReactions++;
                        }
                    }
                    else
                    {
                        existingGetReactions++;
                    }
                }
            }
            Logger.Log($"RunGame - GetReactionsReturned: {getReactionsCount} EventReactions: {eventReactionsCount} BadUsers: {badGetReactions} AddedUsers: {addedGetReactions} ExistingUsers: {existingGetReactions} TotalPlayers: {newPlayersNickNameLookup.Count}");
        }

        protected virtual string GetRunGameMessage(string bhgRoleMention, string userName, int maxUsers, int maxMinutesToWait, bool startWhenMaxUsers)
        {
            return $"{bhgRoleMention} Preparing to start a Game for ```Markdown\r\n<{userName}> in {maxMinutesToWait} seconds"
                + (startWhenMaxUsers ? $" or when we get {maxUsers} reactions!" : $"!  At the start of the game the # of players will be reduced to {maxUsers} if needed.") + "```\r\n"
                + $"React to this message with ANY emoji to enter!  Multiple Reactions(emojis) will NOT enter you more than once.\r\nPlayer entered: ";
        }

        protected virtual string GetRunGameMessage(string bhgRoleMention, string userName, int maxMinutesToWait, int maxTurns, int maxScore) { return ""; }

        protected virtual async Task RunGameInternal(int numWinners, int secondsDelayBetweenDays, List<Player> players, int maxPlayers)
        {
            await new BHungerGames().Run(numWinners, secondsDelayBetweenDays, players, LogToChannel, SendMsg, GetCancelGame, SendWinnerToAuthor, maxPlayers);
        }
        protected virtual void RunGameInternal(int maxScore, int maxTurns, List<Player> players) { }

        private async Task RunGame(int numWinners, int maxUsers, int maxMinutesToWait, int secondsDelayBetweenDays, ICommandContext context, string userName, ulong userId, bool startWhenMaxUsers = true)
        {
            bool removeHandler = false;
            try
            {
                _channel = context.Channel;
                _guild = context.Guild;
                gameAuthor = Bot.GetUser(userId);
                // see if BHG exists, if so mention it
                var roles = context.Guild.Roles;
                string bhgRoleMention = "";
                foreach (IRole role in roles)
                {
                    if (role.IsMentionable && string.Equals(role.Name, "BHG", StringComparison.OrdinalIgnoreCase))
                    {
                        bhgRoleMention = role.Mention;
                        break;
                    }
                }

                string gameMessageText = GetRunGameMessage(bhgRoleMention, userName, maxUsers, maxMinutesToWait, startWhenMaxUsers);
                Task<IUserMessage> messageTask = _channel.SendMessageAsync(gameMessageText + "0");
                Logger.Log(gameMessageText);
                messageTask.Wait();

                if (messageTask.IsFaulted)
                {
                    LogAndReplyError("Error getting players.", "RunGame");
                    return;
                }
                var gameMessage = messageTask.Result;
                if (gameMessage == null)
                {
                    LogAndReplyError("Error accessing Game Message.", "RunGame");
                    return;
                }
                await gameMessage.AddReactionAsync(new Emoji("😃"));
                lock (SyncObj)
                {
                    _players = new Dictionary<Player, Player>();
                    _cheatingPlayers = new Dictionary<Player, List<string>>();
                    _firstPlayersToReact = new List<string>();
                    MessageId = gameMessage.Id;
                }

                Bot.DiscordClient.ReactionAdded += HandleReactionAdded;
                //Bot.DiscordClient.ReactionRemoved += HandleReactionRemoved;
                removeHandler = true;

                Dictionary<Player, Player> newPlayersUserNameLookup;
                Dictionary<Player, List<string>> newCheatingPlayersLookup;
                List<string> newFirstPlayersToReact;
                DateTime now = DateTime.Now;
                int secondCounter = 0;
                int lastPlayerCount = 0;
                while (true)
                {
                    int currentPlayerCount;
                    lock (SyncObj)
                    {
                        if (CancelGame)
                            return;
                        currentPlayerCount = _players.Count;
                        if (startWhenMaxUsers && currentPlayerCount >= maxUsers)
                        {
                            MessageId = 0;
                            newPlayersUserNameLookup = _players;
                            newCheatingPlayersLookup = _cheatingPlayers;
                            newFirstPlayersToReact = _firstPlayersToReact;
                            _players = new Dictionary<Player, Player>();
                            _cheatingPlayers = new Dictionary<Player, List<string>>();
                            _firstPlayersToReact = new List<string>();
                            break;
                        }
                    }
                    if (secondCounter > 10)
                    {
                        secondCounter = 0;
                        if (currentPlayerCount != lastPlayerCount)
                        {
                            lastPlayerCount = currentPlayerCount;
                            await gameMessage.ModifyAsync(x => x.Content = gameMessageText + currentPlayerCount); // + "```\r\n");
                        }
                    }

                    await Task.Delay(1000);
                    secondCounter++;
                    if ((DateTime.Now - now).TotalSeconds >= maxMinutesToWait || StartSooner)//changes to seconds for faster testing
                    {
                        lock (SyncObj)
                        {
                            if (CancelGame)
                                return;
                            MessageId = 0;
                            newPlayersUserNameLookup = _players;
                            newCheatingPlayersLookup = _cheatingPlayers;
                            newFirstPlayersToReact = _firstPlayersToReact;
                            _players = new Dictionary<Player, Player>();
                            _cheatingPlayers = new Dictionary<Player, List<string>>();
                            _firstPlayersToReact = new List<string>();
                        }
                        break;
                    }
                }

                Bot.DiscordClient.ReactionAdded -= HandleReactionAdded;
                //Bot.DiscordClient.ReactionRemoved -= HandleReactionRemoved;
                removeHandler = false;

                //CheckReactionUsers(gameMessage, newPlayersNickNameLookup);

                // for now we don't use this anymore so don't update it.
                //lock (_syncObj)
                //{
                //    _players = new Dictionary<string, string>(newPlayersNickNameLookup);
                //}

                List<Player> players;
                if (testUsers > 0)
                {
                    players = new List<Player>(testUsers);
                    for (int i = 0; i < testUsers; i++)
                        players.Add(new Player(i));
                }
                else
                {
                    players = new List<Player>(newPlayersUserNameLookup.Values);
                    if (players.Count < 1)
                    {
                        LogAndReplyError("Error, no players reacted.", "RunGame");
                        return;
                    }
                }

                if (lastPlayerCount != players.Count)
                {
                    await gameMessage.ModifyAsync(x => x.Content = gameMessageText + players.Count); //  + "```\r\n"
                }

                if (newFirstPlayersToReact.Count > 0)
                {
                    StringBuilder sb = new StringBuilder(2000);
                    foreach (string fullUserName in newFirstPlayersToReact)
                    {
                        sb.Append($"<{fullUserName}> ## ");
                    }
                    string guildName = context.Guild?.Name ?? "NULL";
                    Logger.LogInternal($"G: {guildName} First Users To React: " + sb);
                }

                if (newCheatingPlayersLookup.Count > 0)
                {
                    StringBuilder sb = new StringBuilder(2000);
                    foreach (KeyValuePair<Player, List<string>> pair in newCheatingPlayersLookup)
                    {
                        sb.Append($"(ID:<{pair.Key.UserId}>): ");
                        foreach (string fullUserName in pair.Value)
                        {
                            sb.Append($"<{fullUserName}> ");
                        }
                        sb.Append("\r\n");
                    }
                    LogToChannel("Players REMOVED from game due to multiple NickNames:\r\n" + sb);
                }

                await RunGameInternal(numWinners, secondsDelayBetweenDays, players, startWhenMaxUsers ? 0 : maxUsers);
            }
            catch (Exception ex)
            {
                await Logger.Log(new LogMessage(LogSeverity.Error, "RunGame", "Unexpected Exception", ex));
                try
                {
                    LogAndReply("Error Starting Game. " + ex.ToString());
                }
                catch (Exception ex2)
                {
                    await Logger.Log(new LogMessage(LogSeverity.Error, "RunGame", "Unexpected Exception Sending Error to Discord", ex2));
                }
            }
            finally
            {
                try
                {
                    if (removeHandler)
                    {
                        Bot.DiscordClient.ReactionAdded -= HandleReactionAdded;
                        //Bot.DiscordClient.ReactionRemoved -= HandleReactionRemoved;
                    }
                }
                catch (Exception ex)
                {
                    await Logger.Log(new LogMessage(LogSeverity.Error, "RunGame", "Unexpected Exception In Finally", ex));
                }
                try
                {
                    BaseCommands.RemoveChannelCommandInstance(context.Channel.Id);
                }
                catch (Exception ex)
                {
                    await Logger.Log(new LogMessage(LogSeverity.Error, "RunGame", "Unexpected Exception In Finally2", ex));
                }
                try
                {
                    string cancelMessage = null;
                    lock (SyncObj)
                    {
                        if (CancelGame)
                        {
                            cancelMessage = "GAME CANCELLED!!!";
                        }
                    }
                    if (cancelMessage != null)
                    {
                        LogAndReply(cancelMessage);
                    }
                }
                catch (Exception ex)
                {
                    await Logger.Log(new LogMessage(LogSeverity.Error, "RunGame", "Unexpected Exception In Finally3", ex));
                }
            }
        }

        //method used for HG3
        private void RunGame( int maxMinutesToWait, int maxScore, int maxTurn, ICommandContext context, string userName)
        {
            bool removeHandler = false;
            try
            {
                _channel = context.Channel;

                // see if BHG exists, if so mention it
                var roles = context.Guild.Roles;
                string bhgRoleMention = "";
                string bannerImagePath = "ArtAssets/HG32.png";
                foreach (IRole role in roles)
                {
                    if (role.IsMentionable && string.Equals(role.Name, "BHG", StringComparison.OrdinalIgnoreCase))
                    {
                        bhgRoleMention = role.Mention;
                        break;
                    }
                }
                //string bhgRoleMention, string userName, int maxMinutesToWait, int maxTurns, int maxScore
                string gameMessageText = GetRunGameMessage(bhgRoleMention, userName, maxMinutesToWait, maxTurn, maxScore);
                Task<IUserMessage> messageTask;
                messageTask = _channel.SendFileAsync(bannerImagePath);
                Thread.Sleep(3000);
                messageTask = _channel.SendMessageAsync(gameMessageText + "0");
                Logger.Log(gameMessageText);
                messageTask.Wait();

                if (messageTask.IsFaulted)
                {
                    LogAndReplyError("Error getting players.", "RunGame");
                    return;
                }
                var gameMessage = messageTask.Result;
                if (gameMessage == null)
                {
                    LogAndReplyError("Error accessing Game Message.", "RunGame");
                    return;
                }
                gameMessage.AddReactionAsync(new Emoji("😃"));
                lock (SyncObj)
                {
                    _players = new Dictionary<Player, Player>();
                    _cheatingPlayers = new Dictionary<Player, List<string>>();
                    _firstPlayersToReact = new List<string>();
                    MessageId = gameMessage.Id;
                }

                Bot.DiscordClient.ReactionAdded += HandleReactionAdded;
                //Bot.DiscordClient.ReactionRemoved += HandleReactionRemoved;
                removeHandler = true;

                Dictionary<Player, Player> newPlayersUserNameLookup;
                Dictionary<Player, List<string>> newCheatingPlayersLookup;
                List<string> newFirstPlayersToReact;
                DateTime now = DateTime.Now;
                int secondCounter = 0;
                int lastPlayerCount = 0;
                while (true)
                {
                    int currentPlayerCount;
                    lock (SyncObj)
                    {
                        if (CancelGame)
                            return;
                        currentPlayerCount = _players.Count;
                        //if (startWhenMaxUsers && currentPlayerCount >= maxUsers)
                        //{
                        //    MessageId = 0;
                        //    newPlayersUserNameLookup = _players;
                        //    newCheatingPlayersLookup = _cheatingPlayers;
                        //    newFirstPlayersToReact = _firstPlayersToReact;
                        //    _players = new Dictionary<Player, Player>();
                        //    _cheatingPlayers = new Dictionary<Player, List<string>>();
                        //    _firstPlayersToReact = new List<string>();
                        //    break;
                        //}
                    }
                    if (secondCounter > 10)
                    {
                        secondCounter = 0;
                        if (currentPlayerCount != lastPlayerCount)
                        {
                            lastPlayerCount = currentPlayerCount;
                            gameMessage.ModifyAsync(x => x.Content = gameMessageText + currentPlayerCount); // + "```\r\n");
                        }
                    }

                    Thread.Sleep(1000);
                    secondCounter++;
                    if ((DateTime.Now - now).TotalSeconds >= maxMinutesToWait)//changes to seconds for faster testing
                    {
                        lock (SyncObj)
                        {
                            if (CancelGame)
                                return;
                            MessageId = 0;
                            newPlayersUserNameLookup = _players;
                            newCheatingPlayersLookup = _cheatingPlayers;
                            newFirstPlayersToReact = _firstPlayersToReact;
                            _players = new Dictionary<Player, Player>();
                            _cheatingPlayers = new Dictionary<Player, List<string>>();
                            _firstPlayersToReact = new List<string>();
                        }
                        break;
                    }
                }

                Bot.DiscordClient.ReactionAdded -= HandleReactionAdded;
                //Bot.DiscordClient.ReactionRemoved -= HandleReactionRemoved;
                removeHandler = false;

                //CheckReactionUsers(gameMessage, newPlayersNickNameLookup);

                // for now we don't use this anymore so don't update it.
                //lock (_syncObj)
                //{
                //    _players = new Dictionary<string, string>(newPlayersNickNameLookup);
                //}

                List<Player> players;
                if (testUsers > 0)
                {
                    players = new List<Player>(testUsers);
                    for (int i = 0; i < testUsers; i++)
                        players.Add(new Player(i));
                }
                else
                {
                    players = new List<Player>(newPlayersUserNameLookup.Values);
                    if (players.Count < 1)
                    {
                        LogAndReplyError("Error, no players reacted.", "RunGame");
                        return;
                    }
                }

                if (lastPlayerCount != players.Count)
                {
                    gameMessage.ModifyAsync(x => x.Content = gameMessageText + players.Count); //  + "```\r\n"
                }

                if (newFirstPlayersToReact.Count > 0)
                {
                    StringBuilder sb = new StringBuilder(2000);
                    foreach (string fullUserName in newFirstPlayersToReact)
                    {
                        sb.Append($"<{fullUserName}> ## ");
                    }
                    string guildName = context.Guild?.Name ?? "NULL";
                    Logger.LogInternal($"G: {guildName} First Users To React: " + sb);
                }

                if (newCheatingPlayersLookup.Count > 0)
                {
                    StringBuilder sb = new StringBuilder(2000);
                    foreach (KeyValuePair<Player, List<string>> pair in newCheatingPlayersLookup)
                    {
                        sb.Append($"(ID:<{pair.Key.UserId}>): ");
                        foreach (string fullUserName in pair.Value)
                        {
                            sb.Append($"<{fullUserName}> ");
                        }
                        sb.Append("\r\n");
                    }
                    LogToChannel("Players REMOVED from game due to multiple NickNames:\r\n" + sb);
                }
                //RunGameInternal(int secondsDelayBetweenDays, int maxScore, int maxTurns, List<Player> players, int maxPlayers = 0)
                RunGameInternal(maxScore, maxTurn, players);
            }
            catch (Exception ex)
            {
                Logger.Log(new LogMessage(LogSeverity.Error, "RunGame", "Unexpected Exception", ex));
                try
                {
                    LogAndReply("Error Starting Game.");
                }
                catch (Exception ex2)
                {
                    Logger.Log(new LogMessage(LogSeverity.Error, "RunGame", "Unexpected Exception Sending Error to Discord", ex2));
                }
            }
            finally
            {
                try
                {
                    if (removeHandler)
                    {
                        Bot.DiscordClient.ReactionAdded -= HandleReactionAdded;
                        //Bot.DiscordClient.ReactionRemoved -= HandleReactionRemoved;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(new LogMessage(LogSeverity.Error, "RunGame", "Unexpected Exception In Finally", ex));
                }
                try
                {
                    BaseCommands.RemoveChannelCommandInstance(context.Channel.Id);
                }
                catch (Exception ex)
                {
                    Logger.Log(new LogMessage(LogSeverity.Error, "RunGame", "Unexpected Exception In Finally2", ex));
                }
                try
                {
                    string cancelMessage = null;
                    lock (SyncObj)
                    {
                        if (CancelGame)
                        {
                            cancelMessage = "GAME CANCELLED!!!";
                        }
                    }
                    if (cancelMessage != null)
                    {
                        LogAndReply(cancelMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(new LogMessage(LogSeverity.Error, "RunGame", "Unexpected Exception In Finally3", ex));
                }
            }
        }
        
        protected IUserMessage SendMarkdownMsg(string msg)
        {
            var message = _channel.SendMessageAsync("```Markdown\r\n" + msg + "```\r\n").GetAwaiter().GetResult();
            lock (SyncObj)
            {
                MessageId = message.Id;
            }
            return message;
        }

        protected void SendMsg(string msg, string logMsgOnly = null, IReadOnlyList<IEmote> emotes = null)
        {
            Logger.LogInternal(msg);
            var message = _channel.SendMessageAsync(msg + "\r\n").GetAwaiter().GetResult();
            lock (SyncObj)
            {
                MessageId = message.Id;
            }
        }

        private bool ChrEqualToBreakableChr(char chr)
        {
            return chr == ' ' || chr == '\n' || chr == '\r' || chr == '\t';
        }

        private int GetSizeOfFirstBreakableChr(string msg, int startingSize) // space, tabe \r \n are breakable characters.
        {
            while (ChrEqualToBreakableChr(msg[startingSize - 1]) == false)
            {
                startingSize--;
            }
            return startingSize;
        }

        protected void LogToChannel(string msg, string logMsgOnly = null, IReadOnlyList<IEmote> emotes = null)
        {
            const int maxMessageSize = 1930; // 2000 minus markdown used below.
            if (string.IsNullOrEmpty(logMsgOnly) == false)
            {
                Logger.LogInternal("LogMsgOnly: " + logMsgOnly);
            }
            Logger.LogInternal(msg);
            IUserMessage message;

            if (msg.Length > maxMessageSize)
            {
                while (msg.Length > maxMessageSize)
                {
                    int partSize = GetSizeOfFirstBreakableChr(msg, maxMessageSize);
                    SendMarkdownMsg(msg.Substring(0, partSize));
                    msg = msg.Substring(partSize);
                }
                message = SendMarkdownMsg(msg);
            }
            else
            {
                message = SendMarkdownMsg(msg);
            }

            if (emotes != null && message != null)
            {
                foreach (IEmote emoji in emotes)
                {
                    message.AddReactionAsync(emoji);
                    Thread.Sleep(new TimeSpan(0, 0, 2));
                }

            }
        }
    }
}
