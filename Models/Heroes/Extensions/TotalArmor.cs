using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        public static int TotalArmor(this AutomaticHero hero)
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
