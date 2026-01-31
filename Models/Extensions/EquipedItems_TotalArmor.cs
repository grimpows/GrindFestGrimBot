using GrindFest;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        public static int EquipedItems_TotalArmor(this AutomaticHero hero)
        {

            int totalArmor = 0;
            foreach (var equipment in hero.Equipment._items.Values)
            {
                if (equipment?.Item?.Armor != null)
                {
                    totalArmor += equipment.Item.Armor.Armor;
                }
            }
            return totalArmor;
        }


    }
}
