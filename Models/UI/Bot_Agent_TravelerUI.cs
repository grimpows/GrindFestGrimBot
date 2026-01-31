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
        private Vector2 _areaScrollPosition = Vector2.zero;

        public Bot_Agent_TravelerUI(Bot_Agent_Traveler travelerAgent)
        {
            _travelerAgent = travelerAgent;
        }

        public void DrawTravelerAgentPanel(Rect contentArea)
        {
            if (_travelerAgent == null) return;

            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(UITheme.SectionStyle);

            DrawSectionHeader("TRAVELER AGENT");
            GUILayout.Space(UITheme.BUTTON_SPACING);

            // Status Cards Row
            DrawStatusSection();
            GUILayout.Space(UITheme.WINDOW_PADDING);

            // Current & Target Area
            DrawAreaInfoCard();
            GUILayout.Space(UITheme.SECTION_SPACING);

            // Best Area Recommendation
            DrawBestAreaCard();
            GUILayout.Space(UITheme.SECTION_SPACING);

            // Area Level Map (with scroll)
            DrawAreaLevelMap(contentArea.height - 360);

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Label(title, UITheme.SubtitleStyle);
            UITheme.DrawSeparator();
        }

        private void DrawStatusSection()
        {
            bool isTraveling = !string.IsNullOrEmpty(_travelerAgent.TargetAreaName);
            string statusText = isTraveling ? "TRAVELING" : "IDLE";
            Color statusColor = isTraveling ? UITheme.Warning : UITheme.Positive;

            GUILayout.BeginHorizontal();
            DrawStatBox("Status", statusText, statusColor);
            GUILayout.Space(UITheme.BUTTON_SPACING);
            
            string bestArea = "";
            try { bestArea = _travelerAgent.GetBestArea(); } catch { bestArea = "Unknown"; }
            bool isInBestArea = _travelerAgent.TargetAreaName == "" && !string.IsNullOrEmpty(bestArea);
            DrawStatBox("Optimal", isInBestArea ? "YES" : "NO", isInBestArea ? UITheme.Positive : UITheme.Warning);
            GUILayout.Space(UITheme.BUTTON_SPACING);
            
            int areaCount = _travelerAgent.MinLevelAreaDictionary?.Count ?? 0;
            DrawStatBox("Areas", areaCount.ToString(), UITheme.Info);
            GUILayout.EndHorizontal();
        }

        private void DrawStatBox(string label, string value, Color valueColor)
        {
            GUILayout.BeginVertical(GUILayout.Width(90));
            GUILayout.Label(label, UITheme.LabelStyle);
            GUILayout.Label(value, UITheme.CreateValueStyle(valueColor, 14));
            GUILayout.EndVertical();
        }

        private void DrawAreaInfoCard()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = UITheme.Info;
            GUILayout.Label("✈", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Travel Status", UITheme.SubtitleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            // Current Area
            GUILayout.BeginHorizontal();
            GUILayout.Label("Current Area:", UITheme.LabelStyle, GUILayout.Width(100));
            // Note: We don't have direct access to current area here, so we show target info
            string currentInfo = string.IsNullOrEmpty(_travelerAgent.TargetAreaName) ? "At destination" : "In transit...";
            GUILayout.Label(currentInfo, UITheme.CreateValueStyle(UITheme.TextLight));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Target Area
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Area:", UITheme.LabelStyle, GUILayout.Width(100));
            
            string targetName = string.IsNullOrEmpty(_travelerAgent.TargetAreaName) ? "None" : _travelerAgent.TargetAreaName;
            Color targetColor = string.IsNullOrEmpty(_travelerAgent.TargetAreaName) ? UITheme.TextMuted : UITheme.Warning;
            GUILayout.Label(targetName, UITheme.CreateValueStyle(targetColor));
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Travel indicator
            if (!string.IsNullOrEmpty(_travelerAgent.TargetAreaName))
            {
                GUILayout.Space(UITheme.ELEMENT_SPACING);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label($">>> Traveling to {_travelerAgent.TargetAreaName} >>>", UITheme.CreateLabelStyle(UITheme.Warning, UITheme.FONT_SIZE_SMALL, FontStyle.Italic));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void DrawBestAreaCard()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = UITheme.Accent;
            GUILayout.Label("★", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Recommended Area", UITheme.SubtitleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            string bestArea = "";
            try { bestArea = _travelerAgent.GetBestArea(); } catch { bestArea = "Unknown"; }

            GUILayout.Label(bestArea, UITheme.CreateValueStyle(UITheme.Accent, 14));

            // Show level requirement
            if (_travelerAgent.MinLevelAreaDictionary != null)
            {
                var areaEntry = _travelerAgent.MinLevelAreaDictionary.FirstOrDefault(kv => kv.Value == bestArea);
                if (!string.IsNullOrEmpty(areaEntry.Value))
                {
                    GUILayout.Label($"Minimum Level: {areaEntry.Key}", UITheme.LabelStyle);
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawAreaLevelMap(float availableHeight)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Area Level Requirements", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            if (_travelerAgent.MinLevelAreaDictionary == null || _travelerAgent.MinLevelAreaDictionary.Count == 0)
            {
                GUILayout.Label("No areas configured", UITheme.LabelStyle);
                return;
            }

            string bestArea = "";
            try { bestArea = _travelerAgent.GetBestArea(); } catch { }
            string targetArea = _travelerAgent.TargetAreaName;

            // ScrollView for area list
            float scrollHeight = Mathf.Max(100, availableHeight);
            _areaScrollPosition = GUILayout.BeginScrollView(_areaScrollPosition, GUILayout.Height(scrollHeight));

            foreach (var kvp in _travelerAgent.MinLevelAreaDictionary.OrderBy(kv => kv.Key))
            {
                DrawAreaRow(kvp.Key, kvp.Value, kvp.Value == bestArea, kvp.Value == targetArea);
            }

            GUILayout.EndScrollView();
        }

        private void DrawAreaRow(int minLevel, string areaName, bool isBestArea, bool isTargetArea)
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle, GUILayout.Height(30));

            // Level badge
            Color levelColor = UITheme.Info;
            GUILayout.Label($"Lv.{minLevel}+", UITheme.CreateValueStyle(levelColor, UITheme.FONT_SIZE_NORMAL), GUILayout.Width(50));

            GUILayout.Space(UITheme.BUTTON_SPACING);

            // Area name
            Color nameColor = UITheme.TextLight;
            FontStyle fontStyle = FontStyle.Normal;
            
            if (isBestArea)
            {
                nameColor = UITheme.Accent;
                fontStyle = FontStyle.Bold;
            }
            else if (isTargetArea)
            {
                nameColor = UITheme.Warning;
            }

            GUILayout.Label(areaName, UITheme.CreateLabelStyle(nameColor, UITheme.FONT_SIZE_NORMAL, fontStyle), GUILayout.ExpandWidth(true));

            // Status indicators
            if (isBestArea)
            {
                GUILayout.Label("★ BEST", UITheme.CreateLabelStyle(UITheme.Accent, UITheme.FONT_SIZE_SMALL, FontStyle.Bold), GUILayout.Width(50));
            }
            else if (isTargetArea)
            {
                GUILayout.Label("→ TARGET", UITheme.CreateLabelStyle(UITheme.Warning, UITheme.FONT_SIZE_SMALL, FontStyle.Bold), GUILayout.Width(60));
            }
            else
            {
                GUILayout.Space(50);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }
    }
}
