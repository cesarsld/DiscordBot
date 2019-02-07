using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Discord.Commands;
using Discord;
using DiscordBot.Axie.ApiCalls;
namespace DiscordBot.Axie.Breeding
{
    class PureBreeder
    {
   
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

        public static async Task GetPureBreedingChancesFromAddress(string address, IUserMessage message, List<string> classFilters = null)
        {
            string[] addresses = address.Split(' ');
            var listFromApi = new List<AxieDataOld>();
            foreach(var add in addresses)
            {
                if (AxieHolderListHandler.IsAddressValid(add))
                {
                    listFromApi.AddRange(await StatDataHandler.GetAxieListFromAddress(add));
                }

            }
            //var listFromApi = await StatDataHandler.GetAxieListFromAddress(address);


            var predisposedAxieList = new Dictionary<int, AxieDataOld>();
            var axieList_6_6 = new List<AxieDataOld>();
            var axieList_5_6 = new List<AxieDataOld>();
            var axieList_4_6 = new List<AxieDataOld>();
            var axieList_3_6 = new List<AxieDataOld>();
            Console.WriteLine("Starting filtering : ");
            foreach (var axie in listFromApi)
            {
                Console.Write("|");
                if (classFilters.Count > 0)
                {
                    if (classFilters.Contains(axie.Class))
                    {
                        if (await axie.CanBreed())
                        {
                            switch (axie.GetAbsolutePureness())
                            {
                                case 6:
                                    axieList_6_6.Add(axie);
                                    predisposedAxieList.Add(axie.id, axie);
                                    break;
                                case 5:
                                    axieList_5_6.Add(axie);
                                    predisposedAxieList.Add(axie.id, axie);
                                    break;
                                case 4:
                                    axieList_4_6.Add(axie);
                                    predisposedAxieList.Add(axie.id, axie);
                                    break;
                                case 3:
                                    axieList_3_6.Add(axie);
                                    predisposedAxieList.Add(axie.id, axie);
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    if (await axie.CanBreed())
                    {
                        switch (axie.GetAbsolutePureness())
                        {
                            case 6:
                                axieList_6_6.Add(axie);
                                predisposedAxieList.Add(axie.id, axie);
                                break;
                            case 5:
                                axieList_5_6.Add(axie);
                                predisposedAxieList.Add(axie.id, axie);
                                break;
                            case 4:
                                axieList_4_6.Add(axie);
                                predisposedAxieList.Add(axie.id, axie);
                                break;
                            case 3:
                                axieList_3_6.Add(axie);
                                predisposedAxieList.Add(axie.id, axie);
                                break;
                        }
                    }
                }
            }
            var axieList = predisposedAxieList.Values.ToList();
            var breedMatchList = new List<PureBreedMatch>();
            while (axieList.Count > 1)
            {
                var axieA = axieList[0];
                var axieB = FindBestMatch(ref axieA, axieList);
                if (axieB != null)
                {
                    breedMatchList.Add(new PureBreedMatch(axieA.id, axieB.id, GetBreedingChance(axieA.genes, axieB.genes)));
                    axieList.Remove(axieB);
                }
                axieList.Remove(axieA);

            }
            string breedData = JsonConvert.SerializeObject(breedMatchList.OrderByDescending(m => m.pureChance), Formatting.Indented);


            string breedPath = "PureBreedList.txt";
            if (File.Exists(breedPath))
            {
                using (var tw = new StreamWriter(breedPath))
                {
                    await tw.WriteAsync(breedData);
                }
            }
            await message.Channel.SendFileAsync("PureBreedList.txt");
        }

        private static AxieDataOld FindBestMatch(ref AxieDataOld axie, List<AxieDataOld> axieList, AxieDataOld axieToCompare = null)
        {
            AxieDataOld bestAxie = null;
            float bestChance = 0;
            for (int i = 0; i < axieList.Count; i++)
            {
                if (axie.id != axieList[i].id)
                {
                    if (AreAxieRelated(axie, axieList[i])) continue;
                    else
                    {
                        float chance = GetBreedingChance(axie.genes, axieList[i].genes);
                        if (bestChance < chance)
                        {
                            bestChance = chance;
                            bestAxie = axieList[i];
                        }
                    }
                }
            }
            if (axieToCompare != null && axieToCompare == bestAxie) return bestAxie;
            else
            {
                if (bestChance == 0) return null;
                else
                {
                    axieToCompare = axie;
                    axie = bestAxie;
                    return FindBestMatch(ref axie, axieList, axieToCompare);
                }
            }
        }

        private static bool AreAxieRelated2(AxieDataOld father, AxieDataOld mother)
        {
            if (   father.id == mother.matronId
                || father.id == mother.sireId
                || father.id == father.matronId
                || father.id == father.sireId
                || mother.id == father.matronId
                || mother.id == father.sireId
                || mother.id == mother.matronId
                || mother.id == mother.sireId)
                return true;
            else
                return false; ;
        }

        private static bool AreAxieRelated(AxieDataOld father, AxieDataOld mother)
        {
            if (   
                //father not mother's parents
                father.id == mother.matronId
                || father.id == mother.sireId
                //mother not father's parents
                || mother.id == father.matronId
                || mother.id == father.sireId

                //check if parents are siblings
                || father.matronId == mother.matronId
                || father.matronId == mother.sireId
                || father.sireId == mother.matronId
                || father.sireId == mother.sireId)
               
                return true;
            else
                return false; ;
        }


        public static string calcBinary(string gene)
        {
            BigInteger gene256 = BigInteger.Parse(gene);
            var bytes = gene256.ToByteArray();
            //var idx = bytes.Length - 1;
            var idx = 31;

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
                //base2.Append('0');
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
        public static string[] GetSubGroups(string gene)
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
            string desiredClass1 = axie1.Class;
            string desiredClass2 = axie2.Class;
            float[] probabilityTable = new float[6];
            float probability1 = 1;
            float probability2 = 1;
            if (axie1.Class != axie2.Class)
            {
                probability1 /= 2;
                probability2 /= 2;
            }
            for (int i = 0; i < probabilityTable.Length; i++)
            {
                probability1 *= (axie1.GetTraitProbability(desiredClass1, i) + axie2.GetTraitProbability(desiredClass1, i));
                probability2 *= (axie1.GetTraitProbability(desiredClass2, i) + axie2.GetTraitProbability(desiredClass2, i));
            }
            return probability1 > probability2? probability1 * 100 : probability2 * 100;
        }
    }

    public class PureBreedMatch
    {
        public int sire;
        public int matron;
        public float pureChance;

        public PureBreedMatch()
        {
            sire = 0;
            matron = 0;
            pureChance = 0;
        }

        public PureBreedMatch(int dad, int mom, float chance)
        {
            sire = dad;
            matron = mom;
            pureChance = chance;
        }
    }

}