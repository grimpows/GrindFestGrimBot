using GrindFest;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {



        public static void Display_InteractiveObjets(this AutomaticHero hero, List<string> interactiveItemNames, int distance = 100)
        {
            foreach (var interactiveItemName in interactiveItemNames)
            {
                var interactive = hero.FindNearestInteractive(interactiveItemName, maxDistance: distance);

                if (interactive != null)
                {
                    Vector3 pos = interactive.transform.position;
                    float disC = Vector3.Distance(pos, hero.transform.position);
                    if (Vector3.Distance(hero.transform.position, pos) > distance) return;
                    Vector2 screen = Camera.main.WorldToScreenPoint(pos);
                    screen.y = Screen.height - screen.y;
                    GUI.backgroundColor = Color.clear;
                    float itemX = screen.x - 150 / 2f;
                    float itemY = screen.y - 75f;
                    var rect = new Rect(itemX, itemY, 150, 20);
                    GUI.backgroundColor = Color.green;
                    GUI.contentColor = Color.green;
                    GUI.Box(rect, interactive.name);
                    GUI.backgroundColor = Color.clear;
                    GUI.contentColor = Color.clear;
                }
            }
        }
    }
}
