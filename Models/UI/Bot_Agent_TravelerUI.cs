using Scripts.Models.PathFinding;
using System.Linq;
using UnityEngine;

namespace Scripts.Models
{
    public class Bot_Agent_TravelerUI
    {
        private Bot_Agent_Traveler _travelerAgent;
        private Vector2 _levelAreaScrollPosition = Vector2.zero;
        private Vector2 _levelVectorScrollPosition = Vector2.zero;
        private Vector2 _customAreaScrollPosition = Vector2.zero;
        private Vector2 _vectorZoneScrollPosition = Vector2.zero;
        private string _newCustomAreaName = "";

        // New vector zone input fields
        private string _newVectorZoneName = "";
        private string _newVectorZoneX = "0";
        private string _newVectorZoneY = "0";
        private string _newVectorZoneZ = "0";
        private string _newVectorZoneRadius = "10";

        // New level vector zone input fields
        private string _newLevelVectorLevel = "1";
        private string _newLevelVectorName = "";
        private string _newLevelVectorX = "0";
        private string _newLevelVectorY = "0";
        private string _newLevelVectorZ = "0";
        private string _newLevelVectorRadius = "50";

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

            // Main content layout
            GUILayout.BeginHorizontal();

            // Left Panel - Status & Mode Switch
            DrawLeftPanel();

            GUILayout.Space(UITheme.SECTION_SPACING);

            // Center-Left Panel - Level-based Areas or Level Vectors based on mode
            DrawLevelBasedPanel(contentArea.height - 60);

            GUILayout.Space(UITheme.SECTION_SPACING);

            // Center-Right Panel - Custom Areas
            DrawCustomAreasPanel(contentArea.height - 60);

            GUILayout.Space(UITheme.SECTION_SPACING);

            // Right Panel - Custom Vector Zones
            DrawCustomVectorZonesPanel(contentArea.height - 60);

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
            GUILayout.BeginVertical(GUILayout.Width(260));

            // Status Cards Row
            DrawStatusSection();
            GUILayout.Space(UITheme.SECTION_SPACING);

            // Mode Switch Card
            DrawModeSwitchCard();
            GUILayout.Space(UITheme.SECTION_SPACING);

            // Travel Status Card
            DrawAreaInfoCard();
            GUILayout.Space(UITheme.SECTION_SPACING);

            // Best/Forced Area Card
            DrawBestAreaCard();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DrawModeSwitchCard()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = UITheme.Info;
            GUILayout.Label("⚙", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Travel Mode", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            // Mode buttons
            GUILayout.BeginHorizontal();

            bool isAreaMode = _travelerAgent.TravelMode == TravelMode.AreaBased;
            bool isVectorMode = _travelerAgent.TravelMode == TravelMode.VectorBased;

            // Area Mode Button
            GUI.backgroundColor = isAreaMode ? UITheme.Accent : UITheme.ButtonNormal;
            GUIStyle areaBtnStyle = new GUIStyle(UITheme.ButtonStyle);
            areaBtnStyle.fontSize = UITheme.FONT_SIZE_SMALL;
            if (isAreaMode)
                areaBtnStyle.normal.textColor = UITheme.TextDark;

            if (GUILayout.Button("Area", areaBtnStyle, GUILayout.Height(24)))
            {
                _travelerAgent.TravelMode = TravelMode.AreaBased;
            }

            GUILayout.Space(4);

            // Vector Mode Button
            GUI.backgroundColor = isVectorMode ? UITheme.Info : UITheme.ButtonNormal;
            GUIStyle vectorBtnStyle = new GUIStyle(UITheme.ButtonStyle);
            vectorBtnStyle.fontSize = UITheme.FONT_SIZE_SMALL;
            if (isVectorMode)
                vectorBtnStyle.normal.textColor = UITheme.TextDark;

            if (GUILayout.Button("Vector", vectorBtnStyle, GUILayout.Height(24)))
            {
                _travelerAgent.TravelMode = TravelMode.VectorBased;
            }

            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            // Mode description
            GUILayout.Space(4);
            string modeDesc = isAreaMode 
                ? "Using named areas" 
                : "Using Vector3 positions";
            GUILayout.Label(modeDesc, UITheme.CreateLabelStyle(UITheme.TextMuted, 9));

            GUILayout.EndVertical();
        }

        private void DrawLevelBasedPanel(float availableHeight)
        {
            bool isVectorMode = _travelerAgent.TravelMode == TravelMode.VectorBased;

            if (isVectorMode)
            {
                DrawLevelVectorPanel(availableHeight);
            }
            else
            {
                DrawLevelAreaPanel(availableHeight);
            }
        }

