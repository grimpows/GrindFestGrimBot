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
        private bool _stylesInitialized = false;

        private Rect _windowRect = new Rect(100, 100, 650, 600);
        private const int SHOP_BUTTON_SIZE = 50;

        // GUI Styles
        private GUIStyle _windowStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _valueStyle;
        private GUIStyle _itemCardStyle;
        private GUIStyle _toggleOnStyle;
        private GUIStyle _toggleOffStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _resourceBoxStyle;
        private GUIStyle _buyButtonStyle;

        // Textures
        private Texture2D _windowBgTexture;
        private Texture2D _cardBgTexture;
        private Texture2D _toggleOnTexture;
        private Texture2D _toggleOffTexture;
        private Texture2D _barFillTexture;

        // Colors
        private Color _windowBgColor = new Color(0.08f, 0.08f, 0.1f, 0.98f);
        private Color _cardColor = new Color(0.12f, 0.12f, 0.15f, 0.95f);
        private Color _accentColor = new Color(0.95f, 0.75f, 0.3f, 1f);
        private Color _goldColor = new Color(1f, 0.85f, 0.3f, 1f);
        private Color _soulColor = new Color(0.6f, 0.4f, 1f, 1f);
        private Color _gemColor = new Color(0.3f, 0.9f, 0.7f, 1f);
        private Color _positiveColor = new Color(0.4f, 1f, 0.55f, 1f);
        private Color _negativeColor = new Color(1f, 0.4f, 0.4f, 1f);
        private Color _textLightColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        private Color _textMutedColor = new Color(0.6f, 0.6f, 0.65f, 1f);

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

            if (!_stylesInitialized)
            {
                InitializeStyles();
                _stylesInitialized = true;
            }

            DrawShopButton();

            if (_isShow)
            {
                
                _windowRect = GUI.Window(WindowsConstants.GOLD_SHOP_MANAGER_UI_ID, _windowRect, DrawWindow, "", _windowStyle);
            }
        }

        private void InitializeStyles()
        {
            // Textures
            _windowBgTexture = CreateTexture(_windowBgColor);
            _cardBgTexture = CreateTexture(_cardColor);
            _toggleOnTexture = CreateTexture(new Color(0.2f, 0.6f, 0.3f, 0.95f));
            _toggleOffTexture = CreateTexture(new Color(0.25f, 0.25f, 0.3f, 0.9f));
            _barFillTexture = CreateTexture(Color.white);

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
            _titleStyle.fontSize = 18;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.normal.textColor = _accentColor;

            // Subtitle Style
            _subtitleStyle = new GUIStyle(GUI.skin.label);
            _subtitleStyle.fontSize = 13;
            _subtitleStyle.fontStyle = FontStyle.Bold;
            _subtitleStyle.alignment = TextAnchor.MiddleLeft;
            _subtitleStyle.normal.textColor = _textLightColor;

            // Label Style
            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = 11;
            _labelStyle.alignment = TextAnchor.MiddleLeft;
            _labelStyle.normal.textColor = _goldColor;

            // Value Style
            _valueStyle = new GUIStyle(GUI.skin.label);
            _valueStyle.fontSize = 11;
            _valueStyle.fontStyle = FontStyle.Bold;
            _valueStyle.alignment = TextAnchor.MiddleRight;
            _valueStyle.normal.textColor = _textLightColor;

            // Item Card Style
            _itemCardStyle = new GUIStyle(GUI.skin.box);
            _itemCardStyle.normal.background = _cardBgTexture;
            _itemCardStyle.padding = new RectOffset(10, 10, 8, 8);

            // Toggle Styles
            _toggleOnStyle = new GUIStyle(GUI.skin.button);
            _toggleOnStyle.normal.background = _toggleOnTexture;
            _toggleOnStyle.normal.textColor = _textLightColor;
            _toggleOnStyle.fontStyle = FontStyle.Bold;
            _toggleOnStyle.fontSize = 10;

            _toggleOffStyle = new GUIStyle(GUI.skin.button);
            _toggleOffStyle.normal.background = _toggleOffTexture;
            _toggleOffStyle.normal.textColor = _textMutedColor;
            _toggleOffStyle.fontSize = 10;

            // Button Style
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = 11;
            _buttonStyle.fontStyle = FontStyle.Bold;

            // Resource Box Style
            _resourceBoxStyle = new GUIStyle(GUI.skin.box);
            _resourceBoxStyle.normal.background = _cardBgTexture;
            _resourceBoxStyle.padding = new RectOffset(8, 8, 5, 5);


            // Ressource Buy Button Style, in gold base color and white text
            _buyButtonStyle = new GUIStyle(GUI.skin.button);
            _buyButtonStyle.fontSize = 12;
            _buyButtonStyle.fontStyle = FontStyle.Bold;
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.hover.textColor = _goldColor;




        }

        private Texture2D CreateTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        void DrawShopButton()
        {
            float screenHeight = Screen.height;
            float buttonX = 100;
            float buttonY = screenHeight - SHOP_BUTTON_SIZE - 10;

            Rect buttonRect = new Rect(buttonX, buttonY, SHOP_BUTTON_SIZE, SHOP_BUTTON_SIZE);

            Color originalBgColor = GUI.backgroundColor;
            Color orignalCntColor = GUI.contentColor;
            GUI.backgroundColor = _isShow ? _positiveColor : _goldColor;
            GUI.contentColor = _isShow ? _accentColor : Color.white;


            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 22;
            buttonStyle.fontStyle = FontStyle.Bold;

            if (GUI.Button(buttonRect, "$", buttonStyle))
            {
                _isShow = !_isShow;
            }

            GUI.backgroundColor = originalBgColor;
        }

        void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            DrawHeader();
            GUILayout.Space(10);

            DrawResourcesPanel();
            GUILayout.Space(10);

            DrawAutoBuyToggle();
            GUILayout.Space(10);

            DrawItemsList();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 30));
        }

        void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("GOLD SHOP", _titleStyle, GUILayout.Height(30));
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = _negativeColor;
            if (GUILayout.Button("X", _buttonStyle, GUILayout.Width(30), GUILayout.Height(25)))
            {
                _isShow = false;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();

            // Separator
            Rect sepRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(2));
            GUI.color = _accentColor;
            GUI.DrawTexture(sepRect, _barFillTexture);
            GUI.color = Color.white;
        }

        void DrawResourcesPanel()
        {
            var actualGold = _goldShopManager.Party?.Gold ?? 0;
            var actualSouls = _goldShopManager.Party?.Souls ?? 0;
            var actualGems = _goldShopManager.Party?.Gems ?? 0;

            GUILayout.BeginHorizontal();

            // Gold
            DrawResourceBox("Gold", actualGold.ToString(), _goldColor);
            GUILayout.Space(10);

            // Souls
            DrawResourceBox("Souls", actualSouls.ToString(), _soulColor);
            GUILayout.Space(10);

            // Gems
            DrawResourceBox("Gems", actualGems.ToString(), _gemColor);

            GUILayout.EndHorizontal();
        }

        void DrawResourceBox(string label, string value, Color color)
        {
            GUILayout.BeginVertical(_resourceBoxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(50));
            
            GUILayout.Label(label, _labelStyle);
            
            GUIStyle valueStyle = new GUIStyle(_valueStyle);
            valueStyle.fontSize = 16;
            valueStyle.alignment = TextAnchor.MiddleCenter;
            valueStyle.normal.textColor = color;
            
            GUILayout.Label(value, valueStyle);
            
            GUILayout.EndVertical();
        }

        void DrawAutoBuyToggle()
        {
            GUILayout.BeginHorizontal(_itemCardStyle);

            GUILayout.Label("Auto Buy", _subtitleStyle, GUILayout.Width(100));
            GUILayout.FlexibleSpace();

            GUIStyle toggleStyle = _goldShopManager.IsEnabled ? _toggleOnStyle : _toggleOffStyle;
            string toggleText = _goldShopManager.IsEnabled ? "ENABLED" : "DISABLED";
            
            GUI.backgroundColor = _goldShopManager.IsEnabled ? _positiveColor : _textMutedColor;
            if (GUILayout.Button(toggleText, toggleStyle, GUILayout.Width(100), GUILayout.Height(25)))
            {
                _goldShopManager.IsEnabled = !_goldShopManager.IsEnabled;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();
        }

        void DrawItemsList()
        {
            GUILayout.Label("SHOP ITEMS", _subtitleStyle, GUILayout.Height(25));

            // Separator
            Rect sepRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1));
            GUI.color = new Color(0.3f, 0.3f, 0.35f);
            GUI.DrawTexture(sepRect, _barFillTexture);
            GUI.color = Color.white;
            GUILayout.Space(8);

            if (_goldShopManager.AutoBuyGoldShopItemSelection == null || _goldShopManager.AutoBuyGoldShopItemSelection.Count == 0)
            {
                GUILayout.Label("No items available", _labelStyle);
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
            if (item == null) return;

            GUILayout.BeginHorizontal(_itemCardStyle, GUILayout.Height(50));

            // Toggle button
            GUI.backgroundColor = isEnabled ? _positiveColor : _textMutedColor;
            GUIStyle toggleBtnStyle = isEnabled ? _toggleOnStyle : _toggleOffStyle;
            
            if (GUILayout.Button(isEnabled ? "ON" : "OFF", toggleBtnStyle, GUILayout.Width(45), GUILayout.Height(35)))
            {
                isEnabled = !isEnabled;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // Item info
            GUILayout.BeginVertical();
            
            GUILayout.Label(itemKey, _subtitleStyle);
            
            GUILayout.BeginHorizontal();
            
            // Prices with colors
            if (item.GoldPrice > 0)
            {
                GUI.color = _goldColor;
                GUILayout.Label($"{item.GoldPrice}g", _labelStyle, GUILayout.Width(60));
            }
            if (item.SoulPrice > 0)
            {
                GUI.color = _soulColor;
                GUILayout.Label($"{item.SoulPrice}s", _labelStyle, GUILayout.Width(60));
            }
            if (item.GemPrice > 0)
            {
                GUI.color = _gemColor;
                GUILayout.Label($"{item.GemPrice}gem", _labelStyle, GUILayout.Width(60));
            }
            GUI.color = Color.white;
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();

            GUILayout.Space(10);

            // Buy button
            var oldBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = _goldColor;
            bool isAffordable = _goldShopManager.CanAffordItem(item);

            GUI.enabled = isAffordable;
            if (GUILayout.Button("Buy", _buttonStyle, GUILayout.Width(50), GUILayout.Height(35)))
            {
                try
                {
                    _goldShopManager.Party?.BuyFromGoldShop(item.Name);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error buying item {itemKey}: {ex.Message}");
                }
            }
            GUI.backgroundColor = oldBackgroundColor;
            GUI.enabled = true;

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
    }
}
