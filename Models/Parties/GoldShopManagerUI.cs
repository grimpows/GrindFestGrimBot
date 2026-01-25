using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public class GoldShopManagerUI
    {
        private GoldShopManager _goldShopManager;
        private KeyCode _toggleShowKey;
        private bool _isShow = false;
        private Vector2 _scrollPosition = Vector2.zero;

        // requied rects
        private Rect _goldShopManagerUIRect = new Rect(100, 100, 800, 800);
        private const int SHOP_BUTTON_SIZE = 50;

        private const int HEADER_HEIGHT = 30;
        private const int BUTTON_HEIGHT = 25;
        private const int ITEM_HEIGHT = 60;
        private const int ITEM_PADDING = 5;

        public GoldShopManagerUI(GoldShopManager goldShopManager,KeyCode toggleShowKey)
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

            DrawShopButton();

            if (_isShow)
            {
                _goldShopManagerUIRect = GUI.Window(WindowsConstants.GOLD_SHOP_MANAGER_UI_ID, _goldShopManagerUIRect, DrawGoldShopManagerUI, "Gold Shop Manager");
            }
        }

        void DrawShopButton()
        {
            float screenHeight = Screen.height;
            float buttonX = 10;
            float buttonY = screenHeight - SHOP_BUTTON_SIZE - 10;

            GUILayout.BeginArea(new Rect(buttonX, buttonY, SHOP_BUTTON_SIZE, SHOP_BUTTON_SIZE));

            Color originalBgColor = GUI.backgroundColor;
            Color originalTextColor = GUI.contentColor;

            if (_isShow)
            {
                GUI.backgroundColor = new Color(0.15f, 0.7f, 0.15f);
            }
            else
            {
                GUI.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
            }

            GUI.contentColor = Color.white;

            GUIStyle squareButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            if (GUILayout.Button("$", squareButtonStyle, GUILayout.Width(SHOP_BUTTON_SIZE), GUILayout.Height(SHOP_BUTTON_SIZE)))
            {
                _isShow = !_isShow;
            }

            GUI.backgroundColor = originalBgColor;
            GUI.contentColor = originalTextColor;

            GUILayout.EndArea();
        }

        void DrawGoldShopManagerUI(int windowID)
        {
            GUILayout.BeginVertical();

            DrawHeaderWithToggleButton();

            DrawEnableToggle();

            DrawItemsList();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, _goldShopManagerUIRect.width, HEADER_HEIGHT));
        }

        void DrawHeaderWithToggleButton()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);

            GUILayout.Label("Gold Shop Manager", GUILayout.ExpandWidth(true));

            Color originalBgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.5f, 0.2f);

            if (GUILayout.Button("Close", GUILayout.Width(80), GUILayout.Height(BUTTON_HEIGHT)))
            {
                _isShow = false;
            }

            GUI.backgroundColor = originalBgColor;

            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        void DrawEnableToggle()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);

            var actualGold = _goldShopManager.Party?.Gold;
            var actualSouls = _goldShopManager.Party?.Souls;
            var actualGems = _goldShopManager.Party?.Gems;
            

            _goldShopManager.IsEnabled = GUILayout.Toggle(_goldShopManager.IsEnabled, "Enable Auto Buy", GUILayout.Width(150));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.Label($"Current Resources: {actualGold} Gold, {actualSouls} Souls, {actualGems} Gems", GUI.skin.box);

            GUILayout.Space(5);
        }

        void DrawItemsList()
        {
            GUILayout.Label("Gold Shop Items", GUI.skin.box);

            if (_goldShopManager.AutoBuyGoldShopItemSelection == null || _goldShopManager.AutoBuyGoldShopItemSelection.Count == 0)
            {
                GUILayout.Label("No items available");
                return;
            }

            float scrollViewHeight = _goldShopManagerUIRect.height - HEADER_HEIGHT - 150;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(scrollViewHeight));

            foreach (var itemKey in _goldShopManager.AutoBuyGoldShopItemSelection.Keys.ToList())
            {
                bool isEnabled = _goldShopManager.AutoBuyGoldShopItemSelection[itemKey];

                var goldShopItems = _goldShopManager?.Party?.GetGoldShopItems();
                var item = goldShopItems?.FirstOrDefault(i => i.Name == itemKey);

                string itemName = $"{itemKey} ({item.GoldPrice} Gold, {item.SoulPrice} Souls, {item.GemPrice} Gems)";

                DrawGoldShopItem(itemName, ref isEnabled);

                _goldShopManager.AutoBuyGoldShopItemSelection[itemKey] = isEnabled;
            }

            GUILayout.EndScrollView();
        }

        void DrawGoldShopItem(string itemName, ref bool isEnabled)
        {
            GUILayout.BeginHorizontal(GUI.skin.box);

            Color toggleColor = isEnabled ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.6f, 0.6f, 0.6f);
            Color originalBgColor = GUI.backgroundColor;
            GUI.backgroundColor = toggleColor;

            isEnabled = GUILayout.Toggle(isEnabled, "", GUILayout.Width(25), GUILayout.Height(25));

            GUI.backgroundColor = originalBgColor;

            GUILayout.Label($"{itemName}", GUI.skin.button, GUILayout.ExpandWidth(true), GUILayout.Height(25));

            GUILayout.EndHorizontal();

            GUILayout.Space(ITEM_PADDING);
        }
    }
}
