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
        public static bool AutoLootGoldAndArrow(this AutomaticHero hero)
        {

            var items = hero.FindItemsOnGround("gold", "arrow");

            foreach (var item in items)
            {
                if (item.InteractiveBehaviour != null && item.InteractiveBehaviour.CanInteract(hero.Hero))
                {
                    item.InteractiveBehaviour.OnPlayerHover(hero.Hero);
                    return true;
                    //Debug.Log($"can interact with {TargetedItem.name}");
                    //TargetedItem.InteractiveBehaviour.on(_hero.Hero);

                    //IgnoreItem(TargetedItem);
                    //return false;
                }
            }

            return false;

            

        }


    }
}
