using GrindFest;
using Scripts.Model;
using System;
using UnityEngine;

namespace Scripts.Models
{
    public class Bot
    {
        public Bot_Agent_Looter PickUpAgent = null;
        public Bot_Agent_Fighter FightingAgent = null;
        public Bot_Agent_Consumer ConsumerAgent = null;
        public Bot_Agent_Traveler TravelerAgent = null;
        public Bot_Agent_Unstucker UnstuckerAgent = null;

        public DateTime EquipBestInSlotTimer = DateTime.MinValue;

        /// <summary>
        /// Current status of the bot indicating what action it's performing.
        /// </summary>
        public BotStatus CurrentStatus { get; private set; } = BotStatus.INACTIVE;

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

            if (UnstuckerAgent == null)
            {
                UnstuckerAgent = new Bot_Agent_Unstucker(_hero);
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

        public void OnGUI()
        {
            _botUI?.OnGUI();
        }

        public void OnUpdate()
        {



            if (_hero == null)
                return;
            // Initialize unstucker position tracking
            UnstuckerAgent.Initialize();

            if (!_hero.IsBotting)
            {
                CurrentStatus = BotStatus.INACTIVE;
                UnstuckerAgent?.NotifyActivity();
                return;
            }

            UnstuckerAgent.UpdatePositionTracking();

            // Try keep health up first (emergency consume)
            if (ConsumerAgent.IsActing(true, 0.2f))
            {
                CurrentStatus = BotStatus.CONSUMING;
                UnstuckerAgent?.NotifyActivity();
                return;
            }

            if ((DateTime.Now - EquipBestInSlotTimer).TotalSeconds > 0.5)
            {
                EquipBestInSlotTimer = DateTime.Now;
                _hero.Equip_BestInSlot();
            }

            _hero.Action_UpgradeStats();

            // Check unstuck mode
            if (UnstuckerAgent.IsActing())
            {

                CurrentStatus = BotStatus.UNSTICKING;
                return;
            }

            if (FightingAgent.IsActing())
            {
                CurrentStatus = BotStatus.FIGHTING;
                PickUpAgent.TargetedItem = null;
                UnstuckerAgent?.NotifyActivity();
                return;
            }

            if (PickUpAgent.IsActing())
            {
                CurrentStatus = BotStatus.LOOTING;
                UnstuckerAgent?.NotifyActivity();
                return;
            }

            // Once looting and fighting close is done, try to consume if needed before other actions
            if (ConsumerAgent.IsActing(false, 0.8f))
            {
                CurrentStatus = BotStatus.CONSUMING;
                UnstuckerAgent?.NotifyActivity();
                return;
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

            //if not traveling or interacting, try fight at farther range
            if (FightingAgent.IsActing(true))
            {
                CurrentStatus = BotStatus.FIGHTING;
                PickUpAgent.TargetedItem = null;
                UnstuckerAgent?.NotifyActivity();
                return;
            }


            // Default: run around
            CurrentStatus = BotStatus.RUNAROUND;
            _hero.RunAroundInArea();
        }
    }
}
