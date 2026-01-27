using GrindFest;
using Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.Models
{
    public class Bot
    {
        public Bot_Agent_Looter PickUpAgent = null;
        public Bot_Agent_Fighter FightingAgent = null;
        public Bot_Agent_Consumer ConsumerAgent = null;
        public Bot_Agent_Traveler TravelerAgent = null;

        public Vector3 LastHeroPosition = Vector3.zero;
        public DateTime LastHeroPositionTime = DateTime.MinValue;

        public DateTime BestInSlotTimer = DateTime.MinValue;

        
        public float StuckDistanceThreshold = 2f;
        public float StuckTimeThresholdSeconds = 3f;
        public float UnstickModeDurationSeconds = 30f;

        private bool _isOnUnstickMode = false;
        public bool IsOnUnstickMode
        {
            get { return _isOnUnstickMode; }
            set
            {
                if (_isOnUnstickMode != value)
                {
                    _isOnUnstickMode = value;
                    if (_isOnUnstickMode)
                    {
                        UnstickStartTime = DateTime.Now;
                        _hero.Say("Entering Unstick Mode");
                    }
                    else
                    {
                        _hero.Say("Exiting Unstick Mode");
                        LastHeroPosition = _hero.Character.transform.position;
                        LastHeroPositionTime = DateTime.Now;
                    }
                }
            }
        }
        public DateTime UnstickStartTime = DateTime.MinValue;

        private AutomaticHero _hero;
        private BotUI? _botUI = null;

        // Public access to BotUI visibility
        public bool IsUIVisible => _botUI?.IsVisible ?? false;
        public void SetUIVisible(bool visible) => _botUI?.SetVisible(visible);
        public void ToggleUIVisible() => _botUI?.ToggleVisible();

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

        

            



            if (LastHeroPosition == Vector3.zero)
            {
                LastHeroPosition = _hero.Character.transform.position;
                LastHeroPositionTime = DateTime.Now;
            }


            if (_hero == null)
                return;

            if (!_hero.IsBotting)
            {
                LastHeroPosition = _hero.Character.transform.position;
                LastHeroPositionTime = DateTime.Now;
                return;
            }


            //try keep health up first
            if (ConsumerAgent.IsActing(true, 0.5f))
            {
                this.IDontCareAboutStick();
                return;
            }

            if ((DateTime.Now - BestInSlotTimer).TotalSeconds > 0.5)
            {
                BestInSlotTimer = DateTime.Now;
                _hero.Equip_BestInSlot();
            }

            _hero.Action_UpgradeStats();

            

            if (IsOnUnstickMode)
            {
                _hero.RunAroundInArea();
                if ((DateTime.Now - UnstickStartTime).TotalSeconds > UnstickModeDurationSeconds)
                {
                    IsOnUnstickMode = false;

                }
                return;
            }


            if (FightingAgent.IsActing())
            {
                PickUpAgent.TargetedItem = null;
                this.IDontCareAboutStick();
                return;
            }

            if (PickUpAgent.IsActing())
            {
                this.IDontCareAboutStick();
                return;
            }

            // once looting and fighting is done, try to consume if needed before other actions
            if (ConsumerAgent.IsActing(false, 0.8f))
            {
                this.IDontCareAboutStick();
                return;
            }

            if (Vector3.Distance(LastHeroPosition, _hero.Character.transform.position) > StuckDistanceThreshold)
            {
                LastHeroPosition = _hero.Character.transform.position;
                LastHeroPositionTime = DateTime.Now;
            }


            if ((DateTime.Now - LastHeroPositionTime).TotalSeconds > StuckTimeThresholdSeconds)
            {
                //_hero.RunAroundInArea();
                IsOnUnstickMode = true;
            }



            if (_hero.Action_TryInteractWithObjects())
                return;

            if (_hero.Action_TryMoveToBestFarmArea(IsAllowedToChangeArea))
                return;


            _hero.RunAroundInArea();
        }


        private void IDontCareAboutStick()
        {
            LastHeroPositionTime = DateTime.Now;
        }





    }
}
