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

        private InventoryTab _currentTab = InventoryTab.Equipment;
        private EquipmentSlotFilter _currentEquipmentFilter = EquipmentSlotFilter.All;

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

        public void OnStart(Hero_Base hero, KeyCode toggleShowKey, int windowID)
        {
            _hero = hero;
            _toggleShowKey = toggleShowKey;
            _windowId = windowID;
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
                _InventoryWindowRect = GUI.Window(WindowsConstants.INVENTORY_WINDOW_ID, _InventoryWindowRect, DrawInventoryWindow, "Inventory");
            }
        }

        public void OnUpdate()
        {

        }

        void DrawInventoryWindow(int windowID)
        {
            if (!_isShow || _hero?.Character?.Inventory?.Items == null)
            {
                GUI.DragWindow();
                return;
            }

            var inventory = _hero.Character.Inventory.Items;

            DrawTabs();

            if (_currentTab == InventoryTab.Equipment)
            {
                DrawEquipmentSubTabs();
            }

            float tabAreaOffset = _currentTab == InventoryTab.Equipment ? HEADER_HEIGHT + TAB_HEIGHT + (SUBTAB_HEIGHT * 2) + 10 : HEADER_HEIGHT + TAB_HEIGHT;

            var filteredItems = FilterItemsByTab(inventory, _currentTab).ToList();

            if (_currentTab == InventoryTab.Equipment)
            {
                filteredItems = FilterItemsByEquipmentSlot(filteredItems, _currentEquipmentFilter).ToList();
            }

            int itemCount = filteredItems.Count;

            GUILayout.BeginArea(new Rect(10, tabAreaOffset, _InventoryWindowRect.width - 20, _InventoryWindowRect.height - tabAreaOffset - 40));

            float scrollViewHeight = _InventoryWindowRect.height - tabAreaOffset - 70;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(_InventoryWindowRect.width - 30), GUILayout.Height(scrollViewHeight));

            int index = 0;
            foreach (var item in filteredItems)
            {
                DrawInventoryItem(item, index);
                index++;
            }

            //add a button to dorp all items in the current tab
            if (GUILayout.Button("Drop All Items in Current Tab"))
            {
                foreach (var item in filteredItems.ToList())
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

            var inventory = _hero.Character.Inventory.Items;
            int equipmentCount = FilterItemsByTab(inventory, InventoryTab.Equipment).Count();
            int consumableCount = FilterItemsByTab(inventory, InventoryTab.Consumable).Count();
            int spellbookCount = FilterItemsByTab(inventory, InventoryTab.SpellBook).Count();
            int scrollCount = FilterItemsByTab(inventory, InventoryTab.Scroll).Count();
            int miscCount = FilterItemsByTab(inventory, InventoryTab.Misc).Count();

            if (DrawTab($"Equipment ({equipmentCount})", new Rect(startX, startY, tabWidth, TAB_HEIGHT), _currentTab == InventoryTab.Equipment))
            {
                _currentTab = InventoryTab.Equipment;
                _currentEquipmentFilter = EquipmentSlotFilter.All;
                _scrollPosition = Vector2.zero;
            }

            if (DrawTab($"Consumable ({consumableCount})", new Rect(startX + tabWidth + 5, startY, tabWidth, TAB_HEIGHT), _currentTab == InventoryTab.Consumable))
            {
                _currentTab = InventoryTab.Consumable;
                _scrollPosition = Vector2.zero;
            }

            if (DrawTab($"SpellBook ({spellbookCount})", new Rect(startX + (tabWidth + 5) * 2, startY, tabWidth, TAB_HEIGHT), _currentTab == InventoryTab.SpellBook))
            {
                _currentTab = InventoryTab.SpellBook;
                _scrollPosition = Vector2.zero;
            }

            if (DrawTab($"Scroll ({scrollCount})", new Rect(startX + (tabWidth + 5) * 3, startY, tabWidth, TAB_HEIGHT), _currentTab == InventoryTab.Scroll))
            {
                _currentTab = InventoryTab.Scroll;
                _scrollPosition = Vector2.zero;
            }

            if (DrawTab($"Misc ({miscCount})", new Rect(startX + (tabWidth + 5) * 4, startY, tabWidth, TAB_HEIGHT), _currentTab == InventoryTab.Misc))
            {
                _currentTab = InventoryTab.Misc;
                _scrollPosition = Vector2.zero;
            }

            GUI.backgroundColor = originalColor;
        }

        void DrawEquipmentSubTabs()
        {
            float subTabWidth = (_InventoryWindowRect.width - 50) / 5f;
            float startX = 10;
            float startY = HEADER_HEIGHT + TAB_HEIGHT;

            Color originalColor = GUI.backgroundColor;

            var inventory = _hero.Character.Inventory.Items;
            var equipmentItems = FilterItemsByTab(inventory, InventoryTab.Equipment).ToList();

            int allCount = equipmentItems.Count();
            int headCount = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Head).Count();
            int shouldersCount = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Shoulders).Count();
            int armsCount = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Arms).Count();
            int handsCount = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Hands).Count();

            if (DrawSubTab($"All ({allCount})", new Rect(startX, startY, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.All))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.All;
                _scrollPosition = Vector2.zero;
            }

            if (DrawSubTab($"Head ({headCount})", new Rect(startX + subTabWidth + 5, startY, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Head))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Head;
                _scrollPosition = Vector2.zero;
            }

            if (DrawSubTab($"Shoulders ({shouldersCount})", new Rect(startX + (subTabWidth + 5) * 2, startY, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Shoulders))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Shoulders;
                _scrollPosition = Vector2.zero;
            }

            if (DrawSubTab($"Arms ({armsCount})", new Rect(startX + (subTabWidth + 5) * 3, startY, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Arms))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Arms;
                _scrollPosition = Vector2.zero;
            }

            if (DrawSubTab($"Hands ({handsCount})", new Rect(startX + (subTabWidth + 5) * 4, startY, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Hands))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Hands;
                _scrollPosition = Vector2.zero;
            }

            GUI.backgroundColor = originalColor;

            // Second row of sub-tabs
            int chestCount = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Chest).Count();
            int legsCount = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Legs).Count();
            int feetCount = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Feet).Count();
            int weaponsCount = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Weapons).Count();
            int accessoriesCount = FilterItemsByEquipmentSlot(equipmentItems, EquipmentSlotFilter.Accessories).Count();

            float startY2 = startY + SUBTAB_HEIGHT + 10;

            if (DrawSubTab($"Chest ({chestCount})", new Rect(startX, startY2, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Chest))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Chest;
                _scrollPosition = Vector2.zero;
            }

            if (DrawSubTab($"Legs ({legsCount})", new Rect(startX + subTabWidth + 5, startY2, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Legs))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Legs;
                _scrollPosition = Vector2.zero;
            }

            if (DrawSubTab($"Feet ({feetCount})", new Rect(startX + (subTabWidth + 5) * 2, startY2, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Feet))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Feet;
                _scrollPosition = Vector2.zero;
            }

            if (DrawSubTab($"Weapons ({weaponsCount})", new Rect(startX + (subTabWidth + 5) * 3, startY2, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Weapons))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Weapons;
                _scrollPosition = Vector2.zero;
            }

            if (DrawSubTab($"Accessories ({accessoriesCount})", new Rect(startX + (subTabWidth + 5) * 4, startY2, subTabWidth, SUBTAB_HEIGHT), _currentEquipmentFilter == EquipmentSlotFilter.Accessories))
            {
                _currentEquipmentFilter = EquipmentSlotFilter.Accessories;
                _scrollPosition = Vector2.zero;
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
