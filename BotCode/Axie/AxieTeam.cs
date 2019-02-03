using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Axie.Battles;

namespace DiscordBot.Axie
{
    public class AxieTeam
    {
        public string name;
        public AxieWinrate[] teamMembers;
        public AxieWinrate GetAxieByIndex(int i) => teamMembers[i];
        public AxieTeam()
        {
            name = "";
            teamMembers = new AxieWinrate[3];
            for (int i = 0; i < 3; i++)
            {
                teamMembers[i] = new AxieWinrate();
            }
        }

        public void CalculateTeamWinrate()
        {

        }
    }
}


