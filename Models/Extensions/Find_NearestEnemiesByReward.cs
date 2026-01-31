using GrindFest;
using System.Collections.Generic;
using System.Linq;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        public static List<MonsterBehaviour> Find_NearestEnemiesByReward(this AutomaticHero hero, float maxDistance = 15f, float howNear = 2)
        {
            List<MonsterBehaviour> foundMonster = hero.FindNearestEnemies(maxDistance: maxDistance, howNear: howNear)?.ToList() ?? new List<MonsterBehaviour>();


            foundMonster = foundMonster
            .Where(enemy => enemy.Health.CurrentHealth > 0)
            .OrderByDescending(enemy => enemy?.Rank?.ExperienceReward?.ExperienceReward ?? 0)
            .ToList();



            return foundMonster;
        }

    }
}
