using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DiscordBot.Axie
{
    class PureBreeder
    {
        //static BigInteger value = BigInteger.Parse("8594385836028242049444494044180692155268569324804469339946759268613786701828");
        public static float GetBreedingChance(string gene1, string gene2)
        {
            gene1 = calcBinary(gene1);
            gene2 = calcBinary(gene2);
            string[] gene1Array = GetSubGroups(gene1);
            string[] gene2Array = GetSubGroups(gene2);
            AxieGeneData axieGeneData1 = new AxieGeneData(gene1Array);
            AxieGeneData axieGeneData2 = new AxieGeneData(gene2Array);
            axieGeneData1.GetDataFromGenome();
            axieGeneData2.GetDataFromGenome();
            return GetChance(axieGeneData1, axieGeneData2);
        }

        public static string calcBinary(string gene)
        {
            BigInteger gene256 = BigInteger.Parse(gene);
            var bytes = gene256.ToByteArray();
            var idx = bytes.Length - 1;

            // Create a StringBuilder having appropriate capacity.
            var base2 = new StringBuilder(bytes.Length * 8);

            // Convert first byte to binary.
            var binary = Convert.ToString(bytes[idx], 2).PadLeft(8, '0');

            if (bytes.Length < 32)
            {
                for (int i = 0; i < 32 - bytes.Length; i++) base2.Append("00000000");
            }
            

            // Ensure leading zero exists if value is positive.
            if (binary[0] != '0' && gene256.Sign == 1)
            {
                base2.Append('0');
            }

            // Append binary string to StringBuilder.
            base2.Append(binary);

            // Convert remaining bytes adding leading zeros.
            for (idx--; idx >= 0; idx--)
            {
                base2.Append(Convert.ToString(bytes[idx], 2).PadLeft(8, '0'));
            }

            return base2.ToString();
        }
        private static string[] GetSubGroups(string gene)
        {
            string[] geneArray = new string[8];
            int stringIndex = 0;
            for (int i = 0; i < geneArray.Length; i++)
            {
                StringBuilder subGeneGroup = new StringBuilder();
                for (int j = 0; j < 32; j++)
                {
                    subGeneGroup.Append(gene[stringIndex]);
                    stringIndex++;
                }
                geneArray[i] = subGeneGroup.ToString();
                subGeneGroup.Clear();
            }
            return geneArray;
        }
        private static float GetChance(AxieGeneData axie1, AxieGeneData axie2)
        {
            string desiredClass = axie1.Class;
            float[] probabilityTable = new float[6];
            float probability = 1;
            for (int i = 0; i < probabilityTable.Length; i++)
            {
                probability *= (axie1.GetTraitTransmissionProbability(desiredClass, i) + axie2.GetTraitTransmissionProbability(desiredClass, i));
            }
            return probability * 100;
        }
    }
}