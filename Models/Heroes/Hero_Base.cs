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



        //private List<Item> inventory = new List<Item>();
        //private List<Item> bagContents = new List<Item>();


        public void Start()
        {
            _heroUI.OnStart(this, KeyCode.P, WindowsConstants.HERO_WINDOW_ID);
            _minimapUI.OnStart(this, KeyCode.M, WindowsConstants.MINIMAP_WINDOW_ID);
            _inventoryUI.OnStart(this, KeyCode.I, WindowsConstants.INVENTORY_WINDOW_ID);



            _bot.OnStart(this, KeyCode.B, WindowsConstants.BOT_WINDOW_ID);

            
        }


        public void OnGUI()
        {
            _heroUI.OnGUI();
            _minimapUI.OnGUI();
            _inventoryUI.OnGUI();

            _bot.OnGUI();
        }

        public void Update()
        {
            _heroUI.OnUpdate();
            _minimapUI.OnUpdate();
            _inventoryUI.OnUpdate();


            _bot.OnUpdate();
        }


        


    }

}
