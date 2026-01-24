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

        private Bot_Agent_FigherUI _fightingAgentUI;
        private Bot_Agent_LooterUI _pickUpAgentUI;
        private Bot_Agent_ConsumerUI _consumerAgentUI;

        //rects
        private Rect _botWindowRect = new Rect(100, 100, 800, 800);

        private const int TAB_HEIGHT = 35;
        private const int HEADER_HEIGHT = 30;

        private BotTab _currentTab = BotTab.Global;

        private enum BotTab
        {
            Global,
            FightingAgent,
            PickUpAgent,
            ConsumerAgent
        }

        public BotUI(Bot bot, AutomaticHero hero, KeyCode toggleShowKey, int windowID)
        {
            _bot = bot;
            _hero = hero;
            _toggleShowKey = toggleShowKey;
            _windowID = windowID;

            _fightingAgentUI = new Bot_Agent_FigherUI(bot.FightingAgent);
            _pickUpAgentUI = new Bot_Agent_LooterUI(bot.PickUpAgent);
            _consumerAgentUI = new Bot_Agent_ConsumerUI(bot.ConsumerAgent);
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
                _botWindowRect = GUI.Window(_windowID, _botWindowRect, DrawBotWindow, "BOT SETTINGS");
            }
        }

        void DrawBotWindow(int windowID)
        {
            DrawTabs();

            Rect contentArea = new Rect(10, HEADER_HEIGHT + TAB_HEIGHT, _botWindowRect.width - 20, _botWindowRect.height - HEADER_HEIGHT - TAB_HEIGHT - 40);

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
            }

            GUI.DragWindow(new Rect(0, 0, _botWindowRect.width, 30));
        }

        void DrawTabs()
        {
            float tabWidth = (_botWindowRect.width - 60) / 4f;
            float startX = 10;
            float startY = HEADER_HEIGHT;

            if (DrawTab("Global", new Rect(startX, startY, tabWidth, TAB_HEIGHT), _currentTab == BotTab.Global))
            {
                _currentTab = BotTab.Global;
            }

            if (DrawTab("Fighting Agent", new Rect(startX + tabWidth + 5, startY, tabWidth, TAB_HEIGHT), _currentTab == BotTab.FightingAgent))
            {
                _currentTab = BotTab.FightingAgent;
            }

            if (DrawTab("PickUp Agent", new Rect(startX + (tabWidth + 5) * 2, startY, tabWidth, TAB_HEIGHT), _currentTab == BotTab.PickUpAgent))
            {
                _currentTab = BotTab.PickUpAgent;
            }

            if (DrawTab("Consumer Agent", new Rect(startX + (tabWidth + 5) * 3, startY, tabWidth, TAB_HEIGHT), _currentTab == BotTab.ConsumerAgent))
            {
                _currentTab = BotTab.ConsumerAgent;
            }
        }

        bool DrawTab(string label, Rect rect, bool isActive)
        {
            Color originalColor = GUI.backgroundColor;

            if (isActive)
            {
                GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            }
            else
            {
                GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f);
            }

            bool clicked = GUI.Button(rect, label);

            GUI.backgroundColor = originalColor;

            return clicked;
        }

        void DrawGlobalPanel(Rect contentArea)
        {
            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(GUI.skin.box);

            // Current Area Display
            string areaName = _hero.CurrentArea?.Root.name ?? "Unknown Area";
            GUILayout.BeginHorizontal();
            GUILayout.Label("Current Area:", GUILayout.Width(150));
            GUILayout.Label(areaName, GUILayout.Width(300));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Allow Change Area Toggle
            GUILayout.BeginHorizontal();
            GUILayout.Label("Allow Change Area:", GUILayout.Width(150));
            GUI.backgroundColor = _bot.IsAllowedToChangeArea ? new Color(0.3f, 1f, 0.3f) : new Color(0.6f, 0.6f, 0.6f);
            if (GUILayout.Button(_bot.IsAllowedToChangeArea ? "ON" : "OFF", GUILayout.Width(80)))
            {
                _bot.IsAllowedToChangeArea = !_bot.IsAllowedToChangeArea;
            }
            GUI.backgroundColor = GUI.color;
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.Label("Bot Status", GUI.skin.box);
            GUILayout.Space(10);

            // Bottling Status
            GUILayout.BeginHorizontal();
            GUILayout.Label("Bottling:", GUILayout.Width(150));
            GUI.color = _hero.IsBotting ? Color.green : Color.red;
            GUILayout.Label(_hero.IsBotting ? "ACTIVE" : "INACTIVE", GUILayout.Width(100));
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Currently Using Skill
            GUILayout.BeginHorizontal();
            GUILayout.Label("Using Skill:", GUILayout.Width(150));
            string currentSkillName = _hero.Character.SkillUser.IsUsingSkill ? _hero.Character.SkillUser.CurrentlyUsedSkill.name : "None";
            GUILayout.Label(currentSkillName, GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
