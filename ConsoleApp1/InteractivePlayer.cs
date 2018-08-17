using Discord;

namespace BHungerGaemsBot
{
    public class InteractivePlayer: Player
    {
        public int Hp { get; set; }
        public int ScenarioLikelihood { get; set; }
        public int AlertCooldown { get; set; }
        public int DuelCooldown { get; set; }
        public int DebuffTimer { get; set; }
        public int ScenarioItemFindBonus { get; set; }
        public int GoblinChoice { get; set; }
        public InteractiveDecision InteractiveDecision { get; set; }
        public EnhancedDecision EnhancedDecision { get; set; }
        public Debuff Debuff { get; set; }
        public Item[] Items { get; set; }
        public Familiar Familiar = new Familiar();

        public InteractivePlayer(IUser userParm) : base(userParm)
        {
            Hp = 100;
            ScenarioLikelihood = 30;
            AlertCooldown = 0;
            DuelCooldown = 0;
            DebuffTimer = 0;
            ScenarioItemFindBonus = 0;
            Items = new Item[BHungerGamesV2.NumItemTypes];
            for (var index = 0; index < Items.Length; index++)
            {
                Items[index] = new Item();
            }
        }

        public Item GetItem(ItemType itemType) => Items[(int) itemType];

        public int GetDuelChance()
        {
            int duelChance = 0;
            foreach (var item in Items)
            {
                duelChance += item.GetDuelChance();
            }

            if (Debuff == Debuff.DecreasedDuelChance && DebuffTimer > 0)
            {
                duelChance -= 5;
                DebuffTimer--;
            }
            else if (Debuff == Debuff.SeverlyDecreasedDuelChance && DebuffTimer > 0)
            {
                duelChance -= 10;
                DebuffTimer--;
            }
            switch (Familiar.FamiliarRarity)
            {
                case Rarity.Common:
                    duelChance += 15;
                    break;
                case Rarity.Rare:
                    duelChance += 30;
                    break;
                case Rarity.Epic:
                    duelChance += 45;
                    break;
                case Rarity.Legendary:
                    duelChance += 60;
                    break;
            }

            return duelChance;
        }

        public void Reset()
        {
            ScenarioLikelihood = 20;
            InteractiveDecision = InteractiveDecision.DoNothing;
            EnhancedDecision = EnhancedDecision.None;
            if (AlertCooldown > 0)
            {
                AlertCooldown--;
            }
            if (DuelCooldown > 0)
            {
                DuelCooldown--;
            }
        }

    }
}
