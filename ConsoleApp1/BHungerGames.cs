using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace BHungerGaemsBot
{
    public class BHungerGames
    {
        private const int DelayValue = 80;
        private const int MaxScenariosPerDay = 6;
        private const int PercentChanceToLivePerDay = 90; // 90%
        private const int MaxNoCasualtiesConsecutive = 1;

        private static readonly Scenario[] Scenarios;
        private static readonly int[] ShowPlayersWhenCountEqual = { 20, 10, 5, 2, 0 };

        private readonly Random _random;

        private class Scenario
        {
            private readonly string _description;
            public int PlayersNeeded { get; }
            public int Delay { get; set; }

            public Scenario(string description, int playersNeeded)
            {
                _description = description;
                PlayersNeeded = playersNeeded;
                Delay = 0;
            }

            public void ReduceDelay()
            {
                if (Delay > 0)
                    Delay -= 1;
            }

            public string GetText(string[] players)
            {
                string value = _description?.Replace("{@P1}", players[0]);
                if (players.Length > 1)
                {
                    value = value?.Replace("{@P2}", players[1]);
                    if (players.Length > 2)
                    {
                        value = value?.Replace("{@P3}", players[2]);
                    }
                }
                return value;
            }
        }

        static BHungerGames()
        {
            Scenarios = new[]
            {
                new Scenario("{@P1} made an alliance with {@P2} to kill {@P3}. But {@P1} shouldn't have trusted {@P2}... {@P3} has killed {@P1}.", 3),
                new Scenario("{@P1} stepped on a mine. What a shame.", 1),
                new Scenario("{@P2} is extremely hungry. He hasn't eaten for days. He sees {@P1} eating some berries in the forest but {@P1} suddenly drops dead. ''I wonder what human flesh tastes like!'' tells {@P2} to himself.", 2),
                new Scenario("{@P1} got mauled by a genetically modified Korgz.", 1),
                new Scenario("{@P1} died of a wound infection inflicted by {@P2} whilst fighting for some resources.", 2),
                new Scenario("A giant rock fell on {@P1}'s head... But how?", 1),
                new Scenario("It's a miracle! {@P1} escaped from the deadly Nosdoodoo! Oh wait! Never mind, {@P1} just got eaten by Blubber...", 1),
                new Scenario("Taters has muted {@P1} once and for all.", 1),
                new Scenario("Oh no! {@P1} has just bet his life with Grimz! And he lost :(", 1),
                new Scenario("{@P1} died of sodium chloride overdose.", 1),
                new Scenario("{@P1} has just seen RNGesus in front of him! He asks him if he can win and to that question, RNGesus responds: 'nope'. {@P1} dies.", 1),
                new Scenario("What people don't know is that Mimzy is very territorial... Oh look! {@P1} has stepped into his territory! Bye bye {@P1}.", 1),
                new Scenario("{@P1} broke his ankle. Is that a Zorg I see rushing towards {@P1}? Yes, yes it is.", 1),
                new Scenario("{@P1} finds some food on the ground! Great news.... for {@P2} that was waiting for him.  {@P2} kills {@P1}. ", 2),
                new Scenario("{@P2} is steadily aiming his arrow to {@P1}, drinking water from the river. *woosh* Bullseye", 2),
                new Scenario("A wild Kov' Arg appears! 'Death or snusnu ?' {@P1} replies 'Death'. 'Farewell... Death....By snusnu!'", 1),
                new Scenario("{@P1} encounters Warty. Warty tells him to kiss him so he could transform into a beautiful princess. {@P1} is way too naive! As soon as {@P1} kisses Warty, his poison consumes him into a pile of goo...", 1),
                new Scenario("{@P2} just died\n.\n.\n.\n.\n jk!\n {@P1} died in his place. Ha.", 2),
                new Scenario("{@P1} spent all his gems on large eggs and got nothing... {@P1} committed suicide", 1),
                new Scenario("{@P1} sees a big footprint on the ground. It looks like a yeti's footprint?  Yup. Yeti just ate {@P1}.", 1),
                new Scenario("{@P1} found the one ring to rule them all! He will posses incredible power! Watch out! A Golum is charging at you! Too late :(", 1),
                new Scenario("{@P1} is allergic to peanuts. He was unaware of it and just ate some that {@P2} gave him... Coincidence ? I think not!", 2),
                new Scenario("{@P1} disconnected in the middle of Cpt. Woodbeard heroic fight... RIP", 1),
                new Scenario("{@P1} decided to go hunt for food. {@P1} shoots his arrow - but whats this? the arrow ricochets and unfortunately hits {@P1} in the knee... Damn RNG!", 1),
                new Scenario("{@P1} dies as a result of shaving. {@P1} was unaware that {@P2} had dipped his shaving knife in a poisonous mixture... ", 2),
                new Scenario("The powerful wizard {@P1} is on the run and accidentally trips over his own beard, falling to his death.. ", 1),
                new Scenario("Gobby loves stealing from people. He just stole all of {@P1}'s money... {@P1} died of depression.", 1),
                new Scenario("{@P1} just broke Olxaroth's card castle! Filled with anger, Olxaroth's reduces {@P1} in a pile of ashes.", 1),
                new Scenario("{@P1} is smart, and decides to hide up in the trees. \n Could this be it? is it? YES! - its time to drop the blubber suit and turn into a boootiful butterfly! Unfortunately, fairytales are not real, and {@P1} didnt fly, but fell to his death ", 1),
                new Scenario("{@P1} thought it would be wise to bring a stick to a sword fight... {@P1} dies.. ", 1),
                new Scenario("Everyone hate Mimeido. {@P1} agrees to this statement.But {@P1} forgot Mimeido could oneshot him :) ", 1),
                new Scenario("{@P1} is sneaking around the castle, when suddenly he runs into Gemmroid! 'Do you even lift bro?' - says Gemmroid. Filled with shame and un-impressive gains {@P1} must forfeit.. ", 1),
                new Scenario("{@P1} is walking along side a cliff and slips! He catches the edge. {@P2} sees all the action, approaches {@P1} and flips a coin. 'heads, you live'. It's tails :/ ", 2),
                new Scenario("The gamemakers send in a swarm of Melvinsteins to disrupt the game! While attempting to run from them, {@P1} tripped and fell, allowing the Melvinsteins to eat him alive. ", 1),
                new Scenario("{@P1} makes a run for the remaining items in the Cornucopia! Little did {@P1} know that {@P2} was waiting with an enormous Glimmer, ready to decapitate {@P1} 's head  ", 2),
                new Scenario("Knowing {@P1} cannot survive alone, {@P1} sends out a friend request: 'lvl 6 looking for lvl 250+ to help me complete r2 heroic!' {@P1} 's request is denied, and promptly dies without an alliance. ", 1),
                new Scenario("{@P1} was foraging for food when they spotted a wild Shrump! Unfortunately, {@P1} voted for Nyxn in the last Bit Heroes election, and when Shrump heard about that, he banished {@P1} for life. ", 1),
                new Scenario("{@P1} was doing just fine up until they proposed that Bit Heroes should have more fan fiction. Tarri disagreed, and laid down the banhammer. ", 1),
                new Scenario("Krusty always wanted to open the Krusty Krab....And he's pretty sad about it... For that reason, he crits {@P1} for no reason.", 1),
                new Scenario("{@P1} runs into a wild gang of Pengeys. Shaking with fear and regret of the paths taken, {@P1} decides to make a run for it... \n {@P1} underestimated the powers of this wild gang, and quickly realises, he dun goof'd ", 1),
                new Scenario("{@P1} didn't believe Wemmbo crit chance was high enough to worry about... Boy was he wrong", 1),
                new Scenario("{@P1} didn't use CAPS in Salt_mines. Byleth bans him forever!", 1),
                new Scenario("{@P1} and {@P2} teamed up in an alliance, but in a cruel twist of fate, {@P2} broke that alliance and pushed {@P1} off a cliff. ", 2),
                new Scenario("After hours of searching, {@P1} has found a pool from which to drink! . . . but it turns out to be a pool of Kaleido's tears. {@P1} is banished to R2 never to return. ", 1),
                new Scenario("{@P1} and {@P2} have found a Gemmroid! He challenges both of them to leg day. Sadly, {@P1} gets rhabdomyalysis and dies. ", 2),
                new Scenario("{@P1} finds a rare candy on the ground! But it turns out to be ground candy. {@P1} vomits the entire contents of his stomach and dies of malnutrition. ", 1),
                new Scenario("{@P1} ventures into world chat! Oh no! He is too high of a level and is spammed to death. ", 1),
                new Scenario("'Want to hear a joke?' Warty said to {@P1}. {@P1} said no, so Warty ate him. ", 1),
                new Scenario("Walking up the mountain of Lost Heroes, {@P1} encountered Vedic! Vedic draws and quarters {@P1} with his samurai sword. ", 1),
                new Scenario("Without skipping a beat, {@P1} hides, as he can hear footsteps closing in... \n{@P2} sneaks up on {@P1}... {@P1} dies. ", 2),
                new Scenario("While on the hunt for some delicious booty, {@P1} unfortunately becomes the prey to the ferocious Dina.. ", 1),
                new Scenario("{@P1} attempts to beat Blasian in an Astaroth Flag pvp tournament! {@P1} runs out of time, money, points, and sanity. ", 1),
                new Scenario("{@P1} sees a [K] member. OMG! Unfortunately, the [K] member doesn't have time and kills {@P1}.", 1),
                new Scenario("{@P1} pulls a Leeroy Jenkins and rushes into Gauntlet 211. We all know how that turns out. ", 1),
                new Scenario("Covered in dirt, and tired from battle {@P1} decides that its time to take a swim in the nearby lake. {@P1} forgot he couldnt swim.. rip ", 1),
                new Scenario("{@P1} dies. Like, nothing happened to him, he just collapsed like a pile of shit. :/ ", 1),
                new Scenario("{@P1} makes the mistake of eating Yeti's Frozen Spaghetti and is frozen from the inside out. ", 1),
                new Scenario("{@P1} sees a mysterious shadow near the horizon. Who could it be ? Nanananananananana, Bubbo! And he's here to wreck {@P1}'s booty!", 1),
                new Scenario("{@P1} attempts to talk to Quinn one times too many. Didn't you get the hint the 54th time you asked? ", 1),
                new Scenario("{@P1} quipped mirror wings and astaroth armor, got mistaken by Warty as a fly. {@P1} died due to digestive chemicals...", 1),
                new Scenario("{@P1} and {@P2} have been walking all day. Very tired, they both decide to have a quick picnic. Unfortunately a swarm of Terrumps covered the sky dark and went in for the kill.. \n {@P2} managed to get away, but {@P1} wasnt so lucky.. ", 2),
                new Scenario("{@P1} was offline for 2 days and all his high level friends removed him. {@P1} didn't notice and did R2 Heroic anyways. ", 1),
                new Scenario("{@P1} DC'ed from the game and lost 5 gauntlet tokens. And his life. ", 1),
                new Scenario("{@P1} has found the legendary Juppiomenz in a gauntlet! Too bad he was the only player left on his team. ", 1),
                new Scenario("{@P1} has been declined by one too many familiars! ", 1),
                new Scenario("{@P1} fell off a cliff playing Pokemon GO ", 1),
                new Scenario("{@P1} put on some major speed kicks and ran fast... directly off a cliff. ", 1),
                new Scenario("{@P1} found the legendary Tobey and challenged him to a duel! Big mistake. ", 1),
                new Scenario("{@P1} has drowned in a sea of duel requests. ", 1),
                new Scenario("{@P1} accidentally clicked R2 heroic instead of R2 normal. Whoops! ", 1),
                new Scenario("{@P1} has died of dysentery. ", 1),
                new Scenario("{@P1} Died of exhaustion trying to find the Bobodom schematic for 10 straight weeks. ", 1),
                new Scenario("{@P1} heard SSS1 speak, and therefore SSS1 had to kill {@P1}. ", 1),
                new Scenario("{@P1} didn't celebrate Blubber Day, and Blubber got mad and ate him. ", 1),
                new Scenario("{@P1} attacked Astally! Astally deflected {@P1}'s attack right into {@P1} and killed him. ", 1),
                new Scenario("{@P1} was wandering around and saw a crazy Encon searching for schematics. He tried to duel him, but Encon used pocket salt, {@P1} went blind and died to a Bubbo crit", 1),
                new Scenario("{@P1} sees a Blubbicorn! Rumours say they have 0.000000001%% of spawning in the game! They're also ridiculously strong... {@P1} got rekt by Blubbicorn's Impale!", 1),
                new Scenario("{@P2} sneezed.\nHis sneeze scared the shit out of Mr. Bob who then proceeded to kill {@P1} who happened to walk by.", 2),
                new Scenario("{@P2} stabbed {@P1} and ran away like a rascal that he is.", 2),
                new Scenario("{@P1} played Blackjack with Quinn. Quinn didn't want to play and left. {@P1} died of solitude.", 1),
                new Scenario("{@P1} is stuck in an endless loop redecorating the guild hall and dies of famine.", 1),
                new Scenario("PotatoBot didn't appreciate the extra commands from {@P1}. And thus, {@P1} was no more.", 1),
                new Scenario("{@P1} got tortured to death by 1000 rabbits", 1),
                new Scenario("{@P1} was asked too many drawing requests in the Discord Channel, got carpal tunnel syndrome and commited sudoku :/", 1),
                new Scenario("{@P1} fell in a pit of despair and sorrow. Life's hard but what a unity!", 1),
                new Scenario("{@P1} slips off the ledge but is caught by KKyoji. Sadly, KKyoji is too weak to carry so {@P1} dies.", 1),
                new Scenario("{@P3} and {@P2} pranked {@P1} by wasting all his gems on Major Speed Kicks. {@P1} died from disgust.", 3),
                new Scenario("Mokilok killed {@P1} for suggesting an in-game trading system.", 1),
                new Scenario("{@P1} mentioned @Tarri in discord before she had her coffee and was found dead later that day.",1),
                new Scenario("{@P1} tried to join LoSq, but Phupperbottine found them too lewd and killed them on the spot.", 1),
                new Scenario("{@P1} spent all their money on gems and starved to death.", 1),
                new Scenario("{@P1} tried to outflex Gemm, he flexed himself to death.", 1),
                new Scenario("{@P1} died in awe of the artwork by RedRidingHoodie & BigBadWolfy. ", 1),
                new Scenario("{@P1} decided to watch Borealis fight a Shrampz. {@P1} dies of old age.", 1),
                new Scenario("{@P1} saw a Hypershard and rushed for it. But {@P2} laid a pithole under the Hypershard. Good night {@P1}.", 2),
                new Scenario("{@P1} turned on friend requests.", 1),
                new Scenario("{@P1} used 5 tokens in PvP and was forced to chose between sss1, tobey, and Borealis. ", 1),
                new Scenario("{@P1} divided by zero which caused existence in this universe to become impossible.", 1),
                new Scenario("Remruade sniped {@P2} out of a tree he was hiding in, {@P1} broke the fall and died instead of {@P2}.", 2),
                new Scenario("{@P1} Bet that Null will find an olxa very soon! {@P1} lost the bet and died.", 1),
                new Scenario("{@P1} and {@P2} made a bet against each other that all the grass in Bit Heroes was well made, a LVL 328 Thomas flew in and killed {@P1} for betting that the grass was perfect.", 2),
                new Scenario("{@P1} Laughed at how Luvboi never works. {@P1} Challenged BorealisGaming in a 1v1 duel, {@P1} Suffered as Luvboi Procced 6 times in a row and Died from Deflect Crits. Luvboi Wins.", 1),
                new Scenario("{@P1} pretends to throw the ball to Zorg. Zorg get on his hind legs and rips {@P1}'s head off. Good boy, Zorg. :3", 1),
                new Scenario("{@P1} duels {@P2} to a fight! It seems {@P1} doesn't know yet about cosmetics and that's not exactly a wooden sword.", 2),
                new Scenario("{@P1} has found a Blubber that wanted a hug. {@P1} hugs Blubber but ends up being squished by Blubber's body. Blubber is sad and now wants another hug.", 1),
                new Scenario("Blasian logged in and  kills {@P1}. Blasian logged out and continue being offline.", 1),
                new Scenario("Tobey removed his weapon. Tobey used 1-Bit Punch. Tobey killed {@P1}.", 1),
                new Scenario("{@P1} literally thought this was a 'Hunger' games. {@P1} died from hunger.", 1),
                new Scenario("{@P1} died\n..\n.\n.\nNo joke, {@P1} actually died", 1),
                new Scenario("{@P1} decided to wait for the R3 and Z5 Update... Clearly he did not know what Soon™ meant...", 1),
                new Scenario("{@P1} tried to breath underwater, {@P1} drowned.", 1),
                new Scenario("{@P1} stepped onto Grampz's lawn, Grampz farted on {@P1}, {@P1} suffocated.", 1),
                new Scenario("{@P1} was camping #friendslot_giving Channel for too long, fell asleep and was left behind. ", 1),
                new Scenario("{@P1} found his first hypershard and died from excitement.", 1),
                new Scenario("{@P1} tried to be a gentleman and save {@P2} from a falling tree branch, but didn't think about protecting himself.", 2),
                new Scenario("{@P1} mocked Shadown88's build. Shadown88 shot a dual striked 3.5K arrow to his knee causing his immediate death :D", 1),
                new Scenario("{@P2} attempted to light a fire with two sticks and accidentally stabbed himself, creating a blood trail that attracted Melvins from miles around. {@P2} heard them coming and ran right into {@P1}, who was eaten by the melvins instead.", 2),
                new Scenario("{@P1} thought he escaped the stampede, only to meet the Seedling Tribe and perish anyways.", 1),
                new Scenario("{@P1} tried to bully Bully, but instead Bully bullied him to death!", 1),
                new Scenario("{@P1} got defeated by Beido, who then got a new tattoo of {@P1}’s name", 1),
                new Scenario("Rumor has it, Bargz's cannon is fake. {@P1} tested the theory.", 1),

                new Scenario("Oh yay a dupe phantomlight! {@P1} rerolls it and gets... another phantomlight... two times in a row. {@P1} says farewell to the cruel world.", 1),
                new Scenario("{@P1} didn't realize his tank became DPS and started R3 heroic anyways.", 1),
                new Scenario("{@P1} thought it would be a good idea to try and ride Torlim and we can all imagine how that worked out.", 1),
                new Scenario("{@P1} catches {@P2} fighting Capt. Woodbeard. {@P1} dives in to steal {@P2}'s legendary drop but impales themself on Seism. Oops. What a Twisted Fate.", 2),
                new Scenario("All the familiars in the stable decide to revolt and swarm {@P1}. They pin him to the board and leave him to die.", 1),
                new Scenario("{@P1} thought taunting Krakers was a good idea. He very shortly found that his tentacles weren't fake..", 1),
                new Scenario("{@P1} calculates the number of dubloons it would take to max out a T6 set and faints. No one bothered to revive him.", 1),
                new Scenario("{@P1} meets Ingrid in the dungeon, {@P1} gets infected and turns into a zombie. {@P1} is dead.", 1),
            };
        }

        public static void Test()
        {
            try
            {
                int numUsers = 100000;
                List<Player> players = new List<Player>(numUsers);
                for (int i = 0; i < numUsers; i++)
                    players.Add(new Player(i));

                new BHungerGames().Run(1, 1, players, Console.WriteLine, Console.WriteLine, () => false, 40);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public BHungerGames()
        {
            _random = new Random();
        }

        private Scenario GetScenario(int playersAvailable)
        {
            while (true)
            {
                int randIndex = _random.Next(Scenarios.Length);
                if (Scenarios[randIndex].Delay <= 0 && playersAvailable >= Scenarios[randIndex].PlayersNeeded)
                {
                    Scenarios[randIndex].Delay = DelayValue;
                    return Scenarios[randIndex];
                }
            }
        }

        public void Run(int numWinners, int secondsDelayBetweenDays, List<Player> contestants, BotGameInstance.ShowMessageDelegate showMessageDelegate, BotGameInstance.ShowMessageDelegate sendMsg, Func<bool> cannelGame, int maxPlayers = 0)
        {
            TimeSpan delayBetweenDays = new TimeSpan(0, 0, 0, secondsDelayBetweenDays);
            int day = 0;
            StringBuilder sb = new StringBuilder(2000);
            StringBuilder sbDeath = new StringBuilder(2000);
            int consecutiveNoCasualties = 0;
            int showPlayersWhenCountEqualIndex = 0;

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

            if (maxPlayers > 0 && contestants.Count > maxPlayers)
            {
                int numToRemove = contestants.Count - maxPlayers;
                for (int i = 0; i < numToRemove; i++)
                {
                    int randIndex = _random.Next(contestants.Count);
                    sb.Append($"<{contestants[randIndex].ContestantName}>\t");
                    contestants.RemoveAt(randIndex);
                }
                showMessageDelegate("Players killed in the stampede trying to get to the arena:\r\n" + sb);
                sb.Clear();
            }
            while (contestants.Count > numWinners)
            {
                int startingContestantCount = contestants.Count;
                int scenarioCount = 0;
                for (int i = 0; i < startingContestantCount; i++)
                {
                    int randIndex = _random.Next(100);
                    if (randIndex >= PercentChanceToLivePerDay)
                    { // Someone has to die.
                        randIndex = _random.Next(contestants.Count);
                        string selectedPlayer = "<" + contestants[randIndex].ContestantName + ">";
                        AddDeathID(sbDeath, contestants[randIndex]);
                        contestants.RemoveAt(randIndex);
                        string[] players;
                        if (contestants.Count == 1)
                        {
                            players = new[] { selectedPlayer, "<" + contestants[0].ContestantName + ">" };
                        }
                        else
                        {
                            randIndex = _random.Next(contestants.Count);
                            int randIndex2 = _random.Next(contestants.Count);
                            while (randIndex == randIndex2)
                            {
                                randIndex2 = _random.Next(contestants.Count);
                            }
                            players = new[] { selectedPlayer, "<" + contestants[randIndex].ContestantName + ">", "<" + contestants[randIndex2].ContestantName + ">" };
                        }
                        sb.Append(GetScenario(players.Length).GetText(players)).Append("\n\n");
                        foreach (Scenario scenario in Scenarios)
                        {
                            scenario.ReduceDelay();
                        }
                        if (contestants.Count <= numWinners)
                            break;
                        if (MaxScenariosPerDay <= ++scenarioCount)
                            break;
                    }
                }

                if (startingContestantCount == contestants.Count)
                {
                    if (consecutiveNoCasualties >= MaxNoCasualtiesConsecutive)
                    {
                        continue;
                    }
                    consecutiveNoCasualties++;
                    sb.Append("No casualties.\n\n");
                }
                else
                {
                    consecutiveNoCasualties = 0;
                }

                showMessageDelegate($"\nDay **{++day}**  <{startingContestantCount}> Players remaining\n\n" + sb);
                if (sbDeath.Length > 0)
                {
                    sendMsg($"Dead people:\n" + sbDeath);
                    sbDeath.Clear();
                }
                sb.Clear();

                if (cannelGame())
                    return;

                if (contestants.Count > numWinners) // skip this code when we have the winners.
                {
                    if (contestants.Count <= ShowPlayersWhenCountEqual[showPlayersWhenCountEqualIndex])
                    {
                        showPlayersWhenCountEqualIndex++;
                        foreach (Player contestant in contestants)
                        {
                            sb.Append($"<{contestant.ContestantName}>\t");
                        }
                        showMessageDelegate("Players Remaining:\r\n" + sb);
                        sb.Clear();
                    }

                    Thread.Sleep(delayBetweenDays);
                }
                if (cannelGame())
                    return;
            }

            sb.Clear();
            sb.Append("\n\n**Game Over**\r\n\r\n");
            StringBuilder sbP = new StringBuilder(1000);
            foreach (Player contestant in contestants)
            {
                sbP.Append($"(ID:{contestant.UserId})<{contestant.FullUserName}> is victorious!\r\n");
                sb.Append($"<{contestant.FullUserName}> is victorious!\r\n");
            }
            showMessageDelegate(sb.ToString(), sbP.ToString());
        }

        public void AddDeathID(StringBuilder sb, Player player)
        {
            sb.Append($"<@{player.UserId}> ");
        }
    }
}