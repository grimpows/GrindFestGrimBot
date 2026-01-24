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
        public static void UpgradeStats(this AutomaticHero hero)
        {
            if (hero.StatPoints == 0)
            {
                return;
            }

            int STRBasePoints = 10;
            int DEXBasePoints = 10;
            int INTBasePoints = 10;


            int levelGaigned = hero.Level - 1;

            int targetSTRPoints = STRBasePoints + levelGaigned * 3;
            int targetDEXPoints = DEXBasePoints + levelGaigned * 2;

            

            if (hero.StatPoints > 0)
            {
                if (hero.Strength < targetSTRPoints)
                {
                    hero.AllocateStatPoints(Stat.Strength, 1);
                    hero.Say($"Upgraded Strength");
                    return;
                }

                if (hero.Dexterity < targetDEXPoints)
                {
                    hero.AllocateStatPoints(Stat.Dexterity, 1);
                    hero.Say($"Upgraded Dexterity");
                    return;
                }
            }
        }


    }
}
