using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MongoDB.Driver.Linq;
using DiscordBot;

namespace DiscordBot
{
    public class BaseCommands : ModuleBase
    {
        protected static readonly object SyncObj;
        protected static readonly Dictionary<ulong, RunningCommandInfo> ChannelCommandInstances;

        static BaseCommands()
        {
            ChannelCommandInstances = new Dictionary<ulong, RunningCommandInfo>();
            SyncObj = new object();
        }

        public static RunningCommandInfo GetRunningCommandInfo(ulong channelId)
        {
            RunningCommandInfo instance;
            lock (SyncObj)
            {
                ChannelCommandInstances.TryGetValue(channelId, out instance);
            }
            return instance;
        }

        public static bool CreateChannelCommandInstance(string commandName, ulong userId, ulong channelId, ulong guildId, BotGameInstance gameInstance, out RunningCommandInfo instance)
        {
            lock (SyncObj)
            {
                ChannelCommandInstances.TryGetValue(channelId, out instance);
                if (instance == null)
                {
                    instance = new RunningCommandInfo(commandName, userId, channelId, guildId, gameInstance);
                    ChannelCommandInstances[channelId] = instance;
                    return true;
                }
            }
            return false;
        }

        public static void RemoveChannelCommandInstance(ulong channelId)
        {
            lock (SyncObj)
            {
                ChannelCommandInstances.Remove(channelId);
            }
        }

        public static bool ChannelHasRunningCommand(ulong channelId)
        {
            lock (SyncObj)
            {
                return ChannelCommandInstances.ContainsKey(channelId);
            }
        }

        public async Task LogAndReplyAsync(string message)
        {
            Logger.LogInternal(message);
            await ReplyAsync(message);
        }

        private bool CheckAccessForBitHeroesGuildOnly(bool checkCancelAccess = false)
        {
            HashSet<ulong> rolesWithAccess = new HashSet<ulong>();
            var roles = Context.Guild.Roles;
            if (Context.Guild.Name.Equals("Bit Heroes"))
            {
                foreach (IRole role in roles)
                {
                    if ((checkCancelAccess == false && role.Name.Contains("Level 2"))
                        || role.Name.Contains("300+") || role.Name.Contains("400") || role.Name.Contains("Admin") || role.Name.StartsWith("Mod ") || role.Name.Contains("500") || role.Name.Contains("600"))
                    {
                        rolesWithAccess.Add(role.Id);
                    }
                }
            }
            else
            { // default
                return false;
            }

            var authorRoles = ((IGuildUser)Context.Message.Author).RoleIds;
            foreach (ulong roleId in authorRoles)
            {
                if (rolesWithAccess.Contains(roleId))
                    return true;
            }

            StringBuilder sb = new StringBuilder();
            if (checkCancelAccess)
                sb.AppendLine($"Guild: '{Context.Guild.Name}' User: '{Context.Message.Author.Username}' does not have Access to run Cancel Command");
            else
                sb.AppendLine($"Guild: '{Context.Guild.Name}' User: '{Context.Message.Author.Username}' does not have Access to run Command");

            sb.AppendLine("Guild Roles:");
            foreach (IRole role in roles)
            {
                sb.AppendLine($"{role.Name} -- {role.Id}");
            }

            sb.Append("User Roles: ");
            foreach (ulong id in authorRoles)
            {
                sb.AppendLine(id + ",");
            }

            Logger.LogInternal(sb.ToString());
            return false;
        }

        private bool CheckModAccessBitHeroesGuild()
        {
            HashSet<ulong> rolesWithAccess = new HashSet<ulong>();
            var roles = Context.Guild.Roles;
            if (Context.Guild.Name.Equals("Bit Heroes"))
            {
                foreach (IRole role in roles)
                {
                    if (role.Name.Contains("Admin") || role.Name.StartsWith("Mod") || Context.User.Id == 195567858133106697)
                    {
                        rolesWithAccess.Add(role.Id);
                    }
                }

                var authorRoles = ((IGuildUser)Context.Message.Author).RoleIds;
                foreach (ulong roleId in authorRoles)
                {
                    if (rolesWithAccess.Contains(roleId))
                        return true;
                }
            }
            else if (Context.User.Id == 396792950870245386 || Context.User.Id == 195567858133106697) return true;
            return false;
        }

