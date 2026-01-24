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
       
        public static float TotalDamagePerSecond(this AutomaticHero hero)
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
