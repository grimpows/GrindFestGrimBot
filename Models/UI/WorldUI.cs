using GrindFest;
using UnityEngine;

namespace Scripts.Models
{
    public class WorldUI
    {
        private bool _isShow = false;

        // requied rects
        private Rect _worldUIRect = new Rect(100, 100, 800, 800);


        public WorldUI()
        {

        }

        public void OnGUI(AutomaticHero hero, int distance = 200)
        {
            hero.Display_EnemiesBar(distance);
            hero.Display_ItemsOnGround(distance);
            hero.Display_NameTag();
            hero.Display_InteractiveObjets(GLOBALS.INTERACTIVE_ITEM_LIST, distance);

            if (_isShow)
            {
                _worldUIRect = GUI.Window(GLOBALS.WINDOWS.WORLD_WINDOW_INFO.ID, _worldUIRect, DrawWorldUI, "World");
            }

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