        private bool CheckAccessByGuild(bool checkCancelAccess = false)
        {
            HashSet<ulong> rolesWithAccess = new HashSet<ulong>();
            var roles = Context.Guild.Roles;
            if (Context.Guild.Name.Equals("Bit Heroes"))
            {
                foreach (IRole role in roles)
                {
                    if ((checkCancelAccess == false && role.Name.Contains("Level 6"))
                        || role.Name.Contains("250") || role.Name.Contains("300+") || role.Name.Contains("400") || role.Name.Contains("Admin") || role.Name.StartsWith("Mod ") || Context.User.Username.Contains("Owl of") || role.Name.Contains("[K]") || role.Name.Contains("500") || role.Name.Contains("600"))
                    {
                        rolesWithAccess.Add(role.Id);
                    }
                }
            }
            else if (Context.Guild.Name.Equals("Bit Heroes - FR") || Context.Guild.Name.Equals("Crash Test Server"))
            {
                return true;
                // 150-199  ,  200-249 ,  250+ ,  Admin , Modérateur
                foreach (IRole role in roles)
                {
                    if ((checkCancelAccess == false && (/*role.Name.Contains("150") ||*/ role.Name.Contains("300")))
                        || role.Name.Contains("Admin") || role.Name.StartsWith("Mod"))
                    {
                        rolesWithAccess.Add(role.Id);
                    }
                }
            }
            else
            { // default
                if (checkCancelAccess == false)
                    return true;

                foreach (IRole role in roles)
                {
                    if (role.Name.Contains("Admin") || role.Name.StartsWith("Mod") || role.Name.StartsWith("Git"))
                    {
                        rolesWithAccess.Add(role.Id);
                    }
                }
            }

            var authorRoles = ((IGuildUser)Context.Message.Author).RoleIds;
            foreach (ulong roleId in authorRoles)
            {
                if (rolesWithAccess.Contains(roleId))
                    return true;
            }

            StringBuilder sb = new StringBuilder();
            if (checkCancelAccess)
                sb.AppendLine($"Guild: '{Context.Guild.Name}' User: '{Context.Message.Author.Username}' does not have Access to run Cancel Command");
            else
                sb.AppendLine($"Guild: '{Context.Guild.Name}' User: '{Context.Message.Author.Username}' does not have Access to run Command");

            sb.AppendLine("Guild Roles:");
            foreach (IRole role in roles)
            {
                sb.AppendLine($"{role.Name} -- {role.Id}");
            }

            sb.Append("User Roles: ");
            foreach (ulong id in authorRoles)
            {
                sb.AppendLine(id + ",");
            }

            Logger.LogInternal(sb.ToString());
            return false;
        }

        /* Roles that can run commands on Bit Heros Guild Only
            Server Admin    1 members
            Admin           3 members
            Mod             4 members
            Admin & Mods    8 members
            300+ OH EM GEE  3 members
            Level 250 - 299 1 members
            Level 200 - 249 29 members          */
        private bool CheckAccess(bool allowPrivateMessage = false)
        {
            try
            {
                if (Context.Guild == null)
                {
                    return allowPrivateMessage;
                }
                return CheckAccessByGuild();
            }
            catch (Exception ex)
            {
                Logger.Log(new LogMessage(LogSeverity.Error, "CheckAccess", "Unexpected Exception", ex));
            }
            return false;
        }

        private bool CheckCancelAccess()
        {
            try
            {
                return CheckAccessByGuild(true);
            }
            catch (Exception ex)
            {
                Logger.Log(new LogMessage(LogSeverity.Error, "CheckCancelAccess", "Unexpected Exception", ex));
            }
            return false;
        }

        protected bool NotInBHGeneral()
        {
            return Context.Channel.Id != 241550898998935552;
        }

        public async Task ShowGameHelp()
        {
            await ReplyAsync("```Markdown\r\n<'>StartGame'> - Starts a new game if one is not already running.\n"
                                + "You must provide the first parameter to run, all other parameters are optional, like so:\n"
                                + "StartGame 100 5 10 2\n"
                                + "Parameters are in order:\n"
                                + "<Max User that can play>\n"
                                + "<Max seconds to wait for players (Default: 5)>\n"
                                + "<Seconds to delay between displaying next day (Default: 10)>\n"
                                + "<Number of Winners (Default: 1)>```\r\n");
        }

        public async Task ShowGameHelpV2()
        {
            await ReplyAsync("```Markdown\r\n<'>StartV2'> - Starts a new game if one is not already running.\n"
                                + "You must provide the first parameter to run, all other parameters are optional, like so:\n"
                                + "StartV2 100 5 2\n"
                                + "Parameters are in order:\n"
                                + "<Max User that can play>\n"
                                + "<Max seconds to wait for players (Default: 5)>\n"
                                + "<Number of Winners (Default: 1)>\n"
                                + "For more information on how to play, type the command: <!V2Rules> ```\r\n");
        }

