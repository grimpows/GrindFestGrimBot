using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Scripts.Models.Hero_Base;

namespace Scripts.Models
{
    public class HeroUI
    {
        private Hero_Base _hero;
        private KeyCode _toggleShowKey;
        private bool _isShow = false;
        private int _windowID;
        private Dictionary<EquipmentSlot, HeroUI_EquipedItem> _equipedItems = new Dictionary<EquipmentSlot, HeroUI_EquipedItem>();

        // requied rects
        private Rect _characterWindowRect = new Rect(100, 100, 800, 800);
        private Rect _characterStatRect = new Rect(5, 400, 200, 300);

        private Dictionary<EquipmentSlot, Rect> _slotRects = new Dictionary<EquipmentSlot, Rect>();


        private GUIStyle _leftLabelStyle = null;

        private GUIStyle LeftLabelStyle
        {
            get
            {
                if (_leftLabelStyle != null)
                {
                    return _leftLabelStyle;
                }

                _leftLabelStyle = new GUIStyle(GUI.skin.box);
                _leftLabelStyle.alignment = TextAnchor.UpperLeft;
                _leftLabelStyle.padding = new RectOffset(5, 5, 5, 5);

                return _leftLabelStyle;

            }

        }

        private GUIStyle _wordWrapBoxStyle = null;

        private GUIStyle WordWrapBoxStyle
        {
            get
            {
                if (_wordWrapBoxStyle != null)
                {
                    return _wordWrapBoxStyle;
                }

                _wordWrapBoxStyle = new GUIStyle(GUI.skin.box);
                _wordWrapBoxStyle.wordWrap = true;

                return _wordWrapBoxStyle;

            }

        }

        private Texture2D _durabilityGreenTex;
        private Texture2D _durabilityYellowTex;
        private Texture2D _durabilityOrangeTex;
        private Texture2D _durabilityRedTex;
        private Texture2D _durabilityBackgroundTex;

