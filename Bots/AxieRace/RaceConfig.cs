using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.AxieRace
{
    public struct RaceConfig
    {
        public AxieClass RacerClass;
        public int Pace;
        public int Awaraness;
        public int Diet;
        public int RandomRaceTime;
        public int[] classRanking;

        public RaceConfig(AxieClass _class, int pace, int awareness, int diet, int randomRaceTime, int[] _classranking)
        {
            RacerClass = _class;
            Pace = pace;
            Awaraness = awareness;
            Diet = diet;
            RandomRaceTime = randomRaceTime;
            classRanking = _classranking;
        }
    }
}