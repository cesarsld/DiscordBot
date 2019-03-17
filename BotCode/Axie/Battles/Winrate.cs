using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.IO;

namespace DiscordBot.Axie.Battles
{
    public class AxieWinrate
    {
        public int id;
        public int win;
        public int loss;
        public float winrate;
        public string battleHistory;
        public int mysticCount;
        public int lastBattleDate;
        public string[] moves;

        public List<long> wonBattles;
        public List<long> lostBattles;

        public AxieWinrate()
        { }
        public AxieWinrate(int _id, int _win, int _loss, string history, int date, int battleId, bool outcome)
        {
            id = _id;
            win = _win;
            loss = _loss;
            battleHistory = history;
            mysticCount = 0;
            lastBattleDate = date;
            wonBattles = new List<long>();
            lostBattles = new List<long>();
            if (outcome) wonBattles.Add(battleId);
            else lostBattles.Add(battleId);
        }
        public void GetWinrate()
        {
            winrate = (float)win / (win + loss) * 100;
            TrimHistory();
        }
        public void AddLatestResults(AxieWinrate winrate)
        {
            win += winrate.win;
            loss += winrate.loss;
            GetWinrate();
            battleHistory += winrate.battleHistory.Substring(2);
            lastBattleDate = winrate.lastBattleDate;
            TrimHistory();
        }

        private void TrimHistory()
        {
            if (battleHistory.Length > 44)
            {
                battleHistory = battleHistory.Remove(2, battleHistory.Length - 44);
            }
        }

        public int GetRecentWins()
        {
            var history = battleHistory.Substring(2);
            return history.Count(c => c == '1');
        }
        public int GetRecentLosses()
        {

            var history = battleHistory.Substring(2);
            return history.Count(c => c == '0');
        }

        public float GetRecentWinrate()
        {
            return (float)GetRecentWins() / (GetRecentWins() + GetRecentLosses()) * 100;
        }


        public async Task GetAllLogs(IUserMessage message)
        {
            StringBuilder sbWin = new StringBuilder();
            sbWin.AppendLine();
            StringBuilder sbLoss = new StringBuilder();
            sbLoss.AppendLine();
            foreach (var win in wonBattles.OrderByDescending(a => a))
            {
                sbWin.Append($"Battle #{win}");
                sbWin.AppendLine();
            }
            sbWin.AppendLine();
            sbWin.AppendLine();
            sbWin.AppendLine();
            foreach (var loss in lostBattles.OrderByDescending(a => a))
            {
                sbLoss.Append($"Battle #{loss}");
                sbLoss.AppendLine();
            }
            string data = "Won battles" + sbWin + "\n\nLost Battles" + sbLoss;
            using (var tw = new StreamWriter("BattleLogs.txt"))
            {
                tw.Write(data);
            }
            var dmMessage = await message.Author.SendMessageAsync($"Logs of Axie #{id}");
            await dmMessage.Channel.SendFileAsync("BattleLogs.txt");
        }

    }
    public class Winrate
    {
        public int win;
        public int loss;
        public float winrate;
        public Winrate(int _win, int _loss)
        {
            win = _win;
            loss = _loss;
        }
        public void GetWinrate()
        {
            winrate = (float)win / (win + loss) * 100;
        }
    }

    public class AxieWinrateReduced
    {
        public int id;
        public int win;
        public int loss;
        public float winrate;
        public string battleHistory;
        public int mysticCount;
        public int lastBattleDate;
        public string[] moves;

        public AxieWinrateReduced(AxieWinrate wr)
        {
            id = wr.id;
            win = wr.win;
            loss = wr.loss;
            winrate = wr.winrate;
            battleHistory = wr.battleHistory;
        }
    }
}
