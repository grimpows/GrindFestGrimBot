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

        private Bot_Agent_FighterUI _fightingAgentUI;
        private Bot_Agent_LooterUI _pickUpAgentUI;
        private Bot_Agent_ConsumerUI _consumerAgentUI;
        private Bot_Agent_TravelerUI _travelerAgentUI;
        private Bot_Agent_UnstuckerUI _unstuckerAgentUI;

        private Rect _botWindowRect = new Rect(100, 100, 750, 650);

        private BotTab _currentTab = BotTab.Global;

        private enum BotTab
        {
            Global,
            FightingAgent,
            PickUpAgent,
            ConsumerAgent,
            TravelerAgent,
            UnstuckerAgent
        }

        public bool IsVisible => _isShow;
        public void SetVisible(bool visible) => _isShow = visible;
        public void ToggleVisible() => _isShow = !_isShow;

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
            _unstuckerAgentUI = new Bot_Agent_UnstuckerUI(bot.UnstuckerAgent);
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
                _botWindowRect = GUI.Window(_windowID, _botWindowRect, DrawBotWindow, "", UITheme.WindowStyle);
            }
        }

        void DrawBotWindow(int windowID)
        {
            GUILayout.BeginVertical();

            DrawHeader();
            GUILayout.Space(UITheme.BUTTON_SPACING);

            DrawTabs();
            GUILayout.Space(UITheme.SECTION_SPACING);

            Rect contentArea = new Rect(10, UITheme.HEADER_HEIGHT + UITheme.TAB_HEIGHT + 35, 
                _botWindowRect.width - 20, _botWindowRect.height - UITheme.HEADER_HEIGHT - UITheme.TAB_HEIGHT - 50);

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
                case BotTab.UnstuckerAgent:
                    _unstuckerAgentUI.DrawUnstuckerAgentPanel(contentArea);
                    break;
            }

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, _botWindowRect.width, 30));
        }

        void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("BOT CONTROL CENTER", UITheme.TitleStyle, GUILayout.Height(30));
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = UITheme.Danger;
            if (GUILayout.Button("X", UITheme.CloseButtonStyle, GUILayout.Width(UITheme.CLOSE_BUTTON_SIZE), GUILayout.Height(UITheme.CLOSE_BUTTON_SIZE)))
            {
                _isShow = false;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();

            UITheme.DrawSeparator();
        }

        void DrawTabs()
        {
            GUILayout.BeginHorizontal();

            DrawTab("Global", BotTab.Global, "⚙");
            GUILayout.Space(3);
            DrawTab("Fighter", BotTab.FightingAgent, "⚔");
            GUILayout.Space(3);
            DrawTab("Looter", BotTab.PickUpAgent, "◆");
            GUILayout.Space(3);
            DrawTab("Consumer", BotTab.ConsumerAgent, "♥");
            GUILayout.Space(3);
            DrawTab("Traveler", BotTab.TravelerAgent, "✈");
            GUILayout.Space(3);
            DrawTab("Unstucker", BotTab.UnstuckerAgent, "⚡");

            GUILayout.EndHorizontal();
        }

        void DrawTab(string label, BotTab tab, string icon)
        {
            bool isActive = _currentTab == tab;
            GUIStyle style = isActive ? UITheme.TabActiveStyle : UITheme.TabInactiveStyle;

            if (GUILayout.Button($"{icon} {label}", style, GUILayout.Height(UITheme.TAB_HEIGHT), GUILayout.ExpandWidth(true)))
            {
                _currentTab = tab;
            }
        }

        void DrawGlobalPanel(Rect contentArea)
        {
            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(UITheme.SectionStyle);

            DrawSectionHeader("GLOBAL STATUS");
            GUILayout.Space(UITheme.SECTION_SPACING);

            // Status Cards Row
            GUILayout.BeginHorizontal();
            
            // Bot Status with enum
            string statusText = _bot.CurrentStatus.ToString();
            Color statusColor = GetStatusColor(_bot.CurrentStatus);
            DrawStatusCard("Bot Status", statusText, statusColor);
            
            GUILayout.Space(UITheme.BUTTON_SPACING);
            
            // Unstick Mode from UnstuckerAgent
            bool isUnsticking = _bot.UnstuckerAgent?.IsOnUnstickMode ?? false;
            DrawStatusCard("Unstick Mode", isUnsticking ? "ACTIVE" : "OFF", isUnsticking ? UITheme.Warning : UITheme.TextMuted);
            
            GUILayout.Space(UITheme.BUTTON_SPACING);
            
            // Unstick Count
            int unstickCount = _bot.UnstuckerAgent?.UnstickCount ?? 0;
            DrawStatusCard("Unstick Count", unstickCount.ToString(), UITheme.Info);
            
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.WINDOW_PADDING);

            DrawAreaCard();
            GUILayout.Space(UITheme.SECTION_SPACING);

            DrawActivityCard();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private Color GetStatusColor(BotStatus status)
        {
            return status switch
            {
                BotStatus.INACTIVE => UITheme.Danger,
                BotStatus.IDLE => UITheme.TextMuted,
                BotStatus.FIGHTING => UITheme.Danger,
                BotStatus.LOOTING => UITheme.Gold,
                BotStatus.CONSUMING => UITheme.Health,
                BotStatus.TRAVELING => UITheme.Info,
                BotStatus.RUNAROUND => UITheme.Positive,
                BotStatus.UNSTICKING => UITheme.Warning,
                BotStatus.INTERACTING => UITheme.Accent,
                BotStatus.UPGRADING => UITheme.Intelligence,
                _ => UITheme.TextMuted
            };
        }

        void DrawSectionHeader(string title)
        {
            GUILayout.Label(title, UITheme.SubtitleStyle);
            UITheme.DrawSeparator();
            GUILayout.Space(4);
        }

        void DrawStatusCard(string label, string value, Color valueColor)
        {
            GUILayout.BeginVertical(UITheme.CardStyle, GUILayout.ExpandWidth(true), GUILayout.Height(55));
            GUILayout.Label(label, UITheme.LabelStyle);
            GUILayout.Label(value, UITheme.CreateValueStyle(valueColor, 12));
            GUILayout.EndVertical();
        }

        void DrawAreaCard()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = UITheme.Info;
            GUILayout.Label("◎", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Current Area", UITheme.SubtitleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            

            string rootAreaName = _hero.CurrentArea?.Root.name ?? "Unknown Area";
            GUILayout.Label(rootAreaName, UITheme.CreateValueStyle(UITheme.Info, 13));

            string areaName = _hero.CurrentArea?.name ?? "Unknown Subarea";
            GUILayout.Label(areaName, UITheme.CreateValueStyle(UITheme.TextMuted, 12));

            GUILayout.EndVertical();
        }

        void DrawActivityCard()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = UITheme.Accent;
            GUILayout.Label("★", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Current Activity", UITheme.SubtitleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Using Skill:", UITheme.LabelStyle, GUILayout.Width(90));

            string skillName = _hero.Character.SkillUser.IsUsingSkill ? _hero.Character.SkillUser.CurrentlyUsedSkill.name : "None";
            Color skillColor = _hero.Character.SkillUser.IsUsingSkill ? UITheme.Positive : UITheme.TextMuted;
            GUILayout.Label(skillName, UITheme.CreateValueStyle(skillColor));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
