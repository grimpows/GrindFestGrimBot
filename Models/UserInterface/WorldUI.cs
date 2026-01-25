using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Scripts.Models.Hero_Base;

namespace Scripts.Models
{
    public class WorldUI
    {
        private KeyCode _toggleShowKey;
        private bool _isShow = false;

        // requied rects
        private Rect _worldUIRect = new Rect(100, 100, 800, 800);


        public WorldUI(KeyCode toggleShowKey)
        {
            _toggleShowKey = toggleShowKey;
        }

        public void OnGUI(AutomaticHero hero, int distance = 200)
        {
            hero.Display_EnemiesBar(distance);
            hero.Display_ItemsOnGround(distance);
            hero.Display_NameTag();
            hero.Display_InteractiveObjets(GLOBAL.INTERACTIVE_ITEM_LIST, distance);

            if (_isShow)
            {
                _worldUIRect = GUI.Window(WindowsConstants.WORLD_UI_ID, _worldUIRect, DrawWorldUI, "World");
            }

            //GrindFest.UI.UIHealthBall
        }

        public void OnUpdate()
        {


        }

        void DrawWorldUI(int windowID)
        {
            GUILayout.Label("World UI Settings will be here.");

            //show list of flags in the world


            GUI.DragWindow(new Rect(0, 0, 10000, 20));



        }
    }
}
