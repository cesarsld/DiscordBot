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

        private void CutExcessPlayers(Dictionary<int, List<GiveawayParticipant>> dict, int maxWinners)
        {
            int count = 0;
            foreach(var value in dict.Values)
            {
                count += value.Count;
            }
            if (count <= maxWinners) return;
            else 
            {
                
            }
        }

        public async Task Run(int numWinners, int target, List<Player> contestants, BotGameInstance.ShowMessageDelegate showMessageDelegate, BotGameInstance.ShowMessageDelegate sendMsg, Func<bool> cancelGame)
        {
            bool winnerFound = false;
            TimeSpan delayBetweenMessagess = new TimeSpan(0, 0, 0, 3);
            int bestRollDistance = 1000;
            int minDistanceToWin = 1;
            StringBuilder sb = new StringBuilder(2000);
            StringBuilder sbDeath = new StringBuilder(2000);
            List<GiveawayParticipant> participants = new List<GiveawayParticipant>();
            List<GiveawayParticipant> winningParticipants = new List<GiveawayParticipant>();
            List<GiveawayParticipant> lockedWinners = new List<GiveawayParticipant>();
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
                    if (distanceToTarget <= bestRollDistance)
                    {
                        bestRollDistance = distanceToTarget;
                        //winningParticipants.Clear();
                        winningParticipants.Insert(0, participant);
                        winningDict.Add(distanceToTarget, new List<GiveawayParticipant> { participant });
                        //winningDict.or

                        if (winningParticipants.Count > numWinners)
                        {
                            //winningParticipants = winningParticipants.OrderBy(a => a.GetDistance()).ToList();
                            if (winningParticipants[winningParticipants.Count - 1].GetDistance() >
                               winningParticipants[winningParticipants.Count - 2].GetDistance())
                            {
                                winningParticipants.RemoveAt(winningParticipants.Count - 1);
                            }

                        }
                    }
                    else if (distanceToTarget > minDistanceToWin)
                    {
                        winningParticipants.Add(participant);
                        if(winningParticipants.Count == numWinners) 
                        {
                            winningParticipants = winningParticipants.OrderBy(a => a.GetDistance()).ToList();
                            minDistanceToWin = distanceToTarget;
                        }
                        
                        else
                        {
                            winningParticipants = winningParticipants.OrderBy(a => a.GetDistance()).ToList();
                            winningParticipants.RemoveAt(winningParticipants.Count - 1);
                        }
                    }
                    else if(distanceToTarget == minDistanceToWin)
                    {
                        winningParticipants.Add(participant);
                    }

                    sb.Append($"<@{participant.playerInfo.UserId}>  rolled: **" + participant.GetDistance().ToString() + "** \n");
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
                participants = new List<GiveawayParticipant>(winningParticipants);
                if (participants.Count == 1) winnerFound = true;
                else
                {
                    showMessageDelegate("At least two participants have rolled the same highest roll. A new round will be ran to find the true winner.");
                    bestRollDistance = 1000;
                }
            }

            sendMsg($"End of giveaway! \n <@{winningParticipants[0].playerInfo.UserId}> is the winner!");

        }
    }
}
