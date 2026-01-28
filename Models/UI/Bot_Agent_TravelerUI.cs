using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public class Bot_Agent_TravelerUI
    {
        private Bot_Agent_Traveler _travelerAgent;
        private bool _stylesInitialized = false;

        // GUI Styles
        private GUIStyle _sectionStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _valueStyle;
        private GUIStyle _inputStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _toggleOnStyle;
        private GUIStyle _toggleOffStyle;
        private GUIStyle _listItemStyle;

        // Textures
        private Texture2D _sectionBgTexture;
        private Texture2D _inputBgTexture;
        private Texture2D _toggleOnTexture;
        private Texture2D _toggleOffTexture;
        private Texture2D _barFillTexture;

        // Colors
        private Color _sectionColor = new Color(0.1f, 0.1f, 0.12f, 0.95f);
        private Color _accentColor = new Color(0.95f, 0.75f, 0.3f, 1f);
        private Color _positiveColor = new Color(0.4f, 1f, 0.55f, 1f);
        private Color _warningColor = new Color(1f, 0.7f, 0.3f, 1f);
        private Color _dangerColor = new Color(1f, 0.4f, 0.4f, 1f);
        private Color _infoColor = new Color(0.4f, 0.7f, 1f, 1f);
        private Color _textLightColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        private Color _textMutedColor = new Color(0.65f, 0.65f, 0.7f, 1f);

        public Bot_Agent_TravelerUI(Bot_Agent_Traveler travelerAgent)
        {
            _travelerAgent = travelerAgent;
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _sectionBgTexture = CreateTexture(_sectionColor);
            _inputBgTexture = CreateTexture(new Color(0.15f, 0.15f, 0.18f, 0.95f));
            _toggleOnTexture = CreateTexture(new Color(0.2f, 0.6f, 0.3f, 0.95f));
            _toggleOffTexture = CreateTexture(new Color(0.25f, 0.25f, 0.3f, 0.9f));
            _barFillTexture = CreateTexture(Color.white);

            _sectionStyle = new GUIStyle(GUI.skin.box);
            _sectionStyle.normal.background = _sectionBgTexture;
            _sectionStyle.padding = new RectOffset(12, 12, 10, 10);

            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.fontSize = 12;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.normal.textColor = _textLightColor;

            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = 11;
            _labelStyle.normal.textColor = _textMutedColor;

            _valueStyle = new GUIStyle(GUI.skin.label);
            _valueStyle.fontSize = 11;
            _valueStyle.fontStyle = FontStyle.Bold;
            _valueStyle.normal.textColor = _textLightColor;

            _inputStyle = new GUIStyle(GUI.skin.textField);
            _inputStyle.normal.background = _inputBgTexture;
            _inputStyle.normal.textColor = _textLightColor;
            _inputStyle.fontSize = 11;
            _inputStyle.alignment = TextAnchor.MiddleCenter;

            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = 10;
            _buttonStyle.fontStyle = FontStyle.Bold;

            _toggleOnStyle = new GUIStyle(GUI.skin.button);
            _toggleOnStyle.normal.background = _toggleOnTexture;
            _toggleOnStyle.normal.textColor = _textLightColor;
            _toggleOnStyle.fontStyle = FontStyle.Bold;
            _toggleOnStyle.fontSize = 10;

            _toggleOffStyle = new GUIStyle(GUI.skin.button);
            _toggleOffStyle.normal.background = _toggleOffTexture;
            _toggleOffStyle.normal.textColor = _textMutedColor;
            _toggleOffStyle.fontSize = 10;

            _listItemStyle = new GUIStyle(GUI.skin.label);
            _listItemStyle.fontSize = 10;
            _listItemStyle.normal.textColor = _textMutedColor;

            _stylesInitialized = true;
        }

        private Texture2D CreateTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        public void DrawTravelerAgentPanel(Rect contentArea)
        {
            if (_travelerAgent == null)
                return;

            InitializeStyles();

            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(_sectionStyle);

            // Header
            DrawSectionHeader("TRAVELER AGENT");
            GUILayout.Space(8);

            // Stats Row
            GUILayout.BeginHorizontal();
            //DrawStatBox("Looted", _pickUpAgent.LootedItemCount.ToString(), _positiveColor);
            //GUILayout.Space(8);
            //DrawStatBox("Scanned", _pickUpAgent.ScannedItems.Count.ToString(), _infoColor);
            //GUILayout.Space(8);
            //DrawStatBox("Ignored", _pickUpAgent.IgnoredItems.Count.ToString(), _warningColor);
            GUILayout.EndHorizontal();
            GUILayout.Space(12);

            // Target Item
            //DrawTargetSection();
            GUILayout.Space(10);

            // Settings
            //DrawSettingsSection();
            GUILayout.Space(10);

            // Ignored Items List
            //DrawIgnoredItemsSection();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();


        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Label(title, _titleStyle);
            Rect sepRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1));
            GUI.color = _accentColor;
            GUI.DrawTexture(sepRect, _barFillTexture);
            GUI.color = Color.white;
        }
    }
}