        public async Task ShowGameHelpV3()
        {
            await ReplyAsync("```Markdown\r\n<'>StartV3'> - Starts a new game if one is not already running.\n"
                                + "You must provide the first parameter to run, all other parameters are optional, like so:\n"
                                + "StartV2 120 25\n"
                                + "Parameters are in order:\n"
                                + "<Seconds before start of game>\n"
                                + "<Max Turns before Game OVer>\n"
                                + "For more information on how to play, type the command: <!V3Rules> ```\r\n");
        }

        [Command("V2Rules"), Summary("Show HG2 rules")]
        public async Task Helpv2()
        {
            if (!NotInBHGeneral()) return;
            try
            {
                if (CheckAccess(true))
                {
                    await ReplyAsync("__**Interactive HGv2**__\n"
                        + "__**Objective**__: Be the last survivor.\n"
                        + "__**Initial Options**__\n"
                        + "__Loot__ ( :moneybag: ) = You have a chance to obtain gear (weapon/offhand/armor/helmet). Gear increases your chance to win duels. Weapon is destroyed after 5 duels.\n"
                        + "  •    Safe = No Damage = Low Quality Loot\n"
                        + "  •    Unsafe = Small chance for minor Damage = Medium Quality Loot\n"
                        + "  •    Dangerous= Medium chance for heavy Damage = High Quality Loot\n"
                        + "  •    Deadly = High chance for extreme Damage = Very High Quality Loot\n"
                        + "__Capture Familiars__ ( " + Emote.Parse("<:blubber:244666398738087936>") + " you need an emote with the name : blubber :)= You have the chance to obtain a familiar that will increase your duel chance + small chance that it deals damage on another survivor. As for loot, you will have danger levels. Higher danger level = higher risk to fail = higher quality of familiar.\n"
                        + "  •    Safe = No Damage = Low Quality Familiar\n"
                        + "  •    Unsafe = Small chance for minor Damage = Medium Quality Familiar\n"
                        + "  •    Dangerous_= Medium chance for heavy Damage = High Quality Familiar\n"
                        + "  •    Deadly = High chance for extreme Damage = Very High Quality Familiar\n"
                        + "__Stay Alert__ (:exclamation:) = +10% chance for avoiding scenario's. Less chance of success if used in succession.\n"
                        + "__Duel Immunity__ (:crossed_swords:) = Prevents you from being targeted for a duel. Cannot be used in succession; has a 5 day cooldown.\n"
                        + "__Do Nothing__ = Occurs when you do not input a command.\n"
                        );
                    await ReplyAsync("__**Enhanced Decisions**__\n"
                        + "Players will be randomly selected to perform an additional action after each round.\n"
                        + "  •    Sabotage  (:wrench:)  = Chance to apply a debuff to another player\n"
                        + "  •    Steal (:gun:)  = Chance to steal an opponents item. Will only steal items greater than what you currently have\n"
                        + "  •    Make a Trap (:bomb:) = Chance to create a trap that will target enemy players and apply immediate damage\n"
                        + "__**Game Effects:**__\n"
                        + "__Crowd Decisions__ = every 3 days a public crowd poll will be executed to modify the game\n"
                        + "__Terra_forming__ = every 4 days the arena changes to obtain different advantages and disadvantages.\n"
                        + "__Scenarios__ = Every day will have a chance for a scenario. Each scenario will either heal you (small chance), instantly kill you (small chance) or damage you (common)\n"
                        + "__Duel__ = Start after 4 days. Every day a random duel between people will start. This is not influenced by Stay Alert\n"
                        + "__Debuffs__ = Negative effects incurred by sabotage\n"
                        + "  •    Reduced Item Find\n"
                        + "  •    Increased Scenario Likelihood\n"
                        + "  •    Increased Duel Chance\n"
                        );
                }
            }
            catch (Exception ex)
            {
                await Logger.Log(new LogMessage(LogSeverity.Error, "CancelGame", "Unexpected Exception", ex));
            }
        }


