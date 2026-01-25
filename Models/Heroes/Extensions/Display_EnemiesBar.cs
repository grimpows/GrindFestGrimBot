using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        public static void Display_EnemiesBar(this AutomaticHero hero, int distance = 100)
        {
            var enemies = hero.FindNearestEnemies(maxDistance: distance);

            foreach (var enemy in enemies)
            {
                float hp = enemy.Health.CurrentHealth;
                if (hp <= 0) continue;

                Vector3 pos = enemy.transform.position;
                float distanceFrom = Vector3.Distance(pos, hero.transform.position);
                if (distanceFrom > distance) continue;

                float maxhp = enemy.Health.MaxHealth;
                string name = enemy.name;
                var exp = enemy.GetComponent<ExperienceRewardBehaviour>()?.ExperienceReward;
                var str = enemy.Character.Strength;
                float hpPercent = hp / maxhp;

                Vector2 screen = Camera.main.WorldToScreenPoint(pos);
                screen.y = Screen.height - screen.y;
                GUI.backgroundColor = Color.clear;

                float barX = screen.x - 100 / 2f;
                float barY = screen.y - 75f;
                GUI.backgroundColor = Color.red;
                GUI.Button(new Rect(barX, barY, 100 * hpPercent, 10), GUIContent.none);
                GUI.backgroundColor = Color.clear;

                //show distance above bar
                GUI.Button(new Rect(barX, barY - 50, 100, 50), $"Dist:{distanceFrom:F1}");


                //GUI.Button(new Rect(barX, barY, 100, 10), $"{name} HP: {hp}/{maxhp} ({hpPercent:P1}) Exp: {exp} STR: {str}");
                //if (Vector3.Distance(hero.transform.position, enemy.transform.position) < 5) continue;
            }
        }
    }
}