        private void DrawLevelAreaPanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.Width(200));

            // Header
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
            try { bestArea = _travelerAgent.GetBestAreaForLevel(); } catch { }
            string targetArea = _travelerAgent.TargetAreaName;
            string forcedArea = _travelerAgent.ForcedAreaName;

            float scrollHeight = Mathf.Max(150, availableHeight - 50);
            _levelAreaScrollPosition = GUILayout.BeginScrollView(_levelAreaScrollPosition, GUILayout.Height(scrollHeight));

            foreach (var kvp in _travelerAgent.MinLevelAreaDictionary.OrderBy(kv => kv.Key))
            {
                bool isBest = kvp.Value == bestArea && !_travelerAgent.IsAnyForcedModeEnabled && _travelerAgent.TravelMode == TravelMode.AreaBased;
                bool isTarget = kvp.Value == targetArea;
                bool isForced = kvp.Value == forcedArea;
                DrawLevelAreaRow(kvp.Key, kvp.Value, isBest, isTarget, isForced);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawLevelVectorPanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.Width(280));

            // Header
            GUILayout.BeginHorizontal();
            GUILayout.Label("Level Vectors", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            // Add new level vector zone
            DrawAddLevelVectorForm();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            if (_travelerAgent.MinLevelVectorDictionary == null || _travelerAgent.MinLevelVectorDictionary.Count == 0)
            {
                GUILayout.Label("No level vectors configured", UITheme.LabelStyle);
                GUILayout.Label("Add zones above to use Vector mode", UITheme.CreateLabelStyle(UITheme.TextMuted, 9));
                GUILayout.EndVertical();
                return;
            }

            LevelVectorZone bestZone = null;
            try { bestZone = _travelerAgent.GetBestVectorZoneForLevel(); } catch { }

            float scrollHeight = Mathf.Max(100, availableHeight - 140);
            _levelVectorScrollPosition = GUILayout.BeginScrollView(_levelVectorScrollPosition, GUILayout.Height(scrollHeight));

            foreach (var zone in _travelerAgent.MinLevelVectorDictionary.OrderBy(z => z.MinLevel))
            {
                bool isBest = zone == bestZone && !_travelerAgent.IsAnyForcedModeEnabled && _travelerAgent.TravelMode == TravelMode.VectorBased;
                DrawLevelVectorRow(zone, isBest);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawAddLevelVectorForm()
        {
            // Level and Name row
            GUILayout.BeginHorizontal();
            GUILayout.Label("Lv:", UITheme.LabelStyle, GUILayout.Width(20));
            _newLevelVectorLevel = GUILayout.TextField(_newLevelVectorLevel, UITheme.InputStyle, GUILayout.Width(30), GUILayout.Height(18));
            GUILayout.Label("Name:", UITheme.LabelStyle, GUILayout.Width(38));
            _newLevelVectorName = GUILayout.TextField(_newLevelVectorName, UITheme.InputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(18));
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            // Position row
            GUILayout.BeginHorizontal();
            GUILayout.Label("X:", UITheme.LabelStyle, GUILayout.Width(15));
            _newLevelVectorX = GUILayout.TextField(_newLevelVectorX, UITheme.InputStyle, GUILayout.Width(45), GUILayout.Height(18));
            GUILayout.Label("Y:", UITheme.LabelStyle, GUILayout.Width(15));
            _newLevelVectorY = GUILayout.TextField(_newLevelVectorY, UITheme.InputStyle, GUILayout.Width(45), GUILayout.Height(18));
            GUILayout.Label("Z:", UITheme.LabelStyle, GUILayout.Width(15));
            _newLevelVectorZ = GUILayout.TextField(_newLevelVectorZ, UITheme.InputStyle, GUILayout.Width(45), GUILayout.Height(18));
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            // Radius and Add button row
            GUILayout.BeginHorizontal();
            GUILayout.Label("R:", UITheme.LabelStyle, GUILayout.Width(15));
            _newLevelVectorRadius = GUILayout.TextField(_newLevelVectorRadius, UITheme.InputStyle, GUILayout.Width(40), GUILayout.Height(18));
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = UITheme.Positive;
            GUIStyle addBtnStyle = new GUIStyle(UITheme.ButtonStyle);
            addBtnStyle.fontSize = UITheme.FONT_SIZE_SMALL;
            if (GUILayout.Button("Add", addBtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                TryAddLevelVectorZone();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
        }

        private void TryAddLevelVectorZone()
        {
            if (string.IsNullOrWhiteSpace(_newLevelVectorName)) return;
            if (!int.TryParse(_newLevelVectorLevel, out int level)) return;
            if (!float.TryParse(_newLevelVectorX, out float x)) return;
            if (!float.TryParse(_newLevelVectorY, out float y)) return;
            if (!float.TryParse(_newLevelVectorZ, out float z)) return;
            if (!float.TryParse(_newLevelVectorRadius, out float radius)) return;

            if (radius <= 0) radius = 50f;

            _travelerAgent.AddLevelVectorZone(level, _newLevelVectorName.Trim(), x, y, z, radius);

            // Reset inputs
            _newLevelVectorLevel = "1";
            _newLevelVectorName = "";
            _newLevelVectorX = "0";
            _newLevelVectorY = "0";
            _newLevelVectorZ = "0";
            _newLevelVectorRadius = "50";
        }

        private void DrawLevelVectorRow(LevelVectorZone zone, bool isBest)
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle, GUILayout.Height(36));

            GUILayout.BeginVertical();

            // Level badge and name
            GUILayout.BeginHorizontal();
            Color levelColor = isBest ? UITheme.Accent : UITheme.Info;
            GUILayout.Label($"L{zone.MinLevel}", UITheme.CreateValueStyle(levelColor, UITheme.FONT_SIZE_SMALL), GUILayout.Width(25));

            Color nameColor = isBest ? UITheme.Accent : UITheme.TextLight;
            FontStyle fontStyle = isBest ? FontStyle.Bold : FontStyle.Normal;
            GUILayout.Label(TruncateText(zone.Name, 14), UITheme.CreateLabelStyle(nameColor, UITheme.FONT_SIZE_SMALL, fontStyle));
            GUILayout.EndHorizontal();

            // Position info
            string posInfo = $"({zone.Position.x:F0},{zone.Position.y:F0},{zone.Position.z:F0}) R:{zone.Radius:F0}";
            GUILayout.Label(posInfo, UITheme.CreateLabelStyle(UITheme.TextMuted, 9));

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // Best indicator
            if (isBest)
            {
                GUILayout.Label("★", UITheme.CreateLabelStyle(UITheme.Accent, 12), GUILayout.Width(14));
            }
            else
            {
                GUILayout.Space(14);
            }

            // Remove button
            GUI.backgroundColor = UITheme.Danger;
            GUIStyle removeBtnStyle = new GUIStyle(UITheme.ButtonStyle);
            removeBtnStyle.fontSize = UITheme.FONT_SIZE_SMALL;
            if (GUILayout.Button("X", removeBtnStyle, GUILayout.Width(22), GUILayout.Height(20)))
            {
                _travelerAgent.RemoveLevelVectorZone(zone.MinLevel);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();
            GUILayout.Space(1);
        }

        private void DrawCustomAreasPanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.Width(160));

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

        private void DrawCustomVectorZonesPanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.Width(230));

            // Header with clear button
            GUILayout.BeginHorizontal();
            GUILayout.Label("Custom Vectors", UITheme.SubtitleStyle);
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
            GUILayout.Label("Name:", UITheme.LabelStyle, GUILayout.Width(40));
            _newVectorZoneName = GUILayout.TextField(_newVectorZoneName, UITheme.InputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(18));
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            // Position inputs (X, Y, Z)
            GUILayout.BeginHorizontal();
            GUILayout.Label("X:", UITheme.LabelStyle, GUILayout.Width(15));
            _newVectorZoneX = GUILayout.TextField(_newVectorZoneX, UITheme.InputStyle, GUILayout.Width(45), GUILayout.Height(18));
            GUILayout.Label("Y:", UITheme.LabelStyle, GUILayout.Width(15));
            _newVectorZoneY = GUILayout.TextField(_newVectorZoneY, UITheme.InputStyle, GUILayout.Width(45), GUILayout.Height(18));
            GUILayout.Label("Z:", UITheme.LabelStyle, GUILayout.Width(15));
            _newVectorZoneZ = GUILayout.TextField(_newVectorZoneZ, UITheme.InputStyle, GUILayout.Width(45), GUILayout.Height(18));
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            // Radius input and Add button
            GUILayout.BeginHorizontal();
            GUILayout.Label("Radius:", UITheme.LabelStyle, GUILayout.Width(40));
            _newVectorZoneRadius = GUILayout.TextField(_newVectorZoneRadius, UITheme.InputStyle, GUILayout.Width(40), GUILayout.Height(18));
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = UITheme.Positive;
            GUIStyle addBtnStyle = new GUIStyle(UITheme.ButtonStyle);
            addBtnStyle.fontSize = UITheme.FONT_SIZE_SMALL;
            if (GUILayout.Button("Add", addBtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
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
                GUILayout.Label("No custom vectors", UITheme.LabelStyle);
                GUILayout.EndVertical();
                return;
            }

            float scrollHeight = Mathf.Max(60, availableHeight - 180);
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
            GUILayout.Label("Active", UITheme.CreateLabelStyle(UITheme.Warning, UITheme.FONT_SIZE_SMALL, FontStyle.Bold));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Label(zone.Name, UITheme.CreateValueStyle(UITheme.Warning, UITheme.FONT_SIZE_SMALL));

            float distance = _travelerAgent.GetDistanceToForcedVectorZone();
            bool inZone = _travelerAgent.IsHeroInForcedVectorZone();
            Color distColor = inZone ? UITheme.Positive : UITheme.Warning;
            string distText = distance >= 0 ? $"D:{distance:F0} R:{zone.Radius:F0}" : "N/A";
            GUILayout.Label(distText, UITheme.CreateLabelStyle(distColor, 9));

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

            Color nameColor = isForcedZone ? UITheme.Warning : UITheme.TextLight;
            FontStyle fontStyle = isForcedZone ? FontStyle.Bold : FontStyle.Normal;
            GUILayout.Label(TruncateText(zone.Name, 14), UITheme.CreateLabelStyle(nameColor, UITheme.FONT_SIZE_SMALL, fontStyle));

            string posInfo = $"({zone.Position.x:F0},{zone.Position.y:F0},{zone.Position.z:F0}) R:{zone.Radius:F0}";
            GUILayout.Label(posInfo, UITheme.CreateLabelStyle(UITheme.TextMuted, 9));

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (isForcedZone)
            {
                GUILayout.Label("⚡", UITheme.CreateLabelStyle(UITheme.Warning, 12), GUILayout.Width(14));
            }
            else
            {
                GUILayout.Space(14);
            }

            DrawVectorZoneForceButton(zone, isForcedZone);

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

                if (GUILayout.Button("On", unforceBtnStyle, GUILayout.Width(28), GUILayout.Height(20)))
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

                if (GUILayout.Button("Off", forceBtnStyle, GUILayout.Width(28), GUILayout.Height(20)))
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

            // Mode indicator
            string modeText = "AUTO";
            Color modeColor = UITheme.Positive;

            if (_travelerAgent.IsForcedVectorZoneEnabled)
            {
                modeText = "FORCED V";
                modeColor = UITheme.Warning;
            }
            else if (_travelerAgent.IsForcedAreaEnabled)
            {
                modeText = "FORCED A";
                modeColor = UITheme.Warning;
            }
            else if (_travelerAgent.TravelMode == TravelMode.VectorBased)
            {
                modeText = "VECTOR";
                modeColor = UITheme.Info;
            }
            else
            {
                modeText = "AREA";
                modeColor = UITheme.Accent;
            }

            DrawStatBox("Mode", modeText, modeColor);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawStatBox(string label, string value, Color valueColor)
        {
            GUILayout.BeginVertical(GUILayout.Width(70));
            GUILayout.Label(label, UITheme.LabelStyle);
            GUILayout.Label(value, UITheme.CreateValueStyle(valueColor, 11));
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

            GUILayout.BeginHorizontal();
            GUILayout.Label("Current:", UITheme.LabelStyle, GUILayout.Width(55));
            string currentInfo = string.IsNullOrEmpty(_travelerAgent.TargetAreaName) ? "At dest." : "Transit...";
            GUILayout.Label(currentInfo, UITheme.CreateValueStyle(UITheme.TextLight));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target:", UITheme.LabelStyle, GUILayout.Width(55));
            string targetName = string.IsNullOrEmpty(_travelerAgent.TargetAreaName) ? "None" : _travelerAgent.TargetAreaName;
            Color targetColor = string.IsNullOrEmpty(_travelerAgent.TargetAreaName) ? UITheme.TextMuted : UITheme.Warning;
            GUILayout.Label(TruncateText(targetName, 18), UITheme.CreateValueStyle(targetColor, UITheme.FONT_SIZE_SMALL));
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
                GUI.color = UITheme.Warning;
                GUILayout.Label("⚡", GUILayout.Width(20));
            }
            else if (_travelerAgent.IsForcedAreaEnabled)
            {
                GUI.color = UITheme.Warning;
                GUILayout.Label("⚡", GUILayout.Width(20));
            }
            else if (_travelerAgent.TravelMode == TravelMode.VectorBased)
            {
                GUI.color = UITheme.Info;
                GUILayout.Label("◎", GUILayout.Width(20));
            }
            else
            {
                GUI.color = UITheme.Accent;
                GUILayout.Label("★", GUILayout.Width(20));
            }
            GUI.color = Color.white;

            string headerText = _travelerAgent.IsForcedVectorZoneEnabled ? "Forced Zone" :
                               (_travelerAgent.IsForcedAreaEnabled ? "Forced Area" :
                               (_travelerAgent.TravelMode == TravelMode.VectorBased ? "Best Zone" : "Best Area"));
            GUILayout.Label(headerText, UITheme.SubtitleStyle);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            // Display info based on mode
            if (_travelerAgent.IsForcedVectorZoneEnabled && _travelerAgent.ForcedVectorZone != null)
            {
                var zone = _travelerAgent.ForcedVectorZone;
                GUILayout.Label(zone.Name, UITheme.CreateValueStyle(UITheme.Warning, 11));
                GUILayout.Label($"({zone.Position.x:F0}, {zone.Position.y:F0}, {zone.Position.z:F0})", UITheme.CreateLabelStyle(UITheme.TextMuted, 9));
            }
            else if (_travelerAgent.IsForcedAreaEnabled)
            {
                GUILayout.Label(_travelerAgent.ForcedAreaName, UITheme.CreateValueStyle(UITheme.Warning, 11));
            }
            else if (_travelerAgent.TravelMode == TravelMode.VectorBased)
            {
                var bestZone = _travelerAgent.GetBestVectorZoneForLevel();
                if (bestZone != null)
                {
                    GUILayout.Label($"L{bestZone.MinLevel}: {bestZone.Name}", UITheme.CreateValueStyle(UITheme.Info, 11));
                    GUILayout.Label($"({bestZone.Position.x:F0}, {bestZone.Position.y:F0}, {bestZone.Position.z:F0})", UITheme.CreateLabelStyle(UITheme.TextMuted, 9));
                }
                else
                {
                    GUILayout.Label("No zones configured", UITheme.CreateValueStyle(UITheme.TextMuted, 11));
                }
            }
            else
            {
                string bestArea = "";
                try { bestArea = _travelerAgent.GetBestAreaForLevel(); } catch { bestArea = "Unknown"; }
                GUILayout.Label(bestArea, UITheme.CreateValueStyle(UITheme.Accent, 11));
            }

            GUILayout.EndVertical();
        }

        private void DrawLevelAreaRow(int minLevel, string areaName, bool isBestArea, bool isTargetArea, bool isForcedArea)
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle, GUILayout.Height(28));

