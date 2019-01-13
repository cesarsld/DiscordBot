using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace DiscordBot
{
    public class GiveawayParticipant
    {
        public Player playerInfo;
        private int roll;
        private int distance;

        public GiveawayParticipant(Player _player)
        {
            playerInfo = _player;
            distance = 0;
        }

        public int GetRoll() => roll;

        public void SetRoll(int _roll) => roll = _roll;

        public int GetDistance() => distance;

        public void SetDistance(int _roll) => distance = _roll;
    }
    public class GiveawayRunner
    {
        private readonly Random _random;

        public GiveawayRunner()
        {
            _random = new Random(Guid.NewGuid().GetHashCode());
        }
        
        private int GetDistance(int from, int to) => Math.Abs(from - to);

        private Dictionary<int, List<GiveawayParticipant>> CutExcessPlayers(Dictionary<int, List<GiveawayParticipant>> dict, int maxWinners)
        {
            var newDict = new Dictionary<int, List<GiveawayParticipant>>();
            int count = 0;
            foreach(var value in dict.Values)
            {
                count += value.Count;
            }
            if (count <= maxWinners) return dict;
            else 
            {
                var distList = dict.Keys.ToList().OrderBy(a => a).ToList();
                var newList = new List<int>();
                var cumulPlayers = 0;
                foreach (var key in distList)
                {
                    cumulPlayers += dict[key].Count;
                    newDict.Add(key, dict[key]);
                    if (cumulPlayers >= maxWinners)
                        return newDict;
                }
                return newDict;
            }
        }

        private List<GiveawayParticipant> GetListFromDict(Dictionary<int, List<GiveawayParticipant>> dict)
        {
            var newList = new List<GiveawayParticipant>();
            foreach (var list in dict.Values)
            {
                newList.AddRange(list);
            }
            return newList;
        }

        public async Task Run(int numWinners, int target, List<Player> contestants, BotGameInstance.ShowMessageDelegate showMessageDelegate, BotGameInstance.ShowMessageDelegate sendMsg, Func<bool> cancelGame)
        {
            bool winnerFound = false;
            TimeSpan delayBetweenMessagess = new TimeSpan(0, 0, 0, 3);
            StringBuilder sb = new StringBuilder(2000);
            List<GiveawayParticipant> participants = new List<GiveawayParticipant>();
            var winningDict = new Dictionary<int, List<GiveawayParticipant>>();
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

            foreach (Player player in contestants)
            {
                participants.Add(new GiveawayParticipant(player));
            }

            if(participants.Count == 0)
            {
                showMessageDelegate("Game cancelled, no players are eligible to play.");
                return;
            }
            int participantsPerMessage = 1;
            if (participants.Count >= 100) participantsPerMessage = 5;
            else if (participants.Count >= 40) participantsPerMessage = 3;

            while (!winnerFound)
            {
                
                int participantsPerMessageIndex = 1;
                foreach (GiveawayParticipant participant in participants)
                {
                    //Console.WriteLine("START");
                    if (cancelGame())
                        return;
                    int currentRoll = _random.Next(1000) + 1;

                    int distanceToTarget = GetDistance(target, currentRoll);

                    participant.SetDistance(distanceToTarget);
                    participant.SetRoll(currentRoll);
                    if (winningDict.ContainsKey(distanceToTarget))
                    {
                        winningDict[distanceToTarget].Add(participant);
                    }
                    else
                    {
                        winningDict.Add(distanceToTarget, new List<GiveawayParticipant> { participant });
                    }
                    sb.Append($"<@{participant.playerInfo.UserId}>  rolled: **" + participant.GetRoll().ToString() + "** \n");
                    if (participantsPerMessageIndex == participantsPerMessage)
                    {
                        sendMsg(sb.ToString());
                        participantsPerMessageIndex = 1;
                        sb.Clear();
                        await Task.Delay(delayBetweenMessagess);
                    }
                    else participantsPerMessageIndex++;

                    //Thread.Sleep(delayBetweenMessagess);
                }
                winningDict = CutExcessPlayers(winningDict, numWinners);
                participants = GetListFromDict(winningDict);
                winningDict.Clear();
                if (participants.Count == numWinners) winnerFound = true;
                else
                {
                    showMessageDelegate("A few participants have rolled the same minimum highest roll. A new round will be ran to find the true winner.");
                }
            }
            sb.Clear();
            foreach (var part in participants)
            {
                sb.Append($"<@{part.playerInfo.UserId}>  roll: **" + part.GetRoll().ToString() + "** \n");
            }
            sendMsg($"End of giveaway! \n Winners:\n" + sb);

        }
    }
}
