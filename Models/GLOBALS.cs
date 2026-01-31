using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Models
{
    public static partial class GLOBALS
    {
        public static readonly List<string> INTERACTIVE_ITEM_LIST = new()
        {
            "Chest",
            "Bookcase",
            "Shrine",
            "Totem",
            "Ore",
            "Altar"
        };


        public static class WINDOWS
        {
            public class WINDOW_INFO
            {
                readonly public int ID;
                readonly public KeyCode ToggleKey;

                public WINDOW_INFO(int id, KeyCode toggleKey)
                {
                    ID = id;
                    ToggleKey = toggleKey;
                }
            }

            static readonly public WINDOW_INFO BOT_WINDOW_INFO = new WINDOW_INFO(1, KeyCode.B);
            static readonly public WINDOW_INFO INVENTORY_WINDOW_INFO = new WINDOW_INFO(2, KeyCode.I);
            static readonly public WINDOW_INFO MINIMAP_WINDOW_INFO = new WINDOW_INFO(3, KeyCode.M);
            static readonly public WINDOW_INFO HERO_WINDOW_INFO = new WINDOW_INFO(4, KeyCode.H);
            static readonly public WINDOW_INFO WORLD_WINDOW_INFO = new WINDOW_INFO(5, KeyCode.W);
            static readonly public WINDOW_INFO GOLD_SHOP_MANAGER_WINDOW_INFO = new WINDOW_INFO(6, KeyCode.G);
            static readonly public WINDOW_INFO SKILL_WINDOW_INFO = new WINDOW_INFO(7, KeyCode.K);
        }


    }
}
