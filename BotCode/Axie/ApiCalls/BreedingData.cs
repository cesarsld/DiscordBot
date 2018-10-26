using System;
using System.Threading;
using Discord;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using DiscordBot.Axie;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace DiscordBot.Axie.ApiCalls
{
    public class BreedingData
    {
        public static async Task GetData()
        {
            int mixMatronClass = 0;
            int mixSireClass = 0;
            int pureMatronClass = 0;
            int pureSireClass = 0;
            int pureBreeding = 0;
            int mixBreeding = 0;

            bool dataAvailable = true;
            int axieCount = 7040;
            int axieIndex = 3530;
            int perc = axieCount / 100;
            int currentPerc = axieIndex / perc;
            while (axieIndex < axieCount && dataAvailable)
            {
                axieIndex++;
                if (axieIndex % perc == 0)
                {
                    currentPerc++;
                    Console.WriteLine(currentPerc.ToString() + "%");
                }
                var axieData = await AxieObject.GetAxieFromApi(axieIndex);
                if (axieData != null && axieData.sireId != 0)
                {
                    var matronData = await AxieObject.GetAxieFromApi(axieData.matronId);
                    var sireData = await AxieObject.GetAxieFromApi(axieData.sireId);
                    if (matronData != null)
                    {
                        if (sireData.Class == matronData.Class)
                        {
                            pureBreeding++;
                            if (axieData.Class == matronData.Class) pureMatronClass++;
                            else pureSireClass++;
                        }
                        else
                        {
                            mixBreeding++;
                            if (axieData.Class == matronData.Class) mixMatronClass++;
                            else mixSireClass++;
                        }
                    }

                }
            }
            float matronPerc = (float)mixMatronClass / mixBreeding;
            string data = $"Out of all {mixBreeding} mix breeds, {matronPerc * 100}% took the class of the matron and {(1f - matronPerc) * 100}% toold the class of the sire.";
            string path = "BreedData.txt";
            if (File.Exists(path))
            {
                using (var tw = new StreamWriter(path))
                {
                    tw.Write(data);
                }
            }
        }
    }
}
