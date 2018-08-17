using System;
using System.Collections.Generic;
using System.Text;

namespace BHungerGaemsBot
{
    public class Familiar
    {
        public Rarity FamiliarRarity { get; set; }
        public string FamiliarName { get; set; }

        public Familiar()
        {
            FamiliarRarity = Rarity.None;
            FamiliarName = null;
        }

       /* public void GetFamiliarName(Rarity rarity, int famIndex)
        {
            switch (rarity)
            {
                case Rarity.Common:
                    FamiliarName = ((FamiliarCommonNameList)famIndex).ToString();
                    break;
                case Rarity.Rare:
                    FamiliarName = ((FamiliarRareNameList)famIndex).ToString();
                    break;
                case Rarity.Epic:
                    FamiliarName = ((FamiliarEpicNameList)famIndex).ToString();
                    break;
                case Rarity.Legendary:
                    FamiliarName = ((FamiliarLegendaryNameList)famIndex).ToString();
                    break;
            }
        }*/
    }
}
