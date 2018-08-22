using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;


namespace DiscordBot
{
    class Adventure
    {
        private static readonly Random _random;
        public static StringBuilder sbData = new StringBuilder();
        private static readonly ReadOnlyCollection<LootTable> LootTables;

        static Adventure()
        {

            _random = new Random(Guid.NewGuid().GetHashCode());
            LootTables = new ReadOnlyCollection<LootTable>( new List<LootTable>()
            {//               Tr  Co   Uc   Ra   Ep   He   Le   An   Ar   Un 
                new LootTable( 100, 400, 600, 760, 865, 925, 965, 985, 995),
                new LootTable(  50, 380, 580, 740, 850, 914, 958, 982, 994),
                new LootTable(  25, 360, 560, 720, 835, 903, 951, 979, 993),
                new LootTable(  10, 340, 540, 700, 820, 892, 944, 976, 992),
                new LootTable(   0, 320, 320, 680, 805, 881, 937, 973, 991),
                new LootTable(   0, 300, 500, 660, 790, 870, 930, 970, 990),
                new LootTable(   0,   0,   0,   0, 200, 500, 700, 850, 950),
            });
        }
         
        private class LootTable
        {
            public readonly int TrashChance;
            public readonly int CommonChance;
            public readonly int UncommonChance;
            public readonly int RareChance;
            public readonly int EpicChance;
            public readonly int HeroicChance;
            public readonly int LegendaryChance;
            public readonly int AncientChance;
            public readonly int RelicChance;
            //public readonly int UniqueChance;

            public LootTable(int trashChance, int commonChance, int uncommonChance, int rareChance, int epicChance, int heroicChance, int legendaryChance, int ancientChance, int relicChance)
            {
                TrashChance = trashChance;
                CommonChance = commonChance;
                UncommonChance = uncommonChance;
                RareChance = rareChance;
                EpicChance = epicChance;
                HeroicChance = heroicChance;
                LegendaryChance = legendaryChance;
                AncientChance = ancientChance;
                RelicChance = relicChance;
                //UniqueChance = uniqueChance;
            }

            public RarityRPG GetRarity(int value)
            {
                if (value < TrashChance) return RarityRPG.Trash;
                if (value < CommonChance) return RarityRPG.Common;
                if (value < UncommonChance) return RarityRPG.Common;
                if (value < RareChance) return RarityRPG.Rare;
                if (value < EpicChance) return RarityRPG.Epic;
                if (value < HeroicChance) return RarityRPG.Heroic;
                if (value < LegendaryChance) return RarityRPG.Legendary;
                if (value < AncientChance) return RarityRPG.Ancient;
                if (value < RelicChance) return RarityRPG.Relic;
                //if (value < UniqueChance) return RarityRPG.Unique;
                return RarityRPG.Unique;
            }
        }

        public static string GoblinLoot(PlayerRPG player)
        {
            int luckModifier = Convert.ToInt32(75 * Math.Log(Math.Pow(player.HeroStats[6], 0.5) / 2));
            HeroClass itemClass = player.HeroClass;
            int roll = _random.Next(luckModifier, 1000);
            RarityRPG itemRarity = LootTables[6].GetRarity(roll);
            player.Items[_random.Next(4)].GetNewItem(player.Level, itemRarity, itemClass, player.HeroStatMult, GetDistribution());
            string returnString = $"<{player.NickName}> captured a <{(GoblinList)_random.Next(5)}> and obtained a * {itemRarity} * item!\n";
            return returnString;
        }

        public static StringBuilder PerformAdventure(PlayerRPG player, int turn, HeroClass adventureAffinity, int playerNumber, ScenarioRPG[] scenarios, Tuple<HeroClass, DailyBuff> dailyBuff)
        {
            StringBuilder returnStringBuilder = new StringBuilder(10000);

            int AdventureCompletion = 0;
            HeroClass HeroAffinity = adventureAffinity;
            int turnSCaling = 95;
            int levelScaling = 140;
            float CPscaling = 0.65f;
            int adventureCombatPower = 0;
            if (player.HeroClass == dailyBuff.Item1 && dailyBuff.Item2 == DailyBuff.Increased_Adventure_Completion) CPscaling = 0.4f;
            if (player.AuraBonus && player.InteractiveRPGDecision == InteractiveRPGDecision.LookForCompletion)
            {
                CPscaling = 0.3f;
                player.AuraBonus = !(player.AuraBonus);
            }
            adventureCombatPower = turn * turnSCaling + player.Level * levelScaling + Convert.ToInt32(player.EffectiveCombatPower * CPscaling);
            sbData.Append($"Day = {turn} ; Player name = {player.NickName}; Player CP = {player.EffectiveCombatPower} ; Adventure CP = {adventureCombatPower}\n");
            for (int i = 0; i < 10; i++)
            {
                if (TierTrial(player.EffectiveCombatPower, adventureCombatPower, player.InteractiveRPGDecision))
                {
                    AdventureCompletion++;
                }
            }

            if (AdventureCompletion == 10)
            {
                player.Notoriety++;
                returnStringBuilder.Append($"{player.NickName} has fully completed the adventure! Extra rewards and * notoriety * will be granted to them!\n\n");
            }
            GetLoot(player, HeroAffinity, ref returnStringBuilder, playerNumber, scenarios, AdventureCompletion, dailyBuff);
            player.GiveExp(AdventureCompletion, dailyBuff);
            player.GiveScore(AdventureCompletion, dailyBuff);

            return returnStringBuilder;
        }

