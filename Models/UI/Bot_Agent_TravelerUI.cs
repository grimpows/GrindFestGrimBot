using System.Linq;
using UnityEngine;

namespace Scripts.Models
{
    public class Bot_Agent_TravelerUI
    {
        private Bot_Agent_Traveler _travelerAgent;
        private Vector2 _levelAreaScrollPosition = Vector2.zero;
        private Vector2 _customAreaScrollPosition = Vector2.zero;
        private Vector2 _vectorZoneScrollPosition = Vector2.zero;
        private string _newCustomAreaName = "";

        // New vector zone input fields
        private string _newVectorZoneName = "";
        private string _newVectorZoneX = "0";
        private string _newVectorZoneY = "0";
        private string _newVectorZoneZ = "0";
        private string _newVectorZoneRadius = "10";

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

            // Main content: 4 columns layout
            GUILayout.BeginHorizontal();

            // Left Panel - Status & Info
            DrawLeftPanel();

            GUILayout.Space(UITheme.SECTION_SPACING);

            // Center Panel - Level-based Areas
            DrawCenterPanel(contentArea.height - 60);

            GUILayout.Space(UITheme.SECTION_SPACING);

            // Right Panel - Custom Areas
            DrawRightPanel(contentArea.height - 60);

            GUILayout.Space(UITheme.SECTION_SPACING);

            // Far Right Panel - Vector Zones
            DrawVectorZonePanel(contentArea.height - 60);

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
            try { bestArea = _travelerAgent.GetAreaToTravel(); } catch { }
            string targetArea = _travelerAgent.TargetAreaName;
            string forcedArea = _travelerAgent.ForcedAreaName;

            // ScrollView for level area list
            float scrollHeight = Mathf.Max(150, availableHeight - 50);
            _levelAreaScrollPosition = GUILayout.BeginScrollView(_levelAreaScrollPosition, GUILayout.Height(scrollHeight));

            foreach (var kvp in _travelerAgent.MinLevelAreaDictionary.OrderBy(kv => kv.Key))
            {
                bool isBest = kvp.Value == bestArea && !_travelerAgent.IsAnyForcedModeEnabled;
                bool isTarget = kvp.Value == targetArea;
                bool isForced = kvp.Value == forcedArea;
                DrawLevelAreaRow(kvp.Key, kvp.Value, isBest, isTarget, isForced);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawRightPanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.Width(180));

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

        private void DrawVectorZonePanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.Width(250));

