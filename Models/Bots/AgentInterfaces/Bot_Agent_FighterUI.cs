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
    internal class Bot_Agent_FighterUI
    {
        private Bot_Agent_Fighter _fightingAgent;
        private bool _stylesInitialized = false;

        // GUI Styles
        private GUIStyle _sectionStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _valueStyle;
        private GUIStyle _inputStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _listItemStyle;

        // Textures
        private Texture2D _sectionBgTexture;
        private Texture2D _cardBgTexture;
        private Texture2D _inputBgTexture;
        private Texture2D _barFillTexture;
        private Texture2D _healthBarBgTexture;

        // Colors
        private Color _sectionColor = new Color(0.1f, 0.1f, 0.12f, 0.95f);
        private Color _cardColor = new Color(0.15f, 0.15f, 0.18f, 0.95f);
        private Color _accentColor = new Color(0.95f, 0.75f, 0.3f, 1f);
        private Color _healthColor = new Color(0.9f, 0.3f, 0.3f, 1f);
        private Color _positiveColor = new Color(0.4f, 1f, 0.55f, 1f);
        private Color _warningColor = new Color(1f, 0.7f, 0.3f, 1f);
        private Color _dangerColor = new Color(1f, 0.4f, 0.4f, 1f);
        private Color _combatColor = new Color(1f, 0.5f, 0.3f, 1f);
        private Color _textLightColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        private Color _textMutedColor = new Color(0.65f, 0.65f, 0.7f, 1f);

        public Bot_Agent_FighterUI(Bot_Agent_Fighter fightingAgent)
        {
            _fightingAgent = fightingAgent;
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _sectionBgTexture = CreateTexture(_sectionColor);
            _cardBgTexture = CreateTexture(_cardColor);
            _inputBgTexture = CreateTexture(new Color(0.15f, 0.15f, 0.18f, 0.95f));
            _barFillTexture = CreateTexture(Color.white);
            _healthBarBgTexture = CreateTexture(new Color(0.2f, 0.2f, 0.22f, 0.9f));

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

            _cardStyle = new GUIStyle(GUI.skin.box);
            _cardStyle.normal.background = _cardBgTexture;
            _cardStyle.padding = new RectOffset(10, 10, 8, 8);

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

        public void DrawFightingAgentPanel(Rect contentArea)
        {
            if (_fightingAgent == null) return;

            InitializeStyles();

            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(_sectionStyle);

            // Header
            DrawSectionHeader("FIGHTER AGENT");
            GUILayout.Space(8);

            // Stats Row
            GUILayout.BeginHorizontal();
            DrawStatBox("Kills", _fightingAgent.KillCount.ToString(), _positiveColor);
            GUILayout.Space(8);
            DrawStatBox("Ignored", _fightingAgent.IgnoredMonsters.Count.ToString(), _warningColor);
            GUILayout.Space(8);
            DrawStatBox("Range", $"{_fightingAgent.MaxDistance:F0}m", _accentColor);
            GUILayout.EndHorizontal();
            GUILayout.Space(12);

            // Target Monster Card
            DrawTargetSection();
            GUILayout.Space(10);

            // Settings
            DrawSettingsSection();
            GUILayout.Space(10);

            // Ignored Monsters List
            DrawIgnoredMonstersSection();

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
            GUILayout.BeginVertical(_cardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = _combatColor;
            GUILayout.Label("?", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Current Target", _titleStyle);
            GUILayout.FlexibleSpace();

            if (_fightingAgent.TargetedMonster != null)
            {
                GUI.backgroundColor = _dangerColor;
                if (GUILayout.Button("Reset", _buttonStyle, GUILayout.Width(55), GUILayout.Height(18)))
                {
                    _fightingAgent.TargetedMonster = null;
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(6);

            // Target name
            string targetName = _fightingAgent.TargetedMonster != null ? _fightingAgent.TargetedMonster.name : "None";
            Color targetColor = _fightingAgent.TargetedMonster != null ? _combatColor : _textMutedColor;

            GUIStyle nameStyle = new GUIStyle(_valueStyle);
            nameStyle.fontSize = 12;
            nameStyle.normal.textColor = targetColor;
            GUILayout.Label(targetName, nameStyle);

            // Target health bar
            if (_fightingAgent.TargetedMonster?.Health != null)
            {
                GUILayout.Space(6);
                DrawHealthBar(_fightingAgent.TargetedMonster.Health.CurrentHealth, _fightingAgent.TargetedMonster.Health.MaxHealth);
            }

            // Last health change
            if (_fightingAgent.LastTargetMonsterHealthChanged.HasValue)
            {
                TimeSpan timeSinceChange = DateTime.Now - _fightingAgent.LastTargetMonsterHealthChanged.Value;
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Last Hit:", _labelStyle, GUILayout.Width(60));

                Color hitColor = timeSinceChange.TotalSeconds < 3 ? _positiveColor : _textMutedColor;
                GUIStyle hitStyle = new GUIStyle(_valueStyle);
                hitStyle.normal.textColor = hitColor;
                GUILayout.Label($"{timeSinceChange.TotalSeconds:F1}s ago", hitStyle);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void DrawHealthBar(int current, int max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("HP:", _labelStyle, GUILayout.Width(25));

            // Health values
            float pct = max > 0 ? (float)current / max : 0;
            Color hpColor = pct > 0.5f ? _positiveColor : pct > 0.25f ? _warningColor : _dangerColor;

            GUIStyle hpStyle = new GUIStyle(_valueStyle);
            hpStyle.normal.textColor = hpColor;
            GUILayout.Label($"{current}/{max}", hpStyle, GUILayout.Width(80));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Health bar
            Rect barRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(8));
            GUI.DrawTexture(barRect, _healthBarBgTexture);

            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * pct, barRect.height);
            GUI.color = hpColor;
            GUI.DrawTexture(fillRect, _barFillTexture);
            GUI.color = Color.white;
        }

        private void DrawSettingsSection()
        {
            GUILayout.Label("Settings", _titleStyle);
            GUILayout.Space(4);

            // Max Distance
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Distance:", _labelStyle, GUILayout.Width(100));
            string maxDistStr = GUILayout.TextField(_fightingAgent.MaxDistance.ToString("F0"), _inputStyle, GUILayout.Width(50));
            if (float.TryParse(maxDistStr, out float newMaxDist) && newMaxDist > 0)
            {
                _fightingAgent.MaxDistance = newMaxDist;
            }
            GUILayout.Label("units", _labelStyle, GUILayout.Width(35));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            // Target Timeout
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Timeout:", _labelStyle, GUILayout.Width(100));
            string timeoutStr = GUILayout.TextField(_fightingAgent.TargetMonsterTimeout.ToString(), _inputStyle, GUILayout.Width(50));
            if (int.TryParse(timeoutStr, out int newTimeout) && newTimeout > 0)
            {
                _fightingAgent.TargetMonsterTimeout = newTimeout;
            }
            GUILayout.Label("sec", _labelStyle, GUILayout.Width(35));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawIgnoredMonstersSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Ignored Monsters ({_fightingAgent.IgnoredMonsters.Count})", _titleStyle);
            GUILayout.FlexibleSpace();

            if (_fightingAgent.IgnoredMonsters.Count > 0)
            {
                GUI.backgroundColor = _warningColor;
                if (GUILayout.Button("Clear All", _buttonStyle, GUILayout.Width(70), GUILayout.Height(18)))
                {
                    _fightingAgent.IgnoredMonsters.Clear();
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            if (_fightingAgent.IgnoredMonsters.Count > 0)
            {
                foreach (var monster in _fightingAgent.IgnoredMonsters.Take(5))
                {
                    GUILayout.Label($"  • {monster.name}", _listItemStyle);
                }

                if (_fightingAgent.IgnoredMonsters.Count > 5)
                {
                    GUIStyle moreStyle = new GUIStyle(_listItemStyle);
                    moreStyle.fontStyle = FontStyle.Italic;
                    GUILayout.Label($"  ... +{_fightingAgent.IgnoredMonsters.Count - 5} more", moreStyle);
                }
            }
            else
            {
                GUILayout.Label("  No ignored monsters", _listItemStyle);
            }
        }
    }
}
