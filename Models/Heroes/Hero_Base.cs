using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public class Hero_Base : AutomaticHero
    {
        

        private Bot _bot = new Bot();
        private HeroUI _heroUI = new HeroUI();
        private MinimapUI _minimapUI = new MinimapUI();
        private InventoryUI _inventoryUI = new InventoryUI();
        private SkillUI _skillUI = new SkillUI();
        private QuickAccessUI _quickAccessUI = new QuickAccessUI();


        public void Start()
        {
            _heroUI.OnStart(this, KeyCode.C, GLOBALS.WINDOWS.HERO_WINDOW_INFO.ID);
            _minimapUI.OnStart(this, KeyCode.M, GLOBALS.WINDOWS.MINIMAP_WINDOW_INFO.ID);
            _inventoryUI.OnStart(this, KeyCode.I, GLOBALS.WINDOWS.INVENTORY_WINDOW_INFO.ID);
            _skillUI.OnStart(this, KeyCode.S, GLOBALS.WINDOWS.SKILL_WINDOW_INFO.ID);

            _bot.OnStart(this, KeyCode.B, GLOBALS.WINDOWS.BOT_WINDOW_INFO.ID);


            // Initialize quick access buttons
            _quickAccessUI.OnStart(_heroUI, _inventoryUI, _skillUI, _bot);
        }


        public void OnGUI()
        {
            _heroUI.OnGUI();
            _minimapUI.OnGUI();
            _inventoryUI.OnGUI();
            _skillUI.OnGUI();

            _bot.OnGUI();


            // Draw quick access buttons
            _quickAccessUI.OnGUI();
        }

        public void Update()
        {
            _heroUI.OnUpdate();
            _minimapUI.OnUpdate();
            _inventoryUI.OnUpdate();
            _skillUI.OnUpdate();


            _bot.OnUpdate();
        }


        


    }

}
