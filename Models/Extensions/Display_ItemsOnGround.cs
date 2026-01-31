using GrindFest;
using System;
using UnityEngine;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        public static void Display_ItemsOnGround(this AutomaticHero hero, int distance = 100)
        {
            var items = hero.FindItemsOnGround(maxDistance: distance);
            //var items = hero.FindItemsOnGround(maxDistance: 50);

            foreach (var item in items)
            {
                Vector3 pos = item.transform.position;
                float disC = Vector3.Distance(pos, hero.transform.position);
                if (Vector3.Distance(hero.transform.position, pos) > distance) continue;
                Vector2 screen = Camera.main.WorldToScreenPoint(pos);
                screen.y = Screen.height - screen.y;
                GUI.backgroundColor = Color.clear;
                Int32 itemWidth = item.name.Length * 8 + 5;
                float itemX = screen.x - itemWidth / 2f;
                float itemY = screen.y - 50f;
                var rect = new Rect(itemX, itemY, itemWidth, 30);
                GUI.Box(rect, item.Name);
            }

            var deadBodies = hero.Find_DeadNearbyEnemies(maxDistance: distance);

            foreach (var body in deadBodies)
            {

                //show dead body name
                //Vector3 pos = body.transform.position;
                //float disC = Vector3.Distance(pos, hero.transform.position);
                //if (Vector3.Distance(hero.transform.position, pos) > distance) continue;
                //Vector2 screen = Camera.main.WorldToScreenPoint(pos);
                //screen.y = Screen.height - screen.y;
                //GUI.backgroundColor = Color.clear;
                //Int32 itemWidth = body.name.Length * 8;
                //float itemX = screen.x - itemWidth / 2f;
                //float itemY = screen.y - 70f;
                //var rect = new Rect(itemX, itemY, itemWidth, 30);
                //GUI.Box(rect, body.name);

                var equipedItems = body.Character?.Equipment?._items?.Values;

                if (equipedItems == null) continue;


                foreach (var equipedItem in equipedItems)
                {
                    if (equipedItem == null) continue;

                    var item = equipedItem.Item;
                    if (item == null) continue;
                    Vector3 itemPos = item.transform.position;
                    float itemDisC = Vector3.Distance(itemPos, hero.transform.position);
                    if (Vector3.Distance(hero.transform.position, itemPos) > distance) continue;
                    Vector2 itemScreen = Camera.main.WorldToScreenPoint(itemPos);
                    itemScreen.y = Screen.height - itemScreen.y;
                    GUI.backgroundColor = Color.clear;
                    string itemLabel = $"(equiped on {item.GetComponentInParent(typeof(MonsterBehaviour))?.name}) {item.Name}";
                    Int32 itemWidth2 = itemLabel.Length * 8;
                    float itemX2 = itemScreen.x - itemWidth2 / 2f;
                    float itemY2 = itemScreen.y - 50f;
                    var rect2 = new Rect(itemX2, itemY2, itemWidth2, 30);
                    GUI.Box(rect2, itemLabel);

                }

                //if (body.Character.Inventory == null) continue;

                ////body.Character.

                //var inventoryItems = body.Character.Inventory.Items;

                //foreach (var inventoryItem in inventoryItems)
                //{
                //    if (inventoryItem == null) continue;
                //    var item = inventoryItem;
                //    if (item == null) continue;
                //    Vector3 itemPos = item.transform.position;
                //    float itemDisC = Vector3.Distance(itemPos, hero.transform.position);
                //    if (Vector3.Distance(hero.transform.position, itemPos) > distance) continue;
                //    Vector2 itemScreen = Camera.main.WorldToScreenPoint(itemPos);
                //    itemScreen.y = Screen.height - itemScreen.y;
                //    GUI.backgroundColor = Color.clear;
                //    Int32 itemWidth2 = item.name.Length * 8;
                //    float itemX2 = itemScreen.x - itemWidth2 / 2f;
                //    float itemY2 = itemScreen.y - 50f;
                //    var rect2 = new Rect(itemX2, itemY2, itemWidth2, 30);
                //    GUI.Box(rect2, item.Name);
                //}
            }
        }



    }
}
