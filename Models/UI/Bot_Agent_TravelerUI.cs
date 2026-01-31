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

            // Main content: Left panel (info) + Right panel (area list)
            GUILayout.BeginHorizontal();

            // Left Panel - Status & Info
            DrawLeftPanel();

            GUILayout.Space(UITheme.WINDOW_PADDING);

            // Right Panel - Area Level Requirements with Force buttons
            DrawRightPanel(contentArea.height - 60);

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Label(title, UITheme.SubtitleStyle);
            UITheme.DrawSeparator();
        }

        private void DrawLeftPanel()
        {
            GUILayout.BeginVertical(GUILayout.Width(320));

            // Status Cards Row
            DrawStatusSection();
            GUILayout.Space(UITheme.SECTION_SPACING);

            // Travel Status Card
            DrawAreaInfoCard();
            GUILayout.Space(UITheme.SECTION_SPACING);

            // Best/Forced Area Card
            DrawBestAreaCard();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DrawRightPanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            // Header with clear button
            GUILayout.BeginHorizontal();
            GUILayout.Label("Area Level Requirements", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();

            // Clear forced button
            if (_travelerAgent.IsForcedAreaEnabled)
            {
                GUI.backgroundColor = UITheme.Danger;
                if (GUILayout.Button("Clear Forced", UITheme.ButtonStyle, GUILayout.Width(90), GUILayout.Height(20)))
                {
                    _travelerAgent.ClearForcedArea();
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            if (_travelerAgent.MinLevelAreaDictionary == null || _travelerAgent.MinLevelAreaDictionary.Count == 0)
            {
                GUILayout.Label("No areas configured", UITheme.LabelStyle);
                GUILayout.EndVertical();
                return;
            }

            string bestArea = "";
            try { bestArea = _travelerAgent.GetBestArea(); } catch { }
            string targetArea = _travelerAgent.TargetAreaName;
            string forcedArea = _travelerAgent.ForcedAreaName;

            // ScrollView for area list
            float scrollHeight = Mathf.Max(150, availableHeight - 40);
            _areaScrollPosition = GUILayout.BeginScrollView(_areaScrollPosition, GUILayout.Height(scrollHeight));

            foreach (var kvp in _travelerAgent.MinLevelAreaDictionary.OrderBy(kv => kv.Key))
            {
                bool isBest = kvp.Value == bestArea && !_travelerAgent.IsForcedAreaEnabled;
                bool isTarget = kvp.Value == targetArea;
                bool isForced = kvp.Value == forcedArea;
                DrawAreaRow(kvp.Key, kvp.Value, isBest, isTarget, isForced);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawStatusSection()
        {
            bool isTraveling = !string.IsNullOrEmpty(_travelerAgent.TargetAreaName);
            string statusText = isTraveling ? "TRAVELING" : "IDLE";
            Color statusColor = isTraveling ? UITheme.Warning : UITheme.Positive;

            GUILayout.BeginHorizontal();
            DrawStatBox("Status", statusText, statusColor);
            GUILayout.Space(UITheme.BUTTON_SPACING);
            
            // Mode: AUTO or FORCED
            bool isForced = _travelerAgent.IsForcedAreaEnabled;
            DrawStatBox("Mode", isForced ? "FORCED" : "AUTO", isForced ? UITheme.Warning : UITheme.Positive);
            GUILayout.Space(UITheme.BUTTON_SPACING);
            
            int areaCount = _travelerAgent.MinLevelAreaDictionary?.Count ?? 0;
            DrawStatBox("Areas", areaCount.ToString(), UITheme.Info);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawStatBox(string label, string value, Color valueColor)
        {
            GUILayout.BeginVertical(GUILayout.Width(80));
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
                GUILayout.Label($">>> Traveling >>>", UITheme.CreateLabelStyle(UITheme.Warning, UITheme.FONT_SIZE_SMALL, FontStyle.Italic));
            }

            GUILayout.EndVertical();
        }

        private void DrawBestAreaCard()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = _travelerAgent.IsForcedAreaEnabled ? UITheme.Warning : UITheme.Accent;
            GUILayout.Label(_travelerAgent.IsForcedAreaEnabled ? "⚡" : "★", GUILayout.Width(20));
            GUI.color = Color.white;
            
            string headerText = _travelerAgent.IsForcedAreaEnabled ? "Forced Area" : "Recommended Area";
            GUILayout.Label(headerText, UITheme.SubtitleStyle);
            
            GUILayout.FlexibleSpace();
            
            // Show mode indicator
            if (_travelerAgent.IsForcedAreaEnabled)
            {
                GUILayout.Label("FORCED", UITheme.CreateLabelStyle(UITheme.Warning, UITheme.FONT_SIZE_SMALL, FontStyle.Bold));
            }
            else
            {
                GUILayout.Label("AUTO", UITheme.CreateLabelStyle(UITheme.Positive, UITheme.FONT_SIZE_SMALL, FontStyle.Bold));
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            string bestArea = "";
            try { bestArea = _travelerAgent.GetBestArea(); } catch { bestArea = "Unknown"; }

            Color areaColor = _travelerAgent.IsForcedAreaEnabled ? UITheme.Warning : UITheme.Accent;
            GUILayout.Label(bestArea, UITheme.CreateValueStyle(areaColor, 14));

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

        private void DrawAreaRow(int minLevel, string areaName, bool isBestArea, bool isTargetArea, bool isForcedArea)
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle, GUILayout.Height(32));

            // Level badge
            GUILayout.Label($"Lv.{minLevel}+", UITheme.CreateValueStyle(UITheme.Info, UITheme.FONT_SIZE_NORMAL), GUILayout.Width(45));

            GUILayout.Space(UITheme.BUTTON_SPACING);

            // Area name
            Color nameColor = UITheme.TextLight;
            FontStyle fontStyle = FontStyle.Normal;
            
            if (isForcedArea)
            {
                nameColor = UITheme.Warning;
                fontStyle = FontStyle.Bold;
            }
            else if (isBestArea)
            {
                nameColor = UITheme.Accent;
                fontStyle = FontStyle.Bold;
            }
            else if (isTargetArea)
            {
                nameColor = UITheme.Warning;
            }

            GUILayout.Label(areaName, UITheme.CreateLabelStyle(nameColor, UITheme.FONT_SIZE_NORMAL, fontStyle), GUILayout.ExpandWidth(true));

            // Status indicator
            if (isForcedArea)
            {
                GUILayout.Label("⚡", UITheme.CreateLabelStyle(UITheme.Warning, 14), GUILayout.Width(18));
            }
            else if (isBestArea)
            {
                GUILayout.Label("★", UITheme.CreateLabelStyle(UITheme.Accent, 14), GUILayout.Width(18));
            }
            else if (isTargetArea)
            {
                GUILayout.Label("→", UITheme.CreateLabelStyle(UITheme.Warning, 14), GUILayout.Width(18));
            }
            else
            {
                GUILayout.Space(18);
            }

            GUILayout.Space(4);

            // Force/Unforce button
            if (isForcedArea)
            {
                GUI.backgroundColor = UITheme.Danger;
                GUIStyle unforceBtnStyle = new GUIStyle(UITheme.ButtonStyle);
                unforceBtnStyle.fontSize = UITheme.FONT_SIZE_SMALL;
                unforceBtnStyle.normal.textColor = UITheme.TextLight;
                
                if (GUILayout.Button("Unforce", unforceBtnStyle, GUILayout.Width(55), GUILayout.Height(22)))
                {
                    _travelerAgent.ClearForcedArea();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.backgroundColor = UITheme.ButtonNormal;
                GUIStyle forceBtnStyle = new GUIStyle(UITheme.ButtonStyle);
                forceBtnStyle.fontSize = UITheme.FONT_SIZE_SMALL;
                
                if (GUILayout.Button("Force", forceBtnStyle, GUILayout.Width(55), GUILayout.Height(22)))
                {
                    _travelerAgent.ForcedAreaName = areaName;
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }
    }
}
