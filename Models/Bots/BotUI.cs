using Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GrindFest;

namespace Scripts.Model
{
    internal class BotUI
    {
        private Bot _bot;
        private AutomaticHero _hero;
        private KeyCode _toggleShowKey;
        private bool _isShow = false;
        private int _windowID;
        private bool _stylesInitialized = false;

        private Bot_Agent_FighterUI _fightingAgentUI;
        private Bot_Agent_LooterUI _pickUpAgentUI;
        private Bot_Agent_ConsumerUI _consumerAgentUI;
        private Bot_Agent_TravelerUI _travelerAgentUI;

        private Rect _botWindowRect = new Rect(100, 100, 750, 650);

        private const int TAB_HEIGHT = 35;
        private const int HEADER_HEIGHT = 45;

        private BotTab _currentTab = BotTab.Global;

        // GUI Styles
        private GUIStyle _windowStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _valueStyle;
        private GUIStyle _sectionStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _tabActiveStyle;
        private GUIStyle _tabInactiveStyle;
        private GUIStyle _toggleOnStyle;
        private GUIStyle _toggleOffStyle;

        // Textures
        private Texture2D _windowBgTexture;
        private Texture2D _sectionBgTexture;
        private Texture2D _cardBgTexture;
        private Texture2D _tabActiveTexture;
        private Texture2D _tabInactiveTexture;
        private Texture2D _toggleOnTexture;
        private Texture2D _toggleOffTexture;
        private Texture2D _barFillTexture;

        // Colors
        private Color _windowBgColor = new Color(0.08f, 0.08f, 0.1f, 0.98f);
        private Color _sectionColor = new Color(0.1f, 0.1f, 0.12f, 0.95f);
        private Color _cardColor = new Color(0.15f, 0.15f, 0.18f, 0.95f);
        private Color _accentColor = new Color(0.95f, 0.75f, 0.3f, 1f);
        private Color _positiveColor = new Color(0.4f, 1f, 0.55f, 1f);
        private Color _warningColor = new Color(1f, 0.7f, 0.3f, 1f);
        private Color _dangerColor = new Color(1f, 0.4f, 0.4f, 1f);
        private Color _infoColor = new Color(0.4f, 0.7f, 1f, 1f);
        private Color _textLightColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        private Color _textMutedColor = new Color(0.65f, 0.65f, 0.7f, 1f);

        private enum BotTab
        {
            Global,
            FightingAgent,
            PickUpAgent,
            ConsumerAgent,
            TravelerAgent
        }

