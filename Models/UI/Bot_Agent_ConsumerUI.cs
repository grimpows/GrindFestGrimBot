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

        public Bot_Agent_ConsumerUI(Bot_Agent_Consumer consumerAgent)
        {
            _consumerAgent = consumerAgent;
        }

        public void DrawConsumerAgentPanel(Rect contentArea)
        {
            if (_consumerAgent == null) return;

            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(UITheme.SectionStyle);

            DrawSectionHeader("CONSUMER AGENT");
            GUILayout.Space(UITheme.SECTION_SPACING);

            DrawLastConsumedSection();
            GUILayout.Space(UITheme.WINDOW_PADDING);

            DrawLastSkillSection();
            GUILayout.Space(UITheme.WINDOW_PADDING);

            DrawStatsSection();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Label(title, UITheme.SubtitleStyle);
            UITheme.DrawSeparator();
        }

        private void DrawLastConsumedSection()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = UITheme.Health;
            GUILayout.Label("?", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Last Consumed Item", UITheme.SubtitleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            string itemName = _consumerAgent.LastCosumedItem != null ? _consumerAgent.LastCosumedItem.Name : "None";
            Color itemColor = _consumerAgent.LastCosumedItem != null ? UITheme.Positive : UITheme.TextMuted;
            GUILayout.Label(itemName, UITheme.CreateValueStyle(itemColor, 12));

            // Show quantity if available
            if (_consumerAgent.LastCosumedItem?.LiquidContainer != null)
            {
                int healthAmount = _consumerAgent.LastCosumedItem.LiquidContainer.GetResourceAmount(ResourceType.Health);
                int manaAmount = _consumerAgent.LastCosumedItem.LiquidContainer.GetResourceAmount(ResourceType.Mana);

                GUILayout.BeginHorizontal();

                if (healthAmount > 0)
                    GUILayout.Label($"+{healthAmount} HP", UITheme.CreateValueStyle(UITheme.Health), GUILayout.Width(80));

                if (manaAmount > 0)
                    GUILayout.Label($"+{manaAmount} MP", UITheme.CreateValueStyle(UITheme.Mana), GUILayout.Width(80));

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void DrawLastSkillSection()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = UITheme.Accent;
            GUILayout.Label("?", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Last Used Skill", UITheme.SubtitleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            string skillName = _consumerAgent.LastUsedSkill != null ? _consumerAgent.LastUsedSkill.name : "None";
            Color skillColor = _consumerAgent.LastUsedSkill != null ? UITheme.Accent : UITheme.TextMuted;
            GUILayout.Label(skillName, UITheme.CreateValueStyle(skillColor, 12));

            // Show skill details if available
            if (_consumerAgent.LastUsedSkill != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Level: {_consumerAgent.LastUsedSkill.Level}", UITheme.LabelStyle, GUILayout.Width(80));
                GUILayout.Label($"Range: {_consumerAgent.LastUsedSkill.Range}m", UITheme.LabelStyle, GUILayout.Width(80));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void DrawStatsSection()
        {
            GUILayout.Label("Activity Stats", UITheme.SubtitleStyle);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Status:", UITheme.LabelStyle, GUILayout.Width(60));
            GUILayout.Label("MONITORING", UITheme.CreateValueStyle(UITheme.Positive));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
