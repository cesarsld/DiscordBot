namespace BHungerGaemsBot
{
    public class Item
    {
        public int ItemLife { get; set; }
        public Rarity ItemRarity { get; set; }

        public Item()
        {
            ItemLife = 0;
            ItemRarity = Rarity.None;
        }

        public void ClearItem()
        {
            ItemLife = 0;
            ItemRarity = Rarity.None;
        }
        public int GetDuelChance()
        {
            int duelChance = 0;
            if (ItemLife > 0)
            {
                switch (ItemRarity)
                {
                    case Rarity.Common:
                        duelChance = 5;
                        break;
                    case Rarity.Rare:
                        duelChance = 10;
                        break;
                    case Rarity.Epic:
                        duelChance = 15;
                        break;
                    case Rarity.Legendary:
                        duelChance = 25;
                        break;
                    case Rarity.Set:
                        duelChance = 35;
                        break;
                }

                ItemLife--;
                if (ItemLife <= 0)
                {
                    ItemRarity = Rarity.None;
                }
            }
            return duelChance;
        }
    }
}
