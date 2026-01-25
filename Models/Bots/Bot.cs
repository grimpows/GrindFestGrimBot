using GrindFest;
using Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public class Bot
    {
        public Bot_Agent_Looter PickUpAgent = null;
        public Bot_Agent_Fighter FightingAgent = null;
        public Bot_Agent_Consumer ConsumerAgent = null;



        private AutomaticHero _hero;
        private BotUI? _botUI = null;

        public void OnStart(AutomaticHero hero, KeyCode toggleShowUIKey, int windowID)
        {
            if (_hero == null)
                _hero = hero;

            if (PickUpAgent == null)
            {
                PickUpAgent = new Bot_Agent_Looter(_hero);
            }

            if (FightingAgent == null)
            {
                FightingAgent = new Bot_Agent_Fighter(_hero);
            }

            if (ConsumerAgent == null)
            {
                ConsumerAgent = new Bot_Agent_Consumer(_hero);
            }

            if (_botUI == null)
            {
                _botUI = new BotUI(this, hero, toggleShowUIKey, windowID);
            }



            FightingAgent.OnKill += (sender, args) =>
            {
                PickUpAgent.ForceRescan = true;
            };
        }

        public bool IsAllowedToChangeArea = true;

        public void OnGUI()
        {
            _botUI?.OnGUI();
        }


        public void OnUpdate()
        {
            if (_hero == null)
                return;

            if (!_hero.IsBotting)
            {
                return;
            }

            //if (_hero.TryUsePotions())
            //{
            //    return;
            //}

            if (ConsumerAgent.IsActing())
                return;



            _hero.UpgradeStats();


            PickUpAgent.ScanForItems();

            if (FightingAgent.TargetedMonster == null && PickUpAgent.IsActing())
                return;

            if (FightingAgent.TargetedMonster == null &&  _hero.TryInteractWithObjects())
                return;


            _hero.Equip_BestInSlot();

            if (FightingAgent.TargetedMonster == null && _hero.TryMoveToBestFarmArea(IsAllowedToChangeArea))
                return;

            if (FightingAgent.IsActing(200))
                return;


            _hero.RunAroundInArea();
        }

        




    }
}
