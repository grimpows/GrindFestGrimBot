using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        public static void Action_LearnSpellBook(this AutomaticHero hero, ItemBehaviour spellBook)
        {
            hero.Log($"Learning spell from {spellBook.Name}");

            //if (item?.Usable == null)
            //{
            //    hero.Log($"Item not usable !");
            //    return;
            //}

            if (spellBook?.InteractiveBehaviour == null)
            {
                hero.Log($"Item has no InteractiveBehaviour !");
                return;
            }

            if (spellBook?.InteractiveBehaviour != null && hero?.Hero != null)
            {
                try
                {
                    spellBook.InteractiveBehaviour.OnPlayerDoubleClick(hero.Hero);
                }
                catch (Exception ex)
                {
                    hero.Log($"Error learning spell from {spellBook.Name}: {ex.Message}");
                }
            }
        }
    }
}
