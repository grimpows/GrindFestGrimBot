using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public class GoldShopManagerUI
    {
        private GoldShopManager _goldShopManager;
        private KeyCode _toggleShowKey;
        private bool _isShow = false;


        // requied rects
        private Rect _goldShopManagerUIRect = new Rect(100, 100, 800, 800);


        public GoldShopManagerUI(GoldShopManager goldShopManager,KeyCode toggleShowKey)
        {
            _toggleShowKey = toggleShowKey;
            _goldShopManager = goldShopManager;
        }

        public void OnGUI(AutomaticHero hero, int distance = 200)
        {
            Event e = Event.current;

            if (e.type == EventType.KeyDown && e.keyCode == _toggleShowKey)
            {
                _isShow = !_isShow;
                e.Use();
            }

            if (_isShow)
            {
                _goldShopManagerUIRect = GUI.Window(WindowsConstants.GOLD_SHOP_MANAGER_UI_ID, _goldShopManagerUIRect, DrawGoldShopManagerUI, "GoldShop");
            }
        }

        public void OnUpdate()
        {


        }

        void DrawGoldShopManagerUI(int windowID)
        {
           

        }
    }
}
