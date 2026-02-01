using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public class SkillBar
    {

        public SkillBehaviour[]? Skills = new SkillBehaviour[10];
        public KeyCode[]? SkillHotkeys = new KeyCode[10]
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.Alpha0,
        };

        public KeyCode? AdditionalHoldKeyCode = null;

        public AutomaticHero? OwnerHero = null;

        public void OnStart(AutomaticHero hero)
        {
            OwnerHero = hero;
        }

        public void OnGUI()
        {

        }

        public bool[] HoldedKeys = new bool[10]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
        };

        public void OnUpdate()
        {

            if (AdditionalHoldKeyCode != null && !Input.GetKey(AdditionalHoldKeyCode.Value))
            {
                return;
            }


            //Event e = Event.current;
            for (int i = 0; i < SkillHotkeys.Length; i++)
            {

                if (SkillHotkeys[i] == null) continue;

                if (Input.GetKeyDown(SkillHotkeys[i]))
                {
                    Debug.Log($"[SkillBar] Detected key press: {SkillHotkeys[i]} for skill slot {i}");
                    HoldedKeys[i] = true;
                }

                if (Input.GetKeyUp(SkillHotkeys[i]))
                {
                    HoldedKeys[i] = false;
                }

                if (!HoldedKeys[i]) continue;

                
                if (Skills[i] != null)
                {

                    Debug.Log($"[SkillBar] Using skill at slot {i}");
                    OwnerHero?.Character.UseSkill(Skills[i]);

                }
            }
        }

        public SkillBar()
        {
            for (int i = 0; i < Skills.Length; i++)
            {
                Skills[i] = null;
            }
        }


        public SkillBehaviour? GetSkillAtSlot(int slot)
        {
            if (slot < 0 || slot >= Skills.Length)
            {
                Debug.LogError($"Slot index {slot} for skillbar out of range");
                return null;
            }
            return Skills[slot];
        }

        public void SetSkillAtSlot(int slot, SkillBehaviour? skill)
        {
            if (slot < 0 || slot >= Skills.Length)
            {
                Debug.LogError($"Slot index {slot} for skillbar out of range");
                return;
            }
            Skills[slot] = skill;
        }




    }
}
