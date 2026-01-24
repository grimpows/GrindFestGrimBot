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
        public static bool TryLootItems(this AutomaticHero hero, Predicate<ItemBehaviour> lootFilter = null)
        {
            if (lootFilter == null)
                lootFilter = hero.Default_LootFilter();

            foreach (var item in hero.FindItemsOnGround(lootFilter, maxDistance: 500)) // We loop over each item we find on the ground
            {
                hero.PickUp(item);
                
                return true;

            }

            return false;
        }


    }
}
