using System;
using Discord;
using Discord.Commands;

namespace DiscordBot.AxieRace
{
    public class AxieRacer : Player
    {
        private AxieClass RacerClass;
        private int Pace;
        private int Awaraness;
        private int Diet;

        public int totalScore { get; set; }

        private int PracticeTries;
        public int QualifierRun { get; set; }
        public int raceDiffential = 0;
        public int totalRaceTime;
        public RaceStyle raceStyle { get; set; }
        public bool canPractice {
            get {
                return PracticeTries-- > 0;
            }
        }
        public AxieRacer(IUser param) : base(param)
        {
            RacerClass = AxieClass.undefined;
            Pace = 0;
            Awaraness = 0;
            Diet = 0;
            PracticeTries = 60;
            QualifierRun = 9999;
            totalScore = 0;
        }

        public AxieClass GetClass() => RacerClass;
        public int GetPace() => Pace;
        public int GetAwareness() => Awaraness;
        public int GetDiet() => Diet;

        public void SetRacerData(AxieClass _class, int pace, int awareness, int diet)
        {
            RacerClass = _class;
            Pace = pace;
            Awaraness = awareness;
            Diet = diet;
        }


    }
}
