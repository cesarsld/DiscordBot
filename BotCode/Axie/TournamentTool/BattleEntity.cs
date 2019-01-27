using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Axie.TournamentTool
{
    public class BattleEntity
    {
        public int id;
        public string Class;
        public int team;
        public int position;
        public EntityStats stats;
        public EntityMoveSet[] moveSet;

        public string GetStats()
        {
            string returnString = "**Stats**\n";
            returnString += $"HP = {stats.hp}"+ "\n";
            returnString += $"Speed = {stats.speed}" + "\n";
            returnString += $"Skill = {stats.skill}" + "\n";
            returnString += $"Morale = {stats.morale}" + "\n";
            returnString += $"Last stand = {stats.totalLastStand} \n\n";
            return returnString;
        }

        public string GetMoveset()
        {
            moveSet = moveSet.OrderBy(a => a.index).ToArray();
            string returnString = "**Mover order**\n";
            for (int i = 1; i < 5; i++)
            {
                returnString += $"{i}. {moveSet[i - 1].move.name} - {moveSet[i - 1].move.attack}/{moveSet[i - 1].move.defense}/{moveSet[i - 1].move.accuracy}\n";
            }
            return returnString;
        }

        public string GetRatings()
        {
            var turtle = ":turtle: ";
            var sword = ":crossed_swords: ";
            var str = "";
            var tank = GetTankScore();
            var dps = GetAttackScore();
            if (tank > 80) str += turtle + turtle + turtle + turtle + "\n";
            else if (tank > 75) str += turtle + turtle + turtle + "\n";
            else if (tank > 65) str += turtle + turtle + "\n";
            else if (tank > 55) str += turtle + "\n";

            if (dps > 80) str += sword + sword + sword + sword + "\n";
            else if (dps > 75) str += sword + sword + sword + "\n";
            else if (dps > 65) str += sword + sword + "\n";
            else if (dps > 55) str += sword + "\n";

            return str;
        }
        public int GetAttackScore()
        {
            int dpr = GetDPR();
            return (int)Math.Floor(GetDPR() / 91.5f * 100);
        }
        public int GetTankScore()
        {
            float tnk = GetTNK();
            float minTnk = 33;
            return (int)Math.Floor((tnk - minTnk) / (129 - minTnk) * 100);
        }
        public int GetDPR()
        {
            int dpr = 0;
            foreach (var move in moveSet)
                dpr += move.move.attack * move.move.accuracy / 100;
            return dpr;
        }

        public float GetTNK()
        {
            float tnk = stats.hp;
            foreach (var move in moveSet)
                tnk += move.move.defense;
            return tnk;
        }
    }

    public class EntityStats
    {
        public int hp;
        public int speed;
        public int morale;
        public int skill;
        public int totalLastStand;
    }

    public class EntityMoveSet
    {
        public int index;
        public EntityPart part;
        public EntityMove move;
    }
    public class EntityPart
    {
        public string id;
        public string name;
        public string type;
    }

    public class EntityMove
    {
        public string id;
        public string name;
        public int attack;
        public int defense;
        public int accuracy;
    }
}
