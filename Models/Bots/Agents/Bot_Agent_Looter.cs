using System;
using GrindFest;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


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

            var deadEnemies = _hero.FindDeadNearbyEnemies(maxDistance: 500).ToList();

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
                LootFilter = _hero.Default_LootFilter();

            var itemsOnGround = _hero.FindItemsOnGround(LootFilter, maxDistance: 500).ToList();



            ScannedItems = itemsOnGround.Where(item => LootFilter(item)).ToList();




            //remove ignored items
            ScannedItems = ScannedItems.Except(IgnoredItems).ToList();

            //exept weapon that is not best that 5 best in inventory
            float averageDPS = CalculateAverageDPSFor5BestWeaponsInInventory();

            ScannedItems = ScannedItems.Where(item =>
            {
                if (item.Weapon != null && !item.name.ToLower().Contains("hammer"))
                {
                    if (item.Weapon.DamagePerSecond < averageDPS)
                    {
                        return false;
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

        float CalculateAverageDPSFor5BestWeaponsInInventory()
        {
            if (_hero.Character.Inventory.Items.Count(i => i.Weapon != null) == 0)
                return 0f;

            // dont forget to not consider blacksmith items used for crafting
            var weaponDPS = _hero.Character.Inventory.Items
                .Where(i => i.Weapon != null && !i.name.ToLower().Contains("hammer"))
                .OrderByDescending(i => i.Weapon.DamagePerSecond)
                .Take(5)
                .Average(i => i.Weapon.DamagePerSecond);

            return weaponDPS;
        }

        public void RemoveUnusedWeapon()
        {
            //dontforget to not remove blacksmith items used for crafting
            var itemsToRemove = _hero.Character.Inventory.Items
                .Where(item => item.Weapon != null && !item.name.ToLower().Contains("hammer"))
                .OrderByDescending(item => item.Weapon.DamagePerSecond)
                .ToList();

            if (itemsToRemove.Count <= 5)
                return;

            for (int i = 5; i < itemsToRemove.Count; i++)
            {
                _hero.Drop(itemsToRemove[i]);
            }

        }

    }
}
