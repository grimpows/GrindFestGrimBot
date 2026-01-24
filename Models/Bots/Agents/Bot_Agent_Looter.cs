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

            var equippedItemsOnDeadEnemies = deadEnemies
                .SelectMany(enemy => enemy.Character?.Equipment?._items.Values)
                .Where(item => item != null && item.Item != null)
                .Select(item => item.Item)
                .ToList() ?? new List<ItemBehaviour>();

        
            foreach (var item in equippedItemsOnDeadEnemies)
            {
                //_hero.Say("Found item on dead enemy: " + item.name);
                item?.Equipable?.Unequip();
            }

            var storedItemInDeadEnemies = deadEnemies
                .SelectMany(enemy => enemy?.Character?.Inventory?.Items != null ? enemy.Character.Inventory.Items : new List<ItemBehaviour>())
                .Where(item => item != null)
                .ToList() ?? new List<ItemBehaviour>();

            foreach (var item in storedItemInDeadEnemies)
            {
                _hero.Say("Found stored item on dead enemy: " + item.name);
                item?.Container?.RemoveItem(item);
            }


            LastScanTime = DateTime.Now;




            //_hero.Say("Scanning for items to loot...");
            if (LootFilter == null)
                LootFilter = _hero.Default_LootFilter();

            var itemsOnGround = _hero.FindItemsOnGround(LootFilter, maxDistance: 500).ToList();

            
            

            ScannedItems = itemsOnGround.Where(item => LootFilter(item)).ToList();
            //ScannedItems.AddRange(DeadEnemiesItems.Where(item => LootFilter(item)).ToList());




            //remove ignored items
            ScannedItems = ScannedItems.Except(IgnoredItems).ToList();

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
                TargetedItem = ScannedItems.OrderBy(i => Vector3.Distance(_hero.transform.position, i.transform.position)).First();

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

    }
}
