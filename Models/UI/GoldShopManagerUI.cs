using GrindFest;
using System;
using System.Linq;
using UnityEngine;

namespace Scripts.Models
{
    public class GoldShopManagerUI
    {
        private GoldShopManager _goldShopManager;
        private KeyCode _toggleShowKey;
        private bool _isShow = true;
        private Vector2 _scrollPosition = Vector2.zero;

        private Rect _windowRect = new Rect(100, 100, 650, 600);

        public GoldShopManagerUI(GoldShopManager goldShopManager, KeyCode toggleShowKey)
        {
            _toggleShowKey = toggleShowKey;
            _goldShopManager = goldShopManager;
        }

        public void OnGUI()
        {
            Event e = Event.current;

            if (e.type == EventType.KeyDown && e.keyCode == _toggleShowKey)
            {
                _isShow = !_isShow;
                e.Use();
            }

            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;

            DrawShopButton();

            if (_isShow)
            {
                _windowRect = GUI.Window(GLOBALS.WINDOWS.GOLD_SHOP_MANAGER_WINDOW_INFO.ID, _windowRect, DrawWindow, "", UITheme.WindowStyle);
            }
        }

        void DrawShopButton()
        {
            float screenHeight = Screen.height;
            float buttonX = 100;
            float buttonY = screenHeight - 2 * UITheme.BUTTON_SIZE - 20;

            Rect buttonRect = new Rect(buttonX, buttonY, UITheme.BUTTON_SIZE, UITheme.BUTTON_SIZE);


            var originalBgColor = GUI.backgroundColor;
            var originalCntColor = GUI.contentColor;

            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = UITheme.FONT_SIZE_BUTTON_LARGE;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;

            // Text color
            Color textColor = _isShow ? UITheme.TextLight : UITheme.TextLight;
            buttonStyle.normal.textColor = textColor;
            buttonStyle.hover.textColor = UITheme.TextLight;
            buttonStyle.active.textColor = UITheme.TextLight;
            buttonStyle.focused.textColor = textColor;

            if (_isShow)
            {
                buttonStyle.normal.background = UITheme.ButtonActiveTexture;
                buttonStyle.hover.background = UITheme.ButtonActiveHoverTexture;
                buttonStyle.active.background = UITheme.ButtonActiveTexture;
                buttonStyle.focused.background = UITheme.ButtonActiveTexture;
            }
            else
            {
                buttonStyle.normal.background = UITheme.ButtonNormalTexture;
                buttonStyle.hover.background = UITheme.ButtonHoverTexture;
                buttonStyle.active.background = UITheme.ButtonNormalTexture;
                buttonStyle.focused.background = UITheme.ButtonNormalTexture;
            }

            if (GUI.Button(buttonRect, "$", buttonStyle))
            {
                _isShow = !_isShow;
            }

            GUI.backgroundColor = originalBgColor;
            GUI.contentColor = originalCntColor;

        }

        void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            DrawHeader();
            GUILayout.Space(UITheme.SECTION_SPACING);

            DrawResourcesPanel();
            GUILayout.Space(UITheme.SECTION_SPACING);

            DrawAutoBuyToggle();
            GUILayout.Space(UITheme.SECTION_SPACING);