        [Command("V3Rules"), Summary("Show HG3 rules")]
        public async Task Helpv3()
        {
            if (!NotInBHGeneral()) return;
            try
            {
                if (CheckAccess(true))
                {
                    await ReplyAsync(
                         "**__HG3 RULES__**\n"
                       + "**__Entering__**\n"
                       + " •    As HG2, just react to an emote during selection phase.\n\n"
                       + "__**Initialisation**__\n"
                       + " •    You will be asked to choose a class amongst 7 classes.If you fail to select one, you will be a mage by default.\n\n"
                       + "**__Options__**\n"
                       + " •    There are 4 options in the game:\n"
                       + "   •    ( :bulb: ) = More chances to complete your adventure giving you Notoriety Points(Notoriety still don't do anything... but will evbentually)\n"
                       + "   •    ( :crossed_swords: ) = Gain more EXP while completing an adventure.\n"
                       + "   •    ( :moneybag: ) = Higher chances at obtaining better loot after completing an adventure.\n"
                       + "   •    ( :muscle: ) = Decide to skip the forthcoming adventure and train to gain a large amount of EXP at the cost of points and loot.Small chance of obtaining an Aura Bonus.\n\n"
                       + "**__Events__**\n"
                       + " •    There will be some events during the game that may or may not happen (I did add a pity timer so they should happen). I won't spoil for the time being.\n\n"
                       + "**__Duels__**\n"
                       + " •    Duels will happen if more than 10 players are in the game. They are similar to HG2 duels but you are not eleiminated if you lose. If you win a duel, you are rewarded with 5% of your opponent's EXP and Points on top of receiving an Aura Bonus.\n\n"
                       + "**__How to win__**\n"
                       + " •    Simple, get the most points. There are no deaths. Adventures give points.Adventures scale with your level.The higher level you are the more Points you will earn (game still subject to balancing). A leaderboard with the top 20 players will be shown every 4 turns.\n\n"
                       + "** __Additional features__**\n"
                       + " •    Aura Bonus = Your next day's action will have its bonus doubled.\n"
                       + " •    CP(Combat Power) = Players will have their CP updated all the time.Items with higher rarity and class match will provide the best CP increase.\n"
                       + " •    Loot Distribution = Items will have 3 different stat distribution.Average, Advantageous and Extreme.Extreme allocates the stats in the most efficient way.\n"
                    );
                }
            }
            catch (Exception ex)
            {
                await Logger.Log(new LogMessage(LogSeverity.Error, "CancelGame", "Unexpected Exception", ex));
            }
        }
        [Command("Help"), Summary("Shows Bot Help")]
        public async Task Help()
        {
            try
            {
                //Logger.LogInternal($"Command 'Help' executed by '{Context.Message.Author.Username}'");
                if (CheckAccess(true))
                {
                    await ReplyAsync("```Markdown\r\n<!CancelGame> - Cancels the current running game if there is one.\n"
                                     + "Only the user that started the game or an Admin/Mod can run this command.```\r\n");
                    await ReplyAsync("```Markdown\r\n<!CleanUp> - Deletes all the messages by this Bot in the last 100 messages.```\r\n");
                    await ShowGameHelp();
                    await ShowGameHelpV2();
                    await ShowGameHelpV3();
                }
            }
            catch (Exception ex)
            {
                await Logger.Log(new LogMessage(LogSeverity.Error, "CancelGame", "Unexpected Exception", ex));
            }
        }

        [Command("CancelGame"), Summary("Cancels the current running game.")]
        public async Task CancelGame()
        {
            if (!NotInBHGeneral()) return;
            try
            {
                //Logger.LogInternal($"Command 'CancelGame' executed by '{Context.Message.Author.Username}'");
                if (CheckAccess())
                {
                    string returnMessage = "No Game is currently running!";
                    bool hasCancelAccess = CheckCancelAccess();
                    lock (SyncObj)
                    {
                        RunningCommandInfo commandInfo = GetRunningCommandInfo(Context.Channel.Id);
                        if (commandInfo?.GameInstance != null)
                        {
                            if (Context.Message.Author.Id == commandInfo.UserId || hasCancelAccess)
                            {
                                RemoveChannelCommandInstance(Context.Channel.Id);
                                commandInfo.GameInstance.AbortGame();
                                returnMessage = "Cancelling current running game!";
                            }
                        }
                    }
                    await LogAndReplyAsync(returnMessage);
                }
            }
            catch (Exception ex)
            {
                await Logger.Log(new LogMessage(LogSeverity.Error, "CancelGame", "Unexpected Exception", ex));
            }
        }

        [Command("Promote"), Summary("Promotes user to next rank.")]
        public async Task Promote(SocketGuildUser user)
        {
            if (CheckModAccessBitHeroesGuild())
            {
                var guildRoleList = Context.Guild.Roles.OrderBy(x => x.Position).ToList();
                int curentRole = 0;
                int NextRole = 3;
                var userRoleList = user.Roles;

                foreach (var role in userRoleList)
                {
                    if (role.Position == 23) return;
                    if ((role.Position > 2 && role.Position < 8) || (role.Position > 18 && role.Position < 23))
                    {
                        curentRole = role.Position;
                        if (role.Position == 7) NextRole = 19;
                        else NextRole = role.Position + 1;
                    }
                }
                await user.RemoveRoleAsync(guildRoleList[curentRole]);
                await user.AddRoleAsync(guildRoleList[NextRole]);
                await Context.Message.DeleteAsync();
            }
        }
        [Command("hgBan"), Summary("Ban user from HG games")]
        public async Task HgBan(IUser user)
        {
            if (CheckModAccessBitHeroesGuild())
            {
                BanListHandler banList = new BanListHandler();
                banList.AddUserToBannedList(user.Id);
                await Context.Channel.SendMessageAsync("User banned.");
            }
        }
        [Command("hgUnban"), Summary("unBan user from HG games")]
        public async Task HgUnban(IUser user)
        {
            if (CheckModAccessBitHeroesGuild())
            {
                BanListHandler banList = new BanListHandler();
                banList.UnbanUserFromBannedList(user.Id);
                await Context.Channel.SendMessageAsync("User unbanned.");
            }
        }

