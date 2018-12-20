using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Mongo;
using System.Collections.Generic;

namespace DiscordBot
{
    public class Bot
    {
        /*
         URL to add bot: https://discordapp.com/api/oauth2/authorize?client_id=322530501774540800&scope=bot&permissions=0
        Test Info:
         URL to add bot: https://discordapp.com/api/oauth2/authorize?client_id=326010315760205834&scope=bot&permissions=0
        */
        public static string AppName = "BHungerGamesBot";
        public static string AppVersion = "1.0.1.0";


        public static char CommandPrefix = '>';

        public static DiscordSocketClient DiscordClient { get; set; }
        private readonly CommandService _commands;
        //private IServiceProvider _services;

        /// <summary>
        /// Constructor for the Bot class.
        /// </summary>
        public Bot()
        {
            if (DiscordClient != null)
            {
                throw new Exception("Bot already running");
            }
            Logger.LogInternal("Creating client.");
            DiscordClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000,

                // If your platform doesn't have native websockets,
                // add Discord.Net.Providers.WS4Net from NuGet,
                // add the `using` at the top, and uncomment this line:
                //WebSocketProvider = WS4NetProvider.Instance
            });

            _commands = new CommandService();
            DiscordClient.Log += Logger.Log;
            DiscordClient.MessageReceived += HandleCommandAsync;
            DiscordClient.Ready += Axie.Web3Axie.AxieDataGetter.GetData;
            var id = DiscordClient.ConnectionState;
            
        }

        ~Bot()
        {
            DiscordClient = null;
        }

        private string GetUserName(SocketUser socketUser)
        {
            string userName = "NULL";
            try
            {
                if (socketUser != null)
                {
                    userName = socketUser.ToString();
                    SocketGuildUser user = socketUser as SocketGuildUser;
                    if (user?.Nickname != null)
                    {
                        userName += " NickName: " + user.Nickname;
                    }
                }

            }
            catch { }
            return userName;
        }

        public static IUser GetUser(ulong userId) => DiscordClient.GetUser(userId);

        public static SocketChannel GetChannelContext(ulong channelId) => DiscordClient.GetChannel(channelId);
        


        public async Task HandleCommandAsync(SocketMessage messageParam)
        {
            string userName = "";
            string channelName = "";
            string guildName = "";
            try
            {
                var msg = messageParam as SocketUserMessage;
                if (msg == null) return;

                var context = new CommandContext(DiscordClient, msg);
                guildName = context.Guild?.Name ?? "NULL";

                int argPos = 0;
                if(context.Guild?.Id == 410537146672349205 /*remove 1 at the end*/ || context.Guild?.Id == 329959863545364480)
                {
                    await CheckIfMPFormat(msg);
                }
                if (msg.HasCharPrefix(CommandPrefix, ref argPos)) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */
                {
                    userName = GetUserName(msg.Author);
                    channelName = msg.Channel?.Name ?? "NULL";

                    //var context = new SocketCommandContext(DiscordClient, msg);
                    //var context = new CommandContext(DiscordClient, msg);
                    guildName = context.Guild?.Name ?? "NULL";
                    Logger.LogInternal($"HandleCommandAsync G: {guildName} C: {channelName} User: {userName}  Msg: {msg}");

                    var result = await _commands.ExecuteAsync(context, argPos);

                    if (!result.IsSuccess) // If execution failed, reply with the error message.
                    {
                        string message = "Command Failed: " + result;
                        await Logger.Log(new LogMessage(LogSeverity.Error, "HandleCommandAsync", message));
                        //await context.Channel.SendMessageAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    await Logger.Log(new LogMessage(LogSeverity.Error, "HandleCommandAsync", $"G:{guildName} C:{channelName} U:{userName} Unexpected Exception", ex));
                }
                catch
                {
                }
            }
        }

        private async Task CheckIfMPFormat(SocketUserMessage msg)
        {
            if (msg.Content.Contains("https://axieinfinity"))
            {
                var copy = msg.Content;
                var index = 0;
                var indexList = new List<int>();
                while (index != -1)
                {
                    var occurence = copy.IndexOf("https://axieinfinity", index);
                    if (occurence != -1)
                    {
                        indexList.Add(occurence);
                        index = occurence + 1;
                    }
                    else index = occurence;
                }
                foreach (var i in indexList)
                {
                    if (i == 0)
                    {
                        await msg.Author.SendMessageAsync("Your message has been deleted due to forgetting to put <link> in between you link(s)! " +
                                                         $"Here is the content written: \n {copy}");
                        await msg.DeleteAsync();
                        return;
                    }
                    else
                    {
                        if (copy[i - 1] != '<')
                        {
                            await msg.Author.SendMessageAsync("Your message has been deleted due to forgetting to put <link> in between you link(S)! " +
                                                             $"Here is the content written: \n {copy}");
                            await msg.DeleteAsync();
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Start the Discord client.
        /// </summary>
        public async Task RunAsync()
        {
            Logger.LogInternal("Registering commands.");
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            Logger.LogInternal("Connecting to the server.");
            await DiscordClient.LoginAsync(TokenType.Bot, DiscordKeyGetter.GetKey());
            await DiscordClient.StartAsync();
            await Task.Delay(-1);
        }
    }
}
