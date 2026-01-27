using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public class QuickAccessUI
    {
        private const int BUTTON_SIZE = 50;
        private const int BUTTON_SPACING = 8;

        // References to UI panels
        private HeroUI _heroUI;
        private InventoryUI _inventoryUI;
        private SkillUI _skillUI;
        private Bot _bot;

        // Button textures (initialized once)
        private bool _texturesInitialized = false;
        private Texture2D _normalTexture;
        private Texture2D _hoverTexture;
        private Texture2D _activeTexture;
        private Texture2D _activeHoverTexture;

        // Colors - Unified dark theme
        private Color _normalColor = new Color(0.15f, 0.15f, 0.18f, 0.95f);
        private Color _hoverColor = new Color(0.22f, 0.22f, 0.26f, 0.95f);
        private Color _activeColor = new Color(0.25f, 0.45f, 0.65f, 0.95f);
        private Color _activeHoverColor = new Color(0.3f, 0.52f, 0.72f, 0.95f);
        private Color _textColor = new Color(0.85f, 0.85f, 0.88f, 1f);
        private Color _textActiveColor = new Color(1f, 1f, 1f, 1f);
        private Color _accentColor = new Color(0.95f, 0.75f, 0.3f, 1f);

        public void OnStart(HeroUI heroUI, InventoryUI inventoryUI, SkillUI skillUI, Bot bot)
        {
            _heroUI = heroUI;
            _inventoryUI = inventoryUI;
            _skillUI = skillUI;
            _bot = bot;
        }

        private void InitializeTextures()
        {
            if (_texturesInitialized) return;

            _normalTexture = CreateTexture(_normalColor);
            _hoverTexture = CreateTexture(_hoverColor);
            _activeTexture = CreateTexture(_activeColor);
            _activeHoverTexture = CreateTexture(_activeHoverColor);

            _texturesInitialized = true;
        }

        private Texture2D CreateTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        public void OnGUI()
        {
            InitializeTextures();

            float screenHeight = Screen.height;
            float startX = 100;
            float buttonY = screenHeight - BUTTON_SIZE - 10;

            // GoldShop button is at startX (position 0)
            // Our buttons start after it
            float currentX = startX + BUTTON_SIZE + BUTTON_SPACING;

            // Hero UI Button (C for Character)
            bool heroActive = _heroUI?.IsVisible ?? false;
            DrawQuickButton(currentX, buttonY, "C", heroActive, () =>
            {
                _heroUI?.ToggleVisible();
            });
            currentX += BUTTON_SIZE + BUTTON_SPACING;

            // Inventory UI Button (I)
            bool invActive = _inventoryUI?.IsVisible ?? false;
            DrawQuickButton(currentX, buttonY, "I", invActive, () =>
            {
                _inventoryUI?.ToggleVisible();
            });
            currentX += BUTTON_SIZE + BUTTON_SPACING;

            // Skill UI Button (S)
            bool skillActive = _skillUI?.IsVisible ?? false;
            DrawQuickButton(currentX, buttonY, "S", skillActive, () =>
            {
                _skillUI?.ToggleVisible();
            });
            currentX += BUTTON_SIZE + BUTTON_SPACING;

            // Bot UI Button (B)
            bool botActive = _bot?.IsUIVisible ?? false;
            DrawQuickButton(currentX, buttonY, "B", botActive, () =>
            {
                _bot?.ToggleUIVisible();
            });
        }

        private void DrawQuickButton(float x, float y, string icon, bool isActive, Action onClick)
        {
            Rect buttonRect = new Rect(x, y, BUTTON_SIZE, BUTTON_SIZE);
            bool isHovered = buttonRect.Contains(Event.current.mousePosition);

            // Create button style
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 22;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;

            // Text color
            Color textColor = isActive ? _textActiveColor : _textColor;
            buttonStyle.normal.textColor = textColor;
            buttonStyle.hover.textColor = _textActiveColor;
            buttonStyle.active.textColor = _textActiveColor;
            buttonStyle.focused.textColor = textColor;

            // Background based on state
            if (isActive)
            {
                buttonStyle.normal.background = _activeTexture;
                buttonStyle.hover.background = _activeHoverTexture;
                buttonStyle.active.background = _activeTexture;
                buttonStyle.focused.background = _activeTexture;
            }
            else
            {
                buttonStyle.normal.background = _normalTexture;
                buttonStyle.hover.background = _hoverTexture;
                buttonStyle.active.background = _normalTexture;
                buttonStyle.focused.background = _normalTexture;
            }

            // Draw button
            if (GUI.Button(buttonRect, icon, buttonStyle))
            {
                onClick?.Invoke();
            }

            // Draw accent border when active
            if (isActive)
            {
                DrawButtonBorder(buttonRect, _accentColor);
            }
        }

        private void DrawButtonBorder(Rect rect, Color color)
        {
            Texture2D borderTex = CreateTexture(color);
            float borderWidth = 2f;

            // Top
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, borderWidth), borderTex);
            // Bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - borderWidth, rect.width, borderWidth), borderTex);
            // Left
            GUI.DrawTexture(new Rect(rect.x, rect.y, borderWidth, rect.height), borderTex);
            // Right
            GUI.DrawTexture(new Rect(rect.x + rect.width - borderWidth, rect.y, borderWidth, rect.height), borderTex);
        }
    }
}
