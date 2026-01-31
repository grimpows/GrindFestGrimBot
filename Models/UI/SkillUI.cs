using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Models
{
    public class SkillUI
    {
        private Hero_Base _hero;
        private KeyCode _toggleShowKey;
        private bool _isShow = false;
        private int _windowID;

        private Rect _skillsWindowRect = new Rect(100, 100, SKILL_CARD_WIDTH * 3.3f, 700);
        private Vector2 _scrollPosition = Vector2.zero;

        private List<SkillBehaviour> _skillsList = new List<SkillBehaviour>();
        private const float SKILL_CARD_WIDTH = 280f;
        private const float SKILL_CARD_HEIGHT = 340f;
        private const float SKILL_CARD_PADDING = 15f;

        // GUI Styles
        private GUIStyle _skillCardStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _statLabelStyle;
        private GUIStyle _descriptionStyle;
        private GUIStyle _reqMetStyle;
        private GUIStyle _reqNotMetStyle;

        // Textures
        private Texture2D _panelBackgroundTexture;

        // Colors
        private Color _headerColor = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        private Color _cardColor = new Color(0.2f, 0.2f, 0.25f, 0.9f);
        private Color _accentColor = new Color(0.95f, 0.75f, 0.3f, 1f);
        private Color _positiveColor = new Color(0.4f, 1f, 0.55f, 1f);
        private Color _negativeColor = new Color(1f, 0.4f, 0.4f, 1f);
        private Color _textLightColor = new Color(0.95f, 0.95f, 0.95f, 1f);

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
                UpdateSkillsList();
                InitializeStyles();
                _skillsWindowRect = GUI.Window(_windowID, _skillsWindowRect, DrawSkillsWindow, "SKILLS", GetWindowStyle());
            }
        }

        public void OnStart(Hero_Base hero, KeyCode toggleShowKey, int windowID)
        {
            _hero = hero;
            _toggleShowKey = toggleShowKey;
            _windowID = windowID;
        }

        public void OnUpdate()
        {
        }

        private void UpdateSkillsList()
        {
            if (_hero?.Skills != null)
            {
                _skillsList = _hero.Skills.ToList();
            }
        }

        private GUIStyle GetWindowStyle()
        {
            GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
            Texture2D bgTexture = GetBackgroundTexture();
            windowStyle.normal.background = bgTexture;
            windowStyle.onNormal.background = bgTexture;
            windowStyle.focused.background = bgTexture;
            windowStyle.onFocused.background = bgTexture;
            windowStyle.active.background = bgTexture;
            windowStyle.onActive.background = bgTexture;
            return windowStyle;
        }

        private void InitializeStyles()
        {
            if (_skillCardStyle != null)
                return;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = _accentColor }
            };

            _statLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = _textLightColor },
                wordWrap = true
            };

            _descriptionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f) },
                wordWrap = true
            };

            _reqMetStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = _positiveColor },
                fontStyle = FontStyle.Bold
            };

            _reqNotMetStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = _negativeColor },
                fontStyle = FontStyle.Bold
            };

            _skillCardStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = GetCardTexture(_cardColor) },
                border = new RectOffset(2, 2, 2, 2),
                padding = new RectOffset(10, 10, 10, 10)
            };
        }

        private void DrawSkillsWindow(int windowID)
        {
            try
            {
                GUILayout.BeginVertical(GUILayout.Height(_skillsWindowRect.height - 40));

                DrawSkillHeader();
                GUILayout.Space(10);

                DrawSkillsGrid();

                GUILayout.EndVertical();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error drawing skills window: {ex.Message}");
            }

            GUI.DragWindow(new Rect(0, 0, _skillsWindowRect.width, 25));
        }

        private void DrawSkillHeader()
        {
            GUI.backgroundColor = _headerColor;
            GUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = Color.white;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Available Skill Points", _titleStyle, GUILayout.Height(25));
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = _accentColor;
            GUILayout.Box($"{_hero.SkillPoints}", GUILayout.Width(50), GUILayout.Height(25));
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // Close button
            GUI.backgroundColor = _negativeColor;
            GUIStyle closeStyle = new GUIStyle(GUI.skin.button);
            closeStyle.fontSize = 14;
            closeStyle.fontStyle = FontStyle.Bold;
            closeStyle.normal.textColor = Color.white;
            closeStyle.hover.textColor = Color.white;

            if (GUILayout.Button("X", closeStyle, GUILayout.Width(28), GUILayout.Height(25)))
            {
                _isShow = false;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawSkillsGrid()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (_skillsList.Count == 0)
            {
                GUILayout.Label("No skills learned yet", _descriptionStyle);
            }
            else
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

                int skillsPerRow = Mathf.Max(1, (int)((_skillsWindowRect.width - 30) / (SKILL_CARD_WIDTH + SKILL_CARD_PADDING)));
                int skillInRow = 0;

                GUILayout.BeginHorizontal();

                for (int i = 0; i < _skillsList.Count; i++)
                {
                    var skill = _skillsList[i];
                    if (skill == null) continue;

                    if (skillInRow >= skillsPerRow && skillsPerRow > 0)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.Space(SKILL_CARD_PADDING);
                        GUILayout.BeginHorizontal();
                        skillInRow = 0;
                    }

                    DrawSkillCard(skill, i);
                    skillInRow++;
                }

                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
        }

        private void DrawSkillCard(SkillBehaviour skill, int index)
        {
            GUILayout.BeginVertical(_skillCardStyle, GUILayout.Width(SKILL_CARD_WIDTH), GUILayout.Height(SKILL_CARD_HEIGHT));

            GUILayout.Label(skill.name, _titleStyle, GUILayout.Height(30));
            GUILayout.Space(5);

            DrawLevelBar(skill.Level, skill.MaxLevel == 1 ? skill.MaxLevel : 100);
            GUILayout.Space(5);

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Level: {skill.Level}", _statLabelStyle, GUILayout.Height(20));
            GUILayout.Label($"Command: {skill.Command}", _statLabelStyle, GUILayout.Height(20));
            GUILayout.Label($"Range: {skill.Range}m", _statLabelStyle, GUILayout.Height(20));

            // Requirements for next level with color coding
            int nextLevel = skill.Level + 1;
            int reqStr = skill.GetRequiredStrength(nextLevel);
            int reqDex = skill.GetRequiredDexterity(nextLevel);
            int reqInt = skill.GetRequiredIntelligence(nextLevel);

            int heroStr = _hero?.Character?.Strength ?? 0;
            int heroDex = _hero?.Character?.Dexterity ?? 0;
            int heroInt = _hero?.Character?.Intelligence ?? 0;

            GUILayout.BeginHorizontal();

            // STR requirement
            GUIStyle strStyle = heroStr >= reqStr ? _reqMetStyle : _reqNotMetStyle;
            GUILayout.Label($"STR: {reqStr}", strStyle, GUILayout.Height(20), GUILayout.Width(75));

            // DEX requirement
            GUIStyle dexStyle = heroDex >= reqDex ? _reqMetStyle : _reqNotMetStyle;
            GUILayout.Label($"DEX: {reqDex}", dexStyle, GUILayout.Height(20), GUILayout.Width(75));

            // INT requirement
            GUIStyle intStyle = heroInt >= reqInt ? _reqMetStyle : _reqNotMetStyle;
            GUILayout.Label($"INT: {reqInt}", intStyle, GUILayout.Height(20), GUILayout.Width(75));

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(5);
            GUILayout.Label(skill.Description, _descriptionStyle, GUILayout.ExpandHeight(true));
            GUILayout.Space(8);

            GUILayout.BeginHorizontal();

            if (skill.CheckSkillRequirement(nextLevel, _hero.Hero, false))
            {
                GUI.backgroundColor = _positiveColor;
                if (GUILayout.Button("Upgrade", GUILayout.Height(25)))
                {
                    _hero.AllocateSkillPoints(skill.name, 1);
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Button("Upgrade", GUILayout.Height(25));
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawLevelBar(int currentLevel, int maxLevel)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Progress: {currentLevel}/{maxLevel}", _statLabelStyle, GUILayout.Height(18));

            float progress = (float)currentLevel / maxLevel;

            Rect barRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20), GUILayout.ExpandWidth(true));
            barRect.x += 5;
            barRect.width -= 10;

            GUI.Box(barRect, "", GUI.skin.box);

            Rect fillRect = new Rect(barRect.x + 2, barRect.y + 2, (barRect.width - 4) * progress, barRect.height - 4);
            Color progressColor = Color.Lerp(new Color(0.8f, 0.2f, 0.2f), _positiveColor, progress);
            GUI.color = progressColor;
            GUI.Box(fillRect, "");
            GUI.color = Color.white;

            GUILayout.EndVertical();
        }

        private Texture2D GetBackgroundTexture()
        {
            if (_panelBackgroundTexture == null)
            {
                _panelBackgroundTexture = new Texture2D(1, 1);
                _panelBackgroundTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.12f, 0.95f));
                _panelBackgroundTexture.Apply();
            }
            return _panelBackgroundTexture;
        }

        private Texture2D GetCardTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        public bool IsVisible => _isShow;
        public void SetVisible(bool visible) => _isShow = visible;
        public void ToggleVisible() => _isShow = !_isShow;
    }
}
