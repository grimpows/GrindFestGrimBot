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
    internal class Bot_Agent_LooterUI
    {
        private Bot_Agent_Looter _pickUpAgent;

        private const int ELEMENT_HEIGHT = 30;
        private const int ELEMENT_PADDING = 10;
        private const int INPUT_FIELD_WIDTH = 80;

        public Bot_Agent_LooterUI(Bot_Agent_Looter pickUpAgent)
        {
            _pickUpAgent = pickUpAgent;
        }

        public void DrawPickUpAgentPanel(Rect contentArea)
        {
            if (_pickUpAgent == null)
                return;

            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(GUI.skin.box);

            // Looted Item Count Display
            GUILayout.BeginHorizontal();
            GUILayout.Label("Looted Items:", GUILayout.Width(150));
            GUILayout.Label($"{_pickUpAgent.LootedItemCount}", GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Target Item Display
            GUILayout.BeginHorizontal();
            GUILayout.Label("Targeted Item:", GUILayout.Width(150));
            string targetItemName = _pickUpAgent.TargetedItem != null ? _pickUpAgent.TargetedItem.name : "None";
            GUILayout.Label(targetItemName, GUILayout.Width(200));
            
            Color originalBgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Reset", GUILayout.Width(80)))
            {
                _pickUpAgent.TargetedItem = null;
            }
            GUI.backgroundColor = originalBgColor;
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Scan Frequency
            GUILayout.BeginHorizontal();
            GUILayout.Label("Scan Frequency (s):", GUILayout.Width(150));
            string scanFreqStr = GUILayout.TextField(_pickUpAgent.ScanFrequency.ToString(), GUILayout.Width(INPUT_FIELD_WIDTH));
            if (int.TryParse(scanFreqStr, out int newScanFreq) && newScanFreq > 0)
            {
                _pickUpAgent.ScanFrequency = newScanFreq;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Last Scan Time
            TimeSpan timeSinceLastScan = DateTime.Now - _pickUpAgent.LastScanTime;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Last Scan:", GUILayout.Width(150));
            GUILayout.Label($"{timeSinceLastScan.TotalSeconds:F1}s ago", GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Target Item Timeout
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Timeout (s):", GUILayout.Width(150));
            string targetTimeoutStr = GUILayout.TextField(_pickUpAgent.TargetItemTimeout.ToString(), GUILayout.Width(INPUT_FIELD_WIDTH));
            if (int.TryParse(targetTimeoutStr, out int newTargetTimeout) && newTargetTimeout > 0)
            {
                _pickUpAgent.TargetItemTimeout = newTargetTimeout;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Scanned Items Count
            GUILayout.BeginHorizontal();
            GUILayout.Label("Scanned Items:", GUILayout.Width(150));
            GUILayout.Label($"{_pickUpAgent.ScannedItems.Count}", GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Ignored Items Count
            GUILayout.BeginHorizontal();
            GUILayout.Label("Ignored Items:", GUILayout.Width(150));
            GUILayout.Label($"{_pickUpAgent.IgnoredItems.Count}", GUILayout.Width(100));
            
            GUI.backgroundColor = new Color(0.8f, 0.6f, 0.3f);
            if (GUILayout.Button("Clear", GUILayout.Width(80)))
            {
                _pickUpAgent.IgnoredItems.Clear();
            }
            GUI.backgroundColor = originalBgColor;
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Force Rescan Toggle
            GUILayout.BeginHorizontal();
            GUILayout.Label("Force Rescan:", GUILayout.Width(150));
            GUI.backgroundColor = _pickUpAgent.ForceRescan ? new Color(0.3f, 1f, 0.3f) : new Color(0.6f, 0.6f, 0.6f);
            if (GUILayout.Button(_pickUpAgent.ForceRescan ? "ON" : "OFF", GUILayout.Width(80)))
            {
                _pickUpAgent.ForceRescan = !_pickUpAgent.ForceRescan;
            }
            GUI.backgroundColor = originalBgColor;
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(ELEMENT_PADDING);

            // Ignored Items List
            if (_pickUpAgent.IgnoredItems.Count > 0)
            {
                GUILayout.Label("Ignored Items List:", GUI.skin.box);
                foreach (var item in _pickUpAgent.IgnoredItems.Take(5))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"• {item.name}", GUILayout.Width(250));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                if (_pickUpAgent.IgnoredItems.Count > 5)
                {
                    GUILayout.Label($"... and {_pickUpAgent.IgnoredItems.Count - 5} more");
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
