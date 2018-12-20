using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Axie.ApiCalls;
namespace DiscordBot.Axie.Breeding
{ 
    public class AxieGeneData
    {
        public string[] genome;
        public string Class;
        public AxieGeneTrait[] TraitData = new AxieGeneTrait[6];

        public AxieGeneData(string[] genes)
        {
            genome = genes;
            for (int i = 0; i < TraitData.Length; i++) TraitData[i] = new AxieGeneTrait();
        }

        public void GetDataFromGenome()
        {
            Class = GetClassFromBinary(genome[0].Substring(0, 4));
            for (int i = 0; i < TraitData.Length; i++)
            {
                TraitData[i].DominantClass = GetClassFromBinary(genome[i+2].Substring(2, 4));
                TraitData[i].R1Class = GetClassFromBinary(genome[i + 2].Substring(12, 4));
                TraitData[i].R2Class = GetClassFromBinary(genome[i + 2].Substring(22, 4));
            }
        }

        private string GetClassFromBinary(string binary)
        {
            switch (binary)
            {
                case "0000":
                    return "beast";
                case "0001":
                    return "bug";
                case "0010":
                    return "bird";
                case "0011":
                    return "plant";
                case "0100":
                    return "aquatic";
                case "0101":
                    return "reptile";
            }
            return "";
        }
        public float GetTraitProbability(string desiredClass, int index)
        {
            float probability = 0;
            probability += TraitData[index].DominantClass == desiredClass ? 0.375f : 0; //0.35
            probability += TraitData[index].R1Class == desiredClass ? 0.09375f : 0;      //0.11
            probability += TraitData[index].R2Class == desiredClass ? 0.03125f : 0;      //0.04 from freak  234375f weird from trung
            return probability;
        }

        public bool ContainsTrait(TraitMap traitMap)
        {
            var group = genome[traitMap.partGroup];
            for (int i = 0; i < 3; i++)
            {
                if (traitMap.classCode == group.Substring(2 + i * 10, 4) &&
                    traitMap.traitCode == group.Substring(6 + i * 10, 5))
                    return true;

            }
            return false;
        }

        public bool ContainsShape(ShapeMap shapeMap)
        {
            var group = genome[shapeMap.partGroup];
            for (int i = 0; i < 3; i++)
            {
                if (shapeMap.shapeCode == group.Substring(2 + i * 6, 6))
                    return true;
            }
            return false;
        }
    }

    public class AxieGeneTrait
    {
        public string DominantClass;
        public string R1Class;
        public string R2Class;
    }
}
