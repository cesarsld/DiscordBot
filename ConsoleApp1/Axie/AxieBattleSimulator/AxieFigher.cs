using System;
using System.Collections.Generic;
using DiscordBot.Axie;
namespace DiscordBot.Axie.AxieBattleSimulator
{
    public class AxieFigher : AxieData
    {
        private int currentHp;
        private Dictionary<int, string> skillOrder;
        private int lastStandCount;

        public AxieFigher()
        {
            currentHp = stats.hp;
            skillOrder = new Dictionary<int, string>();
        }
    }

    public enum Effect
    {
        passive,
        active
    }
}
