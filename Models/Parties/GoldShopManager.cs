using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (_goldShopManagerUI == null)
            {
                _goldShopManagerUI = new GoldShopManagerUI(this, toggleShowKey);
            }

            Party = party;
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
                        //buy item
                        Party.BuyFromGoldShop(item.Name);
                        return;
                    }

                }
                else
                {
                    // add to dictionary if not present and set up default value for gold items
                    string lowerName = item.Name.ToLower();
                    AutoBuyGoldShopItemSelection[itemKey] = lowerName.Contains("gold") || lowerName.Contains("experience") || lowerName.Contains("magicfind") ? true : false;
                }

              
            }
        }
    }
}