            // Header with clear button
            GUILayout.BeginHorizontal();
            GUILayout.Label("Vector Zones", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();

            if (_travelerAgent.IsForcedVectorZoneEnabled)
            {
                GUI.backgroundColor = UITheme.Danger;
                if (GUILayout.Button("Clear", UITheme.ButtonStyle, GUILayout.Width(50), GUILayout.Height(18)))
                {
                    _travelerAgent.ClearForcedVectorZone();
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            // Add new vector zone - Name
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", UITheme.LabelStyle, GUILayout.Width(45));
            _newVectorZoneName = GUILayout.TextField(_newVectorZoneName, UITheme.InputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(18));
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            // Position inputs (X, Y, Z)
            GUILayout.BeginHorizontal();
            GUILayout.Label("X:", UITheme.LabelStyle, GUILayout.Width(15));
            _newVectorZoneX = GUILayout.TextField(_newVectorZoneX, UITheme.InputStyle, GUILayout.Width(50), GUILayout.Height(18));
            GUILayout.Label("Y:", UITheme.LabelStyle, GUILayout.Width(15));
            _newVectorZoneY = GUILayout.TextField(_newVectorZoneY, UITheme.InputStyle, GUILayout.Width(50), GUILayout.Height(18));
            GUILayout.Label("Z:", UITheme.LabelStyle, GUILayout.Width(15));
            _newVectorZoneZ = GUILayout.TextField(_newVectorZoneZ, UITheme.InputStyle, GUILayout.Width(50), GUILayout.Height(18));
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            // Radius input and Add button
            GUILayout.BeginHorizontal();
            GUILayout.Label("Radius:", UITheme.LabelStyle, GUILayout.Width(45));
            _newVectorZoneRadius = GUILayout.TextField(_newVectorZoneRadius, UITheme.InputStyle, GUILayout.Width(50), GUILayout.Height(18));
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = UITheme.Positive;
            GUIStyle addBtnStyle = new GUIStyle(UITheme.ButtonStyle);
            addBtnStyle.fontSize = UITheme.FONT_SIZE_SMALL;
            if (GUILayout.Button("Add Zone", addBtnStyle, GUILayout.Width(70), GUILayout.Height(18)))
            {
                TryAddVectorZone();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            // Display forced zone info if active
            if (_travelerAgent.IsForcedVectorZoneEnabled)
            {
                DrawForcedVectorZoneInfo();
                GUILayout.Space(UITheme.ELEMENT_SPACING);
            }

            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            if (_travelerAgent.CustomVectorZoneList == null || _travelerAgent.CustomVectorZoneList.Count == 0)
            {
                GUILayout.Label("No vector zones", UITheme.LabelStyle);
                GUILayout.EndVertical();
                return;
            }

            // ScrollView for vector zone list
            float scrollHeight = Mathf.Max(80, availableHeight - 180);
            _vectorZoneScrollPosition = GUILayout.BeginScrollView(_vectorZoneScrollPosition, GUILayout.Height(scrollHeight));

            foreach (var zone in _travelerAgent.CustomVectorZoneList.ToList())
            {
                bool isForced = _travelerAgent.ForcedVectorZone == zone;
                DrawVectorZoneRow(zone, isForced);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawForcedVectorZoneInfo()
        {
            var zone = _travelerAgent.ForcedVectorZone;
            if (zone == null) return;

            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = UITheme.Warning;
            GUILayout.Label("⚡", GUILayout.Width(14));
            GUI.color = Color.white;
            GUILayout.Label("Active Zone", UITheme.CreateLabelStyle(UITheme.Warning, UITheme.FONT_SIZE_SMALL, FontStyle.Bold));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Label(zone.Name, UITheme.CreateValueStyle(UITheme.Warning, UITheme.FONT_SIZE_SMALL));

            float distance = _travelerAgent.GetDistanceToForcedVectorZone();
            bool inZone = _travelerAgent.IsHeroInForcedVectorZone();
            Color distColor = inZone ? UITheme.Positive : UITheme.Warning;
            string distText = distance >= 0 ? $"Dist: {distance:F1} / R:{zone.Radius:F1}" : "N/A";
            GUILayout.Label(distText, UITheme.CreateLabelStyle(distColor, UITheme.FONT_SIZE_SMALL));

            GUILayout.EndVertical();
        }

        private void TryAddVectorZone()
        {
            if (string.IsNullOrWhiteSpace(_newVectorZoneName))
                return;

            if (!float.TryParse(_newVectorZoneX, out float x)) return;
            if (!float.TryParse(_newVectorZoneY, out float y)) return;
            if (!float.TryParse(_newVectorZoneZ, out float z)) return;
            if (!float.TryParse(_newVectorZoneRadius, out float radius)) return;

            if (radius <= 0) radius = 10f;

            _travelerAgent.AddCustomVectorZone(_newVectorZoneName.Trim(), x, y, z, radius);

            // Reset inputs
            _newVectorZoneName = "";
            _newVectorZoneX = "0";
            _newVectorZoneY = "0";
            _newVectorZoneZ = "0";
            _newVectorZoneRadius = "10";
        }

        private void DrawVectorZoneRow(VectorZone zone, bool isForcedZone)
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle, GUILayout.Height(36));

            GUILayout.BeginVertical();

            // Zone name
            Color nameColor = isForcedZone ? UITheme.Warning : UITheme.TextLight;
            FontStyle fontStyle = isForcedZone ? FontStyle.Bold : FontStyle.Normal;
            GUILayout.Label(TruncateText(zone.Name, 16), UITheme.CreateLabelStyle(nameColor, UITheme.FONT_SIZE_SMALL, fontStyle));

            // Position and radius info
            string posInfo = $"({zone.Position.x:F0},{zone.Position.y:F0},{zone.Position.z:F0}) R:{zone.Radius:F0}";
            GUILayout.Label(posInfo, UITheme.CreateLabelStyle(UITheme.TextMuted, 9));

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // Status indicator
            if (isForcedZone)
            {
                GUILayout.Label("⚡", UITheme.CreateLabelStyle(UITheme.Warning, 12), GUILayout.Width(14));
            }
            else
            {
                GUILayout.Space(14);
            }

            // Force/Unforce button
            DrawVectorZoneForceButton(zone, isForcedZone);

            // Remove button
            GUI.backgroundColor = UITheme.Danger;
            GUIStyle removeBtnStyle = new GUIStyle(UITheme.ButtonStyle);
            removeBtnStyle.fontSize = UITheme.FONT_SIZE_SMALL;
            if (GUILayout.Button("X", removeBtnStyle, GUILayout.Width(22), GUILayout.Height(20)))
            {
                if (isForcedZone)
                {
                    _travelerAgent.ClearForcedVectorZone();
                }
                _travelerAgent.RemoveCustomVectorZone(zone);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();
            GUILayout.Space(1);
        }

        private void DrawVectorZoneForceButton(VectorZone zone, bool isForcedZone)
        {
            if (isForcedZone)
            {
                GUI.backgroundColor = UITheme.Warning;
                GUIStyle unforceBtnStyle = new GUIStyle(UITheme.ButtonStyle);
                unforceBtnStyle.fontSize = UITheme.FONT_SIZE_SMALL;
                unforceBtnStyle.normal.textColor = UITheme.TextDark;

                if (GUILayout.Button("On", unforceBtnStyle, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    _travelerAgent.ClearForcedVectorZone();
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
                    _travelerAgent.ForcedVectorZone = zone;
                }
                GUI.backgroundColor = Color.white;
            }
        }

        private void DrawStatusSection()
        {
            bool isTraveling = !string.IsNullOrEmpty(_travelerAgent.TargetAreaName);
            string statusText = isTraveling ? "TRAVELING" : "IDLE";
            Color statusColor = isTraveling ? UITheme.Warning : UITheme.Positive;

            GUILayout.BeginHorizontal();
            DrawStatBox("Status", statusText, statusColor);
            GUILayout.Space(UITheme.BUTTON_SPACING);

            // Mode: AUTO, FORCED AREA, or FORCED ZONE
            string modeText = "AUTO";
            Color modeColor = UITheme.Positive;

            if (_travelerAgent.IsForcedVectorZoneEnabled)
            {
                modeText = "VECTOR";
                modeColor = UITheme.Info;
            }
            else if (_travelerAgent.IsForcedAreaEnabled)
            {
                modeText = "AREA";
                modeColor = UITheme.Warning;
            }

            DrawStatBox("Mode", modeText, modeColor);
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

            // Icon based on mode
            if (_travelerAgent.IsForcedVectorZoneEnabled)
            {
                GUI.color = UITheme.Info;
                GUILayout.Label("◎", GUILayout.Width(20));
            }
            else if (_travelerAgent.IsForcedAreaEnabled)
            {
                GUI.color = UITheme.Warning;
                GUILayout.Label("⚡", GUILayout.Width(20));
            }
            else
            {
                GUI.color = UITheme.Accent;
                GUILayout.Label("★", GUILayout.Width(20));
            }
            GUI.color = Color.white;

            string headerText = _travelerAgent.IsForcedVectorZoneEnabled ? "Forced Zone" :
                               (_travelerAgent.IsForcedAreaEnabled ? "Forced Area" : "Best Area");
            GUILayout.Label(headerText, UITheme.SubtitleStyle);

            GUILayout.FlexibleSpace();

            // Show mode indicator
            string modeText = _travelerAgent.IsForcedVectorZoneEnabled ? "VECTOR" :
                             (_travelerAgent.IsForcedAreaEnabled ? "FORCED" : "AUTO");
            Color modeColor = _travelerAgent.IsForcedVectorZoneEnabled ? UITheme.Info :
                             (_travelerAgent.IsForcedAreaEnabled ? UITheme.Warning : UITheme.Positive);
            GUILayout.Label(modeText, UITheme.CreateLabelStyle(modeColor, UITheme.FONT_SIZE_SMALL, FontStyle.Bold));
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            // Display zone or area info
            if (_travelerAgent.IsForcedVectorZoneEnabled && _travelerAgent.ForcedVectorZone != null)
            {
                var zone = _travelerAgent.ForcedVectorZone;
                GUILayout.Label(zone.Name, UITheme.CreateValueStyle(UITheme.Info, 12));
                GUILayout.Label($"({zone.Position.x:F0}, {zone.Position.y:F0}, {zone.Position.z:F0})", UITheme.CreateLabelStyle(UITheme.TextMuted, 9));
            }
            else
            {
                string bestArea = "";
                try { bestArea = _travelerAgent.GetAreaToTravel(); } catch { bestArea = "Unknown"; }

                Color areaColor = _travelerAgent.IsForcedAreaEnabled ? UITheme.Warning : UITheme.Accent;
                GUILayout.Label(bestArea, UITheme.CreateValueStyle(areaColor, 12));
            }

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

            GUILayout.Label(TruncateText(areaName, 20), UITheme.CreateLabelStyle(nameColor, UITheme.FONT_SIZE_SMALL, fontStyle), GUILayout.ExpandWidth(true));

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
