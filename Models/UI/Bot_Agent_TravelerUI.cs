using Scripts.Models.PathFinding;
using System.Collections.Generic;
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

        // Mini-map textures (cached)
        private Texture2D _mapBackgroundTexture;

        // Mini-map colors
        private static readonly Color MapBackgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        private static readonly Color WaypointNormalColor = new Color(0.5f, 0.5f, 0.6f);
        private static readonly Color WaypointBestColor = new Color(0.2f, 0.8f, 0.4f);
        private static readonly Color WaypointTransitColor = new Color(0.6f, 0.8f, 1f);
        private static readonly Color PlayerColor = new Color(1f, 0.8f, 0.2f);
        private static readonly Color PathLineColor = new Color(0.4f, 0.4f, 0.5f, 0.6f);

        public Bot_Agent_TravelerUI(Bot_Agent_Traveler travelerAgent)
        {
            _travelerAgent = travelerAgent;
            InitializeMapTextures();
        }

        private void InitializeMapTextures()
        {
            _mapBackgroundTexture = new Texture2D(1, 1);
            _mapBackgroundTexture.SetPixel(0, 0, Color.white);
            _mapBackgroundTexture.Apply();
        }

        public void DrawTravelerAgentPanel(Rect contentArea)
        {
            if (_travelerAgent == null) return;

            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(UITheme.SectionStyle);

            DrawSectionHeader("TRAVELER AGENT");
            GUILayout.Space(UITheme.BUTTON_SPACING);

            GUILayout.BeginHorizontal();
            DrawLeftPanel();
            GUILayout.Space(UITheme.SECTION_SPACING);
            DrawLevelBasedPanel(contentArea.height - 60);
            GUILayout.Space(UITheme.SECTION_SPACING);

            if (_travelerAgent.TravelMode == TravelMode.VectorBased)
                DrawMiniMapPanel(contentArea.height - 60);
            else
                DrawCustomAreasPanel(contentArea.height - 60);

            GUILayout.Space(UITheme.SECTION_SPACING);
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
            DrawStatusSection();
            GUILayout.Space(UITheme.SECTION_SPACING);
            DrawModeSwitchCard();
            GUILayout.Space(UITheme.SECTION_SPACING);
            DrawAreaInfoCard();
            GUILayout.Space(UITheme.SECTION_SPACING);
            DrawBestAreaCard();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DrawModeSwitchCard()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);
            GUILayout.BeginHorizontal();
            GUI.color = UITheme.Info;
            GUILayout.Label("?", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Travel Mode", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            GUILayout.BeginHorizontal();
            bool isAreaMode = _travelerAgent.TravelMode == TravelMode.AreaBased;
            bool isVectorMode = _travelerAgent.TravelMode == TravelMode.VectorBased;

            GUI.backgroundColor = isAreaMode ? UITheme.Accent : UITheme.ButtonNormal;
            GUIStyle areaBtnStyle = new GUIStyle(UITheme.ButtonStyle) { fontSize = UITheme.FONT_SIZE_SMALL };
            if (isAreaMode) areaBtnStyle.normal.textColor = UITheme.TextDark;
            if (GUILayout.Button("Area", areaBtnStyle, GUILayout.Height(24)))
                _travelerAgent.TravelMode = TravelMode.AreaBased;

            GUILayout.Space(4);

            GUI.backgroundColor = isVectorMode ? UITheme.Info : UITheme.ButtonNormal;
            GUIStyle vectorBtnStyle = new GUIStyle(UITheme.ButtonStyle) { fontSize = UITheme.FONT_SIZE_SMALL };
            if (isVectorMode) vectorBtnStyle.normal.textColor = UITheme.TextDark;
            if (GUILayout.Button("Vector", vectorBtnStyle, GUILayout.Height(24)))
                _travelerAgent.TravelMode = TravelMode.VectorBased;

            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            GUILayout.Label(isAreaMode ? "Using named areas" : "Using Vector3 positions", UITheme.CreateLabelStyle(UITheme.TextMuted, 9));
            GUILayout.EndVertical();
        }

        private void DrawLevelBasedPanel(float availableHeight)
        {
            if (_travelerAgent.TravelMode == TravelMode.VectorBased)
                DrawLevelVectorPanel(availableHeight);
            else
                DrawLevelAreaPanel(availableHeight);
        }

        private void DrawLevelAreaPanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.Width(200));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Level Areas", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();
            if (_travelerAgent.IsForcedAreaEnabled && _travelerAgent.MinLevelAreaDictionary.ContainsValue(_travelerAgent.ForcedAreaName))
            {
                GUI.backgroundColor = UITheme.Danger;
                if (GUILayout.Button("Clear", UITheme.ButtonStyle, GUILayout.Width(50), GUILayout.Height(18)))
                    _travelerAgent.ClearForcedArea();
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

            float scrollHeight = Mathf.Max(150, availableHeight - 50);
            _levelAreaScrollPosition = GUILayout.BeginScrollView(_levelAreaScrollPosition, GUILayout.Height(scrollHeight));
            foreach (var kvp in _travelerAgent.MinLevelAreaDictionary.OrderBy(kv => kv.Key))
            {
                bool isBest = kvp.Value == bestArea && !_travelerAgent.IsAnyForcedModeEnabled && _travelerAgent.TravelMode == TravelMode.AreaBased;
                bool isForced = kvp.Value == _travelerAgent.ForcedAreaName;
                DrawLevelAreaRow(kvp.Key, kvp.Value, isBest, isForced);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawLevelVectorPanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.Width(280));
            GUILayout.Label("Level Vectors", UITheme.SubtitleStyle);
            GUILayout.Space(2);
            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.ELEMENT_SPACING);
            DrawAddLevelVectorForm();
            GUILayout.Space(UITheme.ELEMENT_SPACING);
            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            if (_travelerAgent.MinLevelVectorDictionary == null || _travelerAgent.MinLevelVectorDictionary.Count == 0)
            {
                GUILayout.Label("No level vectors configured", UITheme.LabelStyle);
                GUILayout.EndVertical();
                return;
            }

            LevelVectorZone bestZone = null;
            try { bestZone = _travelerAgent.GetBestVectorZoneForLevel(); } catch { }

            float scrollHeight = Mathf.Max(100, availableHeight - 140);
            _levelVectorScrollPosition = GUILayout.BeginScrollView(_levelVectorScrollPosition, GUILayout.Height(scrollHeight));
            foreach (var zone in _travelerAgent.MinLevelVectorDictionary.OrderBy(z => z.MinLevel))
            {
                bool isBest = zone == bestZone && !_travelerAgent.IsAnyForcedModeEnabled;
                DrawLevelVectorRow(zone, isBest);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawAddLevelVectorForm()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Lv:", UITheme.LabelStyle, GUILayout.Width(20));
            _newLevelVectorLevel = GUILayout.TextField(_newLevelVectorLevel, UITheme.InputStyle, GUILayout.Width(30), GUILayout.Height(18));
            GUILayout.Label("Name:", UITheme.LabelStyle, GUILayout.Width(38));
            _newLevelVectorName = GUILayout.TextField(_newLevelVectorName, UITheme.InputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(18));
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            GUILayout.Label("X:", UITheme.LabelStyle, GUILayout.Width(15));
            _newLevelVectorX = GUILayout.TextField(_newLevelVectorX, UITheme.InputStyle, GUILayout.Width(45), GUILayout.Height(18));
            GUILayout.Label("Y:", UITheme.LabelStyle, GUILayout.Width(15));
            _newLevelVectorY = GUILayout.TextField(_newLevelVectorY, UITheme.InputStyle, GUILayout.Width(45), GUILayout.Height(18));
            GUILayout.Label("Z:", UITheme.LabelStyle, GUILayout.Width(15));
            _newLevelVectorZ = GUILayout.TextField(_newLevelVectorZ, UITheme.InputStyle, GUILayout.Width(45), GUILayout.Height(18));
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            GUILayout.Label("R:", UITheme.LabelStyle, GUILayout.Width(15));
            _newLevelVectorRadius = GUILayout.TextField(_newLevelVectorRadius, UITheme.InputStyle, GUILayout.Width(40), GUILayout.Height(18));
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = UITheme.Positive;
            if (GUILayout.Button("Add", UITheme.ButtonStyle, GUILayout.Width(50), GUILayout.Height(18)))
                TryAddLevelVectorZone();
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
            _newLevelVectorLevel = "1"; _newLevelVectorName = "";
            _newLevelVectorX = "0"; _newLevelVectorY = "0"; _newLevelVectorZ = "0"; _newLevelVectorRadius = "50";
        }

        private void DrawLevelVectorRow(LevelVectorZone zone, bool isBest)
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle, GUILayout.Height(36));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label($"L{zone.MinLevel}", UITheme.CreateValueStyle(isBest ? UITheme.Accent : UITheme.Info, UITheme.FONT_SIZE_SMALL), GUILayout.Width(25));
            GUILayout.Label(TruncateText(zone.Name, 14), UITheme.CreateLabelStyle(isBest ? UITheme.Accent : UITheme.TextLight, UITheme.FONT_SIZE_SMALL, isBest ? FontStyle.Bold : FontStyle.Normal));
            GUILayout.EndHorizontal();
            GUILayout.Label($"({zone.Position.x:F0},{zone.Position.y:F0},{zone.Position.z:F0}) R:{zone.Radius:F0}", UITheme.CreateLabelStyle(UITheme.TextMuted, 9));
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            if (isBest) GUILayout.Label("?", UITheme.CreateLabelStyle(UITheme.Accent, 12), GUILayout.Width(14));
            else GUILayout.Space(14);

            GUI.backgroundColor = UITheme.Danger;
            if (GUILayout.Button("X", UITheme.ButtonStyle, GUILayout.Width(22), GUILayout.Height(20)))
                _travelerAgent.RemoveLevelVectorZone(zone.MinLevel);
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(1);
        }

        private void DrawMiniMapPanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.Width(200));
            GUILayout.Label("Mini-Map", UITheme.SubtitleStyle);
            GUILayout.Space(2);
            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            float mapSize = Mathf.Min(180, availableHeight - 40);
            Rect mapRect = GUILayoutUtility.GetRect(mapSize, mapSize);
            DrawMiniMapContent(mapRect);
            GUILayout.Space(4);
            DrawMiniMapLegend();
            GUILayout.EndVertical();
        }

        private void DrawMiniMapContent(Rect mapRect)
        {
            if (_travelerAgent.MinLevelVectorDictionary == null || _travelerAgent.MinLevelVectorDictionary.Count == 0)
            {
                GUI.color = MapBackgroundColor;
                GUI.DrawTexture(mapRect, _mapBackgroundTexture);
                GUI.color = Color.white;
                GUIStyle centeredStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                centeredStyle.normal.textColor = UITheme.TextMuted;
                GUI.Label(mapRect, "No waypoints", centeredStyle);
                return;
            }

            GUI.color = MapBackgroundColor;
            GUI.DrawTexture(mapRect, _mapBackgroundTexture);
            GUI.color = Color.white;

            var bounds = CalculateMapBounds();
            if (bounds.size.x == 0 && bounds.size.z == 0) return;

            float padding = 20f;
            float mapWidth = mapRect.width - padding * 2;
            float mapHeight = mapRect.height - padding * 2;

            var orderedZones = _travelerAgent.MinLevelVectorDictionary.OrderBy(z => z.MinLevel).ToList();
            LevelVectorZone bestZone = null;
            try { bestZone = _travelerAgent.GetBestVectorZoneForLevel(); } catch { }
            var transitWaypoint = _travelerAgent.CurrentPathWaypoint;
            Vector3 heroPos = _travelerAgent.HeroPosition;

            DrawPathLines(mapRect, orderedZones, bounds, padding, mapWidth, mapHeight);

            for (int i = 0; i < orderedZones.Count; i++)
            {
                var zone = orderedZones[i];
                Vector2 screenPos = WorldToMapPosition(zone.Position, mapRect, bounds, padding, mapWidth, mapHeight);
                Color wpColor = WaypointNormalColor;
                float wpSize = 8f;
                bool isBest = zone == bestZone;
                bool isTransit = zone == transitWaypoint && transitWaypoint != bestZone;

                if (isBest) { wpColor = WaypointBestColor; wpSize = 12f; }
                else if (isTransit) { wpColor = WaypointTransitColor; wpSize = 10f; }

                DrawCircle(screenPos, wpSize, wpColor);
                DrawCenteredLabel(screenPos, (i + 1).ToString(), isBest || isTransit ? Color.white : UITheme.TextLight, 9);

                if (isBest || isTransit)
                    DrawCircleOutline(screenPos, ScaleRadius(zone.Radius, bounds, mapWidth, mapHeight), wpColor * 0.5f);
            }

            if (_travelerAgent.HasValidHero)
            {
                Vector2 heroScreenPos = WorldToMapPosition(heroPos, mapRect, bounds, padding, mapWidth, mapHeight);
                bool heroAtWaypoint = orderedZones.Any(z => Vector3.Distance(heroPos, z.Position) <= z.Radius);
                float heroSize = heroAtWaypoint ? 10f : 8f;
                Color heroColor = heroAtWaypoint ? UITheme.Positive : PlayerColor;
                DrawDiamond(heroScreenPos, heroSize, heroColor);
                DrawCenteredLabel(heroScreenPos + new Vector2(0, -12), "P", heroColor, 8);
            }

            DrawRectOutline(mapRect, new Color(0.4f, 0.4f, 0.5f));
        }

        private Bounds CalculateMapBounds()
        {
            Bounds bounds = new Bounds();
            bool initialized = false;
            foreach (var zone in _travelerAgent.MinLevelVectorDictionary)
            {
                Vector3 pos = new Vector3(zone.Position.x, 0, zone.Position.z);
                if (!initialized) { bounds = new Bounds(pos, Vector3.zero); initialized = true; }
                else bounds.Encapsulate(pos);
            }
            if (_travelerAgent.HasValidHero)
            {
                Vector3 heroPos2D = new Vector3(_travelerAgent.HeroPosition.x, 0, _travelerAgent.HeroPosition.z);
                if (!initialized) { bounds = new Bounds(heroPos2D, Vector3.zero); initialized = true; }
                else bounds.Encapsulate(heroPos2D);
            }
            bounds.Expand(bounds.size * 0.1f + Vector3.one * 50f);
            return bounds;
        }

        private Vector2 WorldToMapPosition(Vector3 worldPos, Rect mapRect, Bounds bounds, float padding, float mapWidth, float mapHeight)
        {
            float normalizedX = Mathf.Clamp01((worldPos.x - bounds.min.x) / bounds.size.x);
            float normalizedZ = Mathf.Clamp01((worldPos.z - bounds.min.z) / bounds.size.z);
            return new Vector2(mapRect.x + padding + normalizedX * mapWidth, mapRect.y + padding + (1f - normalizedZ) * mapHeight);
        }

        private float ScaleRadius(float worldRadius, Bounds bounds, float mapWidth, float mapHeight)
        {
            float avgWorldSize = (bounds.size.x + bounds.size.z) / 2f;
            float avgMapSize = (mapWidth + mapHeight) / 2f;
            if (avgWorldSize <= 0) return 5f;
            return Mathf.Clamp(worldRadius / avgWorldSize * avgMapSize, 5f, 30f);
        }

        private void DrawPathLines(Rect mapRect, List<LevelVectorZone> zones, Bounds bounds, float padding, float mapWidth, float mapHeight)
        {
            if (zones.Count < 2) return;
            for (int i = 0; i < zones.Count - 1; i++)
            {
                Vector2 pos1 = WorldToMapPosition(zones[i].Position, mapRect, bounds, padding, mapWidth, mapHeight);
                Vector2 pos2 = WorldToMapPosition(zones[i + 1].Position, mapRect, bounds, padding, mapWidth, mapHeight);
                DrawLine(pos1, pos2, PathLineColor, 2f);
            }
        }

        private void DrawCircle(Vector2 center, float radius, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(center.x - radius, center.y - radius, radius * 2, radius * 2), _mapBackgroundTexture);
            GUI.color = Color.white;
        }

        private void DrawCircleOutline(Vector2 center, float radius, Color color)
        {
            int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (float)i / segments * Mathf.PI * 2;
                float angle2 = (float)(i + 1) / segments * Mathf.PI * 2;
                Vector2 p1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
                Vector2 p2 = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;
                DrawLine(p1, p2, color, 1f);
            }
        }

        private void DrawDiamond(Vector2 center, float size, Color color)
        {
            Vector2[] points = { center + new Vector2(0, -size), center + new Vector2(size * 0.7f, 0), center + new Vector2(0, size), center + new Vector2(-size * 0.7f, 0) };
            for (int i = 0; i < 4; i++) DrawLine(points[i], points[(i + 1) % 4], color, 2f);
            GUI.color = color;
            GUI.DrawTexture(new Rect(center.x - size * 0.3f, center.y - size * 0.3f, size * 0.6f, size * 0.6f), _mapBackgroundTexture);
            GUI.color = Color.white;
        }

        private void DrawLine(Vector2 p1, Vector2 p2, Color color, float width)
        {
            Matrix4x4 matrix = GUI.matrix;
            GUI.color = color;
            float angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * Mathf.Rad2Deg;
            float length = Vector2.Distance(p1, p2);
            GUIUtility.RotateAroundPivot(angle, p1);
            GUI.DrawTexture(new Rect(p1.x, p1.y - width / 2, length, width), _mapBackgroundTexture);
            GUI.matrix = matrix;
            GUI.color = Color.white;
        }

        private void DrawRectOutline(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 1), _mapBackgroundTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 1, rect.width, 1), _mapBackgroundTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, 1, rect.height), _mapBackgroundTexture);
            GUI.DrawTexture(new Rect(rect.xMax - 1, rect.y, 1, rect.height), _mapBackgroundTexture);
            GUI.color = Color.white;
        }

        private void DrawCenteredLabel(Vector2 position, string text, Color color, int fontSize)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = fontSize, fontStyle = FontStyle.Bold };
            style.normal.textColor = color;
            Vector2 size = style.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(position.x - size.x / 2, position.y - size.y / 2, size.x, size.y), text, style);
        }

        private void DrawMiniMapLegend()
        {
            GUILayout.BeginHorizontal();
            GUI.color = PlayerColor; GUILayout.Label("?", GUILayout.Width(12)); GUI.color = Color.white;
            GUILayout.Label("Player", UITheme.CreateLabelStyle(UITheme.TextMuted, 8), GUILayout.Width(35));
            GUI.color = WaypointBestColor; GUILayout.Label("?", GUILayout.Width(12)); GUI.color = Color.white;
            GUILayout.Label("Best", UITheme.CreateLabelStyle(UITheme.TextMuted, 8), GUILayout.Width(25));
            GUI.color = WaypointTransitColor; GUILayout.Label("?", GUILayout.Width(12)); GUI.color = Color.white;
            GUILayout.Label("Transit", UITheme.CreateLabelStyle(UITheme.TextMuted, 8));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawCustomAreasPanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.Width(160));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Custom Areas", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();
            if (_travelerAgent.IsForcedAreaEnabled && _travelerAgent.CustomAreaList.Contains(_travelerAgent.ForcedAreaName))
            {
                GUI.backgroundColor = UITheme.Danger;
                if (GUILayout.Button("Clear", UITheme.ButtonStyle, GUILayout.Width(50), GUILayout.Height(18)))
                    _travelerAgent.ClearForcedArea();
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            GUILayout.BeginHorizontal();
            _newCustomAreaName = GUILayout.TextField(_newCustomAreaName, UITheme.InputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            GUI.backgroundColor = UITheme.Positive;
            if (GUILayout.Button("+", UITheme.ButtonStyle, GUILayout.Width(25), GUILayout.Height(20)) && !string.IsNullOrWhiteSpace(_newCustomAreaName))
            {
                _travelerAgent.AddCustomArea(_newCustomAreaName.Trim());
                _newCustomAreaName = "";
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

            float scrollHeight = Mathf.Max(120, availableHeight - 80);
            _customAreaScrollPosition = GUILayout.BeginScrollView(_customAreaScrollPosition, GUILayout.Height(scrollHeight));
            foreach (var areaName in _travelerAgent.CustomAreaList.ToList())
                DrawCustomAreaRow(areaName, areaName == _travelerAgent.ForcedAreaName);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawCustomVectorZonesPanel(float availableHeight)
        {
            GUILayout.BeginVertical(GUILayout.Width(230));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Custom Vectors", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();
            if (_travelerAgent.IsForcedVectorZoneEnabled)
            {
                GUI.backgroundColor = UITheme.Danger;
                if (GUILayout.Button("Clear", UITheme.ButtonStyle, GUILayout.Width(50), GUILayout.Height(18)))
                    _travelerAgent.ClearForcedVectorZone();
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", UITheme.LabelStyle, GUILayout.Width(40));
            _newVectorZoneName = GUILayout.TextField(_newVectorZoneName, UITheme.InputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(18));
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            GUILayout.Label("X:", UITheme.LabelStyle, GUILayout.Width(15));
            _newVectorZoneX = GUILayout.TextField(_newVectorZoneX, UITheme.InputStyle, GUILayout.Width(45), GUILayout.Height(18));
            GUILayout.Label("Y:", UITheme.LabelStyle, GUILayout.Width(15));
            _newVectorZoneY = GUILayout.TextField(_newVectorZoneY, UITheme.InputStyle, GUILayout.Width(45), GUILayout.Height(18));
            GUILayout.Label("Z:", UITheme.LabelStyle, GUILayout.Width(15));
            _newVectorZoneZ = GUILayout.TextField(_newVectorZoneZ, UITheme.InputStyle, GUILayout.Width(45), GUILayout.Height(18));
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Radius:", UITheme.LabelStyle, GUILayout.Width(40));
            _newVectorZoneRadius = GUILayout.TextField(_newVectorZoneRadius, UITheme.InputStyle, GUILayout.Width(40), GUILayout.Height(18));
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = UITheme.Positive;
            if (GUILayout.Button("Add", UITheme.ButtonStyle, GUILayout.Width(50), GUILayout.Height(18)))
                TryAddVectorZone();
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            if (_travelerAgent.IsForcedVectorZoneEnabled)
                DrawForcedVectorZoneInfo();

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
                DrawVectorZoneRow(zone, _travelerAgent.ForcedVectorZone == zone);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void TryAddVectorZone()
        {
            if (string.IsNullOrWhiteSpace(_newVectorZoneName)) return;
            if (!float.TryParse(_newVectorZoneX, out float x)) return;
            if (!float.TryParse(_newVectorZoneY, out float y)) return;
            if (!float.TryParse(_newVectorZoneZ, out float z)) return;
            if (!float.TryParse(_newVectorZoneRadius, out float radius)) return;
            if (radius <= 0) radius = 10f;

            _travelerAgent.AddCustomVectorZone(_newVectorZoneName.Trim(), x, y, z, radius);
            _newVectorZoneName = ""; _newVectorZoneX = "0"; _newVectorZoneY = "0"; _newVectorZoneZ = "0"; _newVectorZoneRadius = "10";
        }

        private void DrawForcedVectorZoneInfo()
        {
            var zone = _travelerAgent.ForcedVectorZone;
            if (zone == null) return;
            GUILayout.BeginVertical(UITheme.CardStyle);
            GUILayout.BeginHorizontal();
            GUI.color = UITheme.Warning; GUILayout.Label("?", GUILayout.Width(14)); GUI.color = Color.white;
            GUILayout.Label("Active", UITheme.CreateLabelStyle(UITheme.Warning, UITheme.FONT_SIZE_SMALL, FontStyle.Bold));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label(zone.Name, UITheme.CreateValueStyle(UITheme.Warning, UITheme.FONT_SIZE_SMALL));
            float distance = _travelerAgent.GetDistanceToForcedVectorZone();
            bool inZone = _travelerAgent.IsHeroInForcedVectorZone();
            GUILayout.Label(distance >= 0 ? $"D:{distance:F0} R:{zone.Radius:F0}" : "N/A", UITheme.CreateLabelStyle(inZone ? UITheme.Positive : UITheme.Warning, 9));
            GUILayout.EndVertical();
            GUILayout.Space(UITheme.ELEMENT_SPACING);
        }

        private void DrawVectorZoneRow(VectorZone zone, bool isForcedZone)
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle, GUILayout.Height(36));
            GUILayout.BeginVertical();
            GUILayout.Label(TruncateText(zone.Name, 14), UITheme.CreateLabelStyle(isForcedZone ? UITheme.Warning : UITheme.TextLight, UITheme.FONT_SIZE_SMALL, isForcedZone ? FontStyle.Bold : FontStyle.Normal));
            GUILayout.Label($"({zone.Position.x:F0},{zone.Position.y:F0},{zone.Position.z:F0}) R:{zone.Radius:F0}", UITheme.CreateLabelStyle(UITheme.TextMuted, 9));
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            if (isForcedZone) GUILayout.Label("?", UITheme.CreateLabelStyle(UITheme.Warning, 12), GUILayout.Width(14));
            else GUILayout.Space(14);

            GUI.backgroundColor = isForcedZone ? UITheme.Warning : UITheme.ButtonNormal;
            if (GUILayout.Button(isForcedZone ? "On" : "Off", UITheme.ButtonStyle, GUILayout.Width(28), GUILayout.Height(20)))
            {
                if (isForcedZone) _travelerAgent.ClearForcedVectorZone();
                else _travelerAgent.ForcedVectorZone = zone;
            }
            GUI.backgroundColor = UITheme.Danger;
            if (GUILayout.Button("X", UITheme.ButtonStyle, GUILayout.Width(22), GUILayout.Height(20)))
            {
                if (isForcedZone) _travelerAgent.ClearForcedVectorZone();
                _travelerAgent.RemoveCustomVectorZone(zone);
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(1);
        }

        private void DrawStatusSection()
        {
            bool isTraveling = !string.IsNullOrEmpty(_travelerAgent.TargetAreaName);
            GUILayout.BeginHorizontal();
            DrawStatBox("Status", isTraveling ? "TRAVELING" : "IDLE", isTraveling ? UITheme.Warning : UITheme.Positive);
            GUILayout.Space(UITheme.BUTTON_SPACING);

            string modeText = "AUTO"; Color modeColor = UITheme.Positive;
            if (_travelerAgent.IsForcedVectorZoneEnabled) { modeText = "FORCED V"; modeColor = UITheme.Warning; }
            else if (_travelerAgent.IsForcedAreaEnabled) { modeText = "FORCED A"; modeColor = UITheme.Warning; }
            else if (_travelerAgent.TravelMode == TravelMode.VectorBased) { modeText = "VECTOR"; modeColor = UITheme.Info; }
            else { modeText = "AREA"; modeColor = UITheme.Accent; }
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
            GUI.color = UITheme.Info; GUILayout.Label("?", GUILayout.Width(20)); GUI.color = Color.white;
            GUILayout.Label("Travel Status", UITheme.SubtitleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Current:", UITheme.LabelStyle, GUILayout.Width(55));
            GUILayout.Label(string.IsNullOrEmpty(_travelerAgent.TargetAreaName) ? "At dest." : "Transit...", UITheme.CreateValueStyle(UITheme.TextLight));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target:", UITheme.LabelStyle, GUILayout.Width(55));
            string targetName = string.IsNullOrEmpty(_travelerAgent.TargetAreaName) ? "None" : _travelerAgent.TargetAreaName;
            GUILayout.Label(TruncateText(targetName, 18), UITheme.CreateValueStyle(string.IsNullOrEmpty(_travelerAgent.TargetAreaName) ? UITheme.TextMuted : UITheme.Warning, UITheme.FONT_SIZE_SMALL));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawBestAreaCard()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);
            GUILayout.BeginHorizontal();

            if (_travelerAgent.IsForcedVectorZoneEnabled) { GUI.color = UITheme.Warning; GUILayout.Label("?", GUILayout.Width(20)); }
            else if (_travelerAgent.IsForcedAreaEnabled) { GUI.color = UITheme.Warning; GUILayout.Label("?", GUILayout.Width(20)); }
            else if (_travelerAgent.TravelMode == TravelMode.VectorBased) { GUI.color = UITheme.Info; GUILayout.Label("?", GUILayout.Width(20)); }
            else { GUI.color = UITheme.Accent; GUILayout.Label("?", GUILayout.Width(20)); }
            GUI.color = Color.white;

            string headerText = _travelerAgent.IsForcedVectorZoneEnabled ? "Forced Zone" :
                               (_travelerAgent.IsForcedAreaEnabled ? "Forced Area" :
                               (_travelerAgent.TravelMode == TravelMode.VectorBased ? "Best Zone" : "Best Area"));
            GUILayout.Label(headerText, UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            if (_travelerAgent.IsForcedVectorZoneEnabled && _travelerAgent.ForcedVectorZone != null)
            {
                var zone = _travelerAgent.ForcedVectorZone;
                GUILayout.Label(zone.Name, UITheme.CreateValueStyle(UITheme.Warning, 11));
                GUILayout.Label($"({zone.Position.x:F0}, {zone.Position.y:F0}, {zone.Position.z:F0})", UITheme.CreateLabelStyle(UITheme.TextMuted, 9));
                float distance = _travelerAgent.GetDistanceToForcedVectorZone();
                bool inZone = _travelerAgent.IsHeroInForcedVectorZone();
                DrawVectorZoneDistanceInfo(distance, zone.Radius, inZone);
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
                    float distance = _travelerAgent.GetDistanceToCurrentVectorZone();
                    bool inZone = distance >= 0 && distance <= bestZone.Radius;
                    DrawVectorZoneDistanceInfo(distance, bestZone.Radius, inZone);
                    DrawTransitionWaypointInfo(bestZone);
                }
                else GUILayout.Label("No zones configured", UITheme.CreateValueStyle(UITheme.TextMuted, 11));
            }
            else
            {
                string bestArea = "";
                try { bestArea = _travelerAgent.GetBestAreaForLevel(); } catch { bestArea = "Unknown"; }
                GUILayout.Label(bestArea, UITheme.CreateValueStyle(UITheme.Accent, 11));
            }
            GUILayout.EndVertical();
        }

        private void DrawTransitionWaypointInfo(LevelVectorZone bestZone)
        {
            var currentWaypoint = _travelerAgent.CurrentPathWaypoint;
            if (currentWaypoint == null || currentWaypoint == bestZone) return;

            GUILayout.Space(4);
            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            GUI.color = new Color(0.6f, 0.8f, 1f); GUILayout.Label("?", GUILayout.Width(14)); GUI.color = Color.white;
            GUILayout.Label("Waypoint", UITheme.CreateLabelStyle(new Color(0.6f, 0.8f, 1f), 10, FontStyle.Bold));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Label($"L{currentWaypoint.MinLevel}: {currentWaypoint.Name}", UITheme.CreateValueStyle(new Color(0.6f, 0.8f, 1f), 10));
            GUILayout.Label($"({currentWaypoint.Position.x:F0}, {currentWaypoint.Position.y:F0}, {currentWaypoint.Position.z:F0})", UITheme.CreateLabelStyle(UITheme.TextMuted, 8));

            float distToWaypoint = _travelerAgent.GetDistanceToCurrentPathWaypoint();
            bool atWaypoint = _travelerAgent.IsHeroAtCurrentPathWaypoint();

            GUILayout.BeginHorizontal();
            if (distToWaypoint >= 0)
                GUILayout.Label($"Dist: {distToWaypoint:F0}", UITheme.CreateLabelStyle(atWaypoint ? UITheme.Positive : new Color(0.6f, 0.8f, 1f), 8), GUILayout.Width(55));
            GUILayout.Label($"R: {currentWaypoint.Radius:F0}", UITheme.CreateLabelStyle(UITheme.TextMuted, 8), GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            GUILayout.Label(atWaypoint ? "? AT WP" : "? TRANSIT", UITheme.CreateLabelStyle(atWaypoint ? UITheme.Positive : new Color(0.6f, 0.8f, 1f), 8, atWaypoint ? FontStyle.Bold : FontStyle.Normal));
            GUILayout.EndHorizontal();
        }

        private void DrawVectorZoneDistanceInfo(float distance, float radius, bool inZone)
        {
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            if (distance >= 0)
            {
                GUILayout.Label($"Dist: {distance:F0}", UITheme.CreateLabelStyle(inZone ? UITheme.Positive : UITheme.Warning, 9), GUILayout.Width(60));
                GUILayout.Label($"R: {radius:F0}", UITheme.CreateLabelStyle(UITheme.TextMuted, 9), GUILayout.Width(45));
                GUILayout.FlexibleSpace();
                GUI.color = inZone ? UITheme.Positive : UITheme.Warning;
                GUILayout.Label(inZone ? "? IN ZONE" : "? TRAVELING", UITheme.CreateLabelStyle(inZone ? UITheme.Positive : UITheme.Warning, 9, inZone ? FontStyle.Bold : FontStyle.Normal));
                GUI.color = Color.white;
            }
            else GUILayout.Label("Distance: N/A", UITheme.CreateLabelStyle(UITheme.TextMuted, 9));
            GUILayout.EndHorizontal();
        }

        private void DrawLevelAreaRow(int minLevel, string areaName, bool isBestArea, bool isForcedArea)
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle, GUILayout.Height(28));
            GUILayout.Label($"L{minLevel}", UITheme.CreateValueStyle(UITheme.Info, UITheme.FONT_SIZE_SMALL), GUILayout.Width(25));
            GUILayout.Space(4);

            Color nameColor = isForcedArea ? UITheme.Warning : (isBestArea ? UITheme.Accent : UITheme.TextLight);
            FontStyle fontStyle = (isForcedArea || isBestArea) ? FontStyle.Bold : FontStyle.Normal;
            GUILayout.Label(TruncateText(areaName, 16), UITheme.CreateLabelStyle(nameColor, UITheme.FONT_SIZE_SMALL, fontStyle), GUILayout.ExpandWidth(true));

            if (isForcedArea) GUILayout.Label("?", UITheme.CreateLabelStyle(UITheme.Warning, 12), GUILayout.Width(14));
            else if (isBestArea) GUILayout.Label("?", UITheme.CreateLabelStyle(UITheme.Accent, 12), GUILayout.Width(14));
            else GUILayout.Space(14);

            GUI.backgroundColor = isForcedArea ? UITheme.Warning : UITheme.ButtonNormal;
            if (GUILayout.Button(isForcedArea ? "On" : "Off", UITheme.ButtonStyle, GUILayout.Width(28), GUILayout.Height(20)))
            {
                if (isForcedArea) _travelerAgent.ClearForcedArea();
                else _travelerAgent.ForcedAreaName = areaName;
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(1);
        }

        private void DrawCustomAreaRow(string areaName, bool isForcedArea)
        {
            GUILayout.BeginHorizontal(UITheme.CardStyle, GUILayout.Height(28));
            GUILayout.Label(TruncateText(areaName, 12), UITheme.CreateLabelStyle(isForcedArea ? UITheme.Warning : UITheme.TextLight, UITheme.FONT_SIZE_SMALL, isForcedArea ? FontStyle.Bold : FontStyle.Normal), GUILayout.ExpandWidth(true));
            if (isForcedArea) GUILayout.Label("?", UITheme.CreateLabelStyle(UITheme.Warning, 12), GUILayout.Width(14));
            else GUILayout.Space(14);

            GUI.backgroundColor = isForcedArea ? UITheme.Warning : UITheme.ButtonNormal;
            if (GUILayout.Button(isForcedArea ? "On" : "Off", UITheme.ButtonStyle, GUILayout.Width(28), GUILayout.Height(20)))
            {
                if (isForcedArea) _travelerAgent.ClearForcedArea();
                else _travelerAgent.ForcedAreaName = areaName;
            }
            GUI.backgroundColor = UITheme.Danger;
            if (GUILayout.Button("X", UITheme.ButtonStyle, GUILayout.Width(22), GUILayout.Height(20)))
            {
                if (isForcedArea) _travelerAgent.ClearForcedArea();
                _travelerAgent.RemoveCustomArea(areaName);
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(1);
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength) return text;
            return text.Substring(0, maxLength - 2) + "..";
        }
    }
}
