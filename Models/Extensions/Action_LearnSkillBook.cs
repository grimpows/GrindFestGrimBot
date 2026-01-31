using GrindFest;
using System;
using UnityEngine;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        public static void Action_LearnSkillBook(this AutomaticHero hero, ItemBehaviour skillBook)
        {
            if (skillBook?.InteractiveBehaviour == null)
            {
                hero.Say($"Item has no InteractiveBehaviour !");
                return;
            }

            if (hero?.Character.Hero == null)
            {
                Debug.Log($"Hero Character is null !");
                return;
            }

            try
            {
                hero.Say($"Learning spell from {skillBook.Name}...");
                skillBook.InteractiveBehaviour.OnPlayerDoubleClick(hero.Character.Hero);
            }
            catch (Exception ex)
            {
                hero.Log($"Error learning spell from {skillBook.Name}: {ex.Message}");
            }

        }
    }


}
