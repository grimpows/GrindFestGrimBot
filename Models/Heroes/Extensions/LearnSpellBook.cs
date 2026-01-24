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
        public static void LearnSpellBook(this AutomaticHero hero, ItemBehaviour item)
        {
            hero.Log($"Learning spell from {item.Name}");

            //if (item?.Usable == null)
            //{
            //    hero.Log($"Item not usable !");
            //    return;
            //}

            if (item?.InteractiveBehaviour == null)
            {
                hero.Log($"Item has no InteractiveBehaviour !");
                return;
            }

            if (item?.InteractiveBehaviour != null && hero?.Hero != null)
            {
                try
                {
                    item.InteractiveBehaviour.OnPlayerDoubleClick(hero.Hero);
                }
                catch (Exception ex)
                {
                    hero.Log($"Error learning spell from {item.Name}: {ex.Message}");
                }
            }
        }
    }
}
