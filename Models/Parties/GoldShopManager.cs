using GrindFest;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Models
{
    public class GoldShopManager
    {
        private GoldShopManagerUI _goldShopManagerUI = null;
        public AutomaticParty Party = null;

        public bool IsEnabled { get; set; } = true;

        public Dictionary<string, bool> AutoBuyGoldShopItemSelection = new Dictionary<string, bool>();


        public GoldShopManager(AutomaticParty party, KeyCode toggleShowKey)
        {
            Party = party;

            if (_goldShopManagerUI == null)
            {
                _goldShopManagerUI = new GoldShopManagerUI(this, toggleShowKey);
            }


        }

        public void OnGUI()
        {
            _goldShopManagerUI?.OnGUI();
        }

        public void OnUpdate()
        {
            if (!IsEnabled)
            {
                return;
            }

            if (this.Party.Party.LevelCap == GetMaxHeroLevel() && AutoBuyGoldShopItemSelection.ContainsKey("LevelCap"))
            {

                AutoBuyGoldShopItemSelection["LevelCap"] = true;
                AutoBuyGoldShopItemSelection["ExperienceBonus"] = false;

            }
            else
            {
                AutoBuyGoldShopItemSelection["LevelCap"] = false;
                AutoBuyGoldShopItemSelection["ExperienceBonus"] = true;
            }

            BuyGoldShopItems();
        }

        public void BuyGoldShopItems()
        {

            var goldShopItems = Party.GetGoldShopItems();
            foreach (var item in goldShopItems)
            {
                string itemKey = item.Name;

                //check if present in selection dictionary 
                if (AutoBuyGoldShopItemSelection.ContainsKey(itemKey))
                {
                    // check if enabled
                    if (!AutoBuyGoldShopItemSelection[itemKey])
                    {
                        continue;
                    }


                    // check if can afford
                    if (Party.Gold >= item.GoldPrice && Party.Souls >= item.SoulPrice && Party.Gems >= item.GemPrice)
                    {
                        Debug.Log($"[GoldShopManager] Buying item: {item.Name} for {item.GoldPrice} Gold, {item.SoulPrice} Souls, {item.GemPrice} Gems");
                        //buy item
                        Party.BuyFromGoldShop(item.Name);
                        return;
                    }

                }
                else
                {
                    // add to dictionary if not present and set up default value for gold items
                    string lowerName = item.Name.ToLower();
                    AutoBuyGoldShopItemSelection[itemKey] =
                        lowerName.Contains("gold") ||
                        lowerName.Contains("experience") ||
                        //lowerName.Contains("magicfind") ||
                        lowerName.Contains("statpoints") ||
                        lowerName.Contains("heroslot")
                        ? true : false;
                }


            }
        }

        public int GetMaxHeroLevel()
        {
            int maxLevel = 0;
            foreach (var hero in this.Party.Party.Heroes)
            {
                if (hero.Level.Level > maxLevel)
                {
                    maxLevel = hero.Level.Level;
                }
            }
            return maxLevel;
        }

        public bool CanAffordItem(GoldShopItem item)
        {
            return Party.Gold >= item.GoldPrice && Party.Souls >= item.SoulPrice && Party.Gems >= item.GemPrice;
        }
    }
}
