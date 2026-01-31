using System;
using System.Linq;
using UnityEngine;

namespace Scripts.Models
{
    internal class Bot_Agent_FighterUI
    {
        private Bot_Agent_Fighter _fightingAgent;

        public Bot_Agent_FighterUI(Bot_Agent_Fighter fightingAgent)
        {
            _fightingAgent = fightingAgent;
        }

        public void DrawFightingAgentPanel(Rect contentArea)
        {
            if (_fightingAgent == null) return;

            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(UITheme.SectionStyle);

            DrawSectionHeader("FIGHTER AGENT");
            GUILayout.Space(UITheme.BUTTON_SPACING);

            // Stats Row
            GUILayout.BeginHorizontal();
            DrawStatBox("Kills", _fightingAgent.KillCount.ToString(), UITheme.Positive);
            GUILayout.Space(UITheme.BUTTON_SPACING);
            DrawStatBox("Ignored", _fightingAgent.IgnoredMonsters.Count.ToString(), UITheme.Warning);
            GUILayout.Space(UITheme.BUTTON_SPACING);
            DrawStatBox("Range", $"{_fightingAgent.MaxDistance:F0}m", UITheme.Accent);
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.WINDOW_PADDING);

            DrawTargetSection();
            GUILayout.Space(UITheme.SECTION_SPACING);

            DrawSettingsSection();
            GUILayout.Space(UITheme.SECTION_SPACING);

            DrawIgnoredMonstersSection();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Label(title, UITheme.SubtitleStyle);
            UITheme.DrawSeparator();
        }

        private void DrawStatBox(string label, string value, Color valueColor)
        {
            GUILayout.BeginVertical(GUILayout.Width(80));
            GUILayout.Label(label, UITheme.LabelStyle);
            GUILayout.Label(value, UITheme.CreateValueStyle(valueColor, 14));
            GUILayout.EndVertical();
        }

        private void DrawTargetSection()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = UITheme.Danger;
            GUILayout.Label("?", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Current Target", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();

            if (_fightingAgent.TargetedMonster != null)
            {
                GUI.backgroundColor = UITheme.Danger;
                if (GUILayout.Button("Reset", UITheme.ButtonStyle, GUILayout.Width(55), GUILayout.Height(18)))
                {
                    _fightingAgent.TargetedMonster = null;
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            // Target name
            string targetName = _fightingAgent.TargetedMonster != null ? _fightingAgent.TargetedMonster.name : "None";
            Color targetColor = _fightingAgent.TargetedMonster != null ? UITheme.Danger : UITheme.TextMuted;
            GUILayout.Label(targetName, UITheme.CreateValueStyle(targetColor, 12));

            // Target health bar
            if (_fightingAgent.TargetedMonster?.Health != null)
            {
                GUILayout.Space(UITheme.ELEMENT_SPACING);
                DrawHealthBar(_fightingAgent.TargetedMonster.Health.CurrentHealth, _fightingAgent.TargetedMonster.Health.MaxHealth);
            }

            // Last health change
            if (_fightingAgent.LastTargetMonsterHealthChanged.HasValue)
            {
                TimeSpan timeSinceChange = DateTime.Now - _fightingAgent.LastTargetMonsterHealthChanged.Value;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Last Hit:", UITheme.LabelStyle, GUILayout.Width(60));
                Color hitColor = UITheme.GetTimeStatusColor(timeSinceChange.TotalSeconds, 3, 10);
                GUILayout.Label($"{timeSinceChange.TotalSeconds:F1}s ago", UITheme.CreateValueStyle(hitColor));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void DrawHealthBar(int current, int max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("HP:", UITheme.LabelStyle, GUILayout.Width(25));

            float pct = max > 0 ? (float)current / max : 0;
            Color hpColor = UITheme.GetDurabilityColor(pct);
            GUILayout.Label($"{current}/{max}", UITheme.CreateValueStyle(hpColor), GUILayout.Width(80));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Health bar
            Rect barRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(8));
            UITheme.DrawProgressBar(barRect, pct, hpColor);
        }

        private void DrawSettingsSection()
        {
            GUILayout.Label("Settings", UITheme.SubtitleStyle);
            GUILayout.Space(4);

            // Max Distance
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Distance:", UITheme.LabelStyle, GUILayout.Width(100));
            string maxDistStr = GUILayout.TextField(_fightingAgent.MaxDistance.ToString("F0"), UITheme.InputStyle, GUILayout.Width(UITheme.INPUT_FIELD_WIDTH));
            if (float.TryParse(maxDistStr, out float newMaxDist) && newMaxDist > 0)
            {
                _fightingAgent.MaxDistance = newMaxDist;
            }
            GUILayout.Label("units", UITheme.LabelStyle, GUILayout.Width(35));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            // Target Timeout
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Timeout:", UITheme.LabelStyle, GUILayout.Width(100));
            string timeoutStr = GUILayout.TextField(_fightingAgent.TargetMonsterTimeout.ToString(), UITheme.InputStyle, GUILayout.Width(UITheme.INPUT_FIELD_WIDTH));
            if (int.TryParse(timeoutStr, out int newTimeout) && newTimeout > 0)
            {
                _fightingAgent.TargetMonsterTimeout = newTimeout;
            }
            GUILayout.Label("sec", UITheme.LabelStyle, GUILayout.Width(35));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawIgnoredMonstersSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Ignored Monsters ({_fightingAgent.IgnoredMonsters.Count})", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();

            if (_fightingAgent.IgnoredMonsters.Count > 0)
            {
                GUI.backgroundColor = UITheme.Warning;
                if (GUILayout.Button("Clear All", UITheme.ButtonStyle, GUILayout.Width(70), GUILayout.Height(18)))
                {
                    _fightingAgent.IgnoredMonsters.Clear();
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            GUIStyle listStyle = UITheme.CreateLabelStyle(UITheme.TextMuted, UITheme.FONT_SIZE_SMALL);

            if (_fightingAgent.IgnoredMonsters.Count > 0)
            {
                foreach (var monster in _fightingAgent.IgnoredMonsters.Take(5))
                {
                    GUILayout.Label($"  • {monster?.name}", listStyle);
                }

                if (_fightingAgent.IgnoredMonsters.Count > 5)
                {
                    GUIStyle moreStyle = UITheme.CreateLabelStyle(UITheme.TextMuted, UITheme.FONT_SIZE_SMALL, FontStyle.Italic);
                    GUILayout.Label($"  ... +{_fightingAgent.IgnoredMonsters.Count - 5} more", moreStyle);
                }
            }
            else
            {
                GUILayout.Label("  No ignored monsters", listStyle);
            }
        }
    }
}
