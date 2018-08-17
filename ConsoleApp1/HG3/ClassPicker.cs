using System;
using System.Collections.Generic;
using System.Text;

namespace BHungerGaemsBot
{
    public class ClassPicker
    {
        WeightedClass[] weightedClass;
        Random Rand;
        public ClassPicker()
        {
            weightedClass = new WeightedClass[7];
            for (int i = 0; i < 7; i++)
            {
                weightedClass[i] = new WeightedClass((HeroClass)i);
            }
            Rand = new Random(Guid.NewGuid().GetHashCode());
        }
        public HeroClass PickAClass()
        {
            int roll = Rand.Next(GetTotalWeight());
            int incWeight = 0;
            for (int i = 0; i < 7; i++)
            {
                incWeight += weightedClass[i].Weight;
                if (roll < incWeight)
                {
                    weightedClass[i].Weight = 15;
                    for (int j = 0; j < 7; j++)
                    {
                        if (j == i) continue;
                        weightedClass[j].Weight += 4;
                    }
                    return (HeroClass)i;
                }
            }
            return HeroClass.Archer;
        }

        public int GetTotalWeight()
        {
            int totalWeight = 0;
            for (int i = 0; i < 7; i++)
            {
                totalWeight += weightedClass[i].Weight;
            }
            return totalWeight;
        }

    }
    public class WeightedClass
    {
        HeroClass HeroClass;
        public int Weight;

        public WeightedClass(HeroClass heroClass)
        {
            HeroClass = heroClass;
            Weight = 15;
        }
    }
}
