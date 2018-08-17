using System;
using System.Collections.Generic;
using System.Text;

namespace BHungerGaemsBot
{
    class ScenarioRPG
    {
        private string Description;
        private RarityRPG Rarity;
        public ScenarioTypeRPG Type;
        public int Timer;

        public ScenarioRPG(string description, RarityRPG rarity)
        {
            Description = description;
            Rarity = rarity;
            Timer = 0;
            //Type = type;
        }
        public ScenarioRPG(string description, ScenarioTypeRPG type)
        {
            Description = description;
            Timer = 0;
            Type = type;
        }

        public string GetText(string player, RarityRPG rarity, HeroClass heroClass, ItemDistribution itemDistribution)
        {
            string value = Description?.Replace("{player_name}", player);
            value = value?.Replace("{rarity_type}", rarity.ToString());
            value = value?.Replace("{class_type}", heroClass.ToString());
            value = value?.Replace("{item_distribution}", itemDistribution.ToString());

            return value;
        }
        public string GetText(string player)
        {
            string value = Description?.Replace("{player_name}", player);
            return value;
        }

        public void ReduceTimer()
        {
            if (Timer != 0) Timer--;
        }
    }
}
