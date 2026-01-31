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
        // References to UI panels
        private HeroUI _heroUI;
        private InventoryUI _inventoryUI;
        private SkillUI _skillUI;
        private Bot _bot;

        public void OnStart(HeroUI heroUI, InventoryUI inventoryUI, SkillUI skillUI, Bot bot)
        {
            _heroUI = heroUI;
            _inventoryUI = inventoryUI;
            _skillUI = skillUI;
            _bot = bot;
        }

        public void OnGUI()
        {
            float screenHeight = Screen.height;
            float startX = 100;
            float buttonY = screenHeight - UITheme.BUTTON_SIZE - 10;

            // GoldShop button is at startX (position 0)
            // Our buttons start after it
            float currentX = startX;

            // Hero UI Button (C for Character)
            bool heroActive = _heroUI?.IsVisible ?? false;
            DrawQuickButton(currentX, buttonY, "C", heroActive, () => _heroUI?.ToggleVisible());
            currentX += UITheme.BUTTON_SIZE + UITheme.BUTTON_SPACING;

            // Inventory UI Button (I)
            bool invActive = _inventoryUI?.IsVisible ?? false;
            DrawQuickButton(currentX, buttonY, "I", invActive, () => _inventoryUI?.ToggleVisible());
            currentX += UITheme.BUTTON_SIZE + UITheme.BUTTON_SPACING;

            // Skill UI Button (S)
            bool skillActive = _skillUI?.IsVisible ?? false;
            DrawQuickButton(currentX, buttonY, "S", skillActive, () => _skillUI?.ToggleVisible());
            currentX += UITheme.BUTTON_SIZE + UITheme.BUTTON_SPACING;

            // Bot UI Button (B)
            bool botActive = _bot?.IsUIVisible ?? false;
            DrawQuickButton(currentX, buttonY, "B", botActive, () => _bot?.ToggleUIVisible());
        }

        private void DrawQuickButton(float x, float y, string icon, bool isActive, Action onClick)
        {
            Rect buttonRect = new Rect(x, y, UITheme.BUTTON_SIZE, UITheme.BUTTON_SIZE);

            // Create button style
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = UITheme.FONT_SIZE_BUTTON_LARGE;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;

            // Text color
            Color textColor = isActive ? UITheme.TextLight : UITheme.TextLight;
            buttonStyle.normal.textColor = textColor;
            buttonStyle.hover.textColor = UITheme.TextLight;
            buttonStyle.active.textColor = UITheme.TextLight;
            buttonStyle.focused.textColor = textColor;

            // Background based on state
            if (isActive)
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

            // Draw button
            if (GUI.Button(buttonRect, icon, buttonStyle))
            {
                onClick?.Invoke();
            }

            // Draw accent border when active
            if (isActive)
            {
                UITheme.DrawBorder(buttonRect, UITheme.Accent);
            }
        }
    }
}
