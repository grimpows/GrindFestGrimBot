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
        public Bot_Agent_Traveler TravelerAgent = null;

        public Vector3 LastHeroPosition = Vector3.zero;
        public DateTime LastHeroPositionTime = DateTime.MinValue;

        public DateTime EquipBestInSlotTimer = DateTime.MinValue;

        public float StuckDistanceThreshold = 2f;
        public float StuckTimeThresholdSeconds = 3f;
        public float UnstickModeDurationSeconds = 30f;

        /// <summary>
        /// Current status of the bot indicating what action it's performing.
        /// </summary>
        public BotStatus CurrentStatus { get; private set; } = BotStatus.INACTIVE;

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

            if (TravelerAgent == null)
            {
                TravelerAgent = new Bot_Agent_Traveler(_hero);
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
                CurrentStatus = BotStatus.INACTIVE;
                LastHeroPosition = _hero.Character.transform.position;
                LastHeroPositionTime = DateTime.Now;
                return;
            }

            // Try keep health up first (emergency consume)
            if (ConsumerAgent.IsActing(true, 0.2f))
            {
                CurrentStatus = BotStatus.CONSUMING;
                this.IDontCareAboutStick();
                return;
            }

            if ((DateTime.Now - EquipBestInSlotTimer).TotalSeconds > 0.5)
            {
                EquipBestInSlotTimer = DateTime.Now;
                _hero.Equip_BestInSlot();
            }

            _hero.Action_UpgradeStats();

            if (IsOnUnstickMode)
            {
                CurrentStatus = BotStatus.UNSTICKING;
                _hero.RunAroundInArea();
                if ((DateTime.Now - UnstickStartTime).TotalSeconds > UnstickModeDurationSeconds)
                {
                    IsOnUnstickMode = false;
                }
                return;
            }

            if (FightingAgent.IsActing())
            {
                CurrentStatus = BotStatus.FIGHTING;
                PickUpAgent.TargetedItem = null;
                this.IDontCareAboutStick();
                return;
            }

            if (PickUpAgent.IsActing())
            {
                CurrentStatus = BotStatus.LOOTING;
                this.IDontCareAboutStick();
                return;
            }

            // Once looting and fighting is done, try to consume if needed before other actions
            if (ConsumerAgent.IsActing(false, 0.8f))
            {
                CurrentStatus = BotStatus.CONSUMING;
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
                IsOnUnstickMode = true;
            }

            if (_hero.Action_TryInteractWithObjects())
            {
                CurrentStatus = BotStatus.INTERACTING;
                return;
            }

            if (TravelerAgent.IsActing())
            {
                CurrentStatus = BotStatus.TRAVELING;
                return;
            }

            // Default: run around
            CurrentStatus = BotStatus.RUNAROUND;
            _hero.RunAroundInArea();
        }

        private void IDontCareAboutStick()
        {
            LastHeroPositionTime = DateTime.Now;
        }
    }
}
