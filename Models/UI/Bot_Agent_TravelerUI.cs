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
        private Vector2 _levelAreaScrollPosition = Vector2.zero;
        private Vector2 _customAreaScrollPosition = Vector2.zero;
        private string _newCustomAreaName = "";

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

            // Main content: 3 columns layout
            GUILayout.BeginHorizontal();

            // Left Panel - Status & Info
            DrawLeftPanel();

            GUILayout.Space(UITheme.SECTION_SPACING);

            // Center Panel - Level-based Areas
            DrawCenterPanel(contentArea.height - 60);

            GUILayout.Space(UITheme.SECTION_SPACING);

            // Right Panel - Custom Areas
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
            GUILayout.BeginVertical(GUILayout.Width(240));

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

        private void DrawCenterPanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.Width(220));

            // Header with clear button
            GUILayout.BeginHorizontal();
            GUILayout.Label("Level Areas", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();

            if (_travelerAgent.IsForcedAreaEnabled && _travelerAgent.MinLevelAreaDictionary.ContainsValue(_travelerAgent.ForcedAreaName))
            {
                GUI.backgroundColor = UITheme.Danger;
                if (GUILayout.Button("Clear", UITheme.ButtonStyle, GUILayout.Width(50), GUILayout.Height(18)))
                {
                    _travelerAgent.ClearForcedArea();
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

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

            // ScrollView for level area list
            float scrollHeight = Mathf.Max(150, availableHeight - 50);
            _levelAreaScrollPosition = GUILayout.BeginScrollView(_levelAreaScrollPosition, GUILayout.Height(scrollHeight));

            foreach (var kvp in _travelerAgent.MinLevelAreaDictionary.OrderBy(kv => kv.Key))
            {
                bool isBest = kvp.Value == bestArea && !_travelerAgent.IsForcedAreaEnabled;
                bool isTarget = kvp.Value == targetArea;
                bool isForced = kvp.Value == forcedArea;
                DrawLevelAreaRow(kvp.Key, kvp.Value, isBest, isTarget, isForced);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawRightPanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            // Header with clear button
            GUILayout.BeginHorizontal();
            GUILayout.Label("Custom Areas", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();

            if (_travelerAgent.IsForcedAreaEnabled && _travelerAgent.CustomAreaList.Contains(_travelerAgent.ForcedAreaName))
            {
                GUI.backgroundColor = UITheme.Danger;
                if (GUILayout.Button("Clear", UITheme.ButtonStyle, GUILayout.Width(50), GUILayout.Height(18)))
                {
                    _travelerAgent.ClearForcedArea();
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            // Add new custom area
            GUILayout.BeginHorizontal();
            _newCustomAreaName = GUILayout.TextField(_newCustomAreaName, UITheme.InputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            
            GUI.backgroundColor = UITheme.Positive;
            GUIStyle addBtnStyle = new GUIStyle(UITheme.ButtonStyle);
            addBtnStyle.fontSize = UITheme.FONT_SIZE_SMALL;
            if (GUILayout.Button("+", addBtnStyle, GUILayout.Width(25), GUILayout.Height(20)))
            {
                if (!string.IsNullOrWhiteSpace(_newCustomAreaName))
                {
                    _travelerAgent.AddCustomArea(_newCustomAreaName.Trim());
                    _newCustomAreaName = "";
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            if (_travelerAgent.CustomAreaList == null || _travelerAgent.CustomAreaList.Count == 0)
            {
                GUILayout.Label("No custom areas", UITheme.LabelStyle);
                GUILayout.EndVertical();
                return;
            }

            string forcedArea = _travelerAgent.ForcedAreaName;
            string targetArea = _travelerAgent.TargetAreaName;

            // ScrollView for custom area list
            float scrollHeight = Mathf.Max(120, availableHeight - 80);
            _customAreaScrollPosition = GUILayout.BeginScrollView(_customAreaScrollPosition, GUILayout.Height(scrollHeight));

            foreach (var areaName in _travelerAgent.CustomAreaList.ToList())
            {
                bool isForced = areaName == forcedArea;
                bool isTarget = areaName == targetArea;
                DrawCustomAreaRow(areaName, isForced, isTarget);
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
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawStatBox(string label, string value, Color valueColor)
        {
            GUILayout.BeginVertical(GUILayout.Width(70));
            GUILayout.Label(label, UITheme.LabelStyle);
            GUILayout.Label(value, UITheme.CreateValueStyle(valueColor, 12));
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
            GUILayout.Label("Current:", UITheme.LabelStyle, GUILayout.Width(60));
            string currentInfo = string.IsNullOrEmpty(_travelerAgent.TargetAreaName) ? "At dest." : "Transit...";
            GUILayout.Label(currentInfo, UITheme.CreateValueStyle(UITheme.TextLight));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Target Area
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target:", UITheme.LabelStyle, GUILayout.Width(60));
            
            string targetName = string.IsNullOrEmpty(_travelerAgent.TargetAreaName) ? "None" : _travelerAgent.TargetAreaName;
            Color targetColor = string.IsNullOrEmpty(_travelerAgent.TargetAreaName) ? UITheme.TextMuted : UITheme.Warning;
            GUILayout.Label(targetName, UITheme.CreateValueStyle(targetColor, UITheme.FONT_SIZE_SMALL));
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawBestAreaCard()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = _travelerAgent.IsForcedAreaEnabled ? UITheme.Warning : UITheme.Accent;
            GUILayout.Label(_travelerAgent.IsForcedAreaEnabled ? "⚡" : "★", GUILayout.Width(20));
            GUI.color = Color.white;
            
            string headerText = _travelerAgent.IsForcedAreaEnabled ? "Forced" : "Best";
            GUILayout.Label(headerText, UITheme.SubtitleStyle);
            
            GUILayout.FlexibleSpace();
            
            // Show mode indicator
            string modeText = _travelerAgent.IsForcedAreaEnabled ? "FORCED" : "AUTO";
            Color modeColor = _travelerAgent.IsForcedAreaEnabled ? UITheme.Warning : UITheme.Positive;
            GUILayout.Label(modeText, UITheme.CreateLabelStyle(modeColor, UITheme.FONT_SIZE_SMALL, FontStyle.Bold));
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            string bestArea = "";
            try { bestArea = _travelerAgent.GetBestArea(); } catch { bestArea = "Unknown"; }

            Color areaColor = _travelerAgent.IsForcedAreaEnabled ? UITheme.Warning : UITheme.Accent;
            GUILayout.Label(bestArea, UITheme.CreateValueStyle(areaColor, 12));

            GUILayout.EndVertical();
        }

        private void DrawLevelAreaRow(int minLevel, string areaName, bool isBestArea, bool isTargetArea, bool isForcedArea)
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle, GUILayout.Height(28));

            // Level badge
            GUILayout.Label($"L{minLevel}", UITheme.CreateValueStyle(UITheme.Info, UITheme.FONT_SIZE_SMALL), GUILayout.Width(25));

            GUILayout.Space(4);

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

            GUILayout.Label(TruncateText(areaName, 12), UITheme.CreateLabelStyle(nameColor, UITheme.FONT_SIZE_SMALL, fontStyle), GUILayout.ExpandWidth(true));

            // Status indicator
            if (isForcedArea)
            {
                GUILayout.Label("⚡", UITheme.CreateLabelStyle(UITheme.Warning, 12), GUILayout.Width(14));
            }
            else if (isBestArea)
            {
                GUILayout.Label("★", UITheme.CreateLabelStyle(UITheme.Accent, 12), GUILayout.Width(14));
            }
            else
            {
                GUILayout.Space(14);
            }

            // Force/Unforce button
            DrawForceButton(areaName, isForcedArea);

            GUILayout.EndHorizontal();
            GUILayout.Space(1);
        }

        private void DrawCustomAreaRow(string areaName, bool isForcedArea, bool isTargetArea)
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle, GUILayout.Height(28));

            // Area name
            Color nameColor = isForcedArea ? UITheme.Warning : UITheme.TextLight;
            FontStyle fontStyle = isForcedArea ? FontStyle.Bold : FontStyle.Normal;

            GUILayout.Label(TruncateText(areaName, 14), UITheme.CreateLabelStyle(nameColor, UITheme.FONT_SIZE_SMALL, fontStyle), GUILayout.ExpandWidth(true));

            // Status indicator
            if (isForcedArea)
            {
                GUILayout.Label("⚡", UITheme.CreateLabelStyle(UITheme.Warning, 12), GUILayout.Width(14));
            }
            else
            {
                GUILayout.Space(14);
            }

            // Force/Unforce button
            DrawForceButton(areaName, isForcedArea);

            // Remove button
            GUI.backgroundColor = UITheme.Danger;
            GUIStyle removeBtnStyle = new GUIStyle(UITheme.ButtonStyle);
            removeBtnStyle.fontSize = UITheme.FONT_SIZE_SMALL;
            if (GUILayout.Button("X", removeBtnStyle, GUILayout.Width(22), GUILayout.Height(20)))
            {
                if (isForcedArea)
                {
                    _travelerAgent.ClearForcedArea();
                }
                _travelerAgent.RemoveCustomArea(areaName);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();
            GUILayout.Space(1);
        }

        private void DrawForceButton(string areaName, bool isForcedArea)
        {
            if (isForcedArea)
            {
                GUI.backgroundColor = UITheme.Warning;
                GUIStyle unforceBtnStyle = new GUIStyle(UITheme.ButtonStyle);
                unforceBtnStyle.fontSize = UITheme.FONT_SIZE_SMALL;
                unforceBtnStyle.normal.textColor = UITheme.TextDark;
                
                if (GUILayout.Button("On", unforceBtnStyle, GUILayout.Width(30), GUILayout.Height(20)))
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
                
                if (GUILayout.Button("Off", forceBtnStyle, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    _travelerAgent.ForcedAreaName = areaName;
                }
                GUI.backgroundColor = Color.white;
            }
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
            return text.Substring(0, maxLength - 2) + "..";
        }
    }
}