        public BotUI(Bot bot, AutomaticHero hero, KeyCode toggleShowKey, int windowID)
        {
            _bot = bot;
            _hero = hero;
            _toggleShowKey = toggleShowKey;
            _windowID = windowID;

            _fightingAgentUI = new Bot_Agent_FighterUI(bot.FightingAgent);
            _pickUpAgentUI = new Bot_Agent_LooterUI(bot.PickUpAgent);
            _consumerAgentUI = new Bot_Agent_ConsumerUI(bot.ConsumerAgent);
            _travelerAgentUI = new Bot_Agent_TravelerUI(bot.TravelerAgent);
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            // Textures
            _windowBgTexture = CreateTexture(_windowBgColor);
            _sectionBgTexture = CreateTexture(_sectionColor);
            _cardBgTexture = CreateTexture(_cardColor);
            _tabActiveTexture = CreateTexture(new Color(0.25f, 0.5f, 0.8f, 0.95f));
            _tabInactiveTexture = CreateTexture(new Color(0.18f, 0.18f, 0.22f, 0.9f));
            _toggleOnTexture = CreateTexture(new Color(0.2f, 0.6f, 0.3f, 0.95f));
            _toggleOffTexture = CreateTexture(new Color(0.25f, 0.25f, 0.3f, 0.9f));
            _barFillTexture = CreateTexture(Color.white);

            // Window Style
            _windowStyle = new GUIStyle(GUI.skin.window);
            _windowStyle.normal.background = _windowBgTexture;
            _windowStyle.onNormal.background = _windowBgTexture;
            _windowStyle.focused.background = _windowBgTexture;
            _windowStyle.onFocused.background = _windowBgTexture;
            _windowStyle.active.background = _windowBgTexture;
            _windowStyle.onActive.background = _windowBgTexture;

            // Title Style
            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.fontSize = 18;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.normal.textColor = _accentColor;

            // Subtitle Style
            _subtitleStyle = new GUIStyle(GUI.skin.label);
            _subtitleStyle.fontSize = 12;
            _subtitleStyle.fontStyle = FontStyle.Bold;
            _subtitleStyle.normal.textColor = _textLightColor;

            // Label Style
            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = 11;
            _labelStyle.normal.textColor = _textMutedColor;

            // Value Style
            _valueStyle = new GUIStyle(GUI.skin.label);
            _valueStyle.fontSize = 11;
            _valueStyle.fontStyle = FontStyle.Bold;
            _valueStyle.normal.textColor = _textLightColor;

            // Section Style
            _sectionStyle = new GUIStyle(GUI.skin.box);
            _sectionStyle.normal.background = _sectionBgTexture;
            _sectionStyle.padding = new RectOffset(12, 12, 10, 10);

            // Card Style
            _cardStyle = new GUIStyle(GUI.skin.box);
            _cardStyle.normal.background = _cardBgTexture;
            _cardStyle.padding = new RectOffset(10, 10, 8, 8);

            // Tab Styles
            _tabActiveStyle = new GUIStyle(GUI.skin.button);
            _tabActiveStyle.fontSize = 11;
            _tabActiveStyle.fontStyle = FontStyle.Bold;
            _tabActiveStyle.normal.background = _tabActiveTexture;
            _tabActiveStyle.normal.textColor = _textLightColor;
            _tabActiveStyle.hover.background = _tabActiveTexture;
            _tabActiveStyle.hover.textColor = _textLightColor;
            _tabActiveStyle.active.background = _tabActiveTexture;
            _tabActiveStyle.focused.background = _tabActiveTexture;

            _tabInactiveStyle = new GUIStyle(GUI.skin.button);
            _tabInactiveStyle.fontSize = 11;
            _tabInactiveStyle.normal.background = _tabInactiveTexture;
            _tabInactiveStyle.normal.textColor = _textMutedColor;
            _tabInactiveStyle.hover.background = CreateTexture(new Color(0.25f, 0.25f, 0.3f, 0.95f));
            _tabInactiveStyle.hover.textColor = _textLightColor;
            _tabInactiveStyle.active.background = _tabInactiveTexture;
            _tabInactiveStyle.focused.background = _tabInactiveTexture;

            // Toggle Styles
            _toggleOnStyle = new GUIStyle(GUI.skin.button);
            _toggleOnStyle.normal.background = _toggleOnTexture;
            _toggleOnStyle.normal.textColor = _textLightColor;
            _toggleOnStyle.fontStyle = FontStyle.Bold;
            _toggleOnStyle.fontSize = 10;

            _toggleOffStyle = new GUIStyle(GUI.skin.button);
            _toggleOffStyle.normal.background = _toggleOffTexture;
            _toggleOffStyle.normal.textColor = _textMutedColor;
            _toggleOffStyle.fontSize = 10;

            _stylesInitialized = true;
        }

        private Texture2D CreateTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        public void OnGUI()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == _toggleShowKey)
            {
                _isShow = !_isShow;
                e.Use();
            }