            DrawItemsList();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 30));
        }

        void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("GOLD SHOP", UITheme.TitleStyle, GUILayout.Height(UITheme.HEADER_HEIGHT));
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = UITheme.Danger;
            if (GUILayout.Button("X", UITheme.CloseButtonStyle, GUILayout.Width(UITheme.CLOSE_BUTTON_SIZE), GUILayout.Height(25)))
            {
                _isShow = false;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();

            UITheme.DrawSeparator();
        }

        void DrawResourcesPanel()
        {
            var actualGold = _goldShopManager.Party?.Gold ?? 0;
            var actualSouls = _goldShopManager.Party?.Souls ?? 0;
            var actualGems = _goldShopManager.Party?.Gems ?? 0;

            GUILayout.BeginHorizontal();
            DrawResourceBox("Gold", actualGold.ToString(), UITheme.Gold);
            GUILayout.Space(UITheme.SECTION_SPACING);
            DrawResourceBox("Souls", actualSouls.ToString(), UITheme.Soul);
            GUILayout.Space(UITheme.SECTION_SPACING);
            DrawResourceBox("Gems", actualGems.ToString(), UITheme.Gem);
            GUILayout.EndHorizontal();
        }

        void DrawResourceBox(string label, string value, Color color)
        {
            GUILayout.BeginVertical(UITheme.CardStyle, GUILayout.ExpandWidth(true), GUILayout.Height(50));

            GUILayout.Label(label, UITheme.LabelStyle);
            GUILayout.Label(value, UITheme.CreateValueStyle(color, 16));

            GUILayout.EndVertical();
        }

        void DrawAutoBuyToggle()
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle);

            GUILayout.Label("Auto Buy", UITheme.SubtitleStyle, GUILayout.Width(100));
            GUILayout.FlexibleSpace();

            GUIStyle toggleStyle = _goldShopManager.IsEnabled ? UITheme.ToggleOnStyle : UITheme.ToggleOffStyle;
            string toggleText = _goldShopManager.IsEnabled ? "ENABLED" : "DISABLED";

            GUI.backgroundColor = _goldShopManager.IsEnabled ? UITheme.Positive : UITheme.TextMuted;
            if (GUILayout.Button(toggleText, toggleStyle, GUILayout.Width(100), GUILayout.Height(25)))
            {
                _goldShopManager.IsEnabled = !_goldShopManager.IsEnabled;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();
        }

        void DrawItemsList()
        {
            GUILayout.Label(" (AUTO) | SHOP ITEMS", UITheme.SubtitleStyle, GUILayout.Height(25));
            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.BUTTON_SPACING);

            if (_goldShopManager.AutoBuyGoldShopItemSelection == null || _goldShopManager.AutoBuyGoldShopItemSelection.Count == 0)
            {
                GUILayout.Label("No items available", UITheme.LabelStyle);
                return;
            }

            float scrollViewHeight = _windowRect.height - 250;
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(scrollViewHeight));

            var goldShopItems = _goldShopManager?.Party?.GetGoldShopItems();

            foreach (var itemKey in _goldShopManager.AutoBuyGoldShopItemSelection.Keys.ToList())
            {
                bool isEnabled = _goldShopManager.AutoBuyGoldShopItemSelection[itemKey];
                var item = goldShopItems?.FirstOrDefault(i => i.Name == itemKey);

                if (item != null)
                {
                    DrawShopItem(itemKey, item, ref isEnabled);
                    _goldShopManager.AutoBuyGoldShopItemSelection[itemKey] = isEnabled;
                }
            }

            GUILayout.EndScrollView();
        }

        void DrawShopItem(string itemKey, GoldShopItem item, ref bool isEnabled)
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle, GUILayout.Height(50));

            // Toggle button
            GUI.backgroundColor = isEnabled ? UITheme.Positive : UITheme.TextMuted;
            GUIStyle toggleBtnStyle = isEnabled ? UITheme.ToggleOnStyle : UITheme.ToggleOffStyle;

            if (GUILayout.Button(isEnabled ? "ON" : "OFF", toggleBtnStyle, GUILayout.Width(45), GUILayout.Height(35)))
            {
                isEnabled = !isEnabled;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(UITheme.SECTION_SPACING);

            // Item info
            GUILayout.BeginVertical();

            GUILayout.Label(itemKey, UITheme.SubtitleStyle);

            GUILayout.BeginHorizontal();

            if (item.GoldPrice > 0)
                GUILayout.Label($"{item.GoldPrice}g", UITheme.CreateLabelStyle(UITheme.Gold), GUILayout.Width(60));
            if (item.SoulPrice > 0)
                GUILayout.Label($"{item.SoulPrice}s", UITheme.CreateLabelStyle(UITheme.Soul), GUILayout.Width(60));
            if (item.GemPrice > 0)
                GUILayout.Label($"{item.GemPrice}gem", UITheme.CreateLabelStyle(UITheme.Gem), GUILayout.Width(60));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(UITheme.SECTION_SPACING);

            // Buy button
            GUIStyle buyStyle = new GUIStyle(UITheme.ButtonStyle);
            bool canAfford = _goldShopManager.CanAffordItem(item);

            buyStyle.normal.background = UITheme.BarBgTexture;
            buyStyle.hover.background = UITheme.BarBgTexture;

            buyStyle.normal.textColor = UITheme.Gold;
            buyStyle.hover.textColor = UITheme.Positive;

            GUI.enabled = canAfford;
            if (GUILayout.Button("Buy", buyStyle, GUILayout.Width(50), GUILayout.Height(35)))
            {
                try
                {
                    _goldShopManager.Party?.BuyFromGoldShop(itemKey);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error buying item {itemKey}: {ex.Message}");
                }
            }

            GUI.enabled = true;
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
    }
}
