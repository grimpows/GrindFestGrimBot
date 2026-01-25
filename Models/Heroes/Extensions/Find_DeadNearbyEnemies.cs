using GrindFest;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        public static IReadOnlyList<MonsterBehaviour> Find_DeadNearbyEnemies(this AutomaticHero hero, string name1 = "", string name2 = null, string name3 = null, float maxDistance = 15f, float howNear = 2f)
        {
            //_nearbyEnemies.Clear();
            
            List<MonsterBehaviour> _nearbyEnemies = new List<MonsterBehaviour>();
            foreach (MonsterBehaviour allMonster in MonsterBehaviour.AllMonsters)
            {

                float distFromDeadBodies = Vector3.Distance(allMonster.transform.position, hero.Character.transform.position);

                if (distFromDeadBodies > maxDistance || distFromDeadBodies < howNear)
                    continue;

                if (allMonster.Health.IsDead)
                {
                    _nearbyEnemies.Add(allMonster);
                }
            }


            return _nearbyEnemies;
        }

    }
}
