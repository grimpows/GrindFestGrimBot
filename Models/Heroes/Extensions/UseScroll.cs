using GrindFest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrindFest.SpellScroll;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        public static void UseScroll(this AutomaticHero hero, ItemBehaviour item)
        {

            try
            {
                //item.SpellScroll.Interactive.OnPlayerDoubleClick(hero.Hero);
                //item.InteractiveBehaviour.OnPlayerDoubleClick(hero.Hero);
                item.SpellScroll.ExecuteSpell(hero.Hero, true);

            }
            catch (Exception ex)
            {
                hero.Log($"Error using scroll {item.Name}: {ex.Message}");
            }

        }
    }
}