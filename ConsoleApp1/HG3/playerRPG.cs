using Discord;
using System;

namespace BHungerGaemsBot
{
    class PlayerRPG : Player
    {
        public long Points { get; set; } // NERF POINTS
        public int Notoriety { get; set; }

        public bool IsInLeaderboard { get; set; }
        public bool HasDueled { get; set; }
        public bool AuraBonus { get; set; }
        public bool PartookInEvent { get; set; }
        public GamblingOptions GamblingOption { get; set; }

        Random _random;

        private const float LevelFactor = 1.4f;
        private const float BaseExp = 50f;
        private long Experience { get; set; }
        public int Level
        {
            get
            {
                return Convert.ToInt32(Math.Pow(Experience / BaseExp, 1f / LevelFactor));
            }
        }

        public int EffectiveCombatPower {
            get
            {
                int CombatPower = 0;
                for (int i = 0; i < HeroStats.Length; i++)
                {
                    CombatPower += Convert.ToInt32(HeroStats[i] * HeroStatMult[i]);
                }
                foreach (ItemRPG item in Items)
                {
                    CombatPower += Convert.ToInt32(item.GetEffectiveCombatStats(HeroStatMult, item.ItemStats));
                }
                return CombatPower;
            }
        }

        public int[] HeroStats = new int[7];
        public float[] HeroStatMult = new float[7];

        public HeroClass HeroClass { get; set; }

        public InteractiveRPGDecision InteractiveRPGDecision { get; set; }

        public ItemRPG[] Items { get; set; }
        public Familiar Familiar = new Familiar();

        public PlayerRPG(IUser userParm) : base(userParm)
        {
            HeroClass = HeroClass.Mage;
            GamblingOption = GamblingOptions._0;
            IsInLeaderboard = false;
            Items = new ItemRPG[BHungerGamesV3.NumItemTypes];
            for (var index = 0; index < Items.Length; index++)
            {
                Items[index] = new ItemRPG();
            }
            _random = new Random(Guid.NewGuid().GetHashCode());
            HasDueled = false;
            AuraBonus = false;
            PartookInEvent = false;
        }
        public PlayerRPG(int index) : base(index)
        {
            HeroClass = HeroClass.Mage;
            GamblingOption = GamblingOptions._0;
            IsInLeaderboard = false;
            Items = new ItemRPG[BHungerGamesV3.NumItemTypes];
            for (var i = 0; i < Items.Length; i++)
            {
                Items[i] = new ItemRPG();
            }
            HasDueled = false;
            AuraBonus = false;
            PartookInEvent = false;
            _random = new Random(Guid.NewGuid().GetHashCode());
        }

        public void WorldBossRewards(bool battleResult)
        {
            float gamblingMult = 0;
            switch (GamblingOption)
            {
                case GamblingOptions._1:
                    gamblingMult = 0.01f;
                    break;
                case GamblingOptions._5:
                    gamblingMult = 0.05f;
                    break;
                case GamblingOptions._10:
                    gamblingMult = 0.1f;
                    break;
                case GamblingOptions._25:
                    gamblingMult = 0.25f;
                    break;
                case GamblingOptions._50:
                    gamblingMult = 0.5f;
                    break;
                case GamblingOptions._75:
                    gamblingMult = 0.75f;
                    break;
                case GamblingOptions._100:
                    gamblingMult = 1f;
                    break;
            }
            if (battleResult)
            {
                Points += Convert.ToInt64(Points * gamblingMult) * 3;
            }
            else
            {
                Points -= Convert.ToInt64(Points * gamblingMult);
            }
        }

        public void GiveExp(int adventureCompletion, Tuple<HeroClass, DailyBuff> dailyBuff)
        {
            int totalExp = 0;
            int exp = 10 + Level * 2;
            if (InteractiveRPGDecision == InteractiveRPGDecision.LookForExp)
            {
                exp = Convert.ToInt32(exp * 1.25);
                if (HeroClass == dailyBuff.Item1 && dailyBuff.Item2 == DailyBuff.Increased_Experience_Gain)
                {
                    exp = Convert.ToInt32(exp * 1.15);
                }
            }
            if (AuraBonus && InteractiveRPGDecision == InteractiveRPGDecision.LookForExp)
            {
                exp *= 2;
                AuraBonus = !AuraBonus;
            }

            for (int i = 0; i < adventureCompletion; i++)
            {
                totalExp += _random.Next(Convert.ToInt32(0.8 * exp), Convert.ToInt32(1.2 * exp));
                exp = Convert.ToInt32(1.2 * exp);
            }
            AddExp(totalExp);
            Points += Convert.ToInt64(totalExp / 4);
        }

        public void GiveScore(int adventureCompletion, Tuple<HeroClass, DailyBuff> dailyBuff)
        {
            double scoreMultiplier = 1.5 + (Level * 1.5) + Math.Pow(Level, 2);
            if (HeroClass == dailyBuff.Item1 && dailyBuff.Item2 == DailyBuff.Increased_Points_Collection)
            {
                scoreMultiplier = Convert.ToInt32(scoreMultiplier * 1.15);
            }
            Points += Convert.ToInt64(adventureCompletion * scoreMultiplier);
        }

        public String Train(ScenarioRPG[] scenarios, Tuple<HeroClass, DailyBuff> dailyBuff)
        {
            String returnString = "";
            String failString = $"<{NickName}> tried very hard to train but lost all concentration on a flying butterfly...\n";
            int trainChance = Level - 50;
            if (_random.Next(100) < trainChance) return failString;
            int exp = Convert.ToInt32(Math.Pow(Level, 1.3) * 1.3);
            if (HeroClass == dailyBuff.Item1 && dailyBuff.Item2 == DailyBuff.Increased_Experience_Gain)
            {
                exp = Convert.ToInt32(exp * 1.15);
            }
            if (AuraBonus && InteractiveRPGDecision == InteractiveRPGDecision.Train)
            {
                exp *= 2;
                AuraBonus = !AuraBonus;
            }
            int totalExp = _random.Next(6 * exp, 10 * exp);
            Console.WriteLine($"totale exp : {totalExp} and threshold is {11 * exp}");
            if (totalExp > (9 * exp))
            {
                AuraBonus = true;
                returnString = (Adventure.GetScenario(scenarios).GetText(NickName) + "\n");
            }
            AddExp(totalExp);
            return returnString;
        }

        public void AddExp(int value)
        {
            Experience += value;
        }

        public long GiveExpValue()
        {
            return Experience;
        }

        public void ResetVars()
        {
            InteractiveRPGDecision = InteractiveRPGDecision.Nothing;
            PartookInEvent = false;
            GamblingOption = GamblingOptions._0;
            HasDueled = false;
        }
    }
}
