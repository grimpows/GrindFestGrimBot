using Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GrindFest;

namespace Scripts.Models
{
    internal class Bot_Agent_LooterUI
    {
        private Bot_Agent_Looter _pickUpAgent;
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

        public Bot_Agent_LooterUI(Bot_Agent_Looter pickUpAgent)
        {
            _pickUpAgent = pickUpAgent;
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

        public void DrawPickUpAgentPanel(Rect contentArea)
        {
            if (_pickUpAgent == null) return;

            InitializeStyles();

            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(_sectionStyle);

            // Header
            DrawSectionHeader("LOOTER AGENT");
            GUILayout.Space(8);

            // Stats Row
            GUILayout.BeginHorizontal();
            DrawStatBox("Looted", _pickUpAgent.LootedItemCount.ToString(), _positiveColor);
            GUILayout.Space(8);
            DrawStatBox("Scanned", _pickUpAgent.ScannedItems.Count.ToString(), _infoColor);
            GUILayout.Space(8);
            DrawStatBox("Ignored", _pickUpAgent.IgnoredItems.Count.ToString(), _warningColor);
            GUILayout.EndHorizontal();
            GUILayout.Space(12);

            // Target Item
            DrawTargetSection();
            GUILayout.Space(10);

            // Settings
            DrawSettingsSection();
            GUILayout.Space(10);

            // Ignored Items List
            DrawIgnoredItemsSection();

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

        private void DrawStatBox(string label, string value, Color valueColor)
        {
            GUILayout.BeginVertical(GUILayout.Width(80));
            GUILayout.Label(label, _labelStyle);
            
            GUIStyle valStyle = new GUIStyle(_valueStyle);
            valStyle.fontSize = 14;
            valStyle.normal.textColor = valueColor;
            GUILayout.Label(value, valStyle);
            
            GUILayout.EndVertical();
        }

        private void DrawTargetSection()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Current Target", _titleStyle);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            
            string targetName = _pickUpAgent.TargetedItem != null ? _pickUpAgent.TargetedItem.name : "None";
            Color targetColor = _pickUpAgent.TargetedItem != null ? _positiveColor : _textMutedColor;
            
            GUIStyle targetStyle = new GUIStyle(_valueStyle);
            targetStyle.normal.textColor = targetColor;
            GUILayout.Label(targetName, targetStyle, GUILayout.ExpandWidth(true));

            if (_pickUpAgent.TargetedItem != null)
            {
                GUI.backgroundColor = _dangerColor;
                if (GUILayout.Button("Reset", _buttonStyle, GUILayout.Width(60), GUILayout.Height(20)))
                {
                    _pickUpAgent.TargetedItem = null;
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.EndHorizontal();

            // Last scan time
            TimeSpan timeSinceLastScan = DateTime.Now - _pickUpAgent.LastScanTime;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Last Scan:", _labelStyle, GUILayout.Width(70));
            
            Color scanColor = timeSinceLastScan.TotalSeconds < 5 ? _positiveColor : _textMutedColor;
            GUIStyle scanStyle = new GUIStyle(_valueStyle);
            scanStyle.normal.textColor = scanColor;
            GUILayout.Label($"{timeSinceLastScan.TotalSeconds:F1}s ago", scanStyle);
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawSettingsSection()
        {
            GUILayout.Label("Settings", _titleStyle);
            GUILayout.Space(4);

            // Scan Frequency
            GUILayout.BeginHorizontal();
            GUILayout.Label("Scan Frequency:", _labelStyle, GUILayout.Width(110));
            string scanFreqStr = GUILayout.TextField(_pickUpAgent.ScanFrequency.ToString(), _inputStyle, GUILayout.Width(50));
            if (int.TryParse(scanFreqStr, out int newScanFreq) && newScanFreq > 0)
            {
                _pickUpAgent.ScanFrequency = newScanFreq;
            }
            GUILayout.Label("sec", _labelStyle, GUILayout.Width(30));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            // Target Timeout
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Timeout:", _labelStyle, GUILayout.Width(110));
            string timeoutStr = GUILayout.TextField(_pickUpAgent.TargetItemTimeout.ToString(), _inputStyle, GUILayout.Width(50));
            if (int.TryParse(timeoutStr, out int newTimeout) && newTimeout > 0)
            {
                _pickUpAgent.TargetItemTimeout = newTimeout;
            }
            GUILayout.Label("sec", _labelStyle, GUILayout.Width(30));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            // Force Rescan Toggle
            GUILayout.BeginHorizontal();
            GUILayout.Label("Force Rescan:", _labelStyle, GUILayout.Width(110));
            
            GUIStyle toggleStyle = _pickUpAgent.ForceRescan ? _toggleOnStyle : _toggleOffStyle;
            GUI.backgroundColor = _pickUpAgent.ForceRescan ? _positiveColor : _textMutedColor;
            
            if (GUILayout.Button(_pickUpAgent.ForceRescan ? "ON" : "OFF", toggleStyle, GUILayout.Width(50), GUILayout.Height(20)))
            {
                _pickUpAgent.ForceRescan = !_pickUpAgent.ForceRescan;
            }
            GUI.backgroundColor = Color.white;
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawIgnoredItemsSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Ignored Items ({_pickUpAgent.IgnoredItems.Count})", _titleStyle);
            GUILayout.FlexibleSpace();
            
            if (_pickUpAgent.IgnoredItems.Count > 0)
            {
                GUI.backgroundColor = _warningColor;
                if (GUILayout.Button("Clear All", _buttonStyle, GUILayout.Width(70), GUILayout.Height(18)))
                {
                    _pickUpAgent.IgnoredItems.Clear();
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            if (_pickUpAgent.IgnoredItems.Count > 0)
            {
                foreach (var item in _pickUpAgent.IgnoredItems.Take(5))
                {
                    GUILayout.Label($"  • {item.name}", _listItemStyle);
                }
                
                if (_pickUpAgent.IgnoredItems.Count > 5)
                {
                    GUIStyle moreStyle = new GUIStyle(_listItemStyle);
                    moreStyle.fontStyle = FontStyle.Italic;
                    GUILayout.Label($"  ... +{_pickUpAgent.IgnoredItems.Count - 5} more", moreStyle);
                }
            }
            else
            {
                GUILayout.Label("  No ignored items", _listItemStyle);
            }
        }
    }
}
