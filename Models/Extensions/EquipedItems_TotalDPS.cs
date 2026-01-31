using GrindFest;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {

        public static float EquipedItems_TotalDPS(this AutomaticHero hero)
        {
            float totalDPS = 0f;
            foreach (var equipment in hero.Equipment._items.Values)
            {
                if (equipment?.Item?.Weapon != null)
                {
                    totalDPS += equipment.Item.Weapon.DamagePerSecond;
                }
            }
            return totalDPS;
        }
    }
}
