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
        public static void Equip_BestInSlot(this AutomaticHero hero)
        {
            var weaponInventoryList = hero.Character.Inventory.Items
                .Where(item => item.Weapon != null)
                .ToList();

            //var bestWeapon = weaponInventoryList.FirstOrDefault();

            //if (bestWeapon != null && hero.EquipWeaponIfBetter(bestWeapon))
            //{
            //    return;
            //}

            foreach (var weapon in weaponInventoryList)
            {
                if (hero.EquipWeaponIfBetter(weapon))
                {
                    return;
                }
            }

            var armorInventoryList = hero.Character.Inventory.Items
                .Where(item => item.Armor != null)
                .ToList();

            foreach (var armor in armorInventoryList)
            {
                if (hero.EquipArmorIfBetter(armor))
                {
                    return;
                }
            }
        }

        public static bool EquipWeaponIfBetter(this AutomaticHero hero, ItemBehaviour item)
        {
            if (hero.IsWeaponBetterThanEquiped(item))
            {
                hero.Equip(item);
                return true;
            }
            return false;
        }

        public static bool IsWeaponBetterThanEquiped(this AutomaticHero hero, ItemBehaviour itemWeapon)
        {
            if (itemWeapon.Weapon == null)
            {
                return false;
            }

            var slot = itemWeapon.Equipable.Slot;
            var equipedItem = hero.Equipment[slot]?.Item;

            //if itemWeapon is TwoHanded, force the check to the RightHand slot
            if (itemWeapon.Equipable.IsTwoHanded)
            {
                slot = EquipmentSlot.RightHand;
                equipedItem = hero.Equipment[slot]?.Item;
            }

            if (equipedItem == null)
            {
                return true;
            }

            if (equipedItem.Weapon == null)
            {
                return true;
            }

            float equipedItemDPS = equipedItem.Weapon.DamagePerSecond;
            int equipedItemStat = equipedItem.StatBonusWeight();

            float itemDPS = itemWeapon.Weapon.DamagePerSecond;
            int itemStat = itemWeapon.StatBonusWeight();

            if (itemDPS > equipedItemDPS && itemStat > equipedItemStat)
            {
                //hero.Say($"Better weapon {itemWeapon.name} found based on DPS and Stats. DPS diff = {itemDPS - equipedItemDPS}, Stat diff = {itemStat - equipedItemStat}, slot = {slot}");

                return true;
            }else if (itemDPS > equipedItemDPS)
            {
                //hero.Say($"Better weapon {itemWeapon.name} found based on DPS only. DPS diff = {itemDPS - equipedItemDPS}, slot = {slot}");
                return true;
            }


            return false;
        }

        public static bool EquipArmorIfBetter(this AutomaticHero hero, ItemBehaviour item)
        {
            if (hero.IsArmorBetterThanEquiped(item))
            {
                hero.Equip(item);
                return true;
            }
            return false;
        }

     
        public static bool IsArmorBetterThanEquiped(this AutomaticHero hero, ItemBehaviour item)
        {
            var slot = item.Equipable.Slot;
            var equipedItem = hero.Equipment[slot]?.Item;
            if (equipedItem == null)
            {
                return true;
            }

            if (equipedItem.Weapon != null)
            {
                return false;
            }

            int equipedItemStat = equipedItem.StatBonusWeight();
            int itemStat = item.StatBonusWeight();
            if (itemStat > equipedItemStat)
            {
                return true;
            }
            int equipedItemArmor = equipedItem.Armor.Armor;
            int itemArmor = item.Armor.Armor;
            if (itemStat == equipedItemStat && itemArmor > equipedItemArmor)
            {
                return true;
            }
            return false;
        }


    }
}
