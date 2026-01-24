using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        public static Predicate<ItemBehaviour> Default_LootFilter(this AutomaticHero hero)
        {
           Predicate<ItemBehaviour> lootFilter = delegate (ItemBehaviour item)
            {
                // Always pick up the item  if we don't have it yet
                //if (!hero.Inventory.Where(i => i.name == item.name).Any())
                //{
                //    return true;
                //}

                if (item.Gold != null)
                {
                    return true;
                }

                if (item.name.ToLower().Contains("arrow"))
                {
                    return true;
                }

                if (item.name.ToLower().Contains("book"))
                {
                    return true;
                }

                if (item.name.ToLower().Contains("scroll"))
                {
                    return true;
                }

                if (item.name.Contains("Key"))
                {
                    return true;
                }

                if (hero.IsHealthPotion(item) && item.LiquidContainer.GetResourceAmount(ResourceType.Health) > 0)
                {
                    return true;
                }

                if (hero.IsManaPotion(item) && item.LiquidContainer.GetResourceAmount(ResourceType.Mana) > 0)
                {
                    return true;
                }

                if (!hero.IsHealthPotion(item) && !hero.IsManaPotion(item) && item.Consumable != null)
                {
                    return true;
                }

                if (item.Weapon != null)
                {
                    return true;
                }

                if (item.Armor != null)
                {
                    return true;
                }

                return false;
            };

            return lootFilter;
        }
    }
}
