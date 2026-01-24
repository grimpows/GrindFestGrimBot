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

        private Rect _InventoryWindowRect = new Rect(100, 100, 650, 500);
        private Vector2 _scrollPosition = Vector2.zero;

        private const int ITEM_HEIGHT = 120;
        private const int ITEM_PADDING = 10;
        private const int HEADER_HEIGHT = 30;
        private const int TAB_HEIGHT = 35;
        private const int SUBTAB_HEIGHT = 30;
        private const int BUTTON_WIDTH = 90;
        private const int BUTTON_HEIGHT = 25;
        private const float CACHE_REFRESH_INTERVAL = 0.5f;

        private InventoryTab _currentTab = InventoryTab.Equipment;
        private EquipmentSlotFilter _currentEquipmentFilter = EquipmentSlotFilter.All;
        private ConsumableFilter _currentConsumableFilter = ConsumableFilter.All;

        // Cache system
        private Dictionary<InventoryTab, int> _itemCountCache = new Dictionary<InventoryTab, int>();
        private Dictionary<EquipmentSlotFilter, int> _equipmentSlotCountCache = new Dictionary<EquipmentSlotFilter, int>();
        private Dictionary<ConsumableFilter, int> _consumableCountCache = new Dictionary<ConsumableFilter, int>();
        private List<ItemBehaviour> _filteredItemsCache = new List<ItemBehaviour>();
        private float _lastCacheRefreshTime = 0f;
        private bool _needsCacheUpdate = true;

        private enum InventoryTab
        {
            Equipment,
            Consumable,
            SpellBook,
            Scroll,
            Misc
        }

        private enum EquipmentSlotFilter
        {
            All,
            Head,
            Shoulders,
            Arms,
            Hands,
            Chest,
            Legs,
            Feet,
            Weapons,
            Accessories
        }

        private enum ConsumableFilter
        {
            All,
            EPotion,
            MPotion,
            OPotion,
            Food,
            Misc
        }

        public void OnStart(Hero_Base hero, KeyCode toggleShowKey, int windowID)
        {
            _hero = hero;
            _toggleShowKey = toggleShowKey;
            _windowId = windowID;
            InitializeCache();
        }

        private void InitializeCache()
        {
            _itemCountCache.Clear();
            foreach (InventoryTab tab in System.Enum.GetValues(typeof(InventoryTab)))
            {
                _itemCountCache[tab] = 0;
            }

            _equipmentSlotCountCache.Clear();
            foreach (EquipmentSlotFilter filter in System.Enum.GetValues(typeof(EquipmentSlotFilter)))
            {
                _equipmentSlotCountCache[filter] = 0;
            }

            _consumableCountCache.Clear();
            foreach (ConsumableFilter filter in System.Enum.GetValues(typeof(ConsumableFilter)))
            {
                _consumableCountCache[filter] = 0;
            }

            _needsCacheUpdate = true;
        }

        private void UpdateCacheIfNeeded()
        {
            if (!_needsCacheUpdate && Time.time - _lastCacheRefreshTime < CACHE_REFRESH_INTERVAL)
            {
                return;
            }

            if (_hero?.Character?.Inventory?.Items == null)
                return;

            var inventory = _hero.Character.Inventory.Items;

            // Update tab counts
            _itemCountCache[InventoryTab.Equipment] = FilterItemsByTab(inventory, InventoryTab.Equipment).Count();
            _itemCountCache[InventoryTab.Consumable] = FilterItemsByTab(inventory, InventoryTab.Consumable).Count();
            _itemCountCache[InventoryTab.SpellBook] = FilterItemsByTab(inventory, InventoryTab.SpellBook).Count();
            _itemCountCache[InventoryTab.Scroll] = FilterItemsByTab(inventory, InventoryTab.Scroll).Count();
            _itemCountCache[InventoryTab.Misc] = FilterItemsByTab(inventory, InventoryTab.Misc).Count();

            // Update equipment slot counts
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

            // Update consumable filter counts
            _consumableCountCache[ConsumableFilter.All] = FilterItemsByTab(inventory, InventoryTab.Consumable).Count();
            _consumableCountCache[ConsumableFilter.EPotion] = FilterItemsByConsumableType(inventory, "e_potion").Count();
            _consumableCountCache[ConsumableFilter.MPotion] = FilterItemsByConsumableType(inventory, "m_potion").Count();
            _consumableCountCache[ConsumableFilter.OPotion] = FilterItemsByConsumableType(inventory, "o_potion").Count();
            _consumableCountCache[ConsumableFilter.Food] = FilterItemsByConsumableType(inventory, "food").Count();
            _consumableCountCache[ConsumableFilter.Misc] = FilterItemsByConsumableType(inventory, "misc").Count();

            // Update filtered items for current view
            UpdateFilteredItemsCache();

            _lastCacheRefreshTime = Time.time;
            _needsCacheUpdate = false;
        }

        private void UpdateFilteredItemsCache()
        {
            if (_hero?.Character?.Inventory?.Items == null)
                return;

            var inventory = _hero.Character.Inventory.Items;
            _filteredItemsCache = FilterItemsByTab(inventory, _currentTab).ToList();

            if (_currentTab == InventoryTab.Equipment)
            {
                _filteredItemsCache = FilterItemsByEquipmentSlot(_filteredItemsCache, _currentEquipmentFilter).ToList();
            }
            else if (_currentTab == InventoryTab.Consumable)
            {
                _filteredItemsCache = FilterConsumablesByType(_filteredItemsCache, _currentConsumableFilter).ToList();
            }
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
                UpdateCacheIfNeeded();
                _InventoryWindowRect = GUI.Window(WindowsConstants.INVENTORY_WINDOW_ID, _InventoryWindowRect, DrawInventoryWindow, "Inventory");
            }
        }

        public void OnUpdate()
        {
            // Mark cache as needing update (called once per frame but less frequently than OnGUI)
            // This allows the cache to refresh at a controlled rate
        }

        void DrawInventoryWindow(int windowID)
        {
            if (!_isShow || _hero?.Character?.Inventory?.Items == null)
            {
                GUI.DragWindow();
                return;
            }

            DrawTabs();

            if (_currentTab == InventoryTab.Equipment)
            {
                DrawEquipmentSubTabs();
            }
            else if (_currentTab == InventoryTab.Consumable)
            {
                DrawConsumableSubTabs();
            }

            float tabAreaOffset = (_currentTab == InventoryTab.Equipment || _currentTab == InventoryTab.Consumable)
                ? HEADER_HEIGHT + TAB_HEIGHT + SUBTAB_HEIGHT + 10
                : HEADER_HEIGHT + TAB_HEIGHT;

            int itemCount = _filteredItemsCache.Count;

            GUILayout.BeginArea(new Rect(10, tabAreaOffset, _InventoryWindowRect.width - 20, _InventoryWindowRect.height - tabAreaOffset - 40));

            float scrollViewHeight = _InventoryWindowRect.height - tabAreaOffset - 70;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(_InventoryWindowRect.width - 30), GUILayout.Height(scrollViewHeight));

            int index = 0;
            foreach (var item in _filteredItemsCache)
            {
                DrawInventoryItem(item, index);
                index++;
            }

            //add a button to dorp all items in the current tab
            if (GUILayout.Button("Drop All Items in Current Tab"))
            {
                foreach (var item in _filteredItemsCache.ToList())
                {
                    try
                    {
                        _hero.Drop(item);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error dropping item {item.Name}: {ex.Message}");
                    }
                }
                _needsCacheUpdate = true;
            }


            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUI.DragWindow(new Rect(0, 0, _InventoryWindowRect.width, 30));
        }

        void DrawTabs()
        {
            float tabWidth = (_InventoryWindowRect.width - 60) / 5f;
            float startX = 10;
            float startY = HEADER_HEIGHT;

            Color originalColor = GUI.backgroundColor;

            if (DrawTab($"Equipment ({_itemCountCache[InventoryTab.Equipment]})", new Rect(startX, startY, tabWidth, TAB_HEIGHT), _currentTab == InventoryTab.Equipment))
            {
                _currentTab = InventoryTab.Equipment;
                _currentEquipmentFilter = EquipmentSlotFilter.All;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawTab($"Consumable ({_itemCountCache[InventoryTab.Consumable]})", new Rect(startX + tabWidth + 5, startY, tabWidth, TAB_HEIGHT), _currentTab == InventoryTab.Consumable))
            {
                _currentTab = InventoryTab.Consumable;
                _currentConsumableFilter = ConsumableFilter.All;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawTab($"SpellBook ({_itemCountCache[InventoryTab.SpellBook]})", new Rect(startX + (tabWidth + 5) * 2, startY, tabWidth, TAB_HEIGHT), _currentTab == InventoryTab.SpellBook))
            {
                _currentTab = InventoryTab.SpellBook;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawTab($"Scroll ({_itemCountCache[InventoryTab.Scroll]})", new Rect(startX + (tabWidth + 5) * 3, startY, tabWidth, TAB_HEIGHT), _currentTab == InventoryTab.Scroll))
            {
                _currentTab = InventoryTab.Scroll;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawTab($"Misc ({_itemCountCache[InventoryTab.Misc]})", new Rect(startX + (tabWidth + 5) * 4, startY, tabWidth, TAB_HEIGHT), _currentTab == InventoryTab.Misc))
            {
                _currentTab = InventoryTab.Misc;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            GUI.backgroundColor = originalColor;
        }

        void DrawEquipmentSubTabs()
        {
            float subTabWidth = (_InventoryWindowRect.width - 50) / 5f;
            float startX = 10;
            float startY = HEADER_HEIGHT + TAB_HEIGHT;

            Color originalColor = GUI.backgroundColor;

            if (DrawSubTab($"All ({_equipmentSlotCountCache[EquipmentSlotFilter.All]})", new Rect(startX, startY, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.All))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.All;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawSubTab($"Head ({_equipmentSlotCountCache[EquipmentSlotFilter.Head]})", new Rect(startX + subTabWidth + 5, startY, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Head))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Head;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawSubTab($"Shoulders ({_equipmentSlotCountCache[EquipmentSlotFilter.Shoulders]})", new Rect(startX + (subTabWidth + 5) * 2, startY, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Shoulders))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Shoulders;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawSubTab($"Arms ({_equipmentSlotCountCache[EquipmentSlotFilter.Arms]})", new Rect(startX + (subTabWidth + 5) * 3, startY, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Arms))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Arms;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawSubTab($"Hands ({_equipmentSlotCountCache[EquipmentSlotFilter.Hands]})", new Rect(startX + (subTabWidth + 5) * 4, startY, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Hands))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Hands;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            GUI.backgroundColor = originalColor;

            // Second row of sub-tabs
            float startY2 = startY + SUBTAB_HEIGHT + 10;

            if (DrawSubTab($"Chest ({_equipmentSlotCountCache[EquipmentSlotFilter.Chest]})", new Rect(startX, startY2, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Chest))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Chest;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawSubTab($"Legs ({_equipmentSlotCountCache[EquipmentSlotFilter.Legs]})", new Rect(startX + subTabWidth + 5, startY2, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Legs))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Legs;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawSubTab($"Feet ({_equipmentSlotCountCache[EquipmentSlotFilter.Feet]})", new Rect(startX + (subTabWidth + 5) * 2, startY2, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Feet))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Feet;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawSubTab($"Weapons ({_equipmentSlotCountCache[EquipmentSlotFilter.Weapons]})", new Rect(startX + (subTabWidth + 5) * 3, startY2, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Weapons))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Weapons;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawSubTab($"Accessories ({_equipmentSlotCountCache[EquipmentSlotFilter.Accessories]})", new Rect(startX + (subTabWidth + 5) * 4, startY2, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Accessories))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Accessories;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            GUI.backgroundColor = originalColor;
        }

        void DrawConsumableSubTabs()
        {
            float subTabWidth = (_InventoryWindowRect.width - 50) / 5f;
            float startX = 10;
            float startY = HEADER_HEIGHT + TAB_HEIGHT;

            Color originalColor = GUI.backgroundColor;

            if (DrawSubTab($"All ({_consumableCountCache[ConsumableFilter.All]})", new Rect(startX, startY, subTabWidth, SUBTAB_HEIGHT), _currentConsumableFilter == ConsumableFilter.All))
            {
                _currentConsumableFilter = ConsumableFilter.All;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawSubTab($"E-Potions ({_consumableCountCache[ConsumableFilter.EPotion]})", new Rect(startX + subTabWidth + 5, startY, subTabWidth, SUBTAB_HEIGHT), _currentConsumableFilter == ConsumableFilter.EPotion))
            {
                _currentConsumableFilter = ConsumableFilter.EPotion;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawSubTab($"M-Potions ({_consumableCountCache[ConsumableFilter.MPotion]})", new Rect(startX + (subTabWidth + 5) * 2, startY, subTabWidth, SUBTAB_HEIGHT), _currentConsumableFilter == ConsumableFilter.MPotion))
            {
                _currentConsumableFilter = ConsumableFilter.MPotion;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawSubTab($"O-Potions ({_consumableCountCache[ConsumableFilter.OPotion]})", new Rect(startX + (subTabWidth + 5) * 3, startY, subTabWidth, SUBTAB_HEIGHT), _currentConsumableFilter == ConsumableFilter.OPotion))
            {
                _currentConsumableFilter = ConsumableFilter.OPotion;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            if (DrawSubTab($"Food ({_consumableCountCache[ConsumableFilter.Food]})", new Rect(startX + (subTabWidth + 5) * 4, startY, subTabWidth, SUBTAB_HEIGHT), _currentConsumableFilter == ConsumableFilter.Food))
            {
                _currentConsumableFilter = ConsumableFilter.Food;
                _scrollPosition = Vector2.zero;
                UpdateFilteredItemsCache();
            }

            GUI.backgroundColor = originalColor;
        }

        bool DrawTab(string label, Rect rect, bool isActive)
        {
            Color originalColor = GUI.backgroundColor;

            if (isActive)
            {
                GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            }
            else
            {
                GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f);
            }

            bool clicked = GUI.Button(rect, label);

            GUI.backgroundColor = originalColor;

            return clicked;
        }

        bool DrawSubTab(string label, Rect rect, bool isActive)
        {
            Color originalColor = GUI.backgroundColor;

            if (isActive)
            {
                GUI.backgroundColor = new Color(0.2f, 0.9f, 0.2f);
            }
            else
            {
                GUI.backgroundColor = new Color(0.5f, 0.8f, 0.5f);
            }

            bool clicked = GUI.Button(rect, label, GUI.skin.button);

            GUI.backgroundColor = originalColor;

            return clicked;
        }

        IEnumerable<ItemBehaviour> FilterItemsByTab(IEnumerable<ItemBehaviour> items, InventoryTab tab)
        {
            return tab switch
            {
                InventoryTab.Equipment => items.Where(item => item.Armor != null || item.Weapon != null),
                InventoryTab.Consumable => items.Where(item => item.Consumable != null),
                InventoryTab.SpellBook => items.Where(item => item.Name.ToLower().Contains("book")),
                InventoryTab.Scroll => items.Where(item => item.SpellScroll != null),
                InventoryTab.Misc => items.Where(item => item.Armor == null && item.Weapon == null && item.Consumable == null && item.SpellBook == null && item.SpellScroll == null),
                _ => items
            };
        }

        IEnumerable<ItemBehaviour> FilterItemsByEquipmentSlot(IEnumerable<ItemBehaviour> items, EquipmentSlotFilter filter)
        {
            return filter switch
            {
                EquipmentSlotFilter.All => items,
                EquipmentSlotFilter.Head => items.Where(item => MatchesEquipmentSlot(item, EquipmentSlot.Head)),
                EquipmentSlotFilter.Shoulders => items.Where(item => MatchesEquipmentSlot(item, EquipmentSlot.LeftShoulder) || MatchesEquipmentSlot(item, EquipmentSlot.RightShoulder)),
                EquipmentSlotFilter.Arms => items.Where(item => MatchesEquipmentSlot(item, EquipmentSlot.LeftArm) || MatchesEquipmentSlot(item, EquipmentSlot.RightArm)),
                EquipmentSlotFilter.Hands => items.Where(item => MatchesEquipmentSlot(item, EquipmentSlot.LeftGlove) || MatchesEquipmentSlot(item, EquipmentSlot.RightGlove)),
                EquipmentSlotFilter.Chest => items.Where(item => MatchesEquipmentSlot(item, EquipmentSlot.Chest)),
                EquipmentSlotFilter.Legs => items.Where(item => MatchesEquipmentSlot(item, EquipmentSlot.Legs)),
                EquipmentSlotFilter.Feet => items.Where(item => MatchesEquipmentSlot(item, EquipmentSlot.LeftFeet) || MatchesEquipmentSlot(item, EquipmentSlot.RightFeet)),
                EquipmentSlotFilter.Weapons => items.Where(item => MatchesEquipmentSlot(item, EquipmentSlot.LeftHand) || MatchesEquipmentSlot(item, EquipmentSlot.RightHand)),
                EquipmentSlotFilter.Accessories => items.Where(item => MatchesEquipmentSlot(item, EquipmentSlot.Ring) || MatchesEquipmentSlot(item, EquipmentSlot.Hair) || MatchesEquipmentSlot(item, EquipmentSlot.FacialHair)),
                _ => items
            };
        }

        IEnumerable<ItemBehaviour> FilterConsumablesByType(IEnumerable<ItemBehaviour> items, ConsumableFilter filter)
        {
            return filter switch
            {
                ConsumableFilter.All => items.Where(item => item.Consumable != null),
                ConsumableFilter.EPotion => items.Where(item => HasHealthPotion(item)),
                ConsumableFilter.MPotion => items.Where(item => HasManaPotion(item)),
                ConsumableFilter.OPotion => items.Where(item => HasOtherPotion(item)),
                ConsumableFilter.Food => items.Where(item => IsFood(item)),
                ConsumableFilter.Misc => items.Where(item => IsMiscConsumable(item)),
                _ => items
            };
        }

        IEnumerable<ItemBehaviour> FilterItemsByConsumableType(IEnumerable<ItemBehaviour> items, string consumableType)
        {
            return consumableType switch
            {
                "e_potion" => items.Where(item => HasHealthPotion(item)),
                "m_potion" => items.Where(item => HasManaPotion(item)),
                "o_potion" => items.Where(item => HasOtherPotion(item)),
                "food" => items.Where(item => IsFood(item)),
                "misc" => items.Where(item => IsMiscConsumable(item)),
                _ => items.Where(item => item.Consumable != null)
            };
        }

        private bool HasHealthPotion(ItemBehaviour item)
        {
            if (item?.LiquidContainer != null)
            {
                int healthAmount = item.LiquidContainer.GetResourceAmount(ResourceType.Health);
                return healthAmount > 0;
            }
            return false;
        }

        private bool HasManaPotion(ItemBehaviour item)
        {
            if (item?.LiquidContainer != null)
            {
                int manaAmount = item.LiquidContainer.GetResourceAmount(ResourceType.Mana);
                return manaAmount > 0;
            }
            return false;
        }

        private bool HasOtherPotion(ItemBehaviour item)
        {
            //ignore food
            if (item?.Consumable != null && item.Consumable.Food != null)
            {
                return false;
            }

            if (item?.Consumable != null && item.LiquidContainer == null)
            {
                return true;
            }
            if (item?.LiquidContainer != null)
            {
                int healthAmount = item.LiquidContainer.GetResourceAmount(ResourceType.Health);
                int manaAmount = item.LiquidContainer.GetResourceAmount(ResourceType.Mana);
                return healthAmount == 0 && manaAmount == 0;
            }
            return false;
        }

        private bool IsFood(ItemBehaviour item)
        {
            return item?.Consumable != null && item.Consumable.Food != null;
        }

        private bool IsMiscConsumable(ItemBehaviour item)
        {
            if (item?.Consumable == null)
                return false;

            return !IsFood(item) && !HasHealthPotion(item) && !HasManaPotion(item) && !HasOtherPotion(item);
        }

        bool MatchesEquipmentSlot(ItemBehaviour item, EquipmentSlot slot)
        {
            // Check if the item can be equipped in this slot
            // This is a simplified check - you may need to adjust based on your ItemBehaviour implementation
            if (item.Armor != null)
            {
                return (slot == EquipmentSlot.Head && item.name.Contains("Head")) ||
                       (slot == EquipmentSlot.Chest && item.name.Contains("Chest")) ||
                       (slot == EquipmentSlot.Legs && item.name.Contains("Legs")) ||
                       ((slot == EquipmentSlot.LeftShoulder || slot == EquipmentSlot.RightShoulder) && item.name.Contains("Shoulder")) ||
                       ((slot == EquipmentSlot.LeftArm || slot == EquipmentSlot.RightArm) && item.name.Contains("Arm")) ||
                       ((slot == EquipmentSlot.LeftGlove || slot == EquipmentSlot.RightGlove) && item.name.Contains("Glove")) ||
                       ((slot == EquipmentSlot.LeftFeet || slot == EquipmentSlot.RightFeet) && item.name.Contains("Boots"));
            }

            if (item.Weapon != null)
            {
                return slot == EquipmentSlot.LeftHand || slot == EquipmentSlot.RightHand;
            }

            return false;
        }

        void DrawInventoryItem(ItemBehaviour item, int index)
        {
            if (item == null) return;

            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(ITEM_HEIGHT));

            // Header row with item number, name and type
            GUILayout.BeginHorizontal();
            GUILayout.Label($"#{index + 1}", GUILayout.Width(40));
            GUILayout.Label($"{item.Name}", GUILayout.Width(200));

            DrawItemTypeLabel(item);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Item details
            DrawItemSpecificInfo(item);

            if (item.Level != null && item.Level.Level > 0)
            {
                GUILayout.Label($"Level: {item.Level.Level}");
            }

            if (item.Durability != null && item.Durability.MaxDurability > 0)
            {
                float durabilityPercent = (float)item.Durability.CurrentDurability / item.Durability.MaxDurability * 100f;
                Color durabilityColor = GetDurabilityColor(durabilityPercent);
                Color originalColor = GUI.color;
                GUI.color = durabilityColor;
                GUILayout.Label($"Durability: {durabilityPercent:F0}% ({item.Durability.CurrentDurability}/{item.Durability.MaxDurability})");
                GUI.color = originalColor;
            }


            // Action buttons row
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Specific action button based on item type
            if (item.name.ToLower().Contains("scroll"))
            {
                Color originalBgColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.8f, 0.2f);

                if (GUILayout.Button("Use Scroll", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT)))
                {
                    _hero.UseScroll(item);
                    _needsCacheUpdate = true;
                }

                GUI.backgroundColor = originalBgColor;
            }
            else if (item.name.ToLower().Contains("book"))
            {
                Color originalBgColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.8f, 0.4f, 1f);

                if (GUILayout.Button("Learn", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT)))
                {
                    _hero.LearnSpellBook(item);
                    _needsCacheUpdate = true;
                }

                GUI.backgroundColor = originalBgColor;
            }
            else if (item.Consumable != null)
            {
                Color originalBgColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.3f, 1f, 0.3f);

                if (GUILayout.Button("Consume", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT)))
                {
                    ConsumeItem(item);
                }

                GUI.backgroundColor = originalBgColor;
            }
            else if (item.Armor != null || item.Weapon != null)
            {
                Color originalBgColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);

                if (GUILayout.Button("Equip", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT)))
                {
                    EquipItem(item);
                    _needsCacheUpdate = true;
                }

                GUI.backgroundColor = originalBgColor;
            }

            // Drop button
            Color dropBgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);

            if (GUILayout.Button("Drop", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(BUTTON_HEIGHT)))
            {
                DropItem(item);
            }

            GUI.backgroundColor = dropBgColor;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(ITEM_PADDING);
        }

        void DropItem(ItemBehaviour item)
        {
            if (item != null && _hero != null)
            {
                try
                {
                    _hero.Drop(item);
                    _needsCacheUpdate = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error dropping item {item.Name}: {ex.Message}");
                }
            }
        }

        void ConsumeItem(ItemBehaviour item)
        {
            if (item?.Consumable != null && _hero != null)
            {
                try
                {
                    _hero.ConsumeItem(item);
                    _needsCacheUpdate = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error consuming item {item.Name}: {ex.Message}");
                }
            }
        }

        void EquipItem(ItemBehaviour item)
        {
            if ((item?.Armor != null || item?.Weapon != null) && _hero != null)
            {
                try
                {
                    _hero.Equip(item);
                    _needsCacheUpdate = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error equipping item {item.Name}: {ex.Message}");
                }
            }
        }

        void DrawItemTypeLabel(ItemBehaviour item)
        {
            Color originalColor = GUI.color;
            string typeLabel = "";

            if (item.Weapon != null)
            {
                GUI.color = new Color(1f, 0.3f, 0.3f);
                typeLabel = "[Weapon]";
            }
            else if (item.Armor != null)
            {
                GUI.color = new Color(0.3f, 0.6f, 1f);
                typeLabel = "[Armor]";
            }
            else if (item.Consumable != null)
            {
                GUI.color = new Color(0.3f, 1f, 0.3f);
                typeLabel = "[Consumable]";
            }
            else if (item.SpellScroll != null)
            {
                GUI.color = new Color(1f, 0.8f, 0.2f);
                typeLabel = "[Scroll]";
            }
            else if (item.name.Contains("Book") && item.SpellBook != null)
            {
                GUI.color = new Color(0.8f, 0.4f, 1f);
                typeLabel = "[SpellBook]";
            }
            else
            {
                GUI.color = Color.gray;
                typeLabel = "[Item]";
            }

            GUILayout.Label(typeLabel, GUILayout.Width(100));
            GUI.color = originalColor;
        }

        void DrawItemSpecificInfo(ItemBehaviour item)
        {
            if (item.Weapon != null)
            {
                GUILayout.Label($"DPS: {item.Weapon.DamagePerSecond:F1}");
            }

            if (item.Armor != null)
            {
                GUILayout.Label($"Armor: {item.Armor.Armor}");
            }

            if (item.Consumable != null)
            {
                if (item.LiquidContainer != null)
                {
                    int healthAmount = item.LiquidContainer.GetResourceAmount(ResourceType.Health);
                    if (healthAmount > 0)
                    {
                        GUILayout.Label($"Health: +{healthAmount}");
                    }

                    int manaAmount = item.LiquidContainer.GetResourceAmount(ResourceType.Mana);
                    if (manaAmount > 0)
                    {
                        GUILayout.Label($"Mana: +{manaAmount}");
                    }
                }


            }

            if (item.SpellScroll != null)
            {
                GUILayout.Label($"Type: SpellScroll");
            }

            if (item.name.ToLower().Contains("book"))
            {
                GUILayout.Label($"Type: SpellBook");
            }

            //check for gold value
            if (item.GoldValue > 0)
            {
                GUILayout.Label($"Gold Value: {item.GoldValue}");
            }


            // Display skill requirements if present
            if (item?.Equipable != null)
            {
                if (item.Equipable.RequiredStrength > 0)
                {
                    GUILayout.Label($"Required Strength: {item.Equipable.RequiredStrength}");

                }

                if (item.Equipable.RequiredDexterity > 0)
                {
                    GUILayout.Label($"Required Dexterity: {item.Equipable.RequiredDexterity}");
                }

                if (item.Equipable.RequiredDexterity > 0)
                {
                    GUILayout.Label($"Required Intelligence: {item.Equipable.RequiredDexterity}");
                }
            }
        }

        Color GetRarityColor(string rarityName)
        {
            return rarityName?.ToLower() switch
            {
                "common" => Color.white,
                "uncommon" => Color.green,
                "rare" => Color.blue,
                "epic" => new Color(0.6f, 0.2f, 0.8f),
                "legendary" => new Color(1f, 0.5f, 0f),
                "mythic" => Color.red,
                _ => Color.gray
            };
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
