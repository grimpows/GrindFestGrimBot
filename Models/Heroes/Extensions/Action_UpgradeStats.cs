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
        public static void Action_UpgradeStats(this AutomaticHero hero)
        {
            if (hero.StatPoints == 0)
            {
                return;
            }

            int STRBasePoints = hero.Character.BaseStrength;
            int DEXBasePoints = hero.Character.BaseDexterity;
            int INTBasePoints = hero.Character.BaseIntelligence;

            bool isMelee = hero.Hero.Class.name.ToLower().Contains("hero") || hero.Hero.Class.name.ToLower().Contains("warrior");
            
            if (hero.StatPoints > 0 && isMelee)
            {
                if (DEXBasePoints < (2.0 / 3.0) * STRBasePoints)
                {
                    hero.AllocateStatPoints(Stat.Dexterity, 1);
                    hero.Say($"Upgraded Dexterity");
                    return;
                }


                hero.AllocateStatPoints(Stat.Strength, 1);
                hero.Say($"Upgraded Strength");
                return;

            }
        }

     

    }
}