        private static bool TierTrial(int a, int b, InteractiveRPGDecision decision)
        {
            float heroAdvantage = 2f;
            heroAdvantage = decision == InteractiveRPGDecision.LookForCompletion ? 3.75f : heroAdvantage;
            int totalChances = Convert.ToInt32(a * heroAdvantage);
            if (_random.Next(totalChances + b) < totalChances)
            {
                return true;
            }
            return false;
        }

        private static void GetLoot(PlayerRPG player, HeroClass adventureClass, ref StringBuilder sbLoot, int playerCount, ScenarioRPG[] scenarios, int adventureCompletion, Tuple<HeroClass, DailyBuff> dailyBuff)
        {
            int luckModifier = Convert.ToInt32(75 * Math.Log(Math.Pow(player.HeroStats[6], 0.5) / 2));
            RarityRPG bestRarity = RarityRPG.Trash;
            HeroClass classItem = adventureClass;
            ItemDistribution bestItemDistrition = ItemDistribution.Average;
            if (player.InteractiveRPGDecision == InteractiveRPGDecision.LookForLoot)
            {
                luckModifier = Convert.ToInt32(luckModifier * 1.35);
                if (player.HeroClass == dailyBuff.Item1 && dailyBuff.Item2 == DailyBuff.Increased_Item_Find)
                {
                    luckModifier = Convert.ToInt32(luckModifier * 1.15);
                }
            }
            if (player.AuraBonus && player.InteractiveRPGDecision == InteractiveRPGDecision.LookForLoot)
            {
                luckModifier *= 2;
                player.AuraBonus = !(player.AuraBonus);
            }
            int itemsToLoot = adventureCompletion / 2 + 1;
            for (int i = 0; i < itemsToLoot; i++)
            {
                classItem = adventureClass;
                if (!RngRoll(75)) classItem = (HeroClass)_random.Next(7);
                double lootMultiplier = 2 + player.Level / 7;
                int roll = _random.Next(luckModifier, 1000);
                RarityRPG itemRarity = LootTables[i].GetRarity(roll);
                ItemDistribution itemDistibution = GetDistribution();
                if ((int)itemRarity > (int)bestRarity)
                {
                    bestRarity = itemRarity;
                    bestItemDistrition = itemDistibution;
                }
                bestRarity = ((int)itemRarity > (int)bestRarity) ? itemRarity : bestRarity;
                //Console.WriteLine($"{player.NickName} item dropped rarity : {itemRarity} + luckmod is {luckModifier} + rolls is {roll}");
                player.Items[_random.Next(4)].GetNewItem(player.Level, itemRarity, player.HeroClass, player.HeroStatMult, itemDistibution);
                //player.Points += Convert.ToInt32(Math.Pow((int)itemRarity,lootMultiplier));//NERF
                player.Points += Convert.ToInt64((int)itemRarity * lootMultiplier);
            }
            ReduceTextCongestion(GetScenario(scenarios).GetText(player.NickName, bestRarity, classItem, bestItemDistrition), ref sbLoot, playerCount, bestRarity);
            if (sbLoot.Length > 140)
            sbLoot.Append("\n");
        }

        private static ItemDistribution GetDistribution()
        {
            int index = _random.Next(100);
            if (index < 20) return ItemDistribution.Average;
            if (index < 80) return ItemDistribution.Advantageous;
            if (index < 100) return ItemDistribution.Extreme;
            return ItemDistribution.Average;
        }

        public static ScenarioRPG GetScenario(ScenarioRPG[] scenarios)
        {
            while (true)
            {
                int randIndex = _random.Next(scenarios.Length);
                if (scenarios[randIndex].Timer <= 0 )
                {
                    scenarios[randIndex].Timer = 0;
                    return scenarios[randIndex];
                }
            }
        }

        private static void ReduceTextCongestion(string text, ref StringBuilder sb, int players, RarityRPG rarity)
        {
            if (players < 15 && (int)rarity > 4)
            {
                sb.Append(text + "\n");
            }
            else if (players < 30 && (int)rarity > 5)
            {
                sb.Append(text + "\n");
            }
            else if (players < 60 && (int)rarity > 6)
            {
                sb.Append(text + "\n");
            }
            else if (players >= 60 && (int)rarity > 8)
            {
                sb.Append(text + "\n");
            }
        }

        private static bool RngRoll(int a)
        {
            int chance = a * 10;
            int roll = _random.Next(0, 1000);
            return roll <= chance;
        }
    }
}
