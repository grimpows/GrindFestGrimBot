using System;
using GrindFest;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


namespace Scripts.Models
{
    public class Bot_Agent_Consumer
    {
        private AutomaticHero _hero;
        public ItemBehaviour? LastCosumedItem;
        public SkillBehaviour? LastUsedSkill;

        public Bot_Agent_Consumer(AutomaticHero hero)
        {
            _hero = hero;

        }

        public bool IsActing()
        {
            if (_hero.Character.SkillUser.IsUsingSkill && _hero.Character.SkillUser.CurrentlyUsedSkill == LastUsedSkill)
            {
                _hero.RunAwayFromNearestEnemy(20); // Keep distance while using skills
                return true;
            }

            if (ConsumeHealthPotion())
                return true;

            return false;
        }

        bool ConsumeHealthPotion()
        {
            if (_hero.Health < _hero.MaxHealth * 0.5f && _hero.HasHealthPotion()) // Below 50% health
            {
                DrinkHealthPotion();
                _hero.RunAwayFromNearestEnemy(30); // Good practice to retreat while drinking
                return true;
            }

            return false;
        }

        void DrinkHealthPotion()
        {
            if (!_hero.Character.SkillUser.IsUsingSkill)
            {
                ItemBehaviour itemBehaviour = _hero.Character.FindItem(_hero.IsHealthPotion);
                if (itemBehaviour != null)
                {
                    itemBehaviour.Consumable.Consume(_hero.Hero);
                    LastCosumedItem = itemBehaviour;
                    LastUsedSkill = _hero.Character.SkillUser.CurrentlyUsedSkill;
                }
                else
                {
                    _hero.Character.Speech.SayLine("I don't have any health potions");
                }
            }
        }


    }
}
