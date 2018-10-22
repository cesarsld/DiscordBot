using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Linq;
using Discord;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class AxieConquerGame
    {
        #region Variables
        private volatile bool _ignoreReactions;

        private static string GobbyImage;

        public const int NumItemTypes = 4;

        private const int DelayValue = 1;

        private static readonly TimeSpan DelayBetweenCycles;
        private static readonly TimeSpan DelayAfterOptions;
        private static readonly int[] ShowPlayersWhenCountEqual;
        private static readonly ReadOnlyCollection<IEmote> EmojiClassListOptions;
        private static readonly ReadOnlyCollection<IEmote> EmojiAdventureListOption;
        private static readonly ReadOnlyCollection<IEmote> EmojiListCrowdDecision;
        private static readonly List<ulong> BannedPlayers;

        private static readonly ScenarioRPG[] LootScenarios;
        private static readonly ScenarioRPG[] TrainScenarios;

        private readonly Random _random;
        private readonly HashSet<AxiePlayer> _goblinPlayers;
        private readonly HashSet<AxiePlayer> _duelImmune;
        private List<AxiePlayer> _players;
        private bool _enhancedOptions;
        private bool _crowdOptions;
        private bool _goblinHunt;
        private bool _worldBoss;
        private bool _classInit;
        private HeroClass _adventureAffinity;
        public static Dictionary<HeroClass, List<float>> HeroScalingDictionary;
        public static Dictionary<HeroClass, List<int>> HeroBaseStatsDictionary;
        public static Dictionary<HeroClass, List<int>> HeroLevelUpDictionary;
        private int _reactionA;
        private int _reactionB;
        #endregion


        public AxieConquerGame()
        {
        }

        public async Task Run(int numWinners, List<Player> contestants, BotGameInstance.ShowMessageDelegate showMessageDelegate, BotGameInstance.ShowMessageDelegate sendMsg, Func<bool> cancelGame)
        {
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

            _players = new List<AxiePlayer>();
            foreach (Player player in contestants)
            {
                AxiePlayer axiePlayer = player as AxiePlayer;
                if (axiePlayer != null)
                {
                    _players.Add(axiePlayer);
                }
            }
            await Task.Delay(1);
        }
    }
}
