using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Scripts.Models
{
    public class Bot_Agent_Fighter
    {
        public List<MonsterBehaviour> IgnoredMonsters = new List<MonsterBehaviour>();
        public MonsterBehaviour? TargetedMonster = null;
        public int? LastTargetedMonsterHealth = null;
        public DateTime? LastTargetMonsterHealthChanged = null;
        public int TargetMonsterTimeout = 15;
        public float MaxDistance = 7f;

        private int _killCount = 0;
        public int KillCount
        {
            get
            {
                return _killCount;
            }
            set
            {
                if (value != _killCount)
                {
                    _killCount = value;
                }

                OnKill?.Invoke(this, EventArgs.Empty);
            }
        }

        private AutomaticHero _hero;

        public EventHandler OnKill = null;

        public Bot_Agent_Fighter(AutomaticHero hero)
        {
            _hero = hero;

        }

        public bool IsActing(bool ignoreMaxDistance = false)
        {
            if (!_hero.IsBotting && KillCount > 0)
            {
                KillCount = 0;
            }

            return TryFight(ignoreMaxDistance);

        }

        bool TryFight(bool ignoreMaxDistance)
        {
            float maxDistance = ignoreMaxDistance ? 15f : MaxDistance;

            if (TargetedMonster == null)
            {
                //get ennemies exept ignored ones
                var enemy = _hero.Find_NearestEnemiesByReward(maxDistance)
                    .Where(enemy => !IgnoredMonsters.Contains(enemy))
                    .ToList()
                    .FirstOrDefault();

                if (enemy != null)
                {
                    TargetedMonster = enemy;
                }
            }

            if (TargetedMonster != null)
            {

                if (LastTargetedMonsterHealth == null || LastTargetedMonsterHealth != TargetedMonster.Health.CurrentHealth)
                {
                    LastTargetedMonsterHealth = TargetedMonster.Health.CurrentHealth;
                    LastTargetMonsterHealthChanged = DateTime.Now;
                }
                else
                {
                    //check for stuck
                    if (LastTargetMonsterHealthChanged != null && (DateTime.Now - LastTargetMonsterHealthChanged).Value.TotalSeconds > TargetMonsterTimeout)
                    {
                        IgnoredMonsters.Add(TargetedMonster);
                        TargetedMonster = null;
                        LastTargetedMonsterHealth = null;
                        return true;
                    }
                }


                if (TargetedMonster.Health.IsDead)
                {
                    TargetedMonster = null;
                    LastTargetedMonsterHealth = null;
                    LastTargetMonsterHealthChanged = null;
                    KillCount++;
                }
                else
                {
                    return _hero.Attack(TargetedMonster);
                }
            }
            return false;

        }



    }
}
