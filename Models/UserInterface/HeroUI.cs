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
        private GUIStyle _slotHoverStyle;
        private GUIStyle _tooltipStyle;
        private GUIStyle _tooltipTitleStyle;
        private GUIStyle _tooltipStatStyle;
        private GUIStyle _sectionStyle;
        private GUIStyle _slotLabelStyle;
        private GUIStyle _slotItemNameStyle;
        private GUIStyle _slotStatStyle;

        // Textures
        private Texture2D _windowBgTexture;
        private Texture2D _slotBgTexture;
        private Texture2D _slotEmptyBgTexture;
        private Texture2D _slotHoverBgTexture;
        private Texture2D _sectionBgTexture;
        private Texture2D _tooltipBgTexture;
        private Texture2D _barBgTexture;
        private Texture2D _barFillTexture;

        // Colors
        private Color _windowBgColor = new Color(0.08f, 0.08f, 0.1f, 0.98f);
        private Color _accentColor = new Color(0.95f, 0.75f, 0.3f, 1f);
        private Color _slotEquippedColor = new Color(0.15f, 0.22f, 0.32f, 0.98f);
        private Color _slotEmptyColor = new Color(0.1f, 0.1f, 0.12f, 0.95f);
        private Color _slotHoverColor = new Color(0.22f, 0.32f, 0.45f, 0.98f);
        private Color _sectionColor = new Color(0.08f, 0.08f, 0.1f, 0.95f);
        private Color _healthColor = new Color(0.9f, 0.3f, 0.3f, 1f);
        private Color _manaColor = new Color(0.3f, 0.5f, 0.95f, 1f);
        private Color _armorColor = new Color(0.7f, 0.7f, 0.8f, 1f);
        private Color _strColor = new Color(1f, 0.5f, 0.4f, 1f);
        private Color _dexColor = new Color(0.4f, 1f, 0.5f, 1f);
        private Color _intColor = new Color(0.5f, 0.7f, 1f, 1f);
        private Color _positiveColor = new Color(0.4f, 1f, 0.55f, 1f);
        private Color _weaponColor = new Color(1f, 0.5f, 0.5f, 1f);
        private Color _textLightColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        private Color _textMutedColor = new Color(0.75f, 0.75f, 0.8f, 1f);

        // Layout constants
        private const float STAT_PANEL_WIDTH = 280f;
        private const float SLOT_WIDTH = 145f;
        private const float SLOT_HEIGHT = 72f;
        private const float SLOT_SPACING = 10f;

        // Hover tracking
        private HeroUI_EquipedItem _hoveredItem = null;
        private EquipmentSlot? _hoveredSlot = null;

        public bool IsVisible => _isShow;
        public void SetVisible(bool visible) => _isShow = visible;
        public void ToggleVisible() => _isShow = !_isShow;

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
                _hoveredSlot = null;
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
            _slotHoverBgTexture = CreateTexture(_slotHoverColor);
            _sectionBgTexture = CreateTexture(_sectionColor);
            _tooltipBgTexture = CreateTexture(new Color(0.05f, 0.05f, 0.08f, 0.98f));
            _barBgTexture = CreateTexture(new Color(0.12f, 0.12f, 0.15f, 0.95f));
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
            _subtitleStyle.normal.textColor = _textLightColor;

            // Stat Label Style
            _statLabelStyle = new GUIStyle(GUI.skin.label);
            _statLabelStyle.fontSize = 11;
            _statLabelStyle.alignment = TextAnchor.MiddleLeft;
            _statLabelStyle.normal.textColor = _textMutedColor;

            // Stat Value Style
            _statValueStyle = new GUIStyle(GUI.skin.label);
            _statValueStyle.fontSize = 11;
            _statValueStyle.fontStyle = FontStyle.Bold;
            _statValueStyle.alignment = TextAnchor.MiddleRight;
            _statValueStyle.normal.textColor = _textLightColor;

            // Slot Style
            _slotStyle = new GUIStyle(GUI.skin.box);
            _slotStyle.normal.background = _slotBgTexture;

            // Slot Empty Style
            _slotEmptyStyle = new GUIStyle(GUI.skin.box);
            _slotEmptyStyle.normal.background = _slotEmptyBgTexture;

            // Slot Hover Style
            _slotHoverStyle = new GUIStyle(GUI.skin.box);
            _slotHoverStyle.normal.background = _slotHoverBgTexture;

            // Slot Label Style (for slot names like "Head", "Chest")
            _slotLabelStyle = new GUIStyle(GUI.skin.label);
            _slotLabelStyle.fontSize = 10;
            _slotLabelStyle.fontStyle = FontStyle.Bold;
            _slotLabelStyle.alignment = TextAnchor.UpperLeft;
            _slotLabelStyle.normal.textColor = new Color(0.5f, 0.55f, 0.65f, 1f);

            // Slot Item Name Style
            _slotItemNameStyle = new GUIStyle(GUI.skin.label);
            _slotItemNameStyle.fontSize = 11;
            _slotItemNameStyle.fontStyle = FontStyle.Bold;
            _slotItemNameStyle.alignment = TextAnchor.UpperLeft;
            _slotItemNameStyle.normal.textColor = _textLightColor;

            // Slot Stat Style
            _slotStatStyle = new GUIStyle(GUI.skin.label);
            _slotStatStyle.fontSize = 10;
            _slotStatStyle.alignment = TextAnchor.UpperLeft;
            _slotStatStyle.normal.textColor = _accentColor;

            // Tooltip Style
            _tooltipStyle = new GUIStyle(GUI.skin.box);
            _tooltipStyle.normal.background = _tooltipBgTexture;
            _tooltipStyle.border = new RectOffset(1, 1, 1, 1);
            _tooltipStyle.padding = new RectOffset(0, 0, 0, 0);

            // Tooltip Title Style
            _tooltipTitleStyle = new GUIStyle(GUI.skin.label);
            _tooltipTitleStyle.fontSize = 13;
            _tooltipTitleStyle.fontStyle = FontStyle.Bold;
            _tooltipTitleStyle.alignment = TextAnchor.MiddleLeft;
            _tooltipTitleStyle.normal.textColor = _accentColor;
            _tooltipTitleStyle.padding = new RectOffset(0, 0, 0, 0);

            // Tooltip Stat Style
            _tooltipStatStyle = new GUIStyle(GUI.skin.label);
            _tooltipStatStyle.fontSize = 11;
            _tooltipStatStyle.alignment = TextAnchor.MiddleLeft;
            _tooltipStatStyle.normal.textColor = _textLightColor;
            _tooltipStatStyle.padding = new RectOffset(0, 0, 0, 0);

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

            // Close button
            Color dangerColor = new Color(1f, 0.4f, 0.4f, 1f);
            GUI.backgroundColor = dangerColor;
            GUIStyle closeStyle = new GUIStyle(GUI.skin.button);
            closeStyle.fontSize = 16;
            closeStyle.fontStyle = FontStyle.Bold;
            closeStyle.normal.textColor = Color.white;
            closeStyle.hover.textColor = Color.white;
            
            if (GUILayout.Button("X", closeStyle, GUILayout.Width(30), GUILayout.Height(30)))
            {
                _isShow = false;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();

            Rect sepRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(3));
            Color oldColor = GUI.color;
            GUI.color = _accentColor;
            GUI.DrawTexture(sepRect, _barFillTexture);
            GUI.color = oldColor;
        }

        private void DrawStatsPanel()
        {
            GUILayout.BeginVertical(_sectionStyle, GUILayout.Width(STAT_PANEL_WIDTH), GUILayout.ExpandHeight(true));

            DrawSectionHeader("CHARACTER");
            
            int level = _hero?.Character?.Level?.Level ?? 0;
            string className = _hero?.Hero?.Class?.name ?? "Unknown";
            
            DrawStatRow("Level", $"{level}", _accentColor);
            DrawStatRow("Class", className, _textLightColor);
            GUILayout.Space(12);

            DrawSectionHeader("RESOURCES");
            
            int currentHealth = _hero?.Character?.Health?.CurrentHealth ?? 0;
            int maxHealth = _hero?.Character?.Health?.MaxHealth ?? 1;
            float currentMana = _hero?.Character?.Mana?.CurrentMana ?? 0;
            float maxMana = _hero?.Character?.Mana?.MaxMana ?? 1;
            
            DrawResourceBar("Health", currentHealth, maxHealth, _healthColor);
            GUILayout.Space(6);
            DrawResourceBar("Mana", (int)currentMana, (int)maxMana, _manaColor);
            GUILayout.Space(12);

            DrawSectionHeader("COMBAT");
            
            int armor = _hero?.Character?.Combat?.Armor ?? 0;
            int equipmentArmor = 0;
            try { equipmentArmor = _hero?.EquipedItems_TotalArmor() ?? 0; } catch { }
            
            DrawStatRow("Armor", $"{armor}", _armorColor);
            DrawStatRow("Equipment Bonus", $"+{equipmentArmor}", _positiveColor);
            GUILayout.Space(12);

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
            GUI.color = new Color(0.3f, 0.35f, 0.45f);
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
            
            // Base value - more visible gray
            GUIStyle baseStyle = new GUIStyle(_statLabelStyle);
            baseStyle.normal.textColor = new Color(0.6f, 0.6f, 0.65f);
            GUILayout.Label($"({baseVal} +", baseStyle, GUILayout.Width(45));
            
            // Bonus value - green
            GUIStyle bonusStyle = new GUIStyle(_statLabelStyle);
            bonusStyle.normal.textColor = _positiveColor;
            GUILayout.Label($"{bonus})", bonusStyle, GUILayout.Width(30));
            
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
            DrawEquipmentRow(EquipmentSlot.Hair, "Hair", EquipmentSlot.Head, "Head", EquipmentSlot.FacialHair, "Facial");
            DrawEquipmentRow(EquipmentSlot.LeftShoulder, "L.Shoulder", EquipmentSlot.Chest, "Chest", EquipmentSlot.RightShoulder, "R.Shoulder");
            DrawEquipmentRow(EquipmentSlot.LeftArm, "L.Arm", EquipmentSlot.Ring, "Ring", EquipmentSlot.RightArm, "R.Arm");
            DrawEquipmentRow(EquipmentSlot.LeftGlove, "L.Glove", EquipmentSlot.Legs, "Legs", EquipmentSlot.RightGlove, "R.Glove");
            DrawEquipmentRowWithCenter(EquipmentSlot.LeftHand, "Main Hand", EquipmentSlot.RightHand, "Off Hand");
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
            
            Rect slotRect = GUILayoutUtility.GetRect(SLOT_WIDTH, SLOT_HEIGHT);
            bool isHovered = slotRect.Contains(Event.current.mousePosition);
            
            // Choose style based on state
            GUIStyle style = isHovered ? _slotHoverStyle : (isEmpty ? _slotEmptyStyle : _slotStyle);
            GUI.Box(slotRect, "", style);

            // Draw highlight border on hover
            if (isHovered)
            {
                Color oldColor = GUI.color;
                GUI.color = _accentColor;
                GUI.DrawTexture(new Rect(slotRect.x, slotRect.y, slotRect.width, 2), _barFillTexture);
                GUI.DrawTexture(new Rect(slotRect.x, slotRect.y + slotRect.height - 2, slotRect.width, 2), _barFillTexture);
                GUI.DrawTexture(new Rect(slotRect.x, slotRect.y, 2, slotRect.height), _barFillTexture);
                GUI.DrawTexture(new Rect(slotRect.x + slotRect.width - 2, slotRect.y, 2, slotRect.height), _barFillTexture);
                GUI.color = oldColor;
            }

            // Slot label at top - more visible
            GUIStyle slotNameStyle = new GUIStyle(_slotLabelStyle);
            slotNameStyle.normal.textColor = new Color(0.65f, 0.7f, 0.8f, 1f);
            slotNameStyle.fontSize = 9;
            GUI.Label(new Rect(slotRect.x + 5, slotRect.y + 2, slotRect.width - 10, 14), slotLabel, slotNameStyle);

            if (!isEmpty)
            {
                // Item name with proper color
                string displayName = TruncateString(item.Name, 16);
                Color nameColor = item.Behaviour?.Item?.Weapon != null ? _weaponColor : _armorColor;
                
                GUIStyle nameStyle = new GUIStyle(_slotItemNameStyle);
                nameStyle.normal.textColor = nameColor;
                nameStyle.fontSize = 10;
                GUI.Label(new Rect(slotRect.x + 5, slotRect.y + 16, slotRect.width - 10, 18), displayName, nameStyle);

                // Stats preview
                string statPreview = GetItemStatPreview(item);
                if (!string.IsNullOrEmpty(statPreview))
                {
                    GUI.Label(new Rect(slotRect.x + 5, slotRect.y + 34, slotRect.width - 10, 16), statPreview, _slotStatStyle);
                }

                // Durability bar
                if (item.Durability > 0)
                {
                    DrawSlotDurabilityBar(slotRect, (float)item.Durability);
                }
            }
            else
            {
                GUIStyle emptyStyle = new GUIStyle(_slotItemNameStyle);
                emptyStyle.normal.textColor = new Color(0.4f, 0.4f, 0.45f);
                emptyStyle.fontSize = 10;
                GUI.Label(new Rect(slotRect.x + 5, slotRect.y + 26, slotRect.width - 10, 18), "Empty", emptyStyle);
            }

            // Hover detection
            if (isHovered)
            {
                _hoveredItem = item;
                _hoveredSlot = slot;
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
            if (_hoveredItem == null || _hoveredSlot == null) return;

            Vector2 mousePos = Event.current.mousePosition;
            float tooltipWidth = 220f;
            float tooltipX = mousePos.x + 20;
            float tooltipY = mousePos.y + 10;

            // Calculate content and height
            var tooltipData = BuildTooltipData(_hoveredItem, _hoveredSlot.Value);
            float tooltipHeight = CalculateTooltipHeight(tooltipData);

            // Clamp position
            if (tooltipX + tooltipWidth > _windowRect.width - 15)
                tooltipX = mousePos.x - tooltipWidth - 15;
            if (tooltipY + tooltipHeight > _windowRect.height - 15)
                tooltipY = _windowRect.height - tooltipHeight - 15;

            // Draw tooltip background with border
            Rect tooltipRect = new Rect(tooltipX, tooltipY, tooltipWidth, tooltipHeight);
            
            // Border
            Color oldColor = GUI.color;
            GUI.color = _accentColor;
            GUI.DrawTexture(new Rect(tooltipRect.x - 1, tooltipRect.y - 1, tooltipRect.width + 2, tooltipRect.height + 2), _barFillTexture);
            GUI.color = oldColor;
            
            // Background
            GUI.Box(tooltipRect, "", _tooltipStyle);

            // Content
            DrawTooltipContent(tooltipRect, tooltipData);
        }

        private class TooltipData
        {
            public string Title;
            public string SlotName;
            public List<(string Label, string Value, Color Color)> Stats = new List<(string, string, Color)>();
            public float DurabilityPct;
            public bool IsEmpty;
        }

        private TooltipData BuildTooltipData(HeroUI_EquipedItem item, EquipmentSlot slot)
        {
            var data = new TooltipData();
            data.SlotName = slot.ToString();
            data.IsEmpty = item == null || item.Name == "Empty";

            if (data.IsEmpty)
            {
                data.Title = "Empty Slot";
                return data;
            }

            data.Title = item.Name;
            data.DurabilityPct = (float)item.Durability;

            if (item.Behaviour?.Item != null)
            {
                var itemData = item.Behaviour.Item;

                if (itemData.Weapon != null)
                    data.Stats.Add(("DPS", $"{itemData.Weapon.DamagePerSecond:F1}", _weaponColor));

                if (itemData.Armor != null)
                    data.Stats.Add(("Armor", $"{itemData.Armor.Armor}", _armorColor));

                if (itemData.BonusStrength > 0)
                    data.Stats.Add(("Strength", $"+{itemData.BonusStrength}", _strColor));

                if (itemData.BonusDexterity > 0)
                    data.Stats.Add(("Dexterity", $"+{itemData.BonusDexterity}", _dexColor));

                if (itemData.BonusIntelligence > 0)
                    data.Stats.Add(("Intelligence", $"+{itemData.BonusIntelligence}", _intColor));

                if (itemData.Level?.Level > 0)
                    data.Stats.Add(("Item Level", $"{itemData.Level.Level}", _textMutedColor));
            }

            return data;
        }

        private float CalculateTooltipHeight(TooltipData data)
        {
            float height = 15; // Padding top
            height += 22; // Title
            height += 18; // Slot name
            height += 10; // Separator + spacing
            
            if (data.IsEmpty)
            {
                height += 20; // "No item equipped" text
            }
            else
            {
                height += data.Stats.Count * 20; // Stats
                if (data.DurabilityPct > 0)
                    height += 25; // Durability bar
            }
            
            height += 12; // Padding bottom
            return height;
        }

        private void DrawTooltipContent(Rect rect, TooltipData data)
        {
            float y = rect.y + 12;
            float padding = 12;

            // Title
            GUI.Label(new Rect(rect.x + padding, y, rect.width - padding * 2, 22), data.Title, _tooltipTitleStyle);
            y += 22;

            // Slot name
            GUIStyle slotStyle = new GUIStyle(_tooltipStatStyle);
            slotStyle.normal.textColor = _textMutedColor;
            GUI.Label(new Rect(rect.x + padding, y, rect.width - padding * 2, 16), data.SlotName, slotStyle);
            y += 18;

            // Separator line
            GUI.color = new Color(0.3f, 0.35f, 0.45f);
            GUI.DrawTexture(new Rect(rect.x + padding, y, rect.width - padding * 2, 1), _barFillTexture);
            GUI.color = Color.white;
            y += 8;

            if (data.IsEmpty)
            {
                GUIStyle emptyStyle = new GUIStyle(_tooltipStatStyle);
                emptyStyle.normal.textColor = _textMutedColor;
                emptyStyle.fontStyle = FontStyle.Italic;
                GUI.Label(new Rect(rect.x + padding, y, rect.width - padding * 2, 20), "No item equipped", emptyStyle);
            }
            else
            {
                // Stats
                foreach (var stat in data.Stats)
                {
                    // Label
                    GUI.Label(new Rect(rect.x + padding, y, 100, 18), stat.Label, _tooltipStatStyle);
                    
                    // Value with color
                    GUIStyle valueStyle = new GUIStyle(_tooltipStatStyle);
                    valueStyle.normal.textColor = stat.Color;
                    valueStyle.fontStyle = FontStyle.Bold;
                    valueStyle.alignment = TextAnchor.MiddleRight;
                    GUI.Label(new Rect(rect.x + padding, y, rect.width - padding * 2, 18), stat.Value, valueStyle);
                    
                    y += 20;
                }

                // Durability bar
                if (data.DurabilityPct > 0)
                {
                    y += 5;
                    float barWidth = rect.width - padding * 2;
                    float barHeight = 8;
                    
                    // Label
                    GUI.Label(new Rect(rect.x + padding, y, 70, 14), "Durability", _tooltipStatStyle);
                    
                    // Percentage
                    float pct = data.DurabilityPct * 100f;
                    Color durColor = pct > 50 ? _positiveColor : pct > 25 ? new Color(0.95f, 0.85f, 0.25f) : _healthColor;
                    GUIStyle pctStyle = new GUIStyle(_tooltipStatStyle);
                    pctStyle.normal.textColor = durColor;
                    pctStyle.alignment = TextAnchor.MiddleRight;
                    GUI.Label(new Rect(rect.x + padding, y, barWidth, 14), $"{pct:F0}%", pctStyle);
                    
                    y += 16;
                    
                    // Bar background
                    Rect barRect = new Rect(rect.x + padding, y, barWidth, barHeight);
                    GUI.DrawTexture(barRect, _barBgTexture);
                    
                    // Bar fill
                    GUI.color = durColor;
                    GUI.DrawTexture(new Rect(barRect.x, barRect.y, barWidth * data.DurabilityPct, barHeight), _barFillTexture);
                    GUI.color = Color.white;
                }
            }
        }

        private string TruncateString(string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Length <= maxLength ? str : str.Substring(0, maxLength - 2) + "..";
        }
    }
}