            if (_isShow)
            {
                if (!_stylesInitialized)
                {
                    InitializeStyles();
                }
                _botWindowRect = GUI.Window(_windowID, _botWindowRect, DrawBotWindow, "", _windowStyle);
            }
        }

        void DrawBotWindow(int windowID)
        {
            GUILayout.BeginVertical();

            // Header
            DrawHeader();
            GUILayout.Space(8);

            // Tabs
            DrawTabs();
            GUILayout.Space(10);

            // Content
            Rect contentArea = new Rect(10, HEADER_HEIGHT + TAB_HEIGHT + 20, _botWindowRect.width - 20, _botWindowRect.height - HEADER_HEIGHT - TAB_HEIGHT - 50);

            switch (_currentTab)
            {
                case BotTab.Global:
                    DrawGlobalPanel(contentArea);
                    break;
                case BotTab.FightingAgent:
                    _fightingAgentUI.DrawFightingAgentPanel(contentArea);
                    break;
                case BotTab.PickUpAgent:
                    _pickUpAgentUI.DrawPickUpAgentPanel(contentArea);
                    break;
                case BotTab.ConsumerAgent:
                    _consumerAgentUI.DrawConsumerAgentPanel(contentArea);
                    break;
                case BotTab.TravelerAgent:
                    _travelerAgentUI.DrawTravelerAgentPanel(contentArea);
                    break;
            }

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, _botWindowRect.width, 30));
        }

        void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("BOT CONTROL CENTER", _titleStyle, GUILayout.Height(30));
            GUILayout.FlexibleSpace();

            // Close button
            GUI.backgroundColor = _dangerColor;
            GUIStyle closeStyle = new GUIStyle(GUI.skin.button);
            closeStyle.fontSize = 14;
            closeStyle.fontStyle = FontStyle.Bold;
            closeStyle.normal.textColor = Color.white;
            closeStyle.hover.textColor = Color.white;

            if (GUILayout.Button("X", closeStyle, GUILayout.Width(28), GUILayout.Height(28)))
            {
                _isShow = false;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();

            // Separator
            Rect sepRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(2));
            GUI.color = _accentColor;
            GUI.DrawTexture(sepRect, _barFillTexture);
            GUI.color = Color.white;
        }

        void DrawTabs()
        {
            GUILayout.BeginHorizontal();

            DrawTab("Global", BotTab.Global, "⚙");
            GUILayout.Space(4);
            DrawTab("Fighter", BotTab.FightingAgent, "⚔");
            GUILayout.Space(4);
            DrawTab("Looter", BotTab.PickUpAgent, "◆");
            GUILayout.Space(4);
            DrawTab("Consumer", BotTab.ConsumerAgent, "♥");
            GUILayout.Space(4);
            DrawTab("Traveler", BotTab.TravelerAgent, "✈");

            GUILayout.EndHorizontal();
        }

        void DrawTab(string label, BotTab tab, string icon)
        {
            bool isActive = _currentTab == tab;
            GUIStyle style = isActive ? _tabActiveStyle : _tabInactiveStyle;

            if (GUILayout.Button($"{icon} {label}", style, GUILayout.Height(TAB_HEIGHT), GUILayout.ExpandWidth(true)))
            {
                _currentTab = tab;
            }
        }

        void DrawGlobalPanel(Rect contentArea)
        {
            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(_sectionStyle);

            // Header
            DrawSectionHeader("GLOBAL STATUS");
            GUILayout.Space(10);

            // Status Cards Row
            GUILayout.BeginHorizontal();
            DrawStatusCard("Bot Status", _hero.IsBotting ? "ACTIVE" : "INACTIVE", _hero.IsBotting ? _positiveColor : _dangerColor);
            GUILayout.Space(8);
            DrawStatusCard("Unstick Mode", _bot.IsOnUnstickMode ? "ACTIVE" : "OFF", _bot.IsOnUnstickMode ? _warningColor : _textMutedColor);
            GUILayout.Space(8);
            DrawStatusCard("Change Area", _bot.IsAllowedToChangeArea ? "ALLOWED" : "LOCKED", _bot.IsAllowedToChangeArea ? _positiveColor : _textMutedColor);
            GUILayout.EndHorizontal();
            GUILayout.Space(12);

            // Current Area Card
            DrawAreaCard();
            GUILayout.Space(10);

            // Current Activity Card
            DrawActivityCard();
            GUILayout.Space(10);

            // Position Info Card
            DrawPositionCard();
            GUILayout.Space(10);

            // Settings
            DrawSettingsSection();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawSectionHeader(string title)
        {
            GUILayout.Label(title, _subtitleStyle);
            Rect sepRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1));
            GUI.color = _accentColor;
            GUI.DrawTexture(sepRect, _barFillTexture);
            GUI.color = Color.white;
            GUILayout.Space(4);
        }

        void DrawStatusCard(string label, string value, Color valueColor)
        {
            GUILayout.BeginVertical(_cardStyle, GUILayout.ExpandWidth(true), GUILayout.Height(55));

            GUILayout.Label(label, _labelStyle);

            GUIStyle valStyle = new GUIStyle(_valueStyle);
            valStyle.fontSize = 12;
            valStyle.normal.textColor = valueColor;
            valStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(value, valStyle);

            GUILayout.EndVertical();
        }

        void DrawAreaCard()
        {
            GUILayout.BeginVertical(_cardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = _infoColor;
            GUILayout.Label("◎", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Current Area", _subtitleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            string areaName = _hero.CurrentArea?.Root.name ?? "Unknown Area";
            GUIStyle areaStyle = new GUIStyle(_valueStyle);
            areaStyle.fontSize = 13;
            areaStyle.normal.textColor = _infoColor;
            GUILayout.Label(areaName, areaStyle);

            GUILayout.EndVertical();
        }

        void DrawActivityCard()
        {
            GUILayout.BeginVertical(_cardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = _accentColor;
            GUILayout.Label("★", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Current Activity", _subtitleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            // Using Skill
            GUILayout.BeginHorizontal();
            GUILayout.Label("Using Skill:", _labelStyle, GUILayout.Width(90));

            string skillName = _hero.Character.SkillUser.IsUsingSkill ? _hero.Character.SkillUser.CurrentlyUsedSkill.name : "None";
            Color skillColor = _hero.Character.SkillUser.IsUsingSkill ? _positiveColor : _textMutedColor;

            GUIStyle skillStyle = new GUIStyle(_valueStyle);
            skillStyle.normal.textColor = skillColor;
            GUILayout.Label(skillName, skillStyle);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        void DrawPositionCard()
        {
            GUILayout.BeginVertical(_cardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = _warningColor;
            GUILayout.Label("⊕", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Position Tracking", _subtitleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            // Last Position
            GUILayout.BeginHorizontal();
            GUILayout.Label("Last Position:", _labelStyle, GUILayout.Width(110));
            string posStr = _bot.LastHeroPosition != null
                ? $"({_bot.LastHeroPosition.x:F1}, {_bot.LastHeroPosition.y:F1}, {_bot.LastHeroPosition.z:F1})"
                : "N/A";
            GUILayout.Label(posStr, _valueStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Time Since Move
            TimeSpan timeSinceMove = DateTime.Now - _bot.LastHeroPositionTime;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Time Since Move:", _labelStyle, GUILayout.Width(110));

            Color timeColor = timeSinceMove.TotalSeconds < 5 ? _positiveColor : timeSinceMove.TotalSeconds < 15 ? _warningColor : _dangerColor;
            GUIStyle timeStyle = new GUIStyle(_valueStyle);
            timeStyle.normal.textColor = timeColor;
            GUILayout.Label($"{timeSinceMove.TotalSeconds:F1}s", timeStyle);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Distance from Last
            float distance = Vector3.Distance(_hero.Character.transform.position, _bot.LastHeroPosition);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Distance Moved:", _labelStyle, GUILayout.Width(110));
            GUILayout.Label($"{distance:F1} units", _valueStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        void DrawSettingsSection()
        {
            GUILayout.Label("Quick Settings", _subtitleStyle);
            GUILayout.Space(4);

            // Allow Change Area Toggle
            GUILayout.BeginHorizontal();
            GUILayout.Label("Allow Area Change:", _labelStyle, GUILayout.Width(120));

            GUIStyle toggleStyle = _bot.IsAllowedToChangeArea ? _toggleOnStyle : _toggleOffStyle;
            GUI.backgroundColor = _bot.IsAllowedToChangeArea ? _positiveColor : _textMutedColor;

            if (GUILayout.Button(_bot.IsAllowedToChangeArea ? "ON" : "OFF", toggleStyle, GUILayout.Width(60), GUILayout.Height(22)))
            {
                _bot.IsAllowedToChangeArea = !_bot.IsAllowedToChangeArea;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public bool IsVisible => _isShow;
        public void SetVisible(bool visible) => _isShow = visible;
        public void ToggleVisible() => _isShow = !_isShow;
    }
}
