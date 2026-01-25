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

        private const int ELEMENT_HEIGHT = 30;
        private const int ELEMENT_PADDING = 10;

        public Bot_Agent_ConsumerUI(Bot_Agent_Consumer consumerAgent)
        {
            _consumerAgent = consumerAgent;
        }

        public void DrawConsumerAgentPanel(Rect contentArea)
        {
            if (_consumerAgent == null)
                return;

            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(GUI.skin.box);

            // Last Consumed Item Display
            GUILayout.BeginHorizontal();
            GUILayout.Label("Last Consumed Item:", GUILayout.Width(150));
            string lastItemName = _consumerAgent.LastCosumedItem != null ? _consumerAgent.LastCosumedItem.Name : "None";
            GUILayout.Label(lastItemName, GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //show last consumed item quantity if available
            GUILayout.BeginHorizontal();
            GUILayout.Label("Last Consumed Quantity:", GUILayout.Width(150));
            string lastItemQuantity = _consumerAgent.LastCosumedItem != null && _consumerAgent.LastCosumedItem.LiquidContainer != null ? _consumerAgent.LastCosumedItem.LiquidContainer.GetResourceAmount(ResourceType.Health).ToString() : "N/A";
            GUILayout.Label(lastItemQuantity, GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.Space(ELEMENT_PADDING);

            // Last Used Skill Display
            GUILayout.BeginHorizontal();
            GUILayout.Label("Last Used Skill:", GUILayout.Width(150));
            string lastSkillName = _consumerAgent.LastUsedSkill != null ? _consumerAgent.LastUsedSkill.name : "None";
            GUILayout.Label(lastSkillName, GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Consumption Stats
            GUILayout.Label("Consumption Stats", GUI.skin.box);
            GUILayout.Space(ELEMENT_PADDING);

            //GUILayout.BeginHorizontal();
            //GUILayout.Label("Status:", GUILayout.Width(150));
            //bool isActing = _consumerAgent.IsActing();
            //GUI.color = isActing ? Color.green : Color.red;
            //GUILayout.Label(isActing ? "ACTIVE" : "IDLE", GUILayout.Width(100));
            //GUI.color = Color.white;
            //GUILayout.FlexibleSpace();
            //GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
