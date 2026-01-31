using GrindFest;
using System;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        public static void Action_UseScroll(this AutomaticHero hero, ItemBehaviour scroll)
        {

            try
            {
                //item.SpellScroll.Interactive.OnPlayerDoubleClick(hero.Hero);
                //item.InteractiveBehaviour.OnPlayerDoubleClick(hero.Hero);
                scroll.SpellScroll.ExecuteSpell(hero.Hero, true);

            }
            catch (Exception ex)
            {
                hero.Log($"Error using scroll {scroll.Name}: {ex.Message}");
            }

        }
    }
}