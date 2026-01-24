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
        public static bool TryInteractWithObjects(this AutomaticHero hero)
        {
            var chest = hero.FindNearestInteractive("Chest", maxDistance: 500);
            if (chest != null && chest.Item?.Armor == null)
            {
                bool isPlayerHaveKeys = hero.Character.Inventory.Items.Any(item => item.name.Contains("Key"));

                bool chestIsLocked = chest.name.Contains("Locked");

                if (chestIsLocked && !isPlayerHaveKeys)
                {
                    return false;
                }


                hero.Say($"Activating {chest.name}");
                hero.InteractWith(chest);
                return true;
            }


            var shrine = hero.FindNearestInteractive("Shrine", maxDistance: 500);
            if (shrine != null)
            {
                hero.Say($"Activating {shrine.name}");
                hero.InteractWith(shrine);
                return true;
            }

            return false;
        }

        
    }
}
