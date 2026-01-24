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
        public static void Display_NameTag(this AutomaticHero hero)
        {
            //name and level
            Vector2 screen = Camera.main.WorldToScreenPoint(hero.transform.position);
            screen.y = Screen.height - screen.y;
            GUI.backgroundColor = Color.clear;
            float itemX = screen.x - 150 / 2f;
            float itemY = screen.y - 100f;
            var rect = new Rect(itemX, itemY, 150, 20);
            GUI.contentColor = Color.antiqueWhite;
            GUI.Box(rect, $"{hero.name} - Lv.{hero.Level}({hero.Hero.Class.name})");
            GUI.contentColor = Color.clear;
        }
    }
}
