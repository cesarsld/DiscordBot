using System;
using System.Collections.Generic;
using System.Text;

namespace BHungerGaemsBot
{
    class ItemRPG
    {   
        private static Dictionary<Tuple<HeroClass, ItemDistribution>, List<float>> ItemStatDistributioDictionary = new Dictionary<Tuple<HeroClass, ItemDistribution>, List<float>>()
        {                                                                                        //    STA    STR    AGI    INT    DEX    WIS    LCK
            { Tuple.Create(HeroClass.Mage, ItemDistribution.Average),              new List<float> { 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f } },
            { Tuple.Create(HeroClass.Mage, ItemDistribution.Advantageous),         new List<float> { 0.05f, 0.05f, 0.05f, 0.35f, 0.10f, 0.30f, 0.10f } },
            { Tuple.Create(HeroClass.Mage, ItemDistribution.Extreme),              new List<float> { 0.00f, 0.00f, 0.05f, 0.55f, 0.05f, 0.35f, 0.15f } },

            { Tuple.Create(HeroClass.Knight, ItemDistribution.Average),            new List<float> { 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f } },
            { Tuple.Create(HeroClass.Knight, ItemDistribution.Advantageous),       new List<float> { 0.30f, 0.35f, 0.15f, 0.05f,0.025f,0.025f, 0.15f } },
            { Tuple.Create(HeroClass.Knight, ItemDistribution.Extreme),            new List<float> { 0.35f, 0.40f, 0.05f, 0.00f, 0.05f, 0.00f, 0.15f } },

            { Tuple.Create(HeroClass.Archer, ItemDistribution.Average),            new List<float> { 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f } },
            { Tuple.Create(HeroClass.Archer, ItemDistribution.Advantageous),       new List<float> { 0.05f, 0.05f, 0.30f, 0.05f, 0.35f, 0.10f, 0.10f } },
            { Tuple.Create(HeroClass.Archer, ItemDistribution.Extreme),            new List<float> { 0.00f, 0.00f, 0.05f, 0.55f, 0.05f, 0.35f, 0.15f } },

            { Tuple.Create(HeroClass.Monk, ItemDistribution.Average),              new List<float> { 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f } },
            { Tuple.Create(HeroClass.Monk, ItemDistribution.Advantageous),         new List<float> { 0.05f, 0.10f, 0.05f, 0.30f, 0.05f, 0.35f, 0.10f } },
            { Tuple.Create(HeroClass.Monk, ItemDistribution.Extreme),              new List<float> { 0.00f, 0.00f, 0.05f, 0.35f, 0.05f, 0.55f, 0.15f } },
                
            { Tuple.Create(HeroClass.Necromancer, ItemDistribution.Average),       new List<float> { 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f } },
            { Tuple.Create(HeroClass.Necromancer, ItemDistribution.Advantageous),  new List<float> { 0.05f, 0.05f, 0.10f, 0.35f, 0.10f, 0.30f, 0.10f } },
            { Tuple.Create(HeroClass.Necromancer, ItemDistribution.Extreme),       new List<float> { 0.00f, 0.00f, 0.00f, 0.80f, 0.00f, 0.20f, 0.15f } },

            { Tuple.Create(HeroClass.Assassin, ItemDistribution.Average),          new List<float> { 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f } },
            { Tuple.Create(HeroClass.Assassin, ItemDistribution.Advantageous),     new List<float> { 0.05f, 0.10f, 0.35f, 0.05f, 0.30f, 0.05f, 0.10f } },
            { Tuple.Create(HeroClass.Assassin, ItemDistribution.Extreme),          new List<float> { 0.05f, 0.10f, 0.50f, 0.00f, 0.35f, 0.00f, 0.15f } },

            { Tuple.Create(HeroClass.Elementalist, ItemDistribution.Average),      new List<float> { 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f } },
            { Tuple.Create(HeroClass.Elementalist, ItemDistribution.Advantageous), new List<float> { 0.05f, 0.15f, 0.10f, 0.35f, 0.20f, 0.10f, 0.10f } },
            { Tuple.Create(HeroClass.Elementalist, ItemDistribution.Extreme),      new List<float> { 0.00f, 0.00f, 0.05f, 0.55f, 0.35f, 0.05f, 0.15f } },
        };

        public ItemRPG()
        {
            Rarity = RarityRPG.None;
        }

        private const float ItemScaling = 1.5f;
        public RarityRPG Rarity { get; set; }

        public int[] ItemStats = new int[7];

        public int GetEffectiveCombatStats (float[] heroStatMult, int[] itemStats)
        {  
                int CombatPower = 0;
                for (int i = 0; i < itemStats.Length; i++)
                {
                    CombatPower += Convert.ToInt32(itemStats[i] * heroStatMult[i]);
                }
                return CombatPower;
        }
        public void GetNewItem(int level, RarityRPG rarity, HeroClass itemClassType, float[] heroStatMult, ItemDistribution itemDistribution)
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            int totalStats = (int)rarity * 5 + 50 + rand.Next((int)rarity * 2 + 20);
            float levelMultiplier = 1f + level / 4f;
            totalStats = Convert.ToInt32(totalStats * levelMultiplier);
            int[] newItemStats = new int[7];
            for (int i = 0; i < newItemStats.Length; i++)
            {
                newItemStats[i] = Convert.ToInt32(totalStats * ItemStatDistributioDictionary[Tuple.Create(itemClassType, itemDistribution)][i]);
            }

            int newCombatPower = GetEffectiveCombatStats(heroStatMult, newItemStats);
            if (newCombatPower > GetEffectiveCombatStats(heroStatMult, ItemStats))
            {
                ItemStats = newItemStats;
                Rarity = rarity;
            }
        }
    }
}
