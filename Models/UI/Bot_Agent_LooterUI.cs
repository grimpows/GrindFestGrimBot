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

        public Bot_Agent_LooterUI(Bot_Agent_Looter pickUpAgent)
        {
            _pickUpAgent = pickUpAgent;
        }

        public void DrawPickUpAgentPanel(Rect contentArea)
        {
            if (_pickUpAgent == null) return;

            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(UITheme.SectionStyle);

            DrawSectionHeader("LOOTER AGENT");
            GUILayout.Space(UITheme.BUTTON_SPACING);

            // Stats Row
            GUILayout.BeginHorizontal();
            DrawStatBox("Looted", _pickUpAgent.LootedItemCount.ToString(), UITheme.Positive);
            GUILayout.Space(UITheme.BUTTON_SPACING);
            DrawStatBox("Scanned", _pickUpAgent.ScannedItems.Count.ToString(), UITheme.Info);
            GUILayout.Space(UITheme.BUTTON_SPACING);
            DrawStatBox("Ignored", _pickUpAgent.IgnoredItems.Count.ToString(), UITheme.Warning);
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.WINDOW_PADDING);

            DrawTargetSection();
            GUILayout.Space(UITheme.SECTION_SPACING);

            DrawSettingsSection();
            GUILayout.Space(UITheme.SECTION_SPACING);

            DrawIgnoredItemsSection();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Label(title, UITheme.SubtitleStyle);
            UITheme.DrawSeparator();
        }

        private void DrawStatBox(string label, string value, Color valueColor)
        {
            GUILayout.BeginVertical(GUILayout.Width(80));
            GUILayout.Label(label, UITheme.LabelStyle);
            GUILayout.Label(value, UITheme.CreateValueStyle(valueColor, 14));
            GUILayout.EndVertical();
        }

        private void DrawTargetSection()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);
            GUILayout.Label("Current Target", UITheme.SubtitleStyle);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();

            string targetName = _pickUpAgent.TargetedItem != null ? _pickUpAgent.TargetedItem.name : "None";
            Color targetColor = _pickUpAgent.TargetedItem != null ? UITheme.Positive : UITheme.TextMuted;
            GUILayout.Label(targetName, UITheme.CreateValueStyle(targetColor), GUILayout.ExpandWidth(true));

            if (_pickUpAgent.TargetedItem != null)
            {
                GUI.backgroundColor = UITheme.Danger;
                if (GUILayout.Button("Reset", UITheme.ButtonStyle, GUILayout.Width(60), GUILayout.Height(20)))
                {
                    _pickUpAgent.TargetedItem = null;
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.EndHorizontal();

            // Last scan time
            TimeSpan timeSinceLastScan = DateTime.Now - _pickUpAgent.LastScanTime;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Last Scan:", UITheme.LabelStyle, GUILayout.Width(70));
            Color scanColor = UITheme.GetTimeStatusColor(timeSinceLastScan.TotalSeconds);
            GUILayout.Label($"{timeSinceLastScan.TotalSeconds:F1}s ago", UITheme.CreateValueStyle(scanColor));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawSettingsSection()
        {
            GUILayout.Label("Settings", UITheme.SubtitleStyle);
            GUILayout.Space(4);

            // Scan Frequency
            GUILayout.BeginHorizontal();
            GUILayout.Label("Scan Frequency:", UITheme.LabelStyle, GUILayout.Width(110));
            string scanFreqStr = GUILayout.TextField(_pickUpAgent.ScanFrequency.ToString(), UITheme.InputStyle, GUILayout.Width(UITheme.INPUT_FIELD_WIDTH));
            if (int.TryParse(scanFreqStr, out int newScanFreq) && newScanFreq > 0)
            {
                _pickUpAgent.ScanFrequency = newScanFreq;
            }
            GUILayout.Label("sec", UITheme.LabelStyle, GUILayout.Width(30));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            // Target Timeout
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Timeout:", UITheme.LabelStyle, GUILayout.Width(110));
            string timeoutStr = GUILayout.TextField(_pickUpAgent.TargetItemTimeout.ToString(), UITheme.InputStyle, GUILayout.Width(UITheme.INPUT_FIELD_WIDTH));
            if (int.TryParse(timeoutStr, out int newTimeout) && newTimeout > 0)
            {
                _pickUpAgent.TargetItemTimeout = newTimeout;
            }
            GUILayout.Label("sec", UITheme.LabelStyle, GUILayout.Width(30));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            // Force Rescan Toggle
            GUILayout.BeginHorizontal();
            GUILayout.Label("Force Rescan:", UITheme.LabelStyle, GUILayout.Width(110));

            GUIStyle toggleStyle = _pickUpAgent.ForceRescan ? UITheme.ToggleOnStyle : UITheme.ToggleOffStyle;
            GUI.backgroundColor = _pickUpAgent.ForceRescan ? UITheme.Positive : UITheme.TextMuted;

            if (GUILayout.Button(_pickUpAgent.ForceRescan ? "ON" : "OFF", toggleStyle, GUILayout.Width(UITheme.INPUT_FIELD_WIDTH), GUILayout.Height(20)))
            {
                _pickUpAgent.ForceRescan = !_pickUpAgent.ForceRescan;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawIgnoredItemsSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Ignored Items ({_pickUpAgent.IgnoredItems.Count})", UITheme.SubtitleStyle);
            GUILayout.FlexibleSpace();

            if (_pickUpAgent.IgnoredItems.Count > 0)
            {
                GUI.backgroundColor = UITheme.Warning;
                if (GUILayout.Button("Clear All", UITheme.ButtonStyle, GUILayout.Width(70), GUILayout.Height(18)))
                {
                    _pickUpAgent.IgnoredItems.Clear();
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            GUIStyle listStyle = UITheme.CreateLabelStyle(UITheme.TextMuted, UITheme.FONT_SIZE_SMALL);

            if (_pickUpAgent.IgnoredItems.Count > 0)
            {
                foreach (var item in _pickUpAgent.IgnoredItems.Take(5))
                {
                    GUILayout.Label($"  • {item?.name}", listStyle);
                }

                if (_pickUpAgent.IgnoredItems.Count > 5)
                {
                    GUIStyle moreStyle = UITheme.CreateLabelStyle(UITheme.TextMuted, UITheme.FONT_SIZE_SMALL, FontStyle.Italic);
                    GUILayout.Label($"  ... +{_pickUpAgent.IgnoredItems.Count - 5} more", moreStyle);
                }
            }
            else
            {
                GUILayout.Label("  No ignored items", listStyle);
            }
        }
    }
}
