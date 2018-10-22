using System;
namespace DiscordBot
{
    public enum AxieType
    {
        Beast,
        Aqua,
        Plant,
        Bug,
        Bird,
        Reptile,
        Nocturnal,
        StarLight,
        Mecha
    }
    public class AxieArmy
    {
        private int ArmyCount {
            get 
            {
                int count = 0;
                foreach (var subArmy in ArmyDistribution) count += subArmy;
                return count;
            }
        }
        private int[] ArmyDistribution;
        public AxieArmy()
        {
            ArmyDistribution = new int[9];
        }

        public void AddAxies(int axieType, int count)
        {
            if (count > 0)
            {
                ArmyDistribution[axieType] += count;
            }
        }
    }
}
