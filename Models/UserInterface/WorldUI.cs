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
        private Dictionary<EquipmentSlot, Rect> _slotRects = new Dictionary<EquipmentSlot, Rect>();


        public WorldUI(KeyCode toggleShowKey)
        {
            _toggleShowKey = toggleShowKey;
        }

        public void OnGUI(AutomaticHero hero)
        {
            hero.Display_EnemiesBar(100);
            hero.Display_ItemsOnGround(200);
            hero.Display_NameTag();
            hero.Display_InteractiveObjets(GLOBAL.INTERACTIVE_ITEM_LIST, 100);

            //GrindFest.UI.UIHealthBall
        }

        public void OnUpdate()
        {
            

        }

     

    }
}
