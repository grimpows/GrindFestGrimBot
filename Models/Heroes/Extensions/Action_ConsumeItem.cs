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
        public static void Action_ConsumeItem(this AutomaticHero hero, ItemBehaviour item)
        {
            if (item == null)
            {
                return;
            }
            if (Hero.Character.SkillUser.IsUsingSkill)
            {
                return;
            }
            if (item.Consumable != null)
            {
                item.Consumable.Consume(hero.Hero);
            }
        }
    }
}
