using GrindFest;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Models
{
    /// <summary>
    /// UI component for the AI Chat window - a mini in-game chatbot interface.
    /// </summary>
    public class AIChatUI
    {
        private AIChat _aiChat;
        private AutomaticHero _hero;
        private KeyCode _toggleShowKey;
        private bool _isShow = false;
        private int _windowID;

        private Rect _windowRect = new Rect(100, 100, 450, 550);
        private Vector2 _scrollPosition = Vector2.zero;
        private string _inputText = "";
        private bool _autoScroll = true;
        private bool _showQuickActions = true;

        // Styles (lazy initialized)
        private bool _stylesInitialized = false;
        private GUIStyle _userMessageStyle;
        private GUIStyle _aiMessageStyle;
        private GUIStyle _systemMessageStyle;
        private GUIStyle _timestampStyle;
        private GUIStyle _inputFieldStyle;
        private GUIStyle _quickActionStyle;
        private GUIStyle _toggleStyle;

        public bool IsVisible => _isShow;
        public void SetVisible(bool visible) => _isShow = visible;
        public void ToggleVisible() => _isShow = !_isShow;

        public void OnStart(AIChat aiChat, AutomaticHero hero, KeyCode toggleShowKey, int windowID)
        {
            _aiChat = aiChat;
            _hero = hero;
            _toggleShowKey = toggleShowKey;
            _windowID = windowID;

            // Subscribe to message events for auto-scroll
            if (_aiChat != null)
            {
                _aiChat.OnMessageAdded += (s, e) => _autoScroll = true;
            }
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
                    _stylesInitialized = true;
                }

                _windowRect = GUI.Window(_windowID, _windowRect, DrawChatWindow, "", UITheme.WindowStyle);
            }
        }

        public void OnUpdate()
        {
            // Can be used for periodic updates if needed
        }

        private void InitializeStyles()
        {
            // User message style - right aligned, blue tint
            _userMessageStyle = new GUIStyle(GUI.skin.box);
            _userMessageStyle.normal.background = UITheme.CreateTexture(new Color(0.2f, 0.35f, 0.55f, 0.95f));
            _userMessageStyle.normal.textColor = UITheme.TextLight;
            _userMessageStyle.fontSize = UITheme.FONT_SIZE_NORMAL;
            _userMessageStyle.wordWrap = true;
            _userMessageStyle.alignment = TextAnchor.UpperLeft;
            _userMessageStyle.padding = new RectOffset(10, 10, 8, 8);

            // AI message style - left aligned, darker
            _aiMessageStyle = new GUIStyle(GUI.skin.box);
            _aiMessageStyle.normal.background = UITheme.CreateTexture(new Color(0.18f, 0.18f, 0.22f, 0.95f));
            _aiMessageStyle.normal.textColor = UITheme.TextLight;
            _aiMessageStyle.fontSize = UITheme.FONT_SIZE_NORMAL;
            _aiMessageStyle.wordWrap = true;
            _aiMessageStyle.alignment = TextAnchor.UpperLeft;
            _aiMessageStyle.padding = new RectOffset(10, 10, 8, 8);

            // System message style - centered, muted
            _systemMessageStyle = new GUIStyle(GUI.skin.label);
            _systemMessageStyle.normal.textColor = UITheme.TextMuted;
            _systemMessageStyle.fontSize = UITheme.FONT_SIZE_SMALL;
            _systemMessageStyle.fontStyle = FontStyle.Italic;
            _systemMessageStyle.wordWrap = true;
            _systemMessageStyle.alignment = TextAnchor.MiddleCenter;

            // Timestamp style
            _timestampStyle = new GUIStyle(GUI.skin.label);
            _timestampStyle.normal.textColor = new Color(0.5f, 0.5f, 0.55f, 1f);
            _timestampStyle.fontSize = 9;
            _timestampStyle.alignment = TextAnchor.UpperRight;

            // Input field style
            _inputFieldStyle = new GUIStyle(GUI.skin.textField);
            _inputFieldStyle.normal.background = UITheme.InputBgTexture;
            _inputFieldStyle.normal.textColor = UITheme.TextLight;
            _inputFieldStyle.fontSize = UITheme.FONT_SIZE_NORMAL;
            _inputFieldStyle.padding = new RectOffset(10, 10, 8, 8);

            // Quick action button style
            _quickActionStyle = new GUIStyle(GUI.skin.button);
            _quickActionStyle.normal.background = UITheme.CreateTexture(new Color(0.15f, 0.25f, 0.35f, 0.9f));
            _quickActionStyle.hover.background = UITheme.CreateTexture(new Color(0.2f, 0.35f, 0.5f, 0.95f));
            _quickActionStyle.normal.textColor = UITheme.TextLight;
            _quickActionStyle.hover.textColor = UITheme.TextLight;
            _quickActionStyle.fontSize = UITheme.FONT_SIZE_SMALL;
            _quickActionStyle.wordWrap = true;
            _quickActionStyle.alignment = TextAnchor.MiddleCenter;

            // Toggle style for fallback mode
            _toggleStyle = new GUIStyle(GUI.skin.toggle);
            _toggleStyle.normal.textColor = UITheme.TextMuted;
            _toggleStyle.fontSize = UITheme.FONT_SIZE_SMALL;
        }

        private void DrawChatWindow(int windowID)
        {
            GUILayout.BeginVertical();

            DrawHeader();
            GUILayout.Space(UITheme.BUTTON_SPACING);

            DrawChatMessages();
            GUILayout.Space(UITheme.BUTTON_SPACING);

            if (_showQuickActions)
            {
                DrawQuickActions();
                GUILayout.Space(UITheme.BUTTON_SPACING);
            }

            DrawInputArea();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 30));
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();

            // AI Icon
            GUI.color = UITheme.Accent;
            GUILayout.Label("🤖", GUILayout.Width(25), GUILayout.Height(30));
            GUI.color = Color.white;

            GUILayout.Label("AI ASSISTANT", UITheme.TitleStyle, GUILayout.Height(30));

            GUILayout.FlexibleSpace();

            // Fallback mode indicator
            if (_aiChat?.UseFallbackMode == true)
            {
                GUI.color = UITheme.Warning;
                GUILayout.Label("LOCAL", _timestampStyle, GUILayout.Height(25));
                GUI.color = Color.white;
            }

            // Toggle quick actions button
            string qaIcon = _showQuickActions ? "▼" : "▶";
            if (GUILayout.Button(qaIcon, UITheme.ButtonStyle, GUILayout.Width(25), GUILayout.Height(25)))
            {
                _showQuickActions = !_showQuickActions;
            }

            // Clear history button
            if (GUILayout.Button("🗑", UITheme.ButtonStyle, GUILayout.Width(25), GUILayout.Height(25)))
            {
                _aiChat?.ClearHistory();
            }

            GUILayout.Space(5);

            // Close button
            GUI.backgroundColor = UITheme.Danger;
            if (GUILayout.Button("X", UITheme.CloseButtonStyle, GUILayout.Width(UITheme.CLOSE_BUTTON_SIZE), GUILayout.Height(UITheme.CLOSE_BUTTON_SIZE)))
            {
                _isShow = false;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();

            UITheme.DrawSeparator();
        }

        private void DrawChatMessages()
        {
            float chatHeight = _showQuickActions ? _windowRect.height - 300 : _windowRect.height - 200;
            
            GUILayout.BeginVertical(UITheme.SectionStyle, GUILayout.Height(chatHeight));

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));

            if (_aiChat?.Messages != null)
            {
                foreach (var msg in _aiChat.Messages)
                {
                    DrawMessage(msg);
                    GUILayout.Space(8);
                }
            }

            // Auto-scroll to bottom when new messages arrive
            if (_autoScroll)
            {
                _scrollPosition.y = float.MaxValue;
                _autoScroll = false;
            }

            // Show typing indicator when waiting
            if (_aiChat?.IsWaitingForResponse == true)
            {
                DrawTypingIndicator();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawMessage(AIChatMessage msg)
        {
            // Check if it's a system message
            if (msg.Content.StartsWith("[System]"))
            {
                GUILayout.Label(msg.Content.Replace("[System] ", ""), _systemMessageStyle);
                return;
            }

            float maxWidth = _windowRect.width - 100;

            GUILayout.BeginHorizontal();

            if (msg.IsUserMessage)
            {
                // User message - right aligned
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical(GUILayout.MaxWidth(maxWidth));
                
                GUILayout.Label(msg.Timestamp.ToString("HH:mm"), _timestampStyle);
                GUILayout.Box(msg.Content, _userMessageStyle, GUILayout.MaxWidth(maxWidth));
                
                GUILayout.EndVertical();
            }
            else
            {
                // AI message - left aligned
                GUILayout.BeginVertical(GUILayout.MaxWidth(maxWidth));
                
                GUILayout.BeginHorizontal();
                GUI.color = UITheme.Accent;
                GUILayout.Label("🤖", GUILayout.Width(20));
                GUI.color = Color.white;
                GUILayout.Label(msg.Timestamp.ToString("HH:mm"), _timestampStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.Box(msg.Content, _aiMessageStyle, GUILayout.MaxWidth(maxWidth));
                
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawTypingIndicator()
        {
            GUILayout.BeginHorizontal();
            GUI.color = UITheme.Accent;
            GUILayout.Label("🤖", GUILayout.Width(20));
            GUI.color = Color.white;

            // Animated dots
            int dots = (int)(Time.time * 2) % 4;
            string typingText = "Thinking" + new string('.', dots);
            GUILayout.Label(typingText, _systemMessageStyle);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawQuickActions()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("💡 Quick Actions", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);

            var actions = _aiChat?.GetQuickActions() ?? new List<string>();
            
            // Display in rows of 2
            for (int i = 0; i < actions.Count; i += 2)
            {
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button(actions[i], _quickActionStyle, GUILayout.Height(28)))
                {
                    SendQuickAction(actions[i]);
                }
                
                if (i + 1 < actions.Count)
                {
                    if (GUILayout.Button(actions[i + 1], _quickActionStyle, GUILayout.Height(28)))
                    {
                        SendQuickAction(actions[i + 1]);
                    }
                }
                else
                {
                    GUILayout.FlexibleSpace();
                }
                
                GUILayout.EndHorizontal();
                GUILayout.Space(3);
            }

            GUILayout.EndVertical();
        }

        private void SendQuickAction(string action)
        {
            if (_aiChat != null && !_aiChat.IsWaitingForResponse)
            {
                _aiChat.SendMessage(action);
            }
        }

        private void DrawInputArea()
        {
            // Error display
            if (!string.IsNullOrEmpty(_aiChat?.LastError))
            {
                GUI.color = UITheme.Danger;
                GUILayout.Label(_aiChat.LastError, _systemMessageStyle);
                GUI.color = Color.white;
            }

            GUILayout.BeginHorizontal();

            // Input field
            GUI.SetNextControlName("ChatInput");
            _inputText = GUILayout.TextField(_inputText, _inputFieldStyle, GUILayout.Height(35));

            // Handle Enter key
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "ChatInput")
            {
                SendMessage();
                e.Use();
            }

            // Send button
            bool canSend = !string.IsNullOrWhiteSpace(_inputText) && _aiChat?.IsWaitingForResponse != true;
            GUI.enabled = canSend;
            
            GUI.backgroundColor = canSend ? UITheme.Accent : UITheme.ButtonNormal;
            if (GUILayout.Button("Send", UITheme.ButtonStyle, GUILayout.Width(60), GUILayout.Height(35)))
            {
                SendMessage();
            }
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;

            GUILayout.EndHorizontal();

            // Status line with fallback toggle
            GUILayout.BeginHorizontal();
            
            // Fallback mode toggle
            if (_aiChat != null)
            {
                bool fallbackMode = _aiChat.UseFallbackMode;
                bool newFallbackMode = GUILayout.Toggle(fallbackMode, "Local Mode", _toggleStyle, GUILayout.Width(90));
                if (newFallbackMode != fallbackMode)
                {
                    _aiChat.UseFallbackMode = newFallbackMode;
                    _aiChat.AddSystemMessage(newFallbackMode ? "Switched to local mode (no LLM)" : "Switched to LLM mode");
                }
            }
            
            GUILayout.FlexibleSpace();
            
            // Hero info badge
            if (_hero != null)
            {
                string heroInfo = $"Lv.{_hero.Level} | HP: {_hero.Health}/{_hero.MaxHealth}";
                GUILayout.Label(heroInfo, _timestampStyle);
            }
            GUILayout.EndHorizontal();
        }

        private void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(_inputText) && _aiChat?.IsWaitingForResponse != true)
            {
                _aiChat?.SendMessage(_inputText);
                _inputText = "";
                GUI.FocusControl("ChatInput");
            }
        }
    }
}
