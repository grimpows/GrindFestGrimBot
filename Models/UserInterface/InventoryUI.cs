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
    public class InventoryUI
    {
        private Hero_Base _hero;
        private KeyCode _toggleShowKey;
        private bool _isShow = false;
        private int _windowId;

        private Rect _InventoryWindowRect = new Rect(100, 100, 1250, 780);
        private Vector2 _scrollPosition = Vector2.zero;

        private const float ITEM_CARD_WIDTH = 280f;
        private const float ITEM_CARD_HEIGHT = 180f;
        private const float ITEM_CARD_PADDING = 12f;
        private const float CACHE_REFRESH_INTERVAL = 0.5f;

        private InventoryTab _currentTab = InventoryTab.Equipment;
        private EquipmentSlotFilter _currentEquipmentFilter = EquipmentSlotFilter.All;
        private ConsumableFilter _currentConsumableFilter = ConsumableFilter.All;
        private BookScrollFilter _currentBookScrollFilter = BookScrollFilter.All;

        // Cache system
        private Dictionary<InventoryTab, int> _itemCountCache = new Dictionary<InventoryTab, int>();
        private Dictionary<EquipmentSlotFilter, int> _equipmentSlotCountCache = new Dictionary<EquipmentSlotFilter, int>();
        private Dictionary<ConsumableFilter, int> _consumableCountCache = new Dictionary<ConsumableFilter, int>();
        private Dictionary<BookScrollFilter, int> _bookScrollCountCache = new Dictionary<BookScrollFilter, int>();
        private List<ItemBehaviour> _filteredItemsCache = new List<ItemBehaviour>();
        private float _lastCacheRefreshTime = 0f;
        private bool _needsCacheUpdate = true;

        // GUI Styles
        private GUIStyle _windowStyle;
        private GUIStyle _itemCardStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _statLabelStyle;
        private GUIStyle _tabActiveStyle;
        private GUIStyle _tabInactiveStyle;
        private GUIStyle _subTabActiveStyle;
        private GUIStyle _subTabInactiveStyle;

        // Textures
        private Texture2D _windowBackgroundTexture;
        private Texture2D _cardBackgroundTexture;
        private Texture2D _tabActiveTexture;
        private Texture2D _tabInactiveTexture;
        private Texture2D _subTabActiveTexture;
        private Texture2D _subTabInactiveTexture;

        // Colors
        private Color _windowBgColor = new Color(0.08f, 0.08f, 0.1f, 0.98f);
        private Color _cardColor = new Color(0.15f, 0.15f, 0.18f, 0.95f);
        private Color _accentColor = new Color(0.8f, 0.6f, 0.2f, 1f);
        private Color _equipmentColor = new Color(0.3f, 0.6f, 1f, 1f);
        private Color _consumableColor = new Color(0.2f, 0.9f, 0.4f, 1f);
        private Color _bookScrollColor = new Color(0.9f, 0.7f, 0.3f, 1f);
        private Color _miscColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        private Color _weaponColor = new Color(1f, 0.3f, 0.3f, 1f);
        private Color _armorColor = new Color(0.4f, 0.7f, 1f, 1f);
        private Color _positiveColor = new Color(0.2f, 0.85f, 0.4f, 1f);
        private Color _dangerColor = new Color(0.9f, 0.3f, 0.3f, 1f);

        private enum InventoryTab { Equipment, Consumable, BookScroll, Misc }
        private enum EquipmentSlotFilter { All, Head, Shoulders, Arms, Hands, Chest, Legs, Feet, Weapons, Accessories }
        private enum ConsumableFilter { All, EPotion, MPotion, MiscPot, JunkPot, Food, Misc }
        private enum BookScrollFilter { All, SkillBook, SpellBook, Scroll }

        public void OnStart(Hero_Base hero, KeyCode toggleShowKey, int windowID)
        {
            _hero = hero;
            _toggleShowKey = toggleShowKey;
            _windowId = windowID;
            InitializeCache();
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
                InitializeStyles();
                UpdateCacheIfNeeded();
                _InventoryWindowRect = GUI.Window(_windowId, _InventoryWindowRect, DrawInventoryWindow, "", _windowStyle);
            }
        }

        public void OnUpdate() { }

        private void InitializeStyles()
        {
            if (_windowStyle != null) return;

            // Window Style
            _windowBackgroundTexture = CreateTexture(_windowBgColor);
            _windowStyle = new GUIStyle(GUI.skin.window)
            {
                normal = { background = _windowBackgroundTexture },
                onNormal = { background = _windowBackgroundTexture },
                focused = { background = _windowBackgroundTexture },
                onFocused = { background = _windowBackgroundTexture },
                active = { background = _windowBackgroundTexture },
                onActive = { background = _windowBackgroundTexture },
                border = new RectOffset(2, 2, 2, 2)
            };

            // Card Style
            _cardBackgroundTexture = CreateTexture(_cardColor);
            _itemCardStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = _cardBackgroundTexture },
                border = new RectOffset(1, 1, 1, 1),
                padding = new RectOffset(10, 10, 8, 8)
            };

            // Title Style
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            // Subtitle Style
            _subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = _accentColor }
            };

            // Stat Label Style
            _statLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f) },
                wordWrap = false
            };

            // Tab Styles
            _tabActiveTexture = CreateTexture(new Color(0.25f, 0.5f, 0.8f, 0.95f));
            _tabInactiveTexture = CreateTexture(new Color(0.2f, 0.2f, 0.25f, 0.9f));
            
            _tabActiveStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { background = _tabActiveTexture, textColor = Color.white },
                hover = { background = _tabActiveTexture, textColor = Color.white },
                active = { background = _tabActiveTexture, textColor = Color.white },
                focused = { background = _tabActiveTexture, textColor = Color.white }
            };

            _tabInactiveStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter,
                normal = { background = _tabInactiveTexture, textColor = new Color(0.7f, 0.7f, 0.7f) },
                hover = { background = CreateTexture(new Color(0.3f, 0.3f, 0.35f, 0.95f)), textColor = Color.white },
                active = { background = _tabInactiveTexture, textColor = Color.white },
                focused = { background = _tabInactiveTexture, textColor = new Color(0.7f, 0.7f, 0.7f) }
            };

            // SubTab Styles
            _subTabActiveTexture = CreateTexture(new Color(0.2f, 0.6f, 0.3f, 0.9f));
            _subTabInactiveTexture = CreateTexture(new Color(0.18f, 0.18f, 0.22f, 0.85f));

            _subTabActiveStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { background = _subTabActiveTexture, textColor = Color.white },
                hover = { background = _subTabActiveTexture, textColor = Color.white },
                active = { background = _subTabActiveTexture, textColor = Color.white },
                focused = { background = _subTabActiveTexture, textColor = Color.white }
            };

            _subTabInactiveStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { background = _subTabInactiveTexture, textColor = new Color(0.6f, 0.6f, 0.6f) },
                hover = { background = CreateTexture(new Color(0.25f, 0.25f, 0.3f, 0.9f)), textColor = Color.white },
                active = { background = _subTabInactiveTexture, textColor = Color.white },
                focused = { background = _subTabInactiveTexture, textColor = new Color(0.6f, 0.6f, 0.6f) }
            };
        }

        private Texture2D CreateTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private void DrawInventoryWindow(int windowID)
        {
            if (_hero?.Character?.Inventory?.Items == null)
            {
                GUI.DragWindow();
                return;
            }

            GUILayout.BeginVertical();

            // Header
            DrawHeader();
            GUILayout.Space(8);

            // Main Tabs
            DrawMainTabs();
            GUILayout.Space(6);

            // Sub Tabs
            DrawSubTabs();
            GUILayout.Space(10);

            // Items Grid
            DrawItemsGrid();

            // Footer with actions
            DrawFooter();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, _InventoryWindowRect.width, 30));
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("INVENTORY", _subtitleStyle, GUILayout.Height(30));
            GUILayout.FlexibleSpace();
            
            // Item count badge
            GUI.backgroundColor = _accentColor;
            GUILayout.Box($"{_filteredItemsCache.Count} items", GUILayout.Width(80), GUILayout.Height(25));
            GUI.backgroundColor = Color.white;
            
            GUILayout.EndHorizontal();

            // Separator
            Rect sepRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(2));
            GUI.color = _accentColor;
            GUI.Box(sepRect, "");
            GUI.color = Color.white;
        }

        private void DrawMainTabs()
        {
            GUILayout.BeginHorizontal();

            DrawMainTab("⚔ Equipment", InventoryTab.Equipment, _itemCountCache[InventoryTab.Equipment], _equipmentColor);
            GUILayout.Space(4);
            DrawMainTab("🧪 Consumable", InventoryTab.Consumable, _itemCountCache[InventoryTab.Consumable], _consumableColor);
            GUILayout.Space(4);
            DrawMainTab("📖 Books/Scrolls", InventoryTab.BookScroll, _itemCountCache[InventoryTab.BookScroll], _bookScrollColor);
            GUILayout.Space(4);
            DrawMainTab("📦 Misc", InventoryTab.Misc, _itemCountCache[InventoryTab.Misc], _miscColor);

            GUILayout.EndHorizontal();
        }

        private void DrawMainTab(string label, InventoryTab tab, int count, Color tabColor)
        {
            bool isActive = _currentTab == tab;
            GUIStyle style = isActive ? _tabActiveStyle : _tabInactiveStyle;
            
            if (isActive)
            {
                GUI.backgroundColor = tabColor;
            }

            if (GUILayout.Button($"{label} ({count})", style, GUILayout.Height(32), GUILayout.ExpandWidth(true)))
            {
                _currentTab = tab;
                ResetSubFilters();
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            GUI.backgroundColor = Color.white;
        }

        private void ResetSubFilters()
        {
            _currentEquipmentFilter = EquipmentSlotFilter.All;
            _currentConsumableFilter = ConsumableFilter.All;
            _currentBookScrollFilter = BookScrollFilter.All;
        }

        private void DrawSubTabs()
        {
            GUILayout.BeginHorizontal();

            switch (_currentTab)
            {
                case InventoryTab.Equipment:
                    DrawEquipmentSubTabs();
                    break;
                case InventoryTab.Consumable:
                    DrawConsumableSubTabs();
                    break;
                case InventoryTab.BookScroll:
                    DrawBookScrollSubTabs();
                    break;
                default:
                    GUILayout.Label("All items in this category", _statLabelStyle);
                    break;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawEquipmentSubTabs()
        {
            DrawSubTab("All", EquipmentSlotFilter.All, _equipmentSlotCountCache[EquipmentSlotFilter.All]);
            DrawSubTab("Head", EquipmentSlotFilter.Head, _equipmentSlotCountCache[EquipmentSlotFilter.Head]);
            DrawSubTab("Chest", EquipmentSlotFilter.Chest, _equipmentSlotCountCache[EquipmentSlotFilter.Chest]);
            DrawSubTab("Legs", EquipmentSlotFilter.Legs, _equipmentSlotCountCache[EquipmentSlotFilter.Legs]);
            DrawSubTab("Hands", EquipmentSlotFilter.Hands, _equipmentSlotCountCache[EquipmentSlotFilter.Hands]);
            DrawSubTab("Feet", EquipmentSlotFilter.Feet, _equipmentSlotCountCache[EquipmentSlotFilter.Feet]);
            DrawSubTab("Weapons", EquipmentSlotFilter.Weapons, _equipmentSlotCountCache[EquipmentSlotFilter.Weapons]);
            DrawSubTab("Accessories", EquipmentSlotFilter.Accessories, _equipmentSlotCountCache[EquipmentSlotFilter.Accessories]);
        }

        private void DrawConsumableSubTabs()
        {
            DrawSubTab("All", ConsumableFilter.All, _consumableCountCache[ConsumableFilter.All]);
            DrawSubTab("HP Potion", ConsumableFilter.EPotion, _consumableCountCache[ConsumableFilter.EPotion]);
            DrawSubTab("MP Potion", ConsumableFilter.MPotion, _consumableCountCache[ConsumableFilter.MPotion]);
            DrawSubTab("Food", ConsumableFilter.Food, _consumableCountCache[ConsumableFilter.Food]);
            DrawSubTab("Misc Pot", ConsumableFilter.MiscPot, _consumableCountCache[ConsumableFilter.MiscPot]);
            DrawSubTab("Junk", ConsumableFilter.JunkPot, _consumableCountCache[ConsumableFilter.JunkPot]);
        }

        private void DrawBookScrollSubTabs()
        {
            DrawSubTab("All", BookScrollFilter.All, _bookScrollCountCache[BookScrollFilter.All]);
            DrawSubTab("Skill Books", BookScrollFilter.SkillBook, _bookScrollCountCache[BookScrollFilter.SkillBook]);
            DrawSubTab("Spell Books", BookScrollFilter.SpellBook, _bookScrollCountCache[BookScrollFilter.SpellBook]);
            DrawSubTab("Scrolls", BookScrollFilter.Scroll, _bookScrollCountCache[BookScrollFilter.Scroll]);
        }

        private void DrawSubTab(string label, EquipmentSlotFilter filter, int count)
        {
            bool isActive = _currentEquipmentFilter == filter;
            if (GUILayout.Button($"{label} ({count})", isActive ? _subTabActiveStyle : _subTabInactiveStyle, GUILayout.Height(24)))
            {
                _currentEquipmentFilter = filter;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }
        }

        private void DrawSubTab(string label, ConsumableFilter filter, int count)
        {
            bool isActive = _currentConsumableFilter == filter;
            if (GUILayout.Button($"{label} ({count})", isActive ? _subTabActiveStyle : _subTabInactiveStyle, GUILayout.Height(24)))
            {
                _currentConsumableFilter = filter;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }
        }

        private void DrawSubTab(string label, BookScrollFilter filter, int count)
        {
            bool isActive = _currentBookScrollFilter == filter;
            if (GUILayout.Button($"{label} ({count})", isActive ? _subTabActiveStyle : _subTabInactiveStyle, GUILayout.Height(24)))
            {
                _currentBookScrollFilter = filter;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }
        }

        private void DrawItemsGrid()
        {
            float gridHeight = _InventoryWindowRect.height - 200;
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(gridHeight));

            if (_filteredItemsCache.Count == 0)
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("No items found in this category", _subtitleStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }
            else
            {
                int cardsPerRow = 4;
                int cardInRow = 0;

                GUILayout.BeginHorizontal();

                foreach (var item in _filteredItemsCache)
                {
                    if (item == null) continue;

                    if (cardInRow >= cardsPerRow)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.Space(ITEM_CARD_PADDING);
                        GUILayout.BeginHorizontal();
                        cardInRow = 0;
                    }

                    DrawItemCard(item);
                    cardInRow++;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        private void DrawItemCard(ItemBehaviour item)
        {
            GUILayout.BeginVertical(_itemCardStyle, GUILayout.Width(ITEM_CARD_WIDTH), GUILayout.Height(ITEM_CARD_HEIGHT));

            // Item Header with type color
            GUILayout.BeginHorizontal();
            Color typeColor = GetItemTypeColor(item);
            GUI.color = typeColor;
            GUILayout.Label(GetItemTypeIcon(item), GUILayout.Width(20));
            GUI.color = Color.white;
            
            GUILayout.Label(TruncateString(item.Name, 22), _titleStyle, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            // Type badge
            GUI.color = typeColor;
            GUILayout.Label(GetItemTypeName(item), _statLabelStyle, GUILayout.Height(16));
            GUI.color = Color.white;

            GUILayout.Space(4);

            // Stats
            DrawItemStats(item);

            GUILayout.FlexibleSpace();

            // Action buttons
            DrawItemActions(item);

            GUILayout.EndVertical();
        }

        private void DrawItemStats(ItemBehaviour item)
        {
            if (item.Weapon != null)
            {
                DrawStatLine("DPS", $"{item.Weapon.DamagePerSecond:F1}", _weaponColor);
            }

            if (item.Armor != null)
            {
                DrawStatLine("Armor", $"{item.Armor.Armor}", _armorColor);
            }

            if (item.LiquidContainer != null)
            {
                int health = item.LiquidContainer.GetResourceAmount(ResourceType.Health);
                int mana = item.LiquidContainer.GetResourceAmount(ResourceType.Mana);
                if (health > 0) DrawStatLine("Health", $"+{health}", _positiveColor);
                if (mana > 0) DrawStatLine("Mana", $"+{mana}", new Color(0.4f, 0.6f, 1f));
            }

            if (item.Consumable?.Food != null)
            {
                DrawStatLine("Food", "Restores HP", _consumableColor);
            }

            if (item.Level != null && item.Level.Level > 0)
            {
                DrawStatLine("Level", $"{item.Level.Level}", _accentColor);
            }

            if (item.Durability != null && item.Durability.MaxDurability > 0)
            {
                float pct = item.Durability.DurabilityPercentage * 100f;
                Color durColor = pct > 50 ? _positiveColor : pct > 25 ? _accentColor : _dangerColor;
                DrawStatLine("Durability", $"{pct:F0}%", durColor);
            }

            if (item.GoldValue > 0)
            {
                DrawStatLine("Value", $"{item.GoldValue}g", _accentColor);
            }
        }

        private void DrawStatLine(string label, string value, Color valueColor)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}:", _statLabelStyle, GUILayout.Width(70));
            GUI.color = valueColor;
            GUILayout.Label(value, _statLabelStyle);
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
        }

        private void DrawItemActions(ItemBehaviour item)
        {
            GUILayout.BeginHorizontal();

            // Primary action
            if (item.Armor != null || item.Weapon != null)
            {
                GUI.backgroundColor = _equipmentColor;
                if (GUILayout.Button("Equip", GUILayout.Height(22))) EquipItem(item);
            }
            else if (item.Consumable != null)
            {
                GUI.backgroundColor = _consumableColor;
                if (GUILayout.Button("Use", GUILayout.Height(22))) ConsumeItem(item);
            }
            else if (IsSkillBook(item))
            {
                GUI.backgroundColor = _bookScrollColor;
                if (GUILayout.Button("Learn", GUILayout.Height(22))) { _hero.Action_LearnSkillBook(item); _needsCacheUpdate = true; }
            }
            else if (item.SpellScroll != null)
            {
                GUI.backgroundColor = _bookScrollColor;
                if (GUILayout.Button("Cast", GUILayout.Height(22))) { _hero.Action_UseScroll(item); _needsCacheUpdate = true; }
            }

            // Drop button
            GUI.backgroundColor = _dangerColor;
            if (GUILayout.Button("✕", GUILayout.Width(28), GUILayout.Height(22))) DropItem(item);
            
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
        }

        private void DrawFooter()
        {
            GUILayout.Space(12);
            GUILayout.BeginHorizontal();
            
            GUI.backgroundColor = _dangerColor;
            if (GUILayout.Button("Drop Current Tabs", GUILayout.Height(32), GUILayout.Width(200)))
            {
                foreach (var item in _filteredItemsCache.ToList())
                {
                    try { _hero.Drop(item); } catch { }
                }
                _needsCacheUpdate = true;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Total inventory: {_hero.Character.Inventory.Items.Count()} items", _statLabelStyle);
            
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        private Color GetItemTypeColor(ItemBehaviour item)
        {
            if (item.Weapon != null) return _weaponColor;
            if (item.Armor != null) return _armorColor;
            if (item.Consumable != null) return _consumableColor;
            if (item.SpellScroll != null || IsSkillBook(item)) return _bookScrollColor;
            return _miscColor;
        }

        private string GetItemTypeIcon(ItemBehaviour item)
        {
            if (item.Weapon != null) return "⚔";
            if (item.Armor != null) return "🛡";
            if (item.Consumable != null) return "🧪";
            if (item.SpellScroll != null) return "📜";
            if (IsSkillBook(item)) return "📖";
            return "📦";
        }

        private string GetItemTypeName(ItemBehaviour item)
        {
            if (item.Weapon != null) return "Weapon";
            if (item.Armor != null) return $"Armor • {item.Equipable?.Slot}";
            if (item.Consumable?.Food != null) return "Food";
            if (item.Consumable != null) return "Potion";
            if (item.SpellScroll != null) return "Scroll";
            if (IsSkillBook(item)) return "Skill Book";
            return "Misc";
        }

        private string TruncateString(string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Length <= maxLength ? str : str.Substring(0, maxLength - 2) + "..";
        }

        // ─────────────────────────────────────────────────────────────────
        // Cache & Filter Methods (unchanged logic)
        // ─────────────────────────────────────────────────────────────────

        private void InitializeCache()
        {
            _itemCountCache.Clear();
            foreach (InventoryTab tab in Enum.GetValues(typeof(InventoryTab)))
                _itemCountCache[tab] = 0;

            _equipmentSlotCountCache.Clear();
            foreach (EquipmentSlotFilter filter in Enum.GetValues(typeof(EquipmentSlotFilter)))
                _equipmentSlotCountCache[filter] = 0;

            _consumableCountCache.Clear();
            foreach (ConsumableFilter filter in Enum.GetValues(typeof(ConsumableFilter)))
                _consumableCountCache[filter] = 0;

            _bookScrollCountCache.Clear();
            foreach (BookScrollFilter filter in Enum.GetValues(typeof(BookScrollFilter)))
                _bookScrollCountCache[filter] = 0;

            _needsCacheUpdate = true;
        }

        private void UpdateCacheIfNeeded()
        {
            if (!_needsCacheUpdate && Time.time - _lastCacheRefreshTime < CACHE_REFRESH_INTERVAL)
                return;

            if (_hero?.Character?.Inventory?.Items == null)
                return;

            var inventory = _hero.Character.Inventory.Items;

            _itemCountCache[InventoryTab.Equipment] = FilterItemsByTab(inventory, InventoryTab.Equipment).Count();
            _itemCountCache[InventoryTab.Consumable] = FilterItemsByTab(inventory, InventoryTab.Consumable).Count();
            _itemCountCache[InventoryTab.BookScroll] = FilterItemsByTab(inventory, InventoryTab.BookScroll).Count();
            _itemCountCache[InventoryTab.Misc] = FilterItemsByTab(inventory, InventoryTab.Misc).Count();

            var equipmentItems = FilterItemsByTab(inventory, InventoryTab.Equipment).ToList();
            _equipmentSlotCountCache[EquipmentSlotFilter.All] = equipmentItems.Count;
            _equipmentSlotCountCache[EquipmentSlotFilter.Head] = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Head).Count();
            _equipmentSlotCountCache[EquipmentSlotFilter.Shoulders] = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Shoulders).Count();
            _equipmentSlotCountCache[EquipmentSlotFilter.Arms] = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Arms).Count();
            _equipmentSlotCountCache[EquipmentSlotFilter.Hands] = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Hands).Count();
            _equipmentSlotCountCache[EquipmentSlotFilter.Chest] = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Chest).Count();
            _equipmentSlotCountCache[EquipmentSlotFilter.Legs] = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Legs).Count();
            _equipmentSlotCountCache[EquipmentSlotFilter.Feet] = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Feet).Count();
            _equipmentSlotCountCache[EquipmentSlotFilter.Weapons] = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Weapons).Count();
            _equipmentSlotCountCache[EquipmentSlotFilter.Accessories] = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Accessories).Count();

            _consumableCountCache[ConsumableFilter.All] = FilterItemsByTab(inventory, InventoryTab.Consumable).Count();
            _consumableCountCache[ConsumableFilter.EPotion] = FilterItemsByConsumableType(inventory, "e_potion").Count();
            _consumableCountCache[ConsumableFilter.MPotion] = FilterItemsByConsumableType(inventory, "m_potion").Count();
            _consumableCountCache[ConsumableFilter.MiscPot] = FilterItemsByConsumableType(inventory, "misc_pot").Count();
            _consumableCountCache[ConsumableFilter.JunkPot] = FilterItemsByConsumableType(inventory, "junk_pot").Count();
            _consumableCountCache[ConsumableFilter.Food] = FilterItemsByConsumableType(inventory, "food").Count();
            _consumableCountCache[ConsumableFilter.Misc] = FilterItemsByConsumableType(inventory, "misc").Count();

            var bookScrollItems = FilterItemsByTab(inventory, InventoryTab.BookScroll).ToList();
            _bookScrollCountCache[BookScrollFilter.All] = bookScrollItems.Count;
            _bookScrollCountCache[BookScrollFilter.SkillBook] = bookScrollItems.Where(IsSkillBook).Count();
            _bookScrollCountCache[BookScrollFilter.SpellBook] = bookScrollItems.Where(IsSpellBook).Count();
            _bookScrollCountCache[BookScrollFilter.Scroll] = bookScrollItems.Where(i => i.SpellScroll != null).Count();

            UpdateFilteredItemsCache();
            _lastCacheRefreshTime = Time.time;
            _needsCacheUpdate = false;
        }

        private void UpdateFilteredItemsCache()
        {
            if (_hero?.Character?.Inventory?.Items == null) return;

            var inventory = _hero.Character.Inventory.Items;
            _filteredItemsCache = FilterItemsByTab(inventory, _currentTab).ToList();

            if (_currentTab == InventoryTab.Equipment)
                _filteredItemsCache = FilterItemsByEquipmentSlot(_filteredItemsCache, _currentEquipmentFilter).ToList();
            else if (_currentTab == InventoryTab.Consumable)
                _filteredItemsCache = FilterConsumablesByType(_filteredItemsCache, _currentConsumableFilter).ToList();
            else if (_currentTab == InventoryTab.BookScroll)
                _filteredItemsCache = FilterBooksAndScrollsByType(_filteredItemsCache, _currentBookScrollFilter).ToList();
        }

        private IEnumerable<ItemBehaviour> FilterItemsByTab(IEnumerable<ItemBehaviour> items, InventoryTab tab) => tab switch
        {
            InventoryTab.Equipment => items.Where(i => i.Armor != null || i.Weapon != null),
            InventoryTab.Consumable => items.Where(i => i.Consumable != null),
            InventoryTab.BookScroll => items.Where(i => i.Name.ToLower().Contains("book") || i.SpellScroll != null),
            InventoryTab.Misc => items.Where(i => i.Armor == null && i.Weapon == null && i.Consumable == null && i.SpellBook == null && i.SpellScroll == null),
            _ => items
        };

        private IEnumerable<ItemBehaviour> FilterItemsByEquipmentSlot(IEnumerable<ItemBehaviour> items, EquipmentSlotFilter filter) => filter switch
        {
            EquipmentSlotFilter.All => items,
            EquipmentSlotFilter.Head => items.Where(i => MatchesSlot(i, EquipmentSlot.Head)),
            EquipmentSlotFilter.Shoulders => items.Where(i => MatchesSlot(i, EquipmentSlot.LeftShoulder) || MatchesSlot(i, EquipmentSlot.RightShoulder)),
            EquipmentSlotFilter.Arms => items.Where(i => MatchesSlot(i, EquipmentSlot.LeftArm) || MatchesSlot(i, EquipmentSlot.RightArm)),
            EquipmentSlotFilter.Hands => items.Where(i => MatchesSlot(i, EquipmentSlot.LeftGlove) || MatchesSlot(i, EquipmentSlot.RightGlove)),
            EquipmentSlotFilter.Chest => items.Where(i => MatchesSlot(i, EquipmentSlot.Chest)),
            EquipmentSlotFilter.Legs => items.Where(i => MatchesSlot(i, EquipmentSlot.Legs)),
            EquipmentSlotFilter.Feet => items.Where(i => MatchesSlot(i, EquipmentSlot.LeftFeet) || MatchesSlot(i, EquipmentSlot.RightFeet)),
            EquipmentSlotFilter.Weapons => items.Where(i => MatchesSlot(i, EquipmentSlot.LeftHand) || MatchesSlot(i, EquipmentSlot.RightHand)),
            EquipmentSlotFilter.Accessories => items.Where(i => MatchesSlot(i, EquipmentSlot.Ring) || MatchesSlot(i, EquipmentSlot.Hair) || MatchesSlot(i, EquipmentSlot.FacialHair)),
            _ => items
        };

        private IEnumerable<ItemBehaviour> FilterConsumablesByType(IEnumerable<ItemBehaviour> items, ConsumableFilter filter) => filter switch
        {
            ConsumableFilter.All => items.Where(i => i.Consumable != null),
            ConsumableFilter.EPotion => items.Where(HasHealthPotion),
            ConsumableFilter.MPotion => items.Where(HasManaPotion),
            ConsumableFilter.MiscPot => items.Where(IsMiscPotion),
            ConsumableFilter.JunkPot => items.Where(IsJunkPotion),
            ConsumableFilter.Food => items.Where(IsFood),
            ConsumableFilter.Misc => items.Where(IsMiscConsumable),
            _ => items
        };

        private IEnumerable<ItemBehaviour> FilterBooksAndScrollsByType(IEnumerable<ItemBehaviour> items, BookScrollFilter filter) => filter switch
        {
            BookScrollFilter.All => items.Where(i => i.SpellScroll != null || i.Name.ToLower().Contains("book")),
            BookScrollFilter.SkillBook => items.Where(IsSkillBook),
            BookScrollFilter.SpellBook => items.Where(IsSpellBook),
            BookScrollFilter.Scroll => items.Where(i => i.SpellScroll != null),
            _ => items
        };

        private IEnumerable<ItemBehaviour> FilterItemsByConsumableType(IEnumerable<ItemBehaviour> items, string type) => type switch
        {
            "e_potion" => items.Where(HasHealthPotion),
            "m_potion" => items.Where(HasManaPotion),
            "misc_pot" => items.Where(IsMiscPotion),
            "junk_pot" => items.Where(IsJunkPotion),
            "food" => items.Where(IsFood),
            "misc" => items.Where(IsMiscConsumable),
            _ => items.Where(i => i.Consumable != null)
        };

        private bool MatchesSlot(ItemBehaviour item, EquipmentSlot slot)
        {
            if (item.Armor != null && item?.Equipable?.Slot != null)
                return item.Equipable.Slot == slot;
            if (item.Weapon != null)
                return slot == EquipmentSlot.LeftHand || slot == EquipmentSlot.RightHand;
            return false;
        }

        private bool HasHealthPotion(ItemBehaviour item) => item?.LiquidContainer?.GetResourceAmount(ResourceType.Health) > 0;
        private bool HasManaPotion(ItemBehaviour item) => item?.LiquidContainer?.GetResourceAmount(ResourceType.Mana) > 0;
        private bool IsFood(ItemBehaviour item) => item?.Consumable?.Food != null;
        private bool IsSkillBook(ItemBehaviour item) => item?.GetComponent<SkillBookBehavior>() != null;
        private bool IsSpellBook(ItemBehaviour item) => item?.GetComponent<SkillBookBehavior>() == null && item?.SpellBook != null;

        private bool IsMiscPotion(ItemBehaviour item)
        {
            if (item?.Consumable == null || item.Consumable.Food != null) return false;
            if (item?.LiquidContainer == null) return false;
            if (HasHealthPotion(item) || HasManaPotion(item)) return false;
            return GetTotalLiquid(item) > 0;
        }

        private bool IsJunkPotion(ItemBehaviour item)
        {
            if (item?.Consumable == null || item.Consumable.Food != null) return false;
            if (item?.LiquidContainer == null) return false;
            return GetTotalLiquid(item) == 0;
        }

        private bool IsMiscConsumable(ItemBehaviour item)
        {
            if (item?.Consumable == null) return false;
            return !IsFood(item) && !HasHealthPotion(item) && !HasManaPotion(item) && !IsMiscPotion(item) && !IsJunkPotion(item);
        }

        private int GetTotalLiquid(ItemBehaviour item)
        {
            if (item?.LiquidContainer == null) return 0;
            int total = 0;
            foreach (ResourceType rt in Enum.GetValues(typeof(ResourceType)))
                total += item.LiquidContainer.GetResourceAmount(rt);
            return total;
        }

        private void DropItem(ItemBehaviour item)
        {
            if (item == null || _hero == null) return;
            try { _hero.Drop(item); _needsCacheUpdate = true; }
            catch (Exception ex) { Debug.LogError($"Error dropping {item.Name}: {ex.Message}"); }
        }

        private void ConsumeItem(ItemBehaviour item)
        {
            if (item?.Consumable == null || _hero == null) return;
            try { _hero.Action_ConsumeItem(item); _needsCacheUpdate = true; }
            catch (Exception ex) { Debug.LogError($"Error consuming {item.Name}: {ex.Message}"); }
        }

        private void EquipItem(ItemBehaviour item)
        {
            if ((item?.Armor == null && item?.Weapon == null) || _hero == null) return;
            try { _hero.Equip(item); _needsCacheUpdate = true; }
            catch (Exception ex) { Debug.LogError($"Error equipping {item.Name}: {ex.Message}"); }
        }
    }
}
