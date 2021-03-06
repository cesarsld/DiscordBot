﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading;

namespace DiscordBot
{
    public class GiveawayInstance : BotGameInstance
    {

        private string GetGiveawayMessage(string userName, int maxMinutesToWait, int target)
        {
            return $" Preparing to start a Giveaway for ```Markdown\r\n<{userName}> will start his giveaway in {maxMinutesToWait} seconds. The participant(s) closest to the number  * {target} * wins!"
                + "```\r\n"
                + $"React to this message with ANY emoji to enter!  Multiple Reactions(emojis) will NOT enter you more than once.\r\nPlayer entered: ";
        }


        private async Task RunGiveawayInternal(int numWinners, int target, List<Player> players)
        {
            await new GiveawayRunner().Run(numWinners, target, players, LogToChannel, SendMsg, GetCancelGame);
        }

        public async Task RunGiveaway(int numWinners,int maxMinutesToWait, int target, ICommandContext context, string userName, int testUsers)
        {
            bool removeHandler = false;
            try
            {
                _channel = context.Channel;

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

                string gameMessageText = GetGiveawayMessage(userName, maxMinutesToWait, target);
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
                    _spamAccounts = new List<Player>();
                    _firstPlayersToReact = new List<string>();
                    MessageId = gameMessage.Id;
                }

                Bot.DiscordClient.ReactionAdded += HandleReactionAdded;
                await gameMessage.AddReactionAsync(new Emoji("😃"));
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
                    }
                    if (secondCounter > 5)
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
                removeHandler = false;


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

                if (_spamAccounts.Count > 0)
                {
                    StringBuilder sb = new StringBuilder(2000);
                    foreach (var player in _spamAccounts)
                    {
                        sb.Append($"{player.UserName} ");
                    }
                    LogToChannel("Participants REMOVED from giveaway due to account not old enough : \n" + sb);
                }

                await RunGiveawayInternal(numWinners, target, players);
            }
            catch (Exception ex)
            {
                await Logger.Log(new LogMessage(LogSeverity.Error, "RunGame", "Unexpected Exception", ex));
                try
                {
                    Console.WriteLine(ex.Message);
                    LogAndReply("Error Starting Game.");
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
    }
}
