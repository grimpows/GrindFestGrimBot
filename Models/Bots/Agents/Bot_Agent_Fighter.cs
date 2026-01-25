using System;
using GrindFest;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


namespace Scripts.Models
{
    public class Bot_Agent_Fighter
    {
        public List<MonsterBehaviour> IgnoredMonsters = new List<MonsterBehaviour>();
        public MonsterBehaviour? TargetedMonster = null;
        public int? LastTargetedMonsterHealth = null;
        public DateTime? LastTargetMonsterHealthChanged = null;
        public int TargetMonsterTimeout = 15;
        public float MaxDistance = 10f;

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

        public bool IsActing()
        {
            if (!_hero.IsBotting && KillCount > 0)
            {
                KillCount = 0;
            }

            return TryFight();

        }

        bool TryFight()
        {

            if (TargetedMonster == null)
            {
                //get ennemies exept ignored ones
                var enemy = _hero.FindNearestEnemiesByReward(MaxDistance)
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
                    return Attack(TargetedMonster);
                }
            }
            return false;

        }

        bool Attack(MonsterBehaviour target)
        {
            if (target == null)
            {
                return false;
            }

            _hero.AttackTarget = target;
            if (!IsValidAttackTarget(target))
            {
                _hero.AttackTarget = null;
                return false;
            }

            if (target == null || target.Health.IsDead || !target.gameObject.activeInHierarchy)
            {
                _hero.AttackTarget = null;
                return false;
            }

            var attackWeaponSkill = _hero.Equipment[EquipmentSlot.RightHand]?.Item?.Weapon?.DefaultAttack;

            SkillBehaviour attackSkill = _hero.Character.Combat.AttackSkill;
            float range = attackSkill.Range;

            if (attackWeaponSkill != null)
            {
                attackSkill = attackWeaponSkill;
                range = attackWeaponSkill.Range;
            }

            Vector3 vector = target.transform.position - (target.transform.position - _hero.Character.transform.position).normalized * range * 0.7f;
            float num = Vector3.Distance(_hero.Character.transform.position, target.transform.position);
            if (num > MaxDistance && !_hero.Character.CanNavigateTo(vector, 1f))
            {
                _hero.AttackTarget = null;
                return false;
            }

            if (num < range)
            {
                _hero.Character.UseSkill(attackSkill, target.gameObject, target.transform.position);
                return true;
            }

            _hero.Character.MoveTo(vector);
            return true;
        }

        bool IsValidAttackTarget(MonsterBehaviour target)
        {
            if (target == null || target.Health.IsDead || !target.gameObject.activeInHierarchy)
            {
                return false;
            }

            if (target.Character.IsInWater)
            {
                return false;
            }

            FactionMemberBehaviour factionMember = target.FactionMember;
            if ((object)factionMember != null && !factionMember.IsHostile(_hero.Character.FactionMember))
            {
                return false;
            }

            return Vector3.Distance(_hero.Character.transform.position, target.transform.position) <= 20f;
        }

    }
}
