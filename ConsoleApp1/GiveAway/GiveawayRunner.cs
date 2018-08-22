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

        public void Run(int numWinners, List<Player> contestants, BotGameInstance.ShowMessageDelegate showMessageDelegate, BotGameInstance.ShowMessageDelegate sendMsg, Func<bool> cannelGame)
        {
            bool winnerFound = false;
            TimeSpan delayBetweenDays = new TimeSpan(0, 0, 0, 3);
            int day = 0;
            StringBuilder sb = new StringBuilder(2000);
            StringBuilder sbDeath = new StringBuilder(2000);
            List<GiveawayParticipant> participants = new List<GiveawayParticipant>();
            List<GiveawayParticipant> tiedParticipants = new List<GiveawayParticipant>();
            int highestRoll = 0;
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
            while (!winnerFound)
            {
                foreach (GiveawayParticipant participant in participants)
                {
                    if (cannelGame())
                        return;
                    int currentRoll = _random.Next(1000) + 1;
                    participant.SetRoll(currentRoll);
                    if (currentRoll > highestRoll)
                    {
                        highestRoll = currentRoll;
                        tiedParticipants.Clear();
                        tiedParticipants.Add(participant);
                    }
                    else if (currentRoll == highestRoll) tiedParticipants.Add(participant);

                    sendMsg($" <@{participant.playerInfo.UserId}>  rolled: **" + participant.GetRoll().ToString() + "**");
                    Thread.Sleep(delayBetweenDays);
                }
                participants = new List<GiveawayParticipant>(tiedParticipants);
                if (participants.Count == 1) winnerFound = true;
                else
                {
                    showMessageDelegate("At least two participants have rolled the same highest roll. A new round will be ran to find the true winner.");
                    highestRoll = 0;
                }
            }

            sendMsg($"End of giveaway! \n <@{tiedParticipants[0].playerInfo.UserId}> is the winner!");

        }
    }
}