            GUILayout.Label($"L{minLevel}", UITheme.CreateValueStyle(UITheme.Info, UITheme.FONT_SIZE_SMALL), GUILayout.Width(25));
            GUILayout.Space(4);

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

            GUILayout.Label(TruncateText(areaName, 16), UITheme.CreateLabelStyle(nameColor, UITheme.FONT_SIZE_SMALL, fontStyle), GUILayout.ExpandWidth(true));

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

            DrawForceButton(areaName, isForcedArea);

            GUILayout.EndHorizontal();
            GUILayout.Space(1);
        }

        private void DrawCustomAreaRow(string areaName, bool isForcedArea, bool isTargetArea)
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle, GUILayout.Height(28));

            Color nameColor = isForcedArea ? UITheme.Warning : UITheme.TextLight;
            FontStyle fontStyle = isForcedArea ? FontStyle.Bold : FontStyle.Normal;

            GUILayout.Label(TruncateText(areaName, 12), UITheme.CreateLabelStyle(nameColor, UITheme.FONT_SIZE_SMALL, fontStyle), GUILayout.ExpandWidth(true));

            if (isForcedArea)
            {
                GUILayout.Label("⚡", UITheme.CreateLabelStyle(UITheme.Warning, 12), GUILayout.Width(14));
            }
            else
            {
                GUILayout.Space(14);
            }

            DrawForceButton(areaName, isForcedArea);

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

                if (GUILayout.Button("On", unforceBtnStyle, GUILayout.Width(28), GUILayout.Height(20)))
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

                if (GUILayout.Button("Off", forceBtnStyle, GUILayout.Width(28), GUILayout.Height(20)))
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
