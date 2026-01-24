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
        public static List<MonsterBehaviour> FindNearestEnemiesByReward(this AutomaticHero hero, float maxDistance = 15f, float howNear = 2)
        {
            List<MonsterBehaviour> FoundMonster = hero.FindNearestEnemies(maxDistance: maxDistance, howNear: howNear)
                 .Where(enemy => enemy.Health.CurrentHealth > 0)
                 .OrderByDescending(enemy => enemy?.Rank?.ExperienceReward.ExperienceReward)
                 .ToList();

            

            return FoundMonster;
        }
        
    }
}
