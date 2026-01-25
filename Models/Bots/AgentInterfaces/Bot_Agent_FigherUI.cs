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
    internal class Bot_Agent_FigherUI
    {
        private Bot_Agent_Fighter _fightingAgent;

        private const int ELEMENT_HEIGHT = 30;
        private const int ELEMENT_PADDING = 10;
        private const int INPUT_FIELD_WIDTH = 80;

        public Bot_Agent_FigherUI(Bot_Agent_Fighter fightingAgent)
        {
            _fightingAgent = fightingAgent;
        }

        public void DrawFightingAgentPanel(Rect contentArea)
        {
            if (_fightingAgent == null)
                return;

            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(GUI.skin.box);

            // Kill Count Display
            GUILayout.BeginHorizontal();
            GUILayout.Label("Kill Count:", GUILayout.Width(150));
            GUILayout.Label($"{_fightingAgent.KillCount}", GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Max Distance Display
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Distance:", GUILayout.Width(150));
            string maxDistanceStr = GUILayout.TextField(_fightingAgent.MaxDistance.ToString("F1"), GUILayout.Width(INPUT_FIELD_WIDTH));
            if (float.TryParse(maxDistanceStr, out float newMaxDistance) && newMaxDistance > 0)
            {
                _fightingAgent.MaxDistance = newMaxDistance;
            }
            GUILayout.Label("units", GUILayout.Width(50));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Target Monster Display
            GUILayout.BeginHorizontal();
            GUILayout.Label("Targeted Monster:", GUILayout.Width(150));
            string targetName = _fightingAgent.TargetedMonster != null ? _fightingAgent.TargetedMonster.name : "None";
            GUILayout.Label(targetName, GUILayout.Width(200));
            
            Color originalBgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Reset", GUILayout.Width(80)))
            {
                _fightingAgent.TargetedMonster = null;
            }
            GUI.backgroundColor = originalBgColor;
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Target Monster Health
            if (_fightingAgent.TargetedMonster != null && _fightingAgent.TargetedMonster.Health != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Target Health:", GUILayout.Width(150));
                int targetHealth = _fightingAgent.TargetedMonster.Health.CurrentHealth;
                int targetMaxHealth = _fightingAgent.TargetedMonster.Health.MaxHealth;
                GUILayout.Label($"{targetHealth} / {targetMaxHealth}", GUILayout.Width(200));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(ELEMENT_PADDING);
            }

            // Last Target Health Change
            if (_fightingAgent.LastTargetMonsterHealthChanged.HasValue)
            {
                TimeSpan timeSinceLastHealthChange = DateTime.Now - _fightingAgent.LastTargetMonsterHealthChanged.Value;
                GUILayout.BeginHorizontal();
                GUILayout.Label("Last Health Change:", GUILayout.Width(150));
                GUILayout.Label($"{timeSinceLastHealthChange.TotalSeconds:F1}s ago", GUILayout.Width(200));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(ELEMENT_PADDING);
            }

            // Target Monster Timeout
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Timeout (s):", GUILayout.Width(150));
            string timeoutStr = GUILayout.TextField(_fightingAgent.TargetMonsterTimeout.ToString(), GUILayout.Width(INPUT_FIELD_WIDTH));
            if (int.TryParse(timeoutStr, out int newTimeout) && newTimeout > 0)
            {
                _fightingAgent.TargetMonsterTimeout = newTimeout;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Ignored Monsters Count
            GUILayout.BeginHorizontal();
            GUILayout.Label("Ignored Monsters:", GUILayout.Width(150));
            GUILayout.Label($"{_fightingAgent.IgnoredMonsters.Count}", GUILayout.Width(100));
            
            GUI.backgroundColor = new Color(0.8f, 0.6f, 0.3f);
            if (GUILayout.Button("Clear", GUILayout.Width(80)))
            {
                _fightingAgent.IgnoredMonsters.Clear();
            }
            GUI.backgroundColor = originalBgColor;
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Ignored Monsters List
            if (_fightingAgent.IgnoredMonsters.Count > 0)
            {
                GUILayout.Label("Ignored Monsters List:", GUI.skin.box);
                foreach (var monster in _fightingAgent.IgnoredMonsters.Take(5))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"• {monster.name}", GUILayout.Width(250));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                if (_fightingAgent.IgnoredMonsters.Count > 5)
                {
                    GUILayout.Label($"... and {_fightingAgent.IgnoredMonsters.Count - 5} more");
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
