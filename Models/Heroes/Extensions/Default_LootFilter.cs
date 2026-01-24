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
        public static Predicate<ItemBehaviour> Default_LootFilter(this AutomaticHero hero, bool ignoreRangedWeapon = true)
        {
           Predicate<ItemBehaviour> lootFilter = delegate (ItemBehaviour item)
            {
                
                if (ignoreRangedWeapon && item?.Weapon != null && (item.Weapon.WeaponType == WeaponType.Bow || item.Weapon.WeaponType == WeaponType.Crossbow || item.Weapon.WeaponType == WeaponType.Gun || item.Weapon.WeaponType == WeaponType.Rifle))
                {
                    return false;
                }

                if (item?.Equipable != null)
                {
                    if (item.Equipable.RequiredStrength > 0 && hero.Strength  < item.Equipable.RequiredStrength)
                    {
                        return false;
                    }

                    if (item.Equipable.RequiredDexterity > 0 && hero.Dexterity  < item.Equipable.RequiredDexterity)
                    {
                        return false;
                    }

                    if (item.Equipable.RequiredIntelligence > 0 && hero.Intelligence  < item.Equipable.RequiredIntelligence)
                    {
                        return false;
                    }
                }


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
