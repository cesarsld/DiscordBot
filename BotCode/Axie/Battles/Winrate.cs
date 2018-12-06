using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

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
        public AxieWinrate()
        { }
        public AxieWinrate(int _id, int _win, int _loss, string history, int date)
        {
            id = _id;
            win = _win;
            loss = _loss;
            battleHistory = history;
            mysticCount = 0;
            lastBattleDate = date;
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
}