        [Command("ping"), Summary("unBan user from HG games")]
        public async Task PersonalPing(ulong userId)
        {
            //await Context.Message.Author.SendMessageAsync("Test ping");
            if (Context.Message.Author.Id == 325186783367266306 || Context.Message.Author.Id == 195567858133106697)
            {
                await Bot.GetUser(userId).SendMessageAsync("Ping sent with love by: " + Context.Message.Author.Username);
                await Context.Message.AddReactionAsync(new Emoji("😃"));
            }
        }

        [Command("roll"), Summary("roll a number between 1 and mentioned number")]
        public async Task RngRoll()
        {
            Random _random = new Random(Guid.NewGuid().GetHashCode());
            int finalNumber = _random.Next(1000) + 1;
            await Context.Channel.SendMessageAsync($" <@{ Context.Message.Author.Id}>  rolled: **" + finalNumber.ToString() + "**");
        }

        [Command("Demote"), Summary("Demotes user to next rank.")]
        public async Task Demote(SocketGuildUser user)
        {
            if (CheckModAccessBitHeroesGuild())
            {
                var guildRoleList = Context.Guild.Roles.OrderBy(x => x.Position).ToList();
                int curentRole = 0;
                int NextRole = 0;
                var userRoleList = user.Roles;

                foreach (var role in userRoleList)
                {
                    if ((role.Position > 3 && role.Position < 8) || (role.Position > 18 && role.Position < 24))
                    {
                        curentRole = role.Position;
                        if (role.Position == 19) NextRole = 7;
                        else NextRole = role.Position - 1;
                    }
                }
                await user.RemoveRoleAsync(guildRoleList[curentRole]);
                await user.AddRoleAsync(guildRoleList[NextRole]);
                await Context.Message.DeleteAsync();
            }
        }

        [Command("StartNow"), Summary("Start game before end of timer.")]
        public async Task StartNow()
        {
            try
            {
                if (CheckAccess())
                {
                    string returnMessage = "No Game is currently running!";
                    lock (SyncObj)
                    {
                        RunningCommandInfo commandInfo = GetRunningCommandInfo(Context.Channel.Id);
                        if (commandInfo?.GameInstance != null)
                        {
                            if (Context.Message.Author.Id == commandInfo.UserId)
                            {
                                commandInfo.GameInstance.StartGameSooner();
                                returnMessage = "Starting game sooner.";
                            }
                        }
                    }
                    await LogAndReplyAsync(returnMessage);
                }
            }
            catch (Exception ex)
            {
                await Logger.Log(new LogMessage(LogSeverity.Error, "StartGameSooner", "Unexpected Exception", ex));
            }
        }

