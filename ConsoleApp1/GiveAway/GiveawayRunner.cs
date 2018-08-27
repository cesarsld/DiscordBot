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

        public GiveawayParticipant(Player _player)
        {
            playerInfo = _player;
            roll = 0;
        }

        public int GetRoll() => roll;

        public void SetRoll(int _roll) => roll = _roll;
    }
    public class GiveawayRunner
    {
        private readonly Random _random;

        public GiveawayRunner()
        {
            _random = new Random(Guid.NewGuid().GetHashCode());
        }
        
        private int GetDistance(int from, int to) => Math.Abs(from - to);

        public async Task Run(int numWinners, int target, List<Player> contestants, BotGameInstance.ShowMessageDelegate showMessageDelegate, BotGameInstance.ShowMessageDelegate sendMsg, Func<bool> cancelGame)
        {
            bool winnerFound = false;
            TimeSpan delayBetweenMessagess = new TimeSpan(0, 0, 0, 3);
            int bestRollDistance = 1000;
            StringBuilder sb = new StringBuilder(2000);
            StringBuilder sbDeath = new StringBuilder(2000);
            List<GiveawayParticipant> participants = new List<GiveawayParticipant>();
            List<GiveawayParticipant> tiedParticipants = new List<GiveawayParticipant>();
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

                    participant.SetRoll(currentRoll);
                    if (distanceToTarget < bestRollDistance)
                    {
                        bestRollDistance = distanceToTarget;
                        tiedParticipants.Clear();
                        tiedParticipants.Add(participant);
                    }
                    else if (distanceToTarget == bestRollDistance) tiedParticipants.Add(participant);

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
                participants = new List<GiveawayParticipant>(tiedParticipants);
                if (participants.Count == 1) winnerFound = true;
                else
                {
                    showMessageDelegate("At least two participants have rolled the same highest roll. A new round will be ran to find the true winner.");
                    bestRollDistance = 1000;
                }
            }

            sendMsg($"End of giveaway! \n <@{tiedParticipants[0].playerInfo.UserId}> is the winner!");

        }
    }
}
