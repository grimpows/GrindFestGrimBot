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

        public Vector3 LastHeroPosition = Vector3.zero;
        public DateTime LastHeroPositionTime = DateTime.MinValue;



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


            //try keep health up first
            if (ConsumerAgent.IsActing(0.5f))
                return;



            _hero.UpgradeStats();

            if (FightingAgent.IsActing())
            {
                PickUpAgent.TargetedItem = null;
                return;
            }

            if (PickUpAgent.IsActing())
                return;

            // once looting and fighting is done, try to consume if needed before other actions
            if (ConsumerAgent.IsActing(0.9f))
                return;

            _hero.Equip_BestInSlot();



            if (LastHeroPosition == Vector3.zero)
            {
                LastHeroPosition = _hero.Character.transform.position;
                LastHeroPositionTime = DateTime.Now;
            }
            else
            {
                if(Vector3.Distance(LastHeroPosition, _hero.Character.transform.position) > 1f)
                {
                    LastHeroPosition = _hero.Character.transform.position;
                    LastHeroPositionTime = DateTime.Now;
                }
            }

            if ((DateTime.Now - LastHeroPositionTime).TotalSeconds > 1)
            {
                _hero.RunAroundInArea();
            }

            if (_hero.TryInteractWithObjects())
                return;

            if (_hero.TryMoveToBestFarmArea(IsAllowedToChangeArea))
                return;




            _hero.RunAroundInArea();
        }






    }
}
