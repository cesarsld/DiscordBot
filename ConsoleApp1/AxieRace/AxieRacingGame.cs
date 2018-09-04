using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Linq;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace DiscordBot.AxieRace
{
    class AxieRacingGame
    {
        #region Variables
        private volatile bool _ignoreReactions;

        public const int NumItemTypes = 4;

        private const int DelayValue = 1;

        private static readonly TimeSpan DelayBetweenCycles;
        private static readonly TimeSpan DelayAfterOptions;
        private static readonly int[] ShowPlayersWhenCountEqual;
        private static readonly ReadOnlyCollection<IEmote> EmojiRaceListOption;
        private static readonly ReadOnlyCollection<IEmote> EmojiAdventureListOption;
        private static readonly ReadOnlyCollection<IEmote> EmojiListCrowdDecision;
        private static readonly List<ulong> BannedPlayers;
        private readonly ulong gameId;
        private readonly Random _random;
        private readonly HashSet<AxieRacer> _duelImmune;
        private List<AxieRacer> _players;
        private bool acceptQualifierData;
        private RaceConfig raceConfig;
        #endregion


        public AxieRacingGame(ulong _gameId)
        {
            _random = new Random(Guid.NewGuid().GetHashCode());
            gameId = _gameId;
        }
        static AxieRacingGame()
        {

            EmojiRaceListOption = new ReadOnlyCollection<IEmote>(new List<IEmote> { new Emoji("🇦"), new Emoji("🇧"), new Emoji("🇨") });
        }

        public List<AxieRacer> GetPlayers() => _players;
        public bool GetPrepTimeStatus() => acceptQualifierData;
        public RaceConfig GetRaceConfig() => raceConfig;

        public async Task Run(List<Player> contestants, BotGameInstance.ShowMessageDelegate showMessageDelegate, BotGameInstance.ShowMessageDelegate sendMsg, Func<bool> cancelGame)
        {
            StringBuilder sb = new StringBuilder();
            int PrepTime = 120;
            int time = 0;
            bool timeIsUp = false;
            int raceLaps = 5;
            #region Init
            List<AxiePlayer> participants = new List<AxiePlayer>();
            List<ulong> bannedList = new BanListHandler().GetBanList();
            List<Player> bannedPlayersToRemove = new List<Player>();
            foreach (Player player in contestants)
            {
                if (bannedList.Contains(player.UserId))
                {
                    bannedPlayersToRemove.Add(player);
                }
            }
            contestants = contestants.Except(bannedPlayersToRemove).ToList();
            if (bannedPlayersToRemove.Count > 0)
            {
                showMessageDelegate($"Number of banned players attempting to join game:{bannedPlayersToRemove.Count}\r\n");
            }

            _players = new List<AxieRacer>();
            foreach (Player player in contestants)
            {
                AxieRacer axiePlayer = player as AxieRacer;
                if (axiePlayer != null)
                {
                    _players.Add(axiePlayer);
                }
            }
            #endregion

            raceConfig = SetRaceConfig();
            acceptQualifierData = true;
            showMessageDelegate($"You have * {PrepTime} seconds and 6 practice runs * to submit your setup for the race!\n"
                               +"Please send your setups via DM to the bot using this command: \n"
                               + $">axie SetRacerData {gameId} <axieClass> <pace> <awareness> <diet>\n"
                               + "Parametres with brackets will need to be replaced. \nPace, awareness and diet are variables ranging from 0 to 100. Classes are <beast, plant, aqua, bird, bug, retpile>");
            while (!timeIsUp)
            {
                if (cancelGame()) return;
                time++;
                timeIsUp = time >= PrepTime;

                await Task.Delay(1000);
            }
            acceptQualifierData = false;
            //qualifiers
            RunQualifiers();
            _players = _players.OrderBy(player => player.QualifierRun).ToList();
            sb.Append("QUALIFICATION LEADERBOARD\n\n");
            for (int i = 0; i < _players.Count; i++)
            {
                sb.Append($"{i + 1}. {_players[i].NickName} || Axie type = {_players[i].GetClass()} || Lap time = * {_players[i].QualifierRun} * seconds\n");
            }
            showMessageDelegate("" + sb, null);
            sb.Clear();
            for (int i = 0; i < raceLaps; i++)
            {
                _ignoreReactions = false;
                showMessageDelegate("NEW LAP\n=======\nInput your axie's race style for this new lap", null, EmojiRaceListOption);
                await Task.Delay(15000);
                _ignoreReactions = true; ;
                //List<axier>
                foreach (var player in _players)
                {
                    //add aggressivity
                    player.raceLapTime = Convert.ToInt32(raceConfig.RandomRaceTime * (1 + Math.Pow((player.totalScore / 400), 0.5)) * ((float)_random.Next(90, 111) / 100));
                    player.totalRaceTime += player.raceLapTime;
                }
                //sendMsg("hi");
                _players = _players.OrderBy(player => player.QualifierRun).ToList();
                //compute race duels
                sb.Append("RACE LEADERBOARD\n\n");
                for (int j = 0; j < _players.Count; j++)
                {
                    sb.Append($"{j + 1}. {_players[j].NickName} || Axie type = {_players[j].GetClass()} || total time = * {_players[j].totalRaceTime} * seconds || Last lap time = * {_players[j].raceLapTime} *\n");
                }
                showMessageDelegate("" + sb, null);
                sb.Clear();
            }
            sendMsg("hi, it's game over for now");
        }

        private void ComputeRaceDuels(int index)
        {
            for (int i = index; i < _players.Count - 1; i++)
            {
                int positionDiff = _players[i + 1].totalRaceTime - _players[i].totalRaceTime;
                if (positionDiff < 5)
                {
                    int firstIndex = i;
                    int lastIndex = GetLastIndexOfCLuster(i + 1);
                    //do stuff

                    i = lastIndex;
                }
            }
        }

        private void RaceDuel(int first, int last)
        {
            int[] duelRanking = new int[last - first + 1];

            for (int i = 0; i < duelRanking.Length; i++)
            {
                duelRanking[i] = _random.Next(_players[first + i].totalScore + 1000);
            }
        }

        private int GetLastIndexOfCLuster(int currentIndex)
        {
            int positionDiff = _players[currentIndex + 1].totalRaceTime - _players[currentIndex].totalRaceTime;
            if (positionDiff < 5) return GetLastIndexOfCLuster(currentIndex + 1);
            else return currentIndex;
        }

        private void RunQualifiers()
        {
            foreach (var player in _players)
            {
                int classAdvantage = GetClassAdvantage(player.GetClass());
                int paceAdvantage = GetParamAdvantage(player.GetPace(), raceConfig.Pace);
                int awarenessAdvantage = GetParamAdvantage(player.GetAwareness(), raceConfig.Awaraness);
                int dietAdvantage = GetParamAdvantage(player.GetDiet(), raceConfig.Diet);
                int finalScore = classAdvantage + paceAdvantage + awarenessAdvantage + dietAdvantage;
                if (finalScore == 0) finalScore = 1;
                player.totalScore = finalScore;
                player.QualifierRun = Convert.ToInt32(raceConfig.RandomRaceTime * (400 / (float)player.totalScore) * ((float)_random.Next(90, 111) / 100));
            }
        }

        private RaceConfig SetRaceConfig()
        {
            RaceConfig _raceConfig = new RaceConfig((AxieClass)(_random.Next(6) + 1),
                                                               _random.Next(101),
                                                               _random.Next(101),
                                                               _random.Next(101),
                                                               _random.Next(50, 190),
                                                               ReturnRandomUniqueSequence(6));
            Console.WriteLine($"Class : {_raceConfig.classRanking[0]}|| Pace : {_raceConfig.Pace}|| Awareness : {_raceConfig.Awaraness}|| Diet : {_raceConfig.Diet}|| RaceTime : {_raceConfig.RandomRaceTime}");
            return _raceConfig;
        }

        public async Task TestConfig(ICommandContext context, AxieRacer player)
        {
            if (player.canPractice)
            {
                int hintAmountRoll = _random.Next(100);
                int hintAmount = 1;
                if (hintAmountRoll > 90) hintAmount = 4;
                else if (hintAmountRoll > 70) hintAmount = 3;
                else if (hintAmountRoll > 40) hintAmount = 2;

                int classAdvantage = GetClassAdvantage(player.GetClass());
                //int paceAdvantage = GetParamAdvantage(player.GetPace(), raceConfig.Pace);
                //int awarenessAdvantage = GetParamAdvantage(player.GetAwareness(), raceConfig.Awaraness);
                //int dietAdvantage = GetParamAdvantage(player.GetDiet(), raceConfig.Diet);

                StringBuilder statMessage = new StringBuilder();
                int[] hint = ReturnRandomUniqueSequence(4);
                hint = hint.Take(hintAmount).OrderBy(i => i).ToArray();
                for (int i = 0; i < hintAmount; i++)
                {
                    switch (hint[i])
                    {
                        case 0:
                            statMessage.Append(GetClasshint(classAdvantage));
                            break;
                        case 1:
                            statMessage.Append(GetHint("pace", player.GetPace(), raceConfig.Pace));
                            break;
                        case 2:
                            statMessage.Append(GetHint("awareness", player.GetAwareness(), raceConfig.Awaraness));
                            break;
                        case 3:
                            statMessage.Append(GetHint("diet", player.GetDiet(), raceConfig.Diet));
                            break;
                    }
                    statMessage.AppendLine();
                }
                await context.Channel.SendMessageAsync(statMessage.ToString());

            }
            else await context.Channel.SendMessageAsync("You have used all you practice runs :/");
        }

        private string GetClasshint(int classAdvantage)
        {
            string message = "Your choice of axie is ";
            string diffAdjective = "";
            switch (classAdvantage)
            {
                case 0:
                    diffAdjective = "the worst";
                    break;
                case 20:
                    diffAdjective = "pretty bad";
                    break;
                case 40:
                case 60:
                    diffAdjective = "average";
                    break;
                case 80:
                    diffAdjective = "decent";
                    break;
                case 100:
                    diffAdjective = "optimal";
                    break;
            }

            return message + diffAdjective + " for this race.";
        }

        private string GetHint(string paramName, int paramValue, int trueValue)
        {
            string diffAdjective = "";
            int diff = Math.Abs(paramValue - trueValue);
            if (diff > 40) diffAdjective = " drastically.";
            else if (diff > 30) diffAdjective = " a lot.";
            else if (diff > 20) diffAdjective = " some amount.";
            else if (diff > 10) diffAdjective = " a tiny bit.";
            else if (diff > 5) return $"Your {paramName} value is near perfect. :D";
            string message = $"Your {paramName} value needs to " + (paramValue > trueValue? "decrease" : "increase") + diffAdjective;
            return message;
        }

        private int GetClassAdvantage(AxieClass axieClass)
        {
            for (int i = 0; i < raceConfig.classRanking.Length; i++)
            {
                if ((int)axieClass == raceConfig.classRanking[i])
                {
                    return i * 20;
                }
            }
            return 0;
        }

        private int GetParamAdvantage(int userParam, int raceParam)
        {
            int diff = Math.Abs(userParam - raceParam);
            return Convert.ToInt32(100 - Math.Pow(diff, 2) / 25);
        }

        public int[] ReturnRandomUniqueSequence(int size)
        {
            int[] array = new int[size];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }
            int r = 0;
            for (int i = array.Length - 1; i >= 0; i--)
            {
                r = new Random(Guid.NewGuid().GetHashCode()).Next(array.Length);
                if (r != i)
                {
                    array[i] ^= array[r];
                    array[r] ^= array[i];
                    array[i] ^= array[r];
                }
            }
            return array;
        }

        public void HandlePlayerInput(ulong userId, string reactionName)
        {
            if (_ignoreReactions) return;

            var authenticPlayer = _players.FirstOrDefault(contestant => contestant.UserId == userId);
            if (authenticPlayer != null)
            {
                switch (reactionName)
                { //🇦 🇧 🇨 🇩 🇪 🇫 🇬
                    case "🇦":
                        authenticPlayer.raceStyle = RaceStyle.Conservative;
                        break;
                    case "🇧":
                        authenticPlayer.raceStyle = RaceStyle.Medium_Paced;
                        break;
                    case "🇨":
                        authenticPlayer.raceStyle = RaceStyle.Aggressive;
                        break;
                }
            }
            
        }
    }
}
