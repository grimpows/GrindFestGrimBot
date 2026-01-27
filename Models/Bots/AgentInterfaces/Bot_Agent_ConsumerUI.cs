using Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GrindFest;

namespace Scripts.Models
{
    internal class Bot_Agent_ConsumerUI
    {
        private Bot_Agent_Consumer _consumerAgent;
        private bool _stylesInitialized = false;

        // GUI Styles
        private GUIStyle _sectionStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _valueStyle;
        private GUIStyle _cardStyle;

        // Textures
        private Texture2D _sectionBgTexture;
        private Texture2D _cardBgTexture;
        private Texture2D _barFillTexture;

        // Colors
        private Color _sectionColor = new Color(0.1f, 0.1f, 0.12f, 0.95f);
        private Color _cardColor = new Color(0.15f, 0.15f, 0.18f, 0.95f);
        private Color _accentColor = new Color(0.95f, 0.75f, 0.3f, 1f);
        private Color _healthColor = new Color(0.9f, 0.3f, 0.3f, 1f);
        private Color _manaColor = new Color(0.3f, 0.5f, 0.95f, 1f);
        private Color _skillColor = new Color(0.8f, 0.5f, 1f, 1f);
        private Color _positiveColor = new Color(0.4f, 1f, 0.55f, 1f);
        private Color _textLightColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        private Color _textMutedColor = new Color(0.65f, 0.65f, 0.7f, 1f);

        public Bot_Agent_ConsumerUI(Bot_Agent_Consumer consumerAgent)
        {
            _consumerAgent = consumerAgent;
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _sectionBgTexture = CreateTexture(_sectionColor);
            _cardBgTexture = CreateTexture(_cardColor);
            _barFillTexture = CreateTexture(Color.white);

            _sectionStyle = new GUIStyle(GUI.skin.box);
            _sectionStyle.normal.background = _sectionBgTexture;
            _sectionStyle.padding = new RectOffset(12, 12, 10, 10);

            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.fontSize = 12;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.normal.textColor = _textLightColor;

            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = 11;
            _labelStyle.normal.textColor = _textMutedColor;

            _valueStyle = new GUIStyle(GUI.skin.label);
            _valueStyle.fontSize = 11;
            _valueStyle.fontStyle = FontStyle.Bold;
            _valueStyle.normal.textColor = _textLightColor;

            _cardStyle = new GUIStyle(GUI.skin.box);
            _cardStyle.normal.background = _cardBgTexture;
            _cardStyle.padding = new RectOffset(10, 10, 8, 8);

            _stylesInitialized = true;
        }

        private Texture2D CreateTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        public void DrawConsumerAgentPanel(Rect contentArea)
        {
            if (_consumerAgent == null) return;

            InitializeStyles();

            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(_sectionStyle);

            // Header
            DrawSectionHeader("CONSUMER AGENT");
            GUILayout.Space(10);

            // Last Consumed Item Card
            DrawLastConsumedSection();
            GUILayout.Space(12);

            // Last Used Skill Card
            DrawLastSkillSection();
            GUILayout.Space(12);

            // Stats Summary
            DrawStatsSection();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Label(title, _titleStyle);
            Rect sepRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1));
            GUI.color = _accentColor;
            GUI.DrawTexture(sepRect, _barFillTexture);
            GUI.color = Color.white;
        }

        private void DrawLastConsumedSection()
        {
            GUILayout.BeginVertical(_cardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = _healthColor;
            GUILayout.Label("?", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Last Consumed Item", _titleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(6);

            string itemName = _consumerAgent.LastCosumedItem != null ? _consumerAgent.LastCosumedItem.Name : "None";
            Color itemColor = _consumerAgent.LastCosumedItem != null ? _positiveColor : _textMutedColor;

            GUIStyle nameStyle = new GUIStyle(_valueStyle);
            nameStyle.fontSize = 12;
            nameStyle.normal.textColor = itemColor;
            GUILayout.Label(itemName, nameStyle);

            // Show quantity if available
            if (_consumerAgent.LastCosumedItem?.LiquidContainer != null)
            {
                int healthAmount = _consumerAgent.LastCosumedItem.LiquidContainer.GetResourceAmount(ResourceType.Health);
                int manaAmount = _consumerAgent.LastCosumedItem.LiquidContainer.GetResourceAmount(ResourceType.Mana);

                GUILayout.BeginHorizontal();
                
                if (healthAmount > 0)
                {
                    GUI.color = _healthColor;
                    GUILayout.Label($"+{healthAmount} HP", _valueStyle, GUILayout.Width(80));
                }
                
                if (manaAmount > 0)
                {
                    GUI.color = _manaColor;
                    GUILayout.Label($"+{manaAmount} MP", _valueStyle, GUILayout.Width(80));
                }
                
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void DrawLastSkillSection()
        {
            GUILayout.BeginVertical(_cardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = _skillColor;
            GUILayout.Label("?", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Last Used Skill", _titleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(6);

            string skillName = _consumerAgent.LastUsedSkill != null ? _consumerAgent.LastUsedSkill.name : "None";
            Color skillColor = _consumerAgent.LastUsedSkill != null ? _skillColor : _textMutedColor;

            GUIStyle nameStyle = new GUIStyle(_valueStyle);
            nameStyle.fontSize = 12;
            nameStyle.normal.textColor = skillColor;
            GUILayout.Label(skillName, nameStyle);

            // Show skill details if available
            if (_consumerAgent.LastUsedSkill != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Level: {_consumerAgent.LastUsedSkill.Level}", _labelStyle, GUILayout.Width(80));
                GUILayout.Label($"Range: {_consumerAgent.LastUsedSkill.Range}m", _labelStyle, GUILayout.Width(80));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void DrawStatsSection()
        {
            GUILayout.Label("Activity Stats", _titleStyle);
            GUILayout.Space(4);

            // Could add more stats here in the future
            GUILayout.BeginHorizontal();
            GUILayout.Label("Status:", _labelStyle, GUILayout.Width(60));
            
            GUIStyle statusStyle = new GUIStyle(_valueStyle);
            statusStyle.normal.textColor = _positiveColor;
            GUILayout.Label("MONITORING", statusStyle);
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
