using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public class HeroUI
    {
        private Hero_Base _hero;
        private KeyCode _toggleShowKey;
        private bool _isShow = false;
        private int _windowID;
        private Dictionary<EquipmentSlot, HeroUI_EquipedItem> _equipedItems = new Dictionary<EquipmentSlot, HeroUI_EquipedItem>();

        private Rect _windowRect = new Rect(100, 100, 950, 720);
        private Vector2 _equipmentScrollPos = Vector2.zero;
        private bool _stylesInitialized = false;

        // GUI Styles
        private GUIStyle _windowStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _statLabelStyle;
        private GUIStyle _statValueStyle;
        private GUIStyle _slotStyle;
        private GUIStyle _slotEmptyStyle;
        private GUIStyle _tooltipStyle;
        private GUIStyle _sectionStyle;

        // Textures
        private Texture2D _windowBgTexture;
        private Texture2D _slotBgTexture;
        private Texture2D _slotEmptyBgTexture;
        private Texture2D _sectionBgTexture;
        private Texture2D _tooltipBgTexture;
        private Texture2D _barBgTexture;
        private Texture2D _barFillTexture;

        // Colors
        private Color _windowBgColor = new Color(0.08f, 0.08f, 0.1f, 0.98f);
        private Color _accentColor = new Color(0.85f, 0.65f, 0.25f, 1f);
        private Color _slotEquippedColor = new Color(0.18f, 0.28f, 0.4f, 0.95f);
        private Color _slotEmptyColor = new Color(0.12f, 0.12f, 0.15f, 0.9f);
        private Color _sectionColor = new Color(0.1f, 0.1f, 0.12f, 0.95f);
        private Color _healthColor = new Color(0.85f, 0.25f, 0.25f, 1f);
        private Color _manaColor = new Color(0.25f, 0.45f, 0.9f, 1f);
        private Color _armorColor = new Color(0.65f, 0.65f, 0.75f, 1f);
        private Color _strColor = new Color(0.95f, 0.45f, 0.35f, 1f);
        private Color _dexColor = new Color(0.35f, 0.95f, 0.45f, 1f);
        private Color _intColor = new Color(0.45f, 0.65f, 0.98f, 1f);
        private Color _positiveColor = new Color(0.3f, 0.9f, 0.45f, 1f);
        private Color _weaponColor = new Color(1f, 0.4f, 0.4f, 1f);

        // Layout constants
        private const float STAT_PANEL_WIDTH = 280f;
        private const float SLOT_WIDTH = 145f;
        private const float SLOT_HEIGHT = 72f;
        private const float SLOT_SPACING = 10f;

        // Hover tracking
        private HeroUI_EquipedItem _hoveredItem = null;

        public void OnStart(Hero_Base hero, KeyCode toggleShowKey, int windowID)
        {
            _hero = hero;
            _toggleShowKey = toggleShowKey;
            _windowID = windowID;
            InitializeEquippedItems();
            InitializeTextures();
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
                if (!_stylesInitialized)
                {
                    InitializeStyles();
                    _stylesInitialized = true;
                }
                
                _hoveredItem = null;
                _windowRect = GUI.Window(_windowID, _windowRect, DrawHeroWindow, "", _windowStyle);
            }
        }

        public void OnUpdate()
        {
            if (_isShow)
                UpdateEquippedItems();
        }

        private void InitializeTextures()
        {
            _windowBgTexture = CreateTexture(_windowBgColor);
            _slotBgTexture = CreateTexture(_slotEquippedColor);
            _slotEmptyBgTexture = CreateTexture(_slotEmptyColor);
            _sectionBgTexture = CreateTexture(_sectionColor);
            _tooltipBgTexture = CreateTexture(new Color(0.02f, 0.02f, 0.04f, 0.98f));
            _barBgTexture = CreateTexture(new Color(0.15f, 0.15f, 0.18f, 0.9f));
            _barFillTexture = CreateTexture(Color.white);
        }

        private void InitializeStyles()
        {
            // Window Style
            _windowStyle = new GUIStyle(GUI.skin.window);
            _windowStyle.normal.background = _windowBgTexture;
            _windowStyle.onNormal.background = _windowBgTexture;
            _windowStyle.focused.background = _windowBgTexture;
            _windowStyle.onFocused.background = _windowBgTexture;
            _windowStyle.active.background = _windowBgTexture;
            _windowStyle.onActive.background = _windowBgTexture;

            // Title Style
            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.fontSize = 22;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.normal.textColor = _accentColor;

            // Subtitle Style
            _subtitleStyle = new GUIStyle(GUI.skin.label);
            _subtitleStyle.fontSize = 13;
            _subtitleStyle.fontStyle = FontStyle.Bold;
            _subtitleStyle.alignment = TextAnchor.MiddleLeft;
            _subtitleStyle.normal.textColor = Color.white;

            // Stat Label Style
            _statLabelStyle = new GUIStyle(GUI.skin.label);
            _statLabelStyle.fontSize = 11;
            _statLabelStyle.alignment = TextAnchor.MiddleLeft;
            _statLabelStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);

            // Stat Value Style
            _statValueStyle = new GUIStyle(GUI.skin.label);
            _statValueStyle.fontSize = 11;
            _statValueStyle.fontStyle = FontStyle.Bold;
            _statValueStyle.alignment = TextAnchor.MiddleRight;
            _statValueStyle.normal.textColor = Color.white;

            // Slot Style
            _slotStyle = new GUIStyle(GUI.skin.box);
            _slotStyle.normal.background = _slotBgTexture;

            // Slot Empty Style
            _slotEmptyStyle = new GUIStyle(GUI.skin.box);
            _slotEmptyStyle.normal.background = _slotEmptyBgTexture;

            // Tooltip Style
            _tooltipStyle = new GUIStyle(GUI.skin.box);
            _tooltipStyle.normal.background = _tooltipBgTexture;
            _tooltipStyle.normal.textColor = Color.white;
            _tooltipStyle.alignment = TextAnchor.UpperLeft;
            _tooltipStyle.padding = new RectOffset(12, 12, 10, 10);
            _tooltipStyle.fontSize = 11;
            _tooltipStyle.wordWrap = true;

            // Section Style
            _sectionStyle = new GUIStyle(GUI.skin.box);
            _sectionStyle.normal.background = _sectionBgTexture;
            _sectionStyle.padding = new RectOffset(12, 12, 12, 12);
        }

        private Texture2D CreateTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private void InitializeEquippedItems()
        {
            foreach (var slot in Enum.GetValues(typeof(EquipmentSlot)).Cast<EquipmentSlot>())
            {
                _equipedItems[slot] = new HeroUI_EquipedItem("Empty", slot, 0, null);
            }
        }

        private void UpdateEquippedItems()
        {
            if (_hero?.Character?.Equipment?._items == null) return;

            foreach (var equippedItem in _hero.Character.Equipment._items)
            {
                if (equippedItem.Value == null || equippedItem.Value.Item == null)
                {
                    _equipedItems[equippedItem.Key] = new HeroUI_EquipedItem("Empty", equippedItem.Key, 0, null);
                    continue;
                }

                string itemName = equippedItem.Value.Item?.Name ?? "Empty";
                double itemDurability = equippedItem.Value?.Item?.Durability?.DurabilityPercentage ?? 0;

                _equipedItems[equippedItem.Key].Name = itemName;
                _equipedItems[equippedItem.Key].Durability = itemDurability;
                _equipedItems[equippedItem.Key].Behaviour = equippedItem.Value;
                _equipedItems[equippedItem.Key].Slot = equippedItem.Key;
            }
        }

        private void DrawHeroWindow(int windowID)
        {
            try
            {
                GUILayout.BeginVertical();

                DrawHeader();
                GUILayout.Space(12);

                GUILayout.BeginHorizontal();
                DrawStatsPanel();
                GUILayout.Space(15);
                DrawEquipmentPanel();
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                DrawHoveredTooltip();
            }
            catch (Exception ex)
            {
                Debug.LogError($"HeroUI Error: {ex.Message} {ex.StackTrace}");
            }

            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 30));
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{_hero.name}", _titleStyle, GUILayout.Height(35));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Separator line
            Rect sepRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(3));
            Color oldColor = GUI.color;
            GUI.color = _accentColor;
            GUI.DrawTexture(sepRect, _barFillTexture);
            GUI.color = oldColor;
        }

        private void DrawStatsPanel()
        {
            GUILayout.BeginVertical(_sectionStyle, GUILayout.Width(STAT_PANEL_WIDTH), GUILayout.ExpandHeight(true));

            // Character Info
            DrawSectionHeader("CHARACTER");
            
            int level = _hero?.Character?.Level?.Level ?? 0;
            string className = _hero?.Hero?.Class?.name ?? "Unknown";
            
            DrawStatRow("Level", $"{level}", _accentColor);
            DrawStatRow("Class", className, Color.white);
            GUILayout.Space(12);

            // Resources
            DrawSectionHeader("RESOURCES");
            
            int currentHealth = _hero?.Character?.Health?.CurrentHealth ?? 0;
            int maxHealth = _hero?.Character?.Health?.MaxHealth ?? 1;
            float currentMana = _hero?.Character?.Mana?.CurrentMana ?? 0;
            float maxMana = _hero?.Character?.Mana?.MaxMana ?? 1;
            
            DrawResourceBar("Health", currentHealth, maxHealth, _healthColor);
            GUILayout.Space(6);
            DrawResourceBar("Mana", (int)currentMana, (int)maxMana, _manaColor);
            GUILayout.Space(12);

            // Combat Stats
            DrawSectionHeader("COMBAT");
            
            int armor = _hero?.Character?.Combat?.Armor ?? 0;
            int equipmentArmor = 0;
            try { equipmentArmor = _hero?.EquipedItems_TotalArmor() ?? 0; } catch { }
            
            DrawStatRow("Armor", $"{armor}", _armorColor);
            DrawStatRow("Equipment Bonus", $"+{equipmentArmor}", _positiveColor);
            GUILayout.Space(12);

            // Attributes
            DrawSectionHeader("ATTRIBUTES");
            
            int str = _hero?.Character?.Strength ?? 0;
            int baseStr = _hero?.Character?.BaseStrength ?? 0;
            int bonusStr = _hero?.Character?.ItemStrengthBonus ?? 0;
            
            int dex = _hero?.Character?.Dexterity ?? 0;
            int baseDex = _hero?.Character?.BaseDexterity ?? 0;
            int bonusDex = _hero?.Character?.ItemDexterityBonus ?? 0;
            
            int intel = _hero?.Character?.Intelligence ?? 0;
            int baseInt = _hero?.Character?.BaseIntelligence ?? 0;
            int bonusInt = _hero?.Character?.ItemIntelligenceBonus ?? 0;
            
            DrawAttributeRow("Strength", str, baseStr, bonusStr, _strColor);
            DrawAttributeRow("Dexterity", dex, baseDex, bonusDex, _dexColor);
            DrawAttributeRow("Intelligence", intel, baseInt, bonusInt, _intColor);

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Label(title, _subtitleStyle, GUILayout.Height(22));
            Rect sepRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1));
            GUI.color = new Color(0.35f, 0.35f, 0.4f);
            GUI.DrawTexture(sepRect, _barFillTexture);
            GUI.color = Color.white;
            GUILayout.Space(6);
        }

        private void DrawResourceBar(string label, int current, int max, Color barColor)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _statLabelStyle, GUILayout.Width(55));
            Color oldColor = GUI.color;
            GUI.color = barColor;
            GUILayout.Label($"{current}/{max}", _statValueStyle);
            GUI.color = oldColor;
            GUILayout.EndHorizontal();

            // Progress bar
            Rect barRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(10));
            GUI.DrawTexture(barRect, _barBgTexture);

            float pct = max > 0 ? (float)current / max : 0;
            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * pct, barRect.height);
            GUI.color = barColor;
            GUI.DrawTexture(fillRect, _barFillTexture);
            GUI.color = Color.white;
        }

        private void DrawStatRow(string label, string value, Color valueColor)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _statLabelStyle, GUILayout.Width(110));
            Color oldColor = GUI.color;
            GUI.color = valueColor;
            GUILayout.Label(value, _statValueStyle);
            GUI.color = oldColor;
            GUILayout.EndHorizontal();
        }

        private void DrawAttributeRow(string label, int total, int baseVal, int bonus, Color color)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _statLabelStyle, GUILayout.Width(85));
            
            Color oldColor = GUI.color;
            GUI.color = color;
            GUILayout.Label($"{total}", _statValueStyle, GUILayout.Width(35));
            
            GUI.color = new Color(0.55f, 0.55f, 0.55f);
            GUILayout.Label($"({baseVal} +", _statLabelStyle, GUILayout.Width(45));
            
            GUI.color = _positiveColor;
            GUILayout.Label($"{bonus})", _statLabelStyle, GUILayout.Width(30));
            GUI.color = oldColor;
            
            GUILayout.EndHorizontal();
        }

        private void DrawEquipmentPanel()
        {
            GUILayout.BeginVertical(_sectionStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            DrawSectionHeader("EQUIPMENT");

            _equipmentScrollPos = GUILayout.BeginScrollView(_equipmentScrollPos, GUILayout.ExpandHeight(true));
            DrawEquipmentGrid();
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void DrawEquipmentGrid()
        {
            // Row 1: Accessories top
            DrawEquipmentRow(EquipmentSlot.Hair, "Hair", EquipmentSlot.Head, "Head", EquipmentSlot.FacialHair, "Facial");

            // Row 2: Shoulders + Chest
            DrawEquipmentRow(EquipmentSlot.LeftShoulder, "L.Shoulder", EquipmentSlot.Chest, "Chest", EquipmentSlot.RightShoulder, "R.Shoulder");

            // Row 3: Arms + Ring
            DrawEquipmentRow(EquipmentSlot.LeftArm, "L.Arm", EquipmentSlot.Ring, "Ring", EquipmentSlot.RightArm, "R.Arm");

            // Row 4: Gloves + Legs
            DrawEquipmentRow(EquipmentSlot.LeftGlove, "L.Glove", EquipmentSlot.Legs, "Legs", EquipmentSlot.RightGlove, "R.Glove");

            // Row 5: Weapons
            DrawEquipmentRowWithCenter(EquipmentSlot.LeftHand, "Main Hand", EquipmentSlot.RightHand, "Off Hand");

            // Row 6: Feet
            DrawEquipmentRowWithCenter(EquipmentSlot.LeftFeet, "L.Foot", EquipmentSlot.RightFeet, "R.Foot");
        }

        private void DrawEquipmentRow(EquipmentSlot left, string leftLabel, EquipmentSlot center, string centerLabel, EquipmentSlot right, string rightLabel)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawEquipmentSlot(left, leftLabel);
            GUILayout.Space(SLOT_SPACING);
            DrawEquipmentSlot(center, centerLabel);
            GUILayout.Space(SLOT_SPACING);
            DrawEquipmentSlot(right, rightLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(SLOT_SPACING);
        }

        private void DrawEquipmentRowWithCenter(EquipmentSlot left, string leftLabel, EquipmentSlot right, string rightLabel)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawEquipmentSlot(left, leftLabel);
            GUILayout.Space(SLOT_SPACING + SLOT_WIDTH + SLOT_SPACING);
            DrawEquipmentSlot(right, rightLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(SLOT_SPACING);
        }

        private void DrawEquipmentSlot(EquipmentSlot slot, string slotLabel)
        {
            var item = _equipedItems.ContainsKey(slot) ? _equipedItems[slot] : null;
            bool isEmpty = item == null || item.Name == "Empty";
            GUIStyle style = isEmpty ? _slotEmptyStyle : _slotStyle;

            Rect slotRect = GUILayoutUtility.GetRect(SLOT_WIDTH, SLOT_HEIGHT);
            GUI.Box(slotRect, "", style);

            // Slot label at top
            GUI.color = new Color(0.5f, 0.5f, 0.55f);
            GUI.Label(new Rect(slotRect.x + 6, slotRect.y + 3, slotRect.width - 12, 14), slotLabel, _statLabelStyle);
            GUI.color = Color.white;

            if (!isEmpty)
            {
                // Item name
                string displayName = TruncateString(item.Name, 15);
                Color nameColor = item.Behaviour?.Item?.Weapon != null ? _weaponColor : _armorColor;
                GUI.color = nameColor;
                GUI.Label(new Rect(slotRect.x + 6, slotRect.y + 20, slotRect.width - 12, 18), displayName, _statLabelStyle);
                GUI.color = Color.white;

                // Stats preview
                string statPreview = GetItemStatPreview(item);
                if (!string.IsNullOrEmpty(statPreview))
                {
                    GUI.color = _accentColor;
                    GUI.Label(new Rect(slotRect.x + 6, slotRect.y + 38, slotRect.width - 12, 16), statPreview, _statLabelStyle);
                    GUI.color = Color.white;
                }

                // Durability bar
                if (item.Durability > 0)
                {
                    DrawSlotDurabilityBar(slotRect, (float)item.Durability);
                }
            }
            else
            {
                GUI.color = new Color(0.4f, 0.4f, 0.45f);
                GUI.Label(new Rect(slotRect.x + 6, slotRect.y + 28, slotRect.width - 12, 18), "Empty", _statLabelStyle);
                GUI.color = Color.white;
            }

            // Hover detection
            if (slotRect.Contains(Event.current.mousePosition))
            {
                _hoveredItem = item;
            }
        }

        private string GetItemStatPreview(HeroUI_EquipedItem item)
        {
            if (item?.Behaviour?.Item == null) return "";

            if (item.Behaviour.Item.Weapon != null)
                return $"DPS: {item.Behaviour.Item.Weapon.DamagePerSecond:F1}";
            if (item.Behaviour.Item.Armor != null)
                return $"Armor: {item.Behaviour.Item.Armor.Armor}";

            return "";
        }

        private void DrawSlotDurabilityBar(Rect slotRect, float durabilityPct)
        {
            float barHeight = 5f;
            Rect barRect = new Rect(slotRect.x + 6, slotRect.y + slotRect.height - barHeight - 5, slotRect.width - 12, barHeight);

            GUI.DrawTexture(barRect, _barBgTexture);

            Color fillColor = durabilityPct > 0.75f ? _positiveColor :
                              durabilityPct > 0.5f ? new Color(0.95f, 0.85f, 0.25f) :
                              durabilityPct > 0.25f ? new Color(0.95f, 0.55f, 0.15f) : _healthColor;

            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * durabilityPct, barHeight);
            GUI.color = fillColor;
            GUI.DrawTexture(fillRect, _barFillTexture);
            GUI.color = Color.white;
        }

        private void DrawHoveredTooltip()
        {
            if (_hoveredItem == null) return;

            Vector2 mousePos = Event.current.mousePosition;
            float tooltipWidth = 200f;
            float tooltipX = mousePos.x + 20;
            float tooltipY = mousePos.y + 15;

            if (tooltipX + tooltipWidth > _windowRect.width - 20)
                tooltipX = mousePos.x - tooltipWidth - 15;

            string content = BuildTooltipContent(_hoveredItem);
            int lineCount = content.Split('\n').Length;
            float tooltipHeight = Mathf.Max(80, lineCount * 15 + 25);

            if (tooltipY + tooltipHeight > _windowRect.height - 20)
                tooltipY = _windowRect.height - tooltipHeight - 20;

            GUI.Box(new Rect(tooltipX, tooltipY, tooltipWidth, tooltipHeight), content, _tooltipStyle);
        }

        private string BuildTooltipContent(HeroUI_EquipedItem item)
        {
            if (item == null || item.Name == "Empty")
                return $"{item?.Slot}\n\nEmpty Slot";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(item.Name);
            sb.AppendLine($"Slot: {item.Slot}");
            sb.AppendLine("────────────────");

            if (item.Behaviour?.Item != null)
            {
                var itemData = item.Behaviour.Item;

                if (itemData.Weapon != null)
                    sb.AppendLine($"DPS: {itemData.Weapon.DamagePerSecond:F1}");

                if (itemData.Armor != null)
                    sb.AppendLine($"Armor: {itemData.Armor.Armor}");

                if (itemData.BonusStrength > 0)
                    sb.AppendLine($"+{itemData.BonusStrength} Strength");

                if (itemData.BonusDexterity > 0)
                    sb.AppendLine($"+{itemData.BonusDexterity} Dexterity");

                if (itemData.BonusIntelligence > 0)
                    sb.AppendLine($"+{itemData.BonusIntelligence} Intelligence");

                if (itemData.Level?.Level > 0)
                    sb.AppendLine($"Item Level: {itemData.Level.Level}");
            }

            if (item.Durability > 0)
            {
                float pct = (float)item.Durability * 100f;
                sb.AppendLine($"Durability: {pct:F0}%");
            }

            return sb.ToString().TrimEnd();
        }

        private string TruncateString(string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Length <= maxLength ? str : str.Substring(0, maxLength - 2) + "..";
        }
    }
}
