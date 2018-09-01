using System;
using Discord;

namespace DiscordBot
{
    public class AxiePlayer : Player
    {
        private int LandCount { get; set; }
        private AxieArmy Army;
        private int MaterialCount { get; set; }

        public AxiePlayer(IUser param) : base (param)
        {
            LandCount = 10;
            Army = new AxieArmy();
            MaterialCount = 0;
        }




    }
}
