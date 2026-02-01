using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Scripts.Models
{
    public class Bot_Agent_Looter
    {
        public Predicate<ItemBehaviour> LootFilter = null;

        public int ScanFrequency = 1;
        public DateTime LastScanTime = DateTime.MinValue;
        public List<ItemBehaviour> ScannedItems = new List<ItemBehaviour>();
        public List<ItemBehaviour> IgnoredItems = new List<ItemBehaviour>();

        public ItemBehaviour? TargetedItem = null;
        public DateTime LastItemTargetTime = DateTime.MinValue;
        public int TargetItemTimeout = 10;

        public int MaxWeaponToKeep = 3;
        public int MaxArmorPerSlotToKeep = 3;




        public int LootedItemCount = 0;

        public bool ForceRescan = false;


        private AutomaticHero _hero;

        public Bot_Agent_Looter(AutomaticHero hero, Predicate<ItemBehaviour> lootFilter = null)
        {
            _hero = hero;
            LootFilter = lootFilter;

        }

        public bool IsActing()
        {

            RemoveUsedHealthPotFromInventory();

            RemoveUnusedWeapon();

            RemoveUnusedArmor();

            ScanForItems();

            ChooseItemToTarget();

            if (TargetedItem != null)
            {


                //_hero.Character.Inventory.DropItemInInventory(TargetedItem);
                //OnPickedUp();
                _hero.PickUp(TargetedItem, OnPickedUp);



                return true;
            }

            return false;
        }

        void RemoveUsedHealthPotFromInventory()
        {
            _hero.Character.Inventory.Items
                .Where(item => _hero.IsHealthPotion(item) && item.LiquidContainer.GetResourceAmount(ResourceType.Health) == 0)
                .ToList()
                .ForEach(item => _hero.Character.Inventory.RemoveItem(item));

        }



        public void ScanForItems()
        {
            if (TargetedItem != null && !ForceRescan)
                return;

            if ((DateTime.Now - LastScanTime).TotalSeconds < ScanFrequency && !ForceRescan)
                return;

            var deadEnemies = _hero.Find_DeadNearbyEnemies(maxDistance: 200).ToList();

            if (deadEnemies != null)
            {
                var equippedItemsOnDeadEnemies = deadEnemies
                .Where(enemy => enemy.Character?.Equipment?._items?.Values != null)
                .SelectMany(enemy => enemy.Character?.Equipment?._items?.Values)
                .Where(item => item != null && item.Item != null)
                .Select(item => item.Item)
                .ToList() ?? new List<ItemBehaviour>();


                foreach (var item in equippedItemsOnDeadEnemies)
                {
                    //_hero.Say("Found item on dead enemy: " + item.name);
                    item?.Equipable?.Unequip();
                }
            }



            var storedItemInDeadEnemies = deadEnemies
                .Where(enemy => enemy != null)
                .SelectMany(enemy => enemy.Character?.Inventory?.Items ?? new List<ItemBehaviour>())
                .Where(item => item != null)
                .ToList() ?? new List<ItemBehaviour>();

            foreach (var item in storedItemInDeadEnemies)
            {
                _hero.Say("Found stored item on dead enemy: " + item.name);
                item?.Container?.DropAllOnGround();
            }


            LastScanTime = DateTime.Now;




            //_hero.Say("Scanning for items to loot...");
            if (LootFilter == null)
                LootFilter = _hero.LootFilter_GetDefault();

            var itemsOnGround = _hero.FindItemsOnGround(LootFilter, maxDistance: 500).ToList();



            ScannedItems = itemsOnGround.Where(item => LootFilter(item)).ToList();




            //remove ignored items
            ScannedItems = ScannedItems.Except(IgnoredItems).ToList();

            //exept weapon that is not best that 5 best in inventory
            float averageDPS = CalculateAverageDPSForBestWeaponsInInventory();


            ScannedItems = ScannedItems.Where(item =>
            {
                if (item.Weapon != null)
                {
                    if (item.name.ToLower().Contains("hammer"))
                    {
                        //check if we already have more thatn 3 "hammer" name in inventory
                        int hammerCountInInventory = _hero.Character.Inventory.Items
                            .Count(i => i.Weapon != null && i.name.ToLower().Contains("hammer"));

                        if (hammerCountInInventory >= 3)
                        {
                            return false;
                        }

                        //if we dont have more that 3 hammers, we can loot it
                        return true;
                    }

                    if (item.name.ToLower().Contains("pickaxe"))
                    {
                        //check if we already have more thatn 3 "hammer" name in inventory
                        int hammerCountInInventory = _hero.Character.Inventory.Items
                            .Count(i => i.Weapon != null && i.name.ToLower().Contains("pickaxe"));

                        if (hammerCountInInventory >= 3)
                        {
                            return false;
                        }

                        //if we dont have more that 3 hammers, we can loot it
                        return true;
                    }




                    if (item.Weapon.DamagePerSecond < averageDPS)
                    {
                        return false;
                    }
                }

                if (item.Armor != null)
                {
                    double averageArmorForSlot = CalculateAverageArmorForBestArmorInInventoryPerSlot(item.Equipable.Slot);
                    if ((float)item.Armor.Armor < (float)averageArmorForSlot + 1)
                    {
                        //Debug.Log("Ignoring armor item: " + item.name + " Armor: " + item.Armor.Armor + " Average for slot: " + averageArmorForSlot);
                        return false;
                    }
                    else
                    {
                        //Debug.Log("Considering armor item: " + item.name + " Armor: " + item.Armor.Armor + " Average for slot: " + averageArmorForSlot);
                    }
                }


                return true;
            }).ToList();

            ForceRescan = false;

        }

        public void ChooseItemToTarget()
        {
            TimeSpan timeSinceLastTarget = DateTime.Now - LastItemTargetTime;
            if (TargetedItem != null && timeSinceLastTarget.TotalSeconds > TargetItemTimeout)
            {
                IgnoreItem(TargetedItem);
            }


            if (TargetedItem == null && ScannedItems.Count > 0)
            {
                var scannedItemsWithPosition = ScannedItems.Where(i => i != null && i.transform != null).ToList();


                TargetedItem = scannedItemsWithPosition.OrderBy(i => Vector3.Distance(_hero.transform.position, i.transform.position)).First();

                LastItemTargetTime = DateTime.Now;
            }


        }

        public void OnPickedUp()
        {
            ScannedItems.Remove(TargetedItem);
            LootedItemCount++;

            TargetedItem = null;
        }



        public void IgnoreItem(ItemBehaviour item)
        {
            if (item != null)
            {
                IgnoredItems.Add(item);
                ScannedItems.Remove(item);
                TargetedItem = null;
            }
        }

        float CalculateAverageDPSForBestWeaponsInInventory()
        {
            if (_hero?.Character?.Inventory?.Items == null)
                return 0f;


            var filteredWeapons = _hero.Character.Inventory.Items
                .Where(i => i.Weapon != null && !i.name.ToLower().Contains("hammer") & !i.name.ToLower().Contains("pickaxe"))
                .ToList();

            if (filteredWeapons.Count == 0)
                return 0f;

            var weaponDPS = filteredWeapons
                .OrderByDescending(i => i.Weapon.DamagePerSecond)
                .Take(MaxWeaponToKeep)
                .Average(i => i.Weapon.DamagePerSecond);

            return weaponDPS;
        }

        public void RemoveUnusedWeapon()
        {
            //dontforget to not remove blacksmith items used for crafting
            var itemsToRemove = _hero.Character.Inventory.Items
                .Where(item => item.Weapon != null && !item.name.ToLower().Contains("hammer") && !item.name.ToLower().Contains("pickaxe"))
                .OrderByDescending(item => item.Weapon.DamagePerSecond)
                .ToList();

            if (itemsToRemove.Count <= MaxWeaponToKeep)
                return;

            for (int i = MaxWeaponToKeep; i < itemsToRemove.Count; i++)
            {
                _hero.Drop(itemsToRemove[i]);
            }
        }

        double CalculateAverageArmorForBestArmorInInventoryPerSlot(EquipmentSlot slot)
        {
            var armorItemsInSlot = _hero.Character.Inventory.Items
                .Where(i => i.Armor != null && i.Equipable.Slot == slot)
                .OrderByDescending(i => i.Armor.Armor)
                .Take(MaxArmorPerSlotToKeep)
                .ToList();
            if (armorItemsInSlot.Count == 0)
                return 0;
            //var averageArmor = armorItemsInSlot
            //    .Average(i => i.Armor.Armor);

            var averageArmor = armorItemsInSlot.Min(i => i.Armor.Armor);

            return averageArmor;
        }

        public void RemoveUnusedArmor()
        {
            var armorItems = _hero.Character.Inventory.Items
                .Where(item => item.Armor != null)
                .ToList();
            var armorItemsBySlot = armorItems
                .GroupBy(item => item.Equipable.Slot)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(item => item.Armor.Armor)
                .ToList());

            foreach (var slot in armorItemsBySlot.Keys)
            {
                var itemsInSlot = armorItemsBySlot[slot];
                if (itemsInSlot.Count <= MaxArmorPerSlotToKeep)
                    continue;
                for (int i = MaxArmorPerSlotToKeep; i < itemsInSlot.Count; i++)
                {
                    _hero.Drop(itemsInSlot[i]);
                }
            }
        }



        public Predicate<ItemBehaviour> FilterCombatWeapon = (ItemBehaviour item) =>
        {
            if (item.Weapon != null)
            {
                // Example: Only loot swords and axes
                string itemName = item.name.ToLower();
                if (itemName.Contains("sword") || (itemName.Contains("axe") && !itemName.Contains("pickaxe")) || itemName.Contains("bow") || itemName.Contains("dagger"))
                {
                    return true;
                }
            }
            return false;
        };

    }
}