        /*
                [RequireBotPermission(GuildPermission.ManageMessages)]
                [RequireUserPermission(GuildPermission.ManageMessages)]
        */
        [Command("cleanup", RunMode = RunMode.Async), Summary("Provides Help with commands.")]
        public async Task CleanUp()
        {
            bool cleanupCommandInstance = false;
            try
            {
                //Logger.LogInternal($"Command 'CleanUp' executed by '{Context.Message.Author.Username}'");
                if (CheckAccess())
                {
                    RunningCommandInfo commandInfo;
                    if (CreateChannelCommandInstance("CleanUp", Context.User.Id, Context.Channel.Id, Context.Guild.Id, null, out commandInfo))
                    {
                        cleanupCommandInstance = true;
                        var botUser = Context.Client.CurrentUser;
                        var result = await Context.Channel.GetMessagesAsync().Flatten();
                        List<IMessage> messagesToDelete = new List<IMessage>();
                        if (result != null)
                        {
                            foreach (var message in result)
                            {
                                if (message.Author.Id == botUser.Id)
                                {
                                    messagesToDelete.Add(message);
                                }
                            }
                            if (messagesToDelete.Count > 0)
                            {
                                await Context.Channel.DeleteMessagesAsync(messagesToDelete);
                                if (messagesToDelete.Count == 1)
                                {
                                    Logger.Log("Deleted 1 message.");
                                    //await LogAndReplyAsync("Deleted 1 message.");
                                }
                                else
                                {
                                    Logger.Log($"Deleted {messagesToDelete.Count} messages.");
                                    //await LogAndReplyAsync($"Deleted {messagesToDelete.Count} messages.");
                                }
                                return;
                            }
                        }
                        Logger.Log("No messages to delete.");
                        //await LogAndReplyAsync("No messages to delete.");
                    }
                    else
                    {
                        try
                        {
                            await LogAndReplyAsync($"The '{commandInfo.CommandName}' command is currently running!.  Can't run this command until that finishes");
                        }
                        catch (Exception ex)
                        {
                            await Logger.Log(new LogMessage(LogSeverity.Error, "CleanUp", "Unexpected Exception", ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.Log(new LogMessage(LogSeverity.Error, "CleanUp", "Unexpected Exception : " + ex.ToString(), ex));
            }
            finally
            {
                try
                {
                    if (cleanupCommandInstance)
                        RemoveChannelCommandInstance(Context.Channel.Id);
                }
                catch (Exception ex)
                {
                    await Logger.Log(new LogMessage(LogSeverity.Error, "CleanUp", "Unexpected Exception", ex));
                }
            }
        }

        [Command("startGame"), Summary("Starts the BHungerGames.")]
        public async Task StartGame()
        {
            if (!NotInBHGeneral()) return;
            //Logger.LogInternal($"Command 'startGame <Help>' executed by '{Context.Message.Author.Username}'");

            if (CheckAccess(true))
            {
                await ShowGameHelp();
            }
        }

        [Command("startv2"), Summary("input")]
        public async Task PvP()
        {
            if (!NotInBHGeneral()) return;
            if (CheckAccess())
            {
                Thread.Sleep(5000);
                await ReplyAsync("```md\n * FATAL ERROR CODE 104 *:\nBHungerGamesV2.cs could not be found.```");
                Thread.Sleep(5000);
                await ReplyAsync("```Have SSS1 and Shadown88 bamboozled all of you? Could they have pretended to create a great game but only brought memes?```");
                Thread.Sleep(8000);
                await ReplyAsync("```jk.... it's the wrong command bros... hehe\n\nBot humour, get it? alright, alright, enjoy the event. *beep boop*```");

            }
        }
        [Command("StartV3", RunMode = RunMode.Async), Summary("STart the BitHeroes BattleGriund")]
        public async Task BHBG([Summary("Max minutes to wait for players")]string strMaxMinutesToWait = null,
            [Summary("Max User that can play")]string strMaxTurns = null,
            [Summary("Number of Winners")]string strMaxScore = null
        )
        {
            if (!NotInBHGeneral()) return;
            if (CheckAccess())
            {
                BotV3GameInstance gameInstance = new BotV3GameInstance();
                await StartGameInternal(gameInstance, strMaxMinutesToWait, strMaxTurns, strMaxScore, 0);
            }
        }


        [Command("giveAway", RunMode = RunMode.Async), Summary("Starts a giveaway.")]
        public async Task GiveAway(
            [Summary("Max minutes to wait for players")]string strMaxSecToWait = null,
            [Summary("Target number")]string strTargetNumber = null,
            [Summary("Number of Winners")]string strNumWinners = null,
            [Summary("Number of Winners")]string strTestUsers = null)
        {
            GiveawayInstance gameInstance = new GiveawayInstance();
            await StartGiveAway(gameInstance, strMaxSecToWait, strTargetNumber, strNumWinners, strTestUsers);
        }

        [Command("startGame", RunMode = RunMode.Async), Summary("Starts the BHungerGames.")]
        public async Task StartGame([Summary("Max User that can play")]string strMaxUsers,
            [Summary("Max minutes to wait for players")]string strMaxMinutesToWait = null,
            [Summary("Seconds to delay between displaying next day")]string strSecondsDelayBetweenDays = null,
            [Summary("Number of Winners")]string strNumWinners = null)
        {
            if (!NotInBHGeneral()) return;
            BotGameInstance gameInstance = new BotGameInstance();
            await StartGameInternal(gameInstance, strMaxUsers, strMaxMinutesToWait, strSecondsDelayBetweenDays, strNumWinners, 0);
        }

        [Command("startGameT", RunMode = RunMode.Async), Summary("Starts the BHungerGames.")]
        public async Task StartGameT([Summary("Max User that can play")]string strMaxUsers,
            [Summary("Max minutes to wait for players")]string strMaxMinutesToWait = null,
            [Summary("Seconds to delay between displaying next day")]string strSecondsDelayBetweenDays = null,
            [Summary("Number of Winners")]string strNumWinners = null)
        {
            BotGameInstance gameInstance = new BotGameInstance();
            await StartGameInternal(gameInstance, strMaxUsers, strMaxMinutesToWait, strSecondsDelayBetweenDays, strNumWinners, 300);
        }

        [Command("StartV2", RunMode = RunMode.Async), Summary("Start the Hunger Games V2")]
        public async Task StartV2([Summary("Max User that can play")]string strMaxUsers,
            [Summary("Max minutes to wait for players")]string strMaxMinutesToWait = null,
            [Summary("Number of Winners")]string strNumWinners = null
        )
        {
            // if (CheckAccessForBitHeroesGuildOnly())
            // {
            if (!NotInBHGeneral()) return;
            BotV2GameInstance gameInstance = new BotV2GameInstance();
            await StartGameInternal(gameInstance, strMaxUsers, strMaxMinutesToWait, "1", strNumWinners, 0);
            //}
        }

        protected async Task StartGameInternal(BotGameInstance gameInstance, string strMaxUsers, string strMaxMinutesToWait, string strSecondsDelayBetweenDays, string strNumWinners, int testUsers)
        {
            bool cleanupCommandInstance = false;
            try
            {
                Logger.LogInternal($"G:{Context.Guild.Name}  Command " + (testUsers > 0 ? "T" : "") + $"'startGame' executed by '{Context.Message.Author.Username}'");

                if (CheckAccess())
                {
                    RunningCommandInfo commandInfo;
                    if (CreateChannelCommandInstance("StartGame", Context.User.Id, Context.Channel.Id, Context.Guild.Id, gameInstance, out commandInfo))
                    {
                        cleanupCommandInstance = true;
                        int maxUsers;
                        int maxMinutesToWait;
                        int secondsDelayBetweenDays;
                        int numWinners;

                        if (Int32.TryParse(strMaxUsers, out maxUsers) == false) maxUsers = 100;
                        if (Int32.TryParse(strMaxMinutesToWait, out maxMinutesToWait) == false) maxMinutesToWait = 5;
                        if (Int32.TryParse(strSecondsDelayBetweenDays, out secondsDelayBetweenDays) == false) secondsDelayBetweenDays = 10;
                        if (Int32.TryParse(strNumWinners, out numWinners) == false) numWinners = 1;
                        if (numWinners <= 0) numWinners = 1;
                        if (maxMinutesToWait <= 0) maxMinutesToWait = 1;
                        if (secondsDelayBetweenDays <= 0) secondsDelayBetweenDays = 5;
                        if (maxUsers <= 0) maxUsers = 1;


                        SocketGuildUser user = Context.Message.Author as SocketGuildUser;
                        string userThatStartedGame = user?.Nickname ?? Context.Message.Author.Username;
                        await gameInstance.StartGame(numWinners, maxUsers, maxMinutesToWait, secondsDelayBetweenDays, Context, userThatStartedGame, user.Id, testUsers);
                        cleanupCommandInstance = false;
                        //await Context.Channel.SendMessageAsync($"MaxUsers: {maxUsers}  MaxMinutesToWait: {maxMinutesToWait} SecondsDelayBetweenDays: {secondsDelayBetweenDays} NumWinners: {numWinners}");
                    }
                    else
                    {
                        try
                        {
                            await LogAndReplyAsync($"The '{commandInfo.CommandName}' command is currently running!.  Can't run this command until that finishes");
                        }
                        catch (Exception ex)
                        {
                            await Logger.Log(new LogMessage(LogSeverity.Error, "StartGameInternal", "Unexpected Exception", ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.Log(new LogMessage(LogSeverity.Error, "StartGame", "Unexpected Exception", ex));
            }
            finally
            {
                try
                {
                    if (cleanupCommandInstance)
                        RemoveChannelCommandInstance(Context.Channel.Id);
                }
                catch (Exception ex)
                {
                    await Logger.Log(new LogMessage(LogSeverity.Error, "StartGame", "Unexpected Exception in Finally", ex));
                }
            }
        }
        protected async Task StartGameInternal(BotGameInstance gameInstance, string strMaxMinutesToWait, string strMaxTurns, string strMaxScore, int testUsers)
        {
            bool cleanupCommandInstance = false;
            try
            {
                Logger.LogInternal($"G:{Context.Guild.Name}  Command " + (testUsers > 0 ? "T" : "") + $"'startGame' executed by '{Context.Message.Author.Username}'");

                if (CheckAccess(true)) // CHANGE HERE FOR LAUCNH REMOVE TRUE
                {
                    RunningCommandInfo commandInfo;
                    if (CreateChannelCommandInstance("StartGame", Context.User.Id, Context.Channel.Id, Context.Guild.Id, gameInstance, out commandInfo))
                    {
                        cleanupCommandInstance = true;
                        //int maxUsers;
                        int maxMinutesToWait;
                        int maxScore;
                        int maxTurn;


                        if (Int32.TryParse(strMaxMinutesToWait, out maxMinutesToWait) == false) maxMinutesToWait = 5;
                        if (Int32.TryParse(strMaxScore, out maxScore) == false) maxScore = 1000000000;
                        if (Int32.TryParse(strMaxTurns, out maxTurn) == false) maxTurn = 25;
                        if (maxTurn <= 0) maxTurn = 1;
                        if (maxTurn >= 200) maxTurn = 200;
                        if (maxMinutesToWait <= 0) maxMinutesToWait = 1;
                        if (maxScore <= 0) maxScore = 1000000000;
                        //if (maxUsers <= 0) maxUsers = 1;


                        SocketGuildUser user = Context.Message.Author as SocketGuildUser;
                        string userThatStartedGame = user?.Nickname ?? Context.Message.Author.Username;
                        gameInstance.StartGame(maxTurn, maxMinutesToWait, maxScore, Context, userThatStartedGame, testUsers);
                        cleanupCommandInstance = false;
                        //await Context.Channel.SendMessageAsync($"MaxUsers: {maxUsers}  MaxMinutesToWait: {maxMinutesToWait} SecondsDelayBetweenDays: {secondsDelayBetweenDays} NumWinners: {numWinners}");
                    }
                    else
                    {
                        try
                        {
                            await LogAndReplyAsync($"The '{commandInfo.CommandName}' command is currently running!.  Can't run this command until that finishes");
                        }
                        catch (Exception ex)
                        {
                            await Logger.Log(new LogMessage(LogSeverity.Error, "StartGameInternal", "Unexpected Exception", ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.Log(new LogMessage(LogSeverity.Error, "StartGame", "Unexpected Exception", ex));
            }
            finally
            {
                try
                {
                    if (cleanupCommandInstance)
                        RemoveChannelCommandInstance(Context.Channel.Id);
                }
                catch (Exception ex)
                {
                    await Logger.Log(new LogMessage(LogSeverity.Error, "StartGame", "Unexpected Exception in Finally", ex));
                }
            }
        }

        protected async Task StartGiveAway(GiveawayInstance giveawayInstance, string strSecondsToWait, string strTargetNumber, string strNumWinners, string strTestUsers)
        {
            bool cleanupCommandInstance = false;
            try
            {
                Logger.LogInternal($"G:{Context.Guild.Name}  Command " +  $"'giveAway' executed by '{Context.Message.Author.Username}'");
                RunningCommandInfo commandInfo;
                if (CreateChannelCommandInstance("GiveAway", Context.User.Id, Context.Channel.Id, Context.Guild.Id, giveawayInstance, out commandInfo))
                {
                    cleanupCommandInstance = true;
                    int maxSecsToWait;
                    int targetNumber;

                    int numWinners;
                    int testUsers;

                    if (Int32.TryParse(strSecondsToWait, out maxSecsToWait) == false) maxSecsToWait = 5;
                    if (Int32.TryParse(strTargetNumber, out targetNumber) == false) targetNumber = 1000;
                    if (Int32.TryParse(strNumWinners, out numWinners) == false) numWinners = 1;
                    if (Int32.TryParse(strTestUsers, out testUsers) == false) testUsers = 0;
                    if (targetNumber > 1000) targetNumber = 1000;
                    if (targetNumber <= 0) targetNumber = 1;
                    if (numWinners <= 0) numWinners = 1;
                    if (maxSecsToWait <= 0) maxSecsToWait = 1;


                    SocketGuildUser user = Context.Message.Author as SocketGuildUser;
                    string userThatStartedGame = user?.Nickname ?? Context.Message.Author.Username;
                    await giveawayInstance.RunGiveaway(numWinners, maxSecsToWait, targetNumber, Context, userThatStartedGame, testUsers);
                    cleanupCommandInstance = false;
                    //await Context.Channel.SendMessageAsync($"MaxUsers: {maxUsers}  MaxMinutesToWait: {maxMinutesToWait} SecondsDelayBetweenDays: {secondsDelayBetweenDays} NumWinners: {numWinners}");
                }
                else
                {
                    try
                    {
                        await LogAndReplyAsync($"The '{commandInfo.CommandName}' command is currently running!.  Can't run this command until that finishes");
                    }
                    catch (Exception ex)
                    {
                        await Logger.Log(new LogMessage(LogSeverity.Error, "StartGameInternal", "Unexpected Exception", ex));
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.Log(new LogMessage(LogSeverity.Error, "StartGame", "Unexpected Exception", ex));
            }
            finally
            {
                try
                {
                    if (cleanupCommandInstance)
                        RemoveChannelCommandInstance(Context.Channel.Id);
                }
                catch (Exception ex)
                {
                    await Logger.Log(new LogMessage(LogSeverity.Error, "StartGame", "Unexpected Exception in Finally", ex));
                }
            }
        }
    }
}
