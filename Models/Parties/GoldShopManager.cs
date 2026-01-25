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
        private AutomaticParty _party = null;

        public bool IsEnabled { get; set; } = true;

        public Dictionary<string, bool> AutoBuyGoldShopItemSelection = new Dictionary<string, bool>();


        public GoldShopManager(AutomaticParty party, KeyCode toggleShowKey)
        {
            if (_goldShopManagerUI == null)
            {
                _goldShopManagerUI = new GoldShopManagerUI(this, toggleShowKey);
            }

            _party = party;
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

            var goldShopItems = _party.GetGoldShopItems();
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
                    if (_party.Gold >= item.GoldPrice && _party.Souls >= item.SoulPrice && _party.Gems >= item.GemPrice)
                    {
                        //buy item
                        _party.BuyFromGoldShop(item.Name);
                        return;
                    }

                }
                else
                {
                    AutoBuyGoldShopItemSelection[itemKey] = false;
                }

              
            }
        }
    }
}
