using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Linq;
using Discord;
using System.IO;

namespace DiscordBot
{
    class BHungerGamesV3
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
        private readonly HashSet<PlayerRPG> _goblinPlayers;
        private readonly HashSet<PlayerRPG> _duelImmune;
        private List<PlayerRPG> _players;
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

        static BHungerGamesV3()
        {
            DelayBetweenCycles = new TimeSpan(0, 0, 0, 10);
            DelayAfterOptions = new TimeSpan(0, 0, 0, 12);
            GobbyImage = "ArtAssets/gobby.png";

            ShowPlayersWhenCountEqual = new[] { 20, 10, 5, 2, 0 };
            EmojiClassListOptions = new ReadOnlyCollection<IEmote>(new List<IEmote> { new Emoji("🇦"), new Emoji("🇧"), new Emoji("🇨"), new Emoji("🇩"), new Emoji("🇪"), new Emoji("🇫"), new Emoji("🇬") });
            EmojiAdventureListOption = new ReadOnlyCollection<IEmote>(new List<IEmote> { new Emoji("💡"), new Emoji("⚔"), new Emoji("💰"), new Emoji("💪") });
            EmojiListCrowdDecision = new ReadOnlyCollection<IEmote>(new List<IEmote> { new Emoji("🅰"), new Emoji("🅱") });

            BannedPlayers = new List<ulong>
            {
                330768300386811916,
                364038432525123587,
                329225166737506305,
                258046391694131200,
                236912554797039626,
                335303396540284928,
                245575579808563201,
                279285504250347520,
                350345331772227586,
                344238799389327371,
                350801474814476290,
                325465337971736586,
                335613761408860171,
                143446420182138880,
                291315110109118465,
                205020371152404490,
                263330913700544512
            };

            HeroScalingDictionary = new Dictionary<HeroClass, List<float>>()
            {                                           //  STA    STR    AGI    INT   DEX    WIS  LCK
                {HeroClass.Mage,         new List<float>{ 0.75f,  0.5f,  1.2f,  2.5f,   1f,  1.5f,  1f } },
                {HeroClass.Knight,       new List<float>{    2f,  2.5f,    1f,  0.3f, 0.6f, 0.25f,  1f } },
                {HeroClass.Archer,       new List<float>{    1f,    1f,  2.5f,  0.5f,   2f,  0.5f,  1f } },
                {HeroClass.Monk,         new List<float>{ 0.25f, 0.25f, 0.25f,  2.5f, 1.5f,  2.5f,  1f } },
                {HeroClass.Necromancer,  new List<float>{ 0.75f, 0.75f, 0.5f,     3f,   1f,    2f,  1f } },
                {HeroClass.Assassin,     new List<float>{   1f,   1.5f, 2.5f,  0.25f,   2f, 0.25f,  1f } },
                {HeroClass.Elementalist, new List<float>{   1f,     1f,   1f,   2.5f, 2.5f,    1f,  1f } },
            };

            HeroBaseStatsDictionary = new Dictionary<HeroClass, List<int>>()
            {                                        //  STA    STR    AGI    INT   DEX    WIS  LCK
                {HeroClass.Mage,         new List<int>{   15,    15,    10,    60,   25,    45,  15 } }, //170
                {HeroClass.Knight,       new List<int>{   50,    60,    15,    10,   20,    15,  15 } }, //170
                {HeroClass.Archer,       new List<int>{   20,    25,    50,    15,   50,    10,  15 } }, //170
                {HeroClass.Monk,         new List<int>{   10,    10,    10,    50,   30,    60,  15 } }, //170
                {HeroClass.Necromancer,  new List<int>{   20,    20,    10,    60,   20,    40,  15 } }, //170
                {HeroClass.Assassin,     new List<int>{   20,    35,    50,     5,   55,     5,  15 } }, //170
                {HeroClass.Elementalist, new List<int>{   20,    15,    25,    45,   50,    15,  15 } }, //170
            };

            HeroLevelUpDictionary = new Dictionary<HeroClass, List<int>>()
            {                                        //  STA    STR    AGI    INT   DEX    WIS  LCK
                {HeroClass.Mage,         new List<int>{    3,     0,     3,    15,    4,     8,   3 } },
                {HeroClass.Knight,       new List<int>{   10,    11,     5,     2,    3,     2,   3 } },
                {HeroClass.Archer,       new List<int>{    3,     4,    11,     2,   11,     2,   3 } },
                {HeroClass.Monk,         new List<int>{    0,     2,     3,    10,    4,    13,   3 } },
                {HeroClass.Necromancer,  new List<int>{    3,     2,     2,    12,    4,    10,   3 } },
                {HeroClass.Assassin,     new List<int>{    2,     9,     8,     3,   9,      2,   3 } },
                {HeroClass.Elementalist, new List<int>{    3,     4,     6,     8,    9,     3,   3 } },
            };

            LootScenarios = new[]
            {
                new ScenarioRPG("<{player_name}> stumbled across an abandoned sack. Perhaps a captured Bully dropped it? Opening it, they find an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> was wistfully skulking around the pier in town, hoping that fishing was released! All of a sudden, they lost their balance, and fell in! Oh my goodness! Hidden beneath the surface they found an <{item_distribution}> * {rarity_type} * <{class_type}> loot!", RarityRPG.Common),
                new ScenarioRPG("Astaroth is very lonely these days, no one bothers to come see him anymore. He tries to get <{player_name}>s attention by offering them an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("Just when <{player_name}> was about to use some scissors on their credit card, they find an <{item_distribution}> * {rarity_type} * <{class_type}> loot! Baited again!", RarityRPG.Common),
                new ScenarioRPG("While trying to think of a funny HG scenario, <{player_name}> stumbles across an <{item_distribution}> * {rarity_type} * <{class_type}> loot! How ironic. . .", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> defeats Kaleido in a best-of-three to the death arm wrestling competition. For their bravery and strength, they're awarded an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("{player_name} successfully uses all the Discord channels correctly. As a reward, Tarri slips an <{item_distribution}> * {rarity_type} * <{class_type}> loot into their pocket. Good job!", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> boldly but stupidly tries to win a drinking competition with Taters. They wake up two days later with a 'sorry about the mess' note taped to their forehead, and a brand new <{item_distribution}> * {rarity_type} * <{class_type}> loot on their pillow.", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> finds an extremely rare vending machine in an R4. They deposit 1 rombit. Whirr bzzt zzzzzt nnnngggg bzzzz click thunk! An <{item_distribution}> * {rarity_type} * <{class_type}> loot falls out!", RarityRPG.Common),
                new ScenarioRPG("Congratulations <{player_name}>! You have been visited by the Mythical Magical Mystical Miraculous Gobby of Giving! With a wave of his hand, he conjures up an <{item_distribution}> * {rarity_type} * <{class_type}> loot just for you! Enjoy!", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> has been hiding in z2d4 dressed in their full blubber suit for four days now, and no one has noticed. Finally the moment comes, Gemm turns his back! You steal an <{item_distribution}> * {rarity_type} * <{class_type}> loot!  Muahaha!", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> - 'Wahhhhhh. I did 500 raids with a super scroll and all I found was this <{item_distribution}> * {rarity_type} * <{class_type}> loot. Rigged! Refund! Reeeeeeeee!!'", RarityRPG.Common),
                new ScenarioRPG("Roses are red, Robomax-6000 is blue.\nShadown88 has an <{item_distribution}> * {rarity_type} * <{class_type}> loot,\nAnd now <{player_name}> has one too.", RarityRPG.Common),
                new ScenarioRPG("As <{player_name}> proceeds to peel off Prof. Oak's bark, they find an <{item_distribution}> * {rarity_type} * <{class_type}> loot hidden inside it's shell.", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> signs a death wish under Grimz. Wait! It appears Grimz presented the wrong contract, <{player_name}>  receives an <{item_distribution}> * {rarity_type} * <{class_type}> loot from signing Grimz's death wish.", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> lands a critical empower dual strike  on Capt. Woodbeard and is rewarded with an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> raids the Hyper Dimension for the Walkom schematic. <{player_name}> receives 10 friend requests, turns out it was just an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> blacked out on a Fammy Slop bender and woke up in Quinn’s Stables clutching an <{item_distribution}> * {rarity_type} * <{class_type}> loot as a makeshift pillow.", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> decided to go for a 0/0/1 speedster build. The regulars at #bh_theorycrafting took pity on such a foolish endeavor and gifted the player an <{item_distribution}> * {rarity_type} * <{class_type}> loot out of charity.", RarityRPG.Common),
                new ScenarioRPG("Uh oh, <{player_name}>. Someone spiked the hot cocoa last night. Those aren’t your pants you’re wearing. You check the pockets for identification and find an <{item_distribution}> * {rarity_type} * <{class_type}> loot, instead.", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> sees Sir Quackers waddling around the pier and throws him some breadcrumbs. Overjoyed, Sir Quackers leads them to his secret loot stash and offers the player an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> sees a “Take an item, leave an item” bin by the guild hall entrance. <{player_name}> takes an <{item_distribution}> * {rarity_type} * <{class_type}> loot and leaves a Bronze Coin in its place. Way to go, you jerk.", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> Comes accross Grim!  Grim says 'I have plans for you' and produces an <{item_distribution}> * {rarity_type} * <{class_type}> loot from his robe!", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> finds himself running from XL-Ombis!  XL-Ombis dies from fatigue and {player_name} takes his <{item_distribution}> * {rarity_type} * <{class_type}> loot!  That's why we don't skip leg day boys and girls!", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> stumbles upon X4-Gombo disguised as a dentist!  Gombo says 'Open up.'  and <{player_name}> says 'Sometimes I feel sad'.  Gombo feels bad and gives them an <{item_distribution}> * {rarity_type} * <{class_type}> loot!", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> has horrible luck in hunger games and Wifey feels bad for them.  After secretly rigging the code for hunger games they are suddenly given an <{item_distribution}> * {rarity_type} * <{class_type}> loot!", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> steps into a room to find Gemm bench pressing.  Gemm yells “Chest day is the best day!” and gives {player_name} an <{item_distribution}> * {rarity_type} * <{class_type}> loot because he’s in such a good mood!", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> decides to try a magic trick!?!?  Instead of pulling a rabbit out of their hat they screw up and pull an <{item_distribution}> * {rarity_type} * <{class_type}> loot out instead!", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> was attacked by a wild bully. Fleeing for their lives they lost their way in the raid but came across an empty corridor. Inside the corridor they found an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("While hunting for the elusive shrump, <{player_name}> came across a wounded SSS1. <{player_name}> offered SSS1 their last healing potion and was rewarded an <{item_distribution}> * {rarity_type} * <{class_type}> loot from their new friend!", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> fought vigorously through the gauntlet until their final foe. Losing the battle and out of pots they have one final attack. Fortunately, their weapon becomes Empowered. They strike with Critical Precision slaying their foe and retrieving the * {rarity_type} * <{class_type}> loot from their cold dead fingers.", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> walks calmly through the halls with their strongest familiar, Tubbo.  It's Christmas time and Tubbo offers an <{item_distribution}> * {rarity_type} * <{class_type}> loot to their master for lifelong loyalty and friendship.", RarityRPG.Common),
                new ScenarioRPG("While traversing the countryside, <{player_name}> falls off a cliff right into BH lake. As they begin to sink  they notice a gleam in the sandy sediment. Further investigation reveals an <{item_distribution}> * {rarity_type} * <{class_type}> loot!", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> throws a party with their fellow heroes. After the night has ended, they notice  someone has left an <{item_distribution}> * {rarity_type} * <{class_type}> loot on the floor.  {player_name} decides to hold on to it for safe keeping.", RarityRPG.Common),
                new ScenarioRPG("'Welcome to the arena scrub! Pick your weapon!' - <{player_name}> receives an <{item_distribution}> * {rarity_type} * <{class_type}> loot from the PVP  instructor on training day.", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> saved all of their gems up to buy a large equipment chest. Upon opening it they discover an <{item_distribution}> * {rarity_type} * <{class_type}> loot!", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> trips over something while walking. Oh wait, what WAS that?! They look closer to find an <{item_distribution}> * {rarity_type} * <{class_type}> loot on the ground!", RarityRPG.Common),
                new ScenarioRPG("While hunting wild McGobbelsteins for dinner, <{player_name}> slays one only to find it had mistakenly swallowed an <{item_distribution}> * {rarity_type} * <{class_type}>. This will go great with their outfit!", RarityRPG.Common),
                new ScenarioRPG("Exhausted from battle, {player_name} prays to RNGesus for the strength to defeat their enemies. Suddenly a magical Driffin appears out of nowhere to present {player_name} with an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("After winning a bodybuilding championship against that fabled Gemm, <{player_name}> was awarded with an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("After days of grinding without sleep, <{player_name}> accidentally wanders into unreleased content, before the Devs realize what they've done, they steal an <{item_distribution}> * {rarity_type} * <{class_type}> loot and run back to where they belong.", RarityRPG.Common),
                new ScenarioRPG("A pack of Zorg have lost their favorite chew bone. <{player_name}> finds it and defeats the evil Staeus that stole it from them. The Zorg reward {player_name} with an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("Baby Tubbo has wandered away from its Grampz, <{player_name}> finds the Tubbo and brings it home, subsequently being rewarded an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("After much deliberation the cult of Bebemenz has declared <{player_name}> as their leader, rewarding them with an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("In an attempt to escape an angry Rombolio, <{player_name}> climbs into a tree to hide. There they find an <{item_distribution}> * {rarity_type} * <{class_type}> loot stuck in its bark.", RarityRPG.Common),
                new ScenarioRPG("After months of grinding <{player_name}> decides to replace their Item Find runes with Experience runes. Moments later they find an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("<{player_name}> accidentally stumbles into a secret room in the new raid. Searching its contents they come across an <{item_distribution}> * {rarity_type} * <{class_type}> loot. They're not sure what to do with it, but they take it anyway.", RarityRPG.Common),
                new ScenarioRPG("While fighting Mega Zorg, <{player_name}> accidentally breaks their weapon and begins to cry. Mega Zorg takes pity on them and rewards them with a hug and an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("It turns out that <{player_name}> is a long lost relative of Astaroth. After Astaroth's death, the executor of his will seeks out <{player_name}> to award them an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                new ScenarioRPG("In an effort to balance the community, the 10 weakest players in Bit Heroes are selected to receive better gear. <{player_name}> is rewarded with an <{item_distribution}> * {rarity_type} * <{class_type}> loot.", RarityRPG.Common),
                //new ScenarioRPG("", RarityRPG.Common),
                //new ScenarioRPG("", RarityRPG.Common),
                //new ScenarioRPG("", RarityRPG.Common),
                //new ScenarioRPG("", RarityRPG.Common),


            };
            TrainScenarios = new[]
            {
                new ScenarioRPG("<{player_name}>'s training session has been extremely helpful. He has attained such a level of mastery that he can now predict his opponent's movements 0.5 seconds in the future. Additionally he senses an * Aura Bonus * within his body granting him extra performance on his next action.", ScenarioTypeRPG.Training),
                new ScenarioRPG("After Empower critting for over 99,999 dmg on a practice dummy, <{player_name}> feel re-energised.  As a side effect, they obtained an * Aura Bonus *.", ScenarioTypeRPG.Training),
                new ScenarioRPG("'What doesn't kill you makes you stronger'. <{player_name}> decided to follow this advice and napped all day. When they woke up, they felt an * Aura Bonus * within their body!", ScenarioTypeRPG.Training),
                new ScenarioRPG("Drops of sweat running on his broad chest, <{player_name}> just finished an extemely fulfilling work out. They feel they have reached a state of * Aura Bonus * .", ScenarioTypeRPG.Training),
                new ScenarioRPG("<{player_name}> focused all his energy into his palms and managed to throw a miniature energy beam. They obtained a * Aura Bonus * . ", ScenarioTypeRPG.Training),
                new ScenarioRPG("<{player_name}> outlasted Gemm's insanity workout!. He transfers him an * Aura Bonus * .", ScenarioTypeRPG.Training),
                new ScenarioRPG("Deep in Gemm's Cell <{player_name}> finds Gemm's locker room.  Inside Gemm's locker is Popeye's Mythical Can of Spinach!  After consuming the slimey substance they feel strength coursing through their body!  They have received an * aura bonus * !", ScenarioTypeRPG.Training),
                new ScenarioRPG("<{player_name}> found a long forgotten library in Dryad's Heart.  Blowing the dust off one of the ancient tomes they are amazed to find the grimoire thought long ago destroyed!  It's 'The Idiot's Guide to Bit Heroes'!  Reading through the legendary book they gain deep understanding.  They have received an * aura bonus *!", ScenarioTypeRPG.Training),
                new ScenarioRPG("After drinking Gemm's sweat juices, <{player_name}> feels an * Aura Bonus * entering his body!", ScenarioTypeRPG.Training),
                //new ScenarioRPG("", ScenarioTypeRPG.Training),
                //new ScenarioRPG("", ScenarioTypeRPG.Training),
                //new ScenarioRPG("", ScenarioTypeRPG.Training),
                //new ScenarioRPG("", ScenarioTypeRPG.Training),
                //new ScenarioRPG("", ScenarioTypeRPG.Training),
                //new ScenarioRPG("", ScenarioTypeRPG.Training),
                //new ScenarioRPG("", ScenarioTypeRPG.Training),
                //new ScenarioRPG("", ScenarioTypeRPG.Training),
                //new ScenarioRPG("", ScenarioTypeRPG.Training),
                //new ScenarioRPG("", ScenarioTypeRPG.Training),
                //new ScenarioRPG("", ScenarioTypeRPG.Training),
            };
        }

        public BHungerGamesV3()
        {
            _random = new Random();
            _duelImmune = new HashSet<PlayerRPG>();
            _goblinPlayers = new HashSet<PlayerRPG>();
            _ignoreReactions = true;
            _goblinHunt = false;
            _worldBoss = false;
        }

        public void Run(List<Player> contestantsTransfer, int maxScore, int maxTurns, BotGameInstance.ShowMessageDelegate showMessageDelegate, Func<bool> cannelGame, BotGameInstance.ShowMessageDelegate SendImageFile, BotGameInstance.ShowMessageDelegate sendMsg)
        {
            int day = 0;
            int playerNumberinLeaderboard = 20;
            int duelCooldown = 5;
            //bool crowdExtraDuel = false;
            //bool bonusItemFind = false;
            int goblinFestChance = 5;

            ClassPicker classPicker = new ClassPicker();
            Tuple<HeroClass, DailyBuff> dailyBuff;

            StringBuilder sb = new StringBuilder(20000);
            StringBuilder sbLoot = new StringBuilder(20000);
            StringBuilder sbTrain = new StringBuilder(20000);
            StringBuilder sbDeath = new StringBuilder(20000);

            List<PlayerRPG> playersToBeRemoved = new List<PlayerRPG>();
            List<Player> bannedPlayersToRemove = new List<Player>();
            List<PlayerRPG> descendingList;

            Logger.LogInternal("V3 Game started, total ppl: " + contestantsTransfer.Count);

            #region Game Initialization
            List<ulong> bannedList = new BanListHandler().GetBanList();
            foreach (Player player in contestantsTransfer)
            {
                if (bannedList.Contains(player.UserId))
                {
                    bannedPlayersToRemove.Add(player);
                }
            }
            contestantsTransfer = contestantsTransfer.Except(bannedPlayersToRemove).ToList();
            if (bannedPlayersToRemove.Count > 0)
            {
                showMessageDelegate($"Number of banned players attempting to join game:{bannedPlayersToRemove.Count}\r\n");
            }
            //if (maxPlayers > 0 && contestantsTransfer.Count > maxPlayers)
            //{
            //    int numToRemove = contestantsTransfer.Count - maxPlayers;
            //    for (int i = 0; i < numToRemove; i++)
            //    {
            //        int randIndex = _random.Next(contestantsTransfer.Count);
            //        contestantsTransfer.RemoveAt(randIndex);
            //    }
            //}

            _players = new List<PlayerRPG>();
            foreach (Player player in contestantsTransfer)
            {
                PlayerRPG interactivePlayer = player as PlayerRPG;
                if (interactivePlayer != null)
                {
                    _players.Add(interactivePlayer);
                    //sb.Append($"<{player.ContestantName}>\t");
                }
            }
            //for (int g = 1; g <= 200; g++)
            //{
            //    _players.Add(new PlayerRPG(g));
            //}
            //showMessageDelegate("Players that successfully entered the arena:\r\n" + sb);
            //sb.Clear();
            if (playerNumberinLeaderboard > _players.Count)
            {
                playerNumberinLeaderboard = _players.Count;
            }
            _ignoreReactions = false;
            _classInit = true;
            showMessageDelegate($"\nPlease select the hero class that you would like to become. Available classes are :"
                               + " Mage ( * A * ), Knight ( * B * ), Archer ( * C * ), Monk ( * D * ), Necromancer ( * E * ), Assassin ( * F * ), Elementalist ( * G * )", null, EmojiClassListOptions);

            Thread.Sleep(15000);
            _ignoreReactions = true;
            _classInit = false;
            SortClasses();
            #endregion

            while (_players.Count > /*numWinners*/ 0)
            {
                day++;
                if (RngRoll(goblinFestChance))
                {
                    goblinFestChance = 5;
                    SendImageFile(GobbyImage);
                    Thread.Sleep(new TimeSpan(0, 0, 0, 5));
                    GoblinFest(showMessageDelegate);
                }
                else
                {
                    goblinFestChance += 3;
                }

                if (day > 0 && day % 8 == 0)
                {
                    WorldBossEvent(day, showMessageDelegate);
                }

                _adventureAffinity = classPicker.PickAClass();

                dailyBuff = RandomClassBuff(sb);
                showMessageDelegate("" + sb);
                sb.Clear();

                _ignoreReactions = false;
                showMessageDelegate($"\n Day **{day}**\nYou have {DelayAfterOptions.Seconds} seconds to input your decision\n"
                    + $"Today's Adventure is more suited for: * {_adventureAffinity} *\n"
                    + "You may select how you will want to pursue your * Adventure * . \nYour options are: "
                    + "<:bulb:> to Complete your Adventure, <:crossed_swords:> To gain more EXP or <:money_bag:> To gain better"
                    + " loot or <:muscle:> to skip the Adventure and train.", null, EmojiAdventureListOption);
                Thread.Sleep(DelayAfterOptions);
                _ignoreReactions = true;

                //foreach (PlayerRPG player in _players)
                //{
                //    player.InteractiveRPGDecision = (InteractiveRPGDecision)(_random.Next(4) + 1);
                //    Console.WriteLine(player.InteractiveRPGDecision);
                //}

                foreach (PlayerRPG player in _players)
                {
                    //if (day > 30)
                    //    player.InteractiveRPGDecision = InteractiveRPGDecision.LookForLoot;
                    //else player.InteractiveRPGDecision = InteractiveRPGDecision.Train;
                    if (player.InteractiveRPGDecision == InteractiveRPGDecision.Nothing) continue;
                    if (player.InteractiveRPGDecision != InteractiveRPGDecision.Train)
                    {
                        sb.Append(Adventure.PerformAdventure(player, day, _adventureAffinity, _players.Count, LootScenarios, dailyBuff));
                    }
                    else
                    {
                        sbTrain.Append(player.Train(TrainScenarios, dailyBuff));
                    }
                }

                showMessageDelegate("" + sb + sbTrain, null);
                sb.Clear();
                sbTrain.Clear();

                if (day > 1 && day % 2 == 0) DistributeNotorietyReturns();

                if (day > 2 && day % 4 == 0)
                {
                    descendingList = _players.OrderByDescending(player => player.Points).ToList();
                    sb.Append("LEADERBOARD\n\n");
                    for (int i = 0; i < playerNumberinLeaderboard; i++)
                    {
                        sb.Append($"{i + 1}. {descendingList[i].NickName} || Score = {descendingList[i].Points} || Lvl = {descendingList[i].Level} || Combat power = {descendingList[i].EffectiveCombatPower}\n");
                    }
                    showMessageDelegate("" + sb, null);
                    sb.Clear();
                }
                foreach (ScenarioRPG scenario in TrainScenarios)
                {
                    scenario.ReduceTimer();
                }

                Thread.Sleep(5000);

                if (cannelGame()) return;

                if (duelCooldown != 0) duelCooldown--;
                else if (_players.Count > 9)
                {
                    Duel(sb);
                    showMessageDelegate("Duels\n=====\n\n" + sb, null);
                    sb.Clear();
                }
                Thread.Sleep(DelayBetweenCycles);

                ResetDecisions();

                Logger.LogInternal($"Total players CP is : {_players.Sum(p => p.EffectiveCombatPower)}");


                if (day == maxTurns || _players.Max(player => player.Points) > maxScore) break;
            }

            sb.Append("\n\n**Game Over**\r\n\r\n");
            descendingList = _players.OrderByDescending(player => player.Points).ToList();
            sb.Append("LEADERBOARD\n\n");
            for (int i = 0; i < playerNumberinLeaderboard; i++)
            {
                sb.Append($"{i + 1}. {descendingList[i].NickName} || Score = {descendingList[i].Points} || Lvl = {descendingList[i].Level} || Combat power = {descendingList[i].EffectiveCombatPower}\n");
            }
            showMessageDelegate("" + sb, null);
            Logger.LogInternal("" + Adventure.sbData);
            Adventure.sbData.Clear();


        }

        private void ResetDecisions()
        {
            foreach (PlayerRPG player in _players)
            {
                player.ResetVars();
            }
        }

        private void DistributeNotorietyReturns()
        {
            long pointSum = _players.Sum(player => player.Points);
            foreach (var player in _players)
            {
                if (player.Notoriety > 0)
                {
                    player.Points += Convert.ToInt64(pointSum * player.Notoriety / 100);
                }
            }
        }

        private void WorldBossEvent(int day, BotGameInstance.ShowMessageDelegate showMessageDelegate)
        {
            int cpScaling = 40;
            float dayscaling = (float)(Math.Pow(2, (1 +((float)day / 19))));
            long playersStrength = _players.Sum(player => player.EffectiveCombatPower);
            long worldBossStrength = Convert.ToInt64(_players.Count * day * cpScaling * dayscaling);
            Logger.Log($"player strength = {playersStrength} ||  WB strength = {worldBossStrength}");
            bool battleResult = false;

            long battleRoll = Convert.ToInt64(_random.NextDouble() * (playersStrength + worldBossStrength));
            if (battleRoll > worldBossStrength) battleResult = !battleResult;

            _ignoreReactions = false;
            _worldBoss = true;
            showMessageDelegate($"ATTENTION, GLOBAL WORLD BOSS ALERT\n==================================\n"
                              + $"The Ultimate Boss of Bosses from the <{(WorldBossList)_random.Next(2)}> area has managed to break the Arcane Seal, put in place to prevent his escape from the crypt!\n"
                              + "You can allocate some of your <Points> to help send the boss back in its Crypt. If the Heroes succeed, you will be rewarded with * TRIPLE * the amounts of <Points> allocated. If the Heroes fail, you will lose them. \n"
                              + "Each reaction will allocate a different %.\nA : <1%>\nB : <5%> \nC : <10%> \nD : <25%>\nE : <50%>\nF : <75%>\nG : <100%>\n", null, EmojiClassListOptions);
            Thread.Sleep(new TimeSpan(0, 0, 25));
            _worldBoss = false;
            _ignoreReactions = true;
            showMessageDelegate((battleResult ? "The Heroes have prevailed and managed to send back the boss in its crypt!" : "Sadly the Boss was too powerful for the Heroes to defeat..."));
            foreach (PlayerRPG player in _players)
            {
                player.WorldBossRewards(battleResult);
            }
        }

        private void GoblinFest(BotGameInstance.ShowMessageDelegate showMessageDelegate)
        {
            StringBuilder sb = new StringBuilder();
            int goblinCount = _players.Count / 10 + 1;
            string one = goblinCount == 1 ? ". Yes an army of 1 is still an army.Goblins are vicious beings!" : "";
            _ignoreReactions = false;
            _goblinHunt = true;
            showMessageDelegate("The goblin Fest Banaza has begun!\nAn army of goblins is on the loose and has stolen a large quantity of <rare goods>. Try to capture them to be rewarded with EXP and loot.\n"
                              + $"Amount of Goblins on the loose : <{goblinCount}>" + one + "\n"
                              + "If you chose to participate in the hunt, you won't be able to partake on the next Adventure.\n:a: for YES or :b: for NO.", null, EmojiListCrowdDecision);
            Thread.Sleep(new TimeSpan(0, 0, 20));
            _goblinHunt = false;
            _ignoreReactions = true;
            if (_goblinPlayers.Count == 0)
            {
                showMessageDelegate("No Hero decided to help capture the goblins :(");
                return;
            }
            for (int i = 0; i < goblinCount; i++)
            {
                int index = _random.Next(_goblinPlayers.Count - 1);
                sb.Append(Adventure.GoblinLoot(_goblinPlayers.ElementAt(index)));
            }
            showMessageDelegate("" + sb);
            _goblinPlayers.Clear();
        }

        private Tuple<HeroClass, DailyBuff> RandomClassBuff(StringBuilder sb)
        {
            HeroClass dailyClassBuff = (HeroClass)_random.Next(7);
            DailyBuff dailyBuff = (DailyBuff)_random.Next(4);
            sb.Append($"Today's daily buff affects * {dailyClassBuff} * and are rewarded with a <{dailyBuff}> buff.\n");
            return new Tuple<HeroClass, DailyBuff>(dailyClassBuff, dailyBuff);
        }

        private void Duel(StringBuilder sb)
        {
            int duelAmount = Convert.ToInt32(_players.Count * 0.025) + 1;
            int safetyNet = 0;
            int rangeMod = 0;
            while (duelAmount != 0)
            {
                int index = _random.Next(_players.Count); //selecting first player in duel
                int index2 = index;
                if (_players[index].HasDueled) continue; //restarts loop if player1 already dueled
                while (index2 == index)
                {
                    int value = 0;
                    switch (value)
                    {
                        case 0 when index < 4:
                            index2 = _random.Next(5 + rangeMod);
                            break;
                        case 0 when index > _players.Count - 4:
                            index2 = _players.Count - 1 - _random.Next(5 + rangeMod);
                            break;
                        default:
                            index2 = _random.Next(index - (4 + rangeMod), index + 5 + rangeMod);
                            break;
                    }
                    if (_players[index2].HasDueled)
                    {
                        index2 = index;
                        continue;
                    }
                    safetyNet++;
                    if (safetyNet % 20 == 0) rangeMod++;
                }
                sb.Append(PerformDuel(index, index2));
                duelAmount--;
                rangeMod = 0;
                safetyNet = 0;

            }

        }

        private String PerformDuel(int index1, int index2)
        {
            _players[index1].HasDueled = true;
            _players[index2].HasDueled = true;

            int p1W = 0, p2W = 0;
            float p1Advantage = 1, p2Advantage = 1;
            int classDiff = ((int)_players[index1].HeroClass) - ((int)_players[index1].HeroClass);
            switch (classDiff)
            {
                case 1:
                case 7:
                    p2Advantage = 1.25f;
                    break;
                case 2:
                case 6:
                    p2Advantage = 1.125f;
                    break;
                case -1:
                case -7:
                    p1Advantage = 1.25f;
                    break;
                case -2:
                case -6:
                    p1Advantage = 1.125f;
                    break;
            }
            int p1Chance = Convert.ToInt32((_players[index1].EffectiveCombatPower) * p1Advantage / ((_players[index1].EffectiveCombatPower) * p1Advantage + (_players[index2].EffectiveCombatPower) * p2Advantage) * 100);
            for (int i = 0; i < 5; i++)
            {
                if (RngRoll(p1Chance))
                { p1W++; }
                else { p2W++; }
            }
            if (p1W < p2W)
            {
                index1 = index1 ^ index2;
                index2 = index1 ^ index2;
                index1 = index1 ^ index2;
            }
            _players[index1].AddExp(_players[index2].Level * 6);
            _players[index1].Points += Convert.ToInt64(_players[index2].Points * 0.05);
            _players[index1].AuraBonus = true;
            string returnString = $"<{_players[index1].NickName}> came out victorious in the duel agaisnt <{_players[index2].NickName}>. On top of additional exp and points, he gains an * Aura Bonus * he can use on his next action!\n\n";
            return returnString;
        }

        private bool RngRoll(int a)
        {
            int chance = a * 10;
            int roll = _random.Next(0, 1000);
            return roll <= chance;
        }

        private void SortClasses()
        {
            foreach (PlayerRPG contestant in _players)
            {
                MakeClass(contestant);
            }
        }

        private void MakeClass(PlayerRPG player)
        {
            for (int i = 0; i < player.HeroStats.Length; i++)
            {
                player.HeroStats[i] = HeroBaseStatsDictionary[player.HeroClass][i];
                player.HeroStatMult[i] = HeroScalingDictionary[player.HeroClass][i];
            }
        }

        public void HandlePlayerInput(ulong userId, string reactionName)
        {
            if (_ignoreReactions) return;

            if (_classInit)
            {
                var authenticPlayer = _players.FirstOrDefault(contestant => contestant.UserId == userId);
                if (authenticPlayer != null)
                {
                    switch (reactionName)
                    { //🇦 🇧 🇨 🇩 🇪 🇫 🇬
                        case "🇦":
                            authenticPlayer.HeroClass = HeroClass.Mage;
                            break;
                        case "🇧":
                            authenticPlayer.HeroClass = HeroClass.Knight;
                            break;
                        case "🇨":
                            authenticPlayer.HeroClass = HeroClass.Archer;
                            break;
                        case "🇩":
                            authenticPlayer.HeroClass = HeroClass.Monk;
                            break;
                        case "🇪":
                            authenticPlayer.HeroClass = HeroClass.Necromancer;
                            break;
                        case "🇫":
                            authenticPlayer.HeroClass = HeroClass.Assassin;
                            break;
                        case "🇬":
                            authenticPlayer.HeroClass = HeroClass.Elementalist;
                            break;
                    }
                }
            }
            else if (_crowdOptions)
            {
                switch (reactionName)
                {
                    case "🅰":
                        _reactionA++;
                        break;
                    case "🅱":
                        _reactionB++;
                        break;
                }
            }
            else if (_goblinHunt)
            {
                var authenticPlayer = _players.FirstOrDefault(contestant => contestant.UserId == userId);
                if (authenticPlayer != null)
                {
                    switch (reactionName)
                    {
                        case "🅰":
                            authenticPlayer.PartookInEvent = true;
                            _goblinPlayers.Add(authenticPlayer);
                            break;
                    }
                }
            }
            else if (_worldBoss)
            {
                var authenticPlayer = _players.FirstOrDefault(contestant => contestant.UserId == userId);
                if (authenticPlayer != null)
                {
                    switch (reactionName)
                    { //🇦 🇧 🇨 🇩 🇪 🇫 🇬
                        case "🇦":
                            authenticPlayer.GamblingOption = GamblingOptions._1;
                            break;
                        case "🇧":
                            authenticPlayer.GamblingOption = GamblingOptions._5;
                            break;
                        case "🇨":
                            authenticPlayer.GamblingOption = GamblingOptions._10;
                            break;
                        case "🇩":
                            authenticPlayer.GamblingOption = GamblingOptions._25;
                            break;
                        case "🇪":
                            authenticPlayer.GamblingOption = GamblingOptions._50;
                            break;
                        case "🇫":
                            authenticPlayer.GamblingOption = GamblingOptions._75;
                            break;
                        case "🇬":
                            authenticPlayer.GamblingOption = GamblingOptions._100;
                            break;
                    }
                }
            }
            else
            {
                var authenticPlayer = _players.FirstOrDefault(contestant => contestant.UserId == userId);
                if (authenticPlayer != null && !authenticPlayer.PartookInEvent)
                {

                    switch (reactionName)
                    {
                        case "💡":
                            authenticPlayer.InteractiveRPGDecision = InteractiveRPGDecision.LookForCompletion;
                            break;
                        case "⚔":
                            authenticPlayer.InteractiveRPGDecision = InteractiveRPGDecision.LookForExp;
                            break;
                        case "💰":
                            authenticPlayer.InteractiveRPGDecision = InteractiveRPGDecision.LookForLoot;
                            break;
                        case "💪":
                            authenticPlayer.InteractiveRPGDecision = InteractiveRPGDecision.Train;
                            break;
                    }
                }
            }
        }
    }
}

