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
        public static bool TryUsePotions(this AutomaticHero hero)
        {
            if (hero.Health < hero.MaxHealth * 0.5f && hero.HasHealthPotion()) // Below 50% health
            {
                    hero.DrinkHealthPotion();
                    hero.RunAwayFromNearestEnemy(30); // Good practice to retreat while drinking
                    return true;
            }

            

            return false;
        }
    }
}