        void InitDurabilityTextures()
        {
            if (_durabilityGreenTex == null)
            {
                _durabilityGreenTex = new Texture2D(1, 1);
                _durabilityGreenTex.SetPixel(0, 0, Color.green);
                _durabilityGreenTex.Apply();
            }

            if (_durabilityYellowTex == null)
            {
                _durabilityYellowTex = new Texture2D(1, 1);
                _durabilityYellowTex.SetPixel(0, 0, Color.yellow);
                _durabilityYellowTex.Apply();
            }

            if (_durabilityOrangeTex == null)
            {
                _durabilityOrangeTex = new Texture2D(1, 1);
                _durabilityOrangeTex.SetPixel(0, 0, new Color(1f, 0.5f, 0f));
                _durabilityOrangeTex.Apply();
            }

            if (_durabilityRedTex == null)
            {
                _durabilityRedTex = new Texture2D(1, 1);
                _durabilityRedTex.SetPixel(0, 0, Color.red);
                _durabilityRedTex.Apply();
            }

            if (_durabilityBackgroundTex == null)
            {
                _durabilityBackgroundTex = new Texture2D(1, 1);
                _durabilityBackgroundTex.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 0.8f));
                _durabilityBackgroundTex.Apply();
            }
        }


        public void OnStart(Hero_Base hero, KeyCode toggleShowKey, int windowID)
        {
            _hero = hero;
            _toggleShowKey = toggleShowKey;
            _windowID = windowID;
            InitializeSlotRects();
            InitializeEquippedItems();
            InitDurabilityTextures();
        }

        public void OnGUI()
        {
            Event e = Event.current;

            if (e.type == EventType.KeyDown && e.keyCode == _toggleShowKey)
            {
                _isShow = !_isShow;
                e.Use();
            }

            if (_isShow)
            {
                _characterWindowRect = GUI.Window(_windowID, _characterWindowRect, DrawHeroWindow, "PLAYER INFO");
            }
        }

        public void OnUpdate()
        {
            UpdateEquippedItem();
        }

        void InitializeEquippedItems()
        {
            foreach (var slot in Enum.GetValues(typeof(EquipmentSlot)).Cast<EquipmentSlot>())
            {
                _equipedItems[slot] = new HeroUI_EquipedItem("Empty", slot, 0, null);
            }
        }

        void UpdateEquippedItem()
        {
            foreach (var equippedItem in _hero.Character.Equipment._items)
            {

                if (equippedItem.Value == null || equippedItem.Value.Item == null)
                {
                    _equipedItems[equippedItem.Key] = new HeroUI_EquipedItem("Empty", equippedItem.Key, 0, null);
                    continue;
                }

                //get item name
                string itemName = equippedItem.Value.Item != null ? equippedItem.Value.Item.Name : "Empty";

                double itemDurability = 0;

                if (equippedItem.Value?.Item?.Durability?.CurrentDurability != null)
                {

                    itemDurability = equippedItem.Value.Item.Durability.DurabilityPercentage;
                }

                //replace the inner value without doing a new allocation
                _equipedItems[equippedItem.Key].Name = itemName;
                _equipedItems[equippedItem.Key].Durability = itemDurability;
                _equipedItems[equippedItem.Key].Behaviour = equippedItem.Value;
                _equipedItems[equippedItem.Key].Slot = equippedItem.Key;
            }
        }

        void InitializeSlotRects()
        {
            Int32 slotHeight = 80;
            Int32 slotWidth = 150;
            Int32 HeadX = (int)(_characterWindowRect.width / 3) + 70;
            Int32 HeadY = 80;
            Int32 slotSpacing = 10;

            // Head (as defaut positionning) && Hair && Facial Hair
            _slotRects[EquipmentSlot.Head] = new Rect(HeadX, HeadY, slotWidth, slotHeight);

            Int32 HairX = HeadX - slotWidth - slotSpacing;
            Int32 HairY = HeadY - slotHeight / 2;
            _slotRects[EquipmentSlot.Hair] = new Rect(HairX, HairY, slotWidth, slotHeight);

            Int32 FacialHairX = HeadX + slotWidth + slotSpacing;
            Int32 FacialHairY = HeadY - slotHeight / 2;
            _slotRects[EquipmentSlot.FacialHair] = new Rect(FacialHairX, FacialHairY, slotWidth, slotHeight);

            Int32 LShoulderX = HeadX - slotWidth - slotSpacing;
            Int32 LShoulderY = HeadY + slotHeight / 2 + slotSpacing;
            _slotRects[EquipmentSlot.LeftShoulder] = new Rect(LShoulderX, LShoulderY, slotWidth, slotHeight);
            Int32 RShoulderX = HeadX + slotWidth + slotSpacing;
            Int32 RShoulderY = HeadY + slotHeight / 2 + slotSpacing;
            _slotRects[EquipmentSlot.RightShoulder] = new Rect(RShoulderX, RShoulderY, slotWidth, slotHeight);




            Int32 LArmX = LShoulderX;
            Int32 LArmY = LShoulderY + slotHeight + slotSpacing;
            _slotRects[EquipmentSlot.LeftArm] = new Rect(LArmX, LArmY, slotWidth, slotHeight);
            Int32 RArmX = RShoulderX;
            Int32 RArmY = RShoulderY + slotHeight + slotSpacing;
            _slotRects[EquipmentSlot.RightArm] = new Rect(RArmX, RArmY, slotWidth, slotHeight);


            Int32 LGlovesX = LArmX;
            Int32 LGlovesY = LArmY + slotHeight + slotSpacing;
            _slotRects[EquipmentSlot.LeftGlove] = new Rect(LGlovesX, LGlovesY, slotWidth, slotHeight);

            Int32 RGlovesX = RArmX;
            Int32 RGlovesY = RArmY + slotHeight + slotSpacing;
            _slotRects[EquipmentSlot.RightGlove] = new Rect(RGlovesX, RGlovesY, slotWidth, slotHeight);



            //Weapons && rings
            Int32 RHandX = LGlovesX - slotWidth - slotSpacing;
            Int32 RHandY = LGlovesY;
            _slotRects[EquipmentSlot.RightHand] = new Rect(RHandX, RHandY, slotWidth, slotHeight);

            Int32 LHandX = RHandX;
            Int32 LHandY = RHandY - slotHeight - slotSpacing;
            _slotRects[EquipmentSlot.LeftHand] = new Rect(LHandX, LHandY, slotWidth, slotHeight);

            Int32 RingX = LHandX;
            Int32 RingY = LHandY - slotHeight - slotSpacing;
            _slotRects[EquipmentSlot.Ring] = new Rect(RingX, RingY, slotWidth, slotHeight);



            Int32 ChestX = HeadX;
            Int32 ChestY = HeadY + 1 * (slotHeight + slotSpacing) + slotSpacing;
            _slotRects[EquipmentSlot.Chest] = new Rect(ChestX, ChestY, slotWidth, slotHeight);
            Int32 LegsX = HeadX;
            Int32 LegsY = HeadY + 2 * (slotHeight + slotSpacing) + slotSpacing;
            _slotRects[EquipmentSlot.Legs] = new Rect(LegsX, LegsY, slotWidth, slotHeight);

            // feets
            Int32 LFeetX = HeadX - slotWidth / 2 - slotSpacing / 2;
            Int32 LFeetY = HeadY + 4 * (slotHeight + slotSpacing) - slotSpacing;
            _slotRects[EquipmentSlot.LeftFeet] = new Rect(LFeetX, LFeetY, slotWidth, slotHeight);
            Int32 RFeetX = HeadX + slotWidth / 2 + slotSpacing / 2;
            Int32 RFeetY = HeadY + 4 * (slotHeight + slotSpacing) - slotSpacing;
            _slotRects[EquipmentSlot.RightFeet] = new Rect(RFeetX, RFeetY, slotWidth, slotHeight);

        }

        void DrawHeroWindow(int windowID)
        {
            foreach (var kvp in _slotRects)
            {
                EquipmentSlot slot = kvp.Key;
                Rect rect = kvp.Value;

                HeroUI_EquipedItem item = _equipedItems.ContainsKey(slot) ? _equipedItems[slot] : null;

                // Draw the slot background
                Color slotColor = item != null && item.Name != "Empty" ? new Color(0.2f, 0.8f, 0.2f, 0.8f) : new Color(0.5f, 0.5f, 0.5f, 0.8f);
                GUI.backgroundColor = slotColor;
                GUI.Box(rect, "", GUI.skin.box);
                GUI.backgroundColor = Color.white;

                // Draw item visual
                if (item != null && !string.IsNullOrEmpty(item.Name) && item.Name != "Empty" && item.Behaviour != null)
                {
                    // Draw item name text
                    GUI.Label(new Rect(rect.x + 5, rect.y + 10, rect.width - 10, 60), item.Name, GUI.skin.label);

                    // Draw durability bar over the slot
                    if (item.Durability > 0)
                    {
                        DrawDurabilityBar(rect, (float)item.Durability);
                    }
                }
                else
                {
                    // Draw empty slot indicator
                    GUI.Label(new Rect(rect.x + 5, rect.y + rect.height / 2 - 10, rect.width - 10, 20), "Empty", GUI.skin.label);
                }

                // Handle mouse over for detailed stats
                if (rect.Contains(Event.current.mousePosition))
                {
                    DrawItemTooltip(item, slot, new Rect(Event.current.mousePosition.x + 15, Event.current.mousePosition.y + 15, 350, 150));
                }
            }

            string charactereStatLabel = string.Empty;
            charactereStatLabel += $"Level: {_hero.Character.Level.Level}\n";
            charactereStatLabel += $"\nHealth: {_hero.Character.Health.CurrentHealth}/{_hero.Character.Health.MaxHealth}\n";
            charactereStatLabel += $"\nArmor: {_hero.TotalArmor()}\n";
            GUI.Box(_characterStatRect, charactereStatLabel, LeftLabelStyle);

            GUI.DragWindow();
        }

        void DrawDurabilityBar(Rect slotRect, float durabilityPercent)
        {
            // Draw durability bar at the bottom of the slot
            float barHeight = 4f;
            float barWidth = slotRect.width - 20;
            Rect barRect = new Rect(
                slotRect.x + 10,
                slotRect.y + slotRect.height - barHeight - 5,
                barWidth,
                barHeight
            );

            // Background bar
            GUI.DrawTexture(barRect, _durabilityBackgroundTex);



            // Foreground bar
            Texture2D durabilityTex = null;
            if (durabilityPercent * 100f > 75f) durabilityTex = _durabilityGreenTex;
            else if (durabilityPercent * 100f > 50f) durabilityTex = _durabilityYellowTex;
            else if (durabilityPercent * 100f > 25f) durabilityTex = _durabilityOrangeTex;
            else durabilityTex = _durabilityRedTex;

            GUI.DrawTexture(new Rect(barRect.x, barRect.y, barWidth * durabilityPercent, barHeight), durabilityTex);

            GUI.color = Color.white;
        }

      

        
        void DrawItemTooltip(HeroUI_EquipedItem item, EquipmentSlot slot, Rect tooltipRect)
        {
            if (item == null || item.Name == "Empty")
            {
                GUI.Box(tooltipRect, $"{slot}\n\nEmpty Slot", WordWrapBoxStyle);
                return;
            }

            string tooltipText = $"{item.Name}\n";
            tooltipText += $"Slot: {slot}\n";
            tooltipText += "─────────────────────\n";

            if (item.Durability > 0)
            {
                tooltipText += $"Durability: {(item.Durability * 100):F1}%\n";
            }

            if (item.Behaviour != null && item.Behaviour.Item != null)
            {
                if (item.Behaviour.Item.Armor != null)
                {
                    tooltipText += $"Defense: {item.Behaviour.Item.Armor.Armor}\n";
                }

                if (item.Behaviour.Item.Weapon != null)
                {
                    tooltipText += $"DPS: {Math.Round(item.Behaviour.Item.Weapon.DamagePerSecond, 2)}\n";
                }

                // Add stat bonuses
                string statBonuses = "";
                if (item.Behaviour.Item.BonusDexterity > 0)
                {
                    statBonuses += $"DEX +{item.Behaviour.Item.BonusDexterity} ";
                }
                if (item.Behaviour.Item.BonusStrength > 0)
                {
                    statBonuses += $"STR +{item.Behaviour.Item.BonusStrength}";
                }

                if (!string.IsNullOrEmpty(statBonuses))
                {
                    tooltipText += $"Stats: {statBonuses.Trim()}\n";
                }

                if (item.Behaviour.Item.Level != null && item.Behaviour.Item.Level.Level > 0)
                {
                    tooltipText += $"Level: {item.Behaviour.Item.Level.Level}\n";
                }
            }

            GUI.Box(tooltipRect, tooltipText.TrimEnd(), WordWrapBoxStyle);
        }

        Color GetDurabilityColor(float percentage)
        {
            if (percentage > 75f) return Color.green;
            if (percentage > 50f) return Color.yellow;
            if (percentage > 25f) return new Color(1f, 0.5f, 0f);
            return Color.red;
        }
    }
}
