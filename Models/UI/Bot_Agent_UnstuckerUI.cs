using UnityEngine;

namespace Scripts.Models
{
    public class Bot_Agent_UnstuckerUI
    {
        private Bot_Agent_Unstucker _unstuckerAgent;

        public Bot_Agent_UnstuckerUI(Bot_Agent_Unstucker unstuckerAgent)
        {
            _unstuckerAgent = unstuckerAgent;
        }

        public void DrawUnstuckerAgentPanel(Rect contentArea)
        {
            if (_unstuckerAgent == null) return;

            GUILayout.BeginArea(contentArea);
            GUILayout.BeginVertical(UITheme.SectionStyle);

            DrawSectionHeader("UNSTUCKER AGENT");
            GUILayout.Space(UITheme.BUTTON_SPACING);

            // Main content: Left panel (info) + Right panel (settings)
            GUILayout.BeginHorizontal();

            // Left Panel - Status & Info
            DrawLeftPanel();

            GUILayout.Space(UITheme.WINDOW_PADDING);

            // Right Panel - Settings
            DrawRightPanel();

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Label(title, UITheme.SubtitleStyle);
            UITheme.DrawSeparator();
        }

        private void DrawLeftPanel()
        {
            GUILayout.BeginVertical(GUILayout.Width(320));

            // Status Cards Row
            DrawStatusSection();
            GUILayout.Space(UITheme.SECTION_SPACING);

            // Current Status Card
            DrawStatusCard();
            GUILayout.Space(UITheme.SECTION_SPACING);

            // Position Tracking Card
            DrawPositionCard();
            GUILayout.Space(UITheme.SECTION_SPACING);

            // Actions Card
            DrawActionsCard();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DrawRightPanel()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            GUILayout.Label("Settings", UITheme.SubtitleStyle);
            GUILayout.Space(4);
            UITheme.DrawSeparator(UITheme.TextMuted, 1f);
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            DrawSettingsCard();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DrawStatusSection()
        {
            GUILayout.BeginHorizontal();

            // Unstick Mode Status
            string modeText = _unstuckerAgent.IsOnUnstickMode ? "ACTIVE" : "INACTIVE";
            Color modeColor = _unstuckerAgent.IsOnUnstickMode ? UITheme.Warning : UITheme.Positive;
            DrawStatBox("Mode", modeText, modeColor);

            GUILayout.Space(UITheme.BUTTON_SPACING);

            // Unstick Count
            DrawStatBox("Count", _unstuckerAgent.UnstickCount.ToString(), UITheme.Info);

            GUILayout.Space(UITheme.BUTTON_SPACING);

            // Stuck Check
            bool isStuck = _unstuckerAgent.CheckIfStuck();
            DrawStatBox("Stuck?", isStuck ? "YES" : "NO", isStuck ? UITheme.Danger : UITheme.Positive);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawStatBox(string label, string value, Color valueColor)
        {
            GUILayout.BeginVertical(GUILayout.Width(80));
            GUILayout.Label(label, UITheme.LabelStyle);
            GUILayout.Label(value, UITheme.CreateValueStyle(valueColor, 14));
            GUILayout.EndVertical();
        }

        private void DrawStatusCard()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = _unstuckerAgent.IsOnUnstickMode ? UITheme.Warning : UITheme.Positive;
            GUILayout.Label("?", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Unstick Status", UITheme.SubtitleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            if (_unstuckerAgent.IsOnUnstickMode)
            {
                // Time remaining
                float elapsed = (float)_unstuckerAgent.TimeSinceUnstickStarted.TotalSeconds;
                float remaining = _unstuckerAgent.UnstickModeDurationSeconds - elapsed;
                float progress = elapsed / _unstuckerAgent.UnstickModeDurationSeconds;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Time Remaining:", UITheme.LabelStyle, GUILayout.Width(100));
                GUILayout.Label($"{remaining:F1}s", UITheme.CreateValueStyle(UITheme.Warning));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                // Progress bar
                GUILayout.Space(4);
                Rect barRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(8));
                UITheme.DrawProgressBar(barRect, progress, UITheme.Warning);

                GUILayout.Space(4);
                GUILayout.Label(">>> Running around to get unstuck >>>", UITheme.CreateLabelStyle(UITheme.Warning, UITheme.FONT_SIZE_SMALL, FontStyle.Italic));
            }
            else
            {
                GUILayout.Label("Not in unstick mode", UITheme.CreateValueStyle(UITheme.TextMuted));
            }

            GUILayout.EndVertical();
        }

        private void DrawPositionCard()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.BeginHorizontal();
            GUI.color = UITheme.Info;
            GUILayout.Label("?", GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label("Position Tracking", UITheme.SubtitleStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            // Last Position
            GUILayout.BeginHorizontal();
            GUILayout.Label("Last Position:", UITheme.LabelStyle, GUILayout.Width(100));
            string posStr = $"({_unstuckerAgent.LastHeroPosition.x:F1}, {_unstuckerAgent.LastHeroPosition.y:F1}, {_unstuckerAgent.LastHeroPosition.z:F1})";
            GUILayout.Label(posStr, UITheme.ValueStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Time Since Move
            GUILayout.BeginHorizontal();
            GUILayout.Label("Time Since Move:", UITheme.LabelStyle, GUILayout.Width(100));
            double timeSinceMove = _unstuckerAgent.TimeSinceLastMove.TotalSeconds;
            Color timeColor = UITheme.GetTimeStatusColor(timeSinceMove, _unstuckerAgent.StuckTimeThresholdSeconds * 0.5, _unstuckerAgent.StuckTimeThresholdSeconds);
            GUILayout.Label($"{timeSinceMove:F1}s", UITheme.CreateValueStyle(timeColor));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Distance from Last
            GUILayout.BeginHorizontal();
            GUILayout.Label("Distance Moved:", UITheme.LabelStyle, GUILayout.Width(100));
            float distance = _unstuckerAgent.DistanceFromLastPosition;
            Color distColor = distance > _unstuckerAgent.StuckDistanceThreshold ? UITheme.Positive : UITheme.Warning;
            GUILayout.Label($"{distance:F1} units", UITheme.CreateValueStyle(distColor));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawActionsCard()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            GUILayout.Label("Actions", UITheme.SubtitleStyle);
            GUILayout.Space(UITheme.ELEMENT_SPACING);

            GUILayout.BeginHorizontal();

            // Force Unstick button
            if (_unstuckerAgent.IsOnUnstickMode)
            {
                GUI.backgroundColor = UITheme.Positive;
                if (GUILayout.Button("Exit Unstick", UITheme.ButtonStyle, GUILayout.Height(25)))
                {
                    _unstuckerAgent.ForceExitUnstickMode();
                }
            }
            else
            {
                GUI.backgroundColor = UITheme.Warning;
                if (GUILayout.Button("Force Unstick", UITheme.ButtonStyle, GUILayout.Height(25)))
                {
                    _unstuckerAgent.ForceEnterUnstickMode();
                }
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(UITheme.BUTTON_SPACING);

            // Reset Position button
            GUI.backgroundColor = UITheme.Info;
            if (GUILayout.Button("Reset Position", UITheme.ButtonStyle, GUILayout.Height(25)))
            {
                _unstuckerAgent.ResetPosition();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(UITheme.BUTTON_SPACING);

            // Reset Counter button
            GUI.backgroundColor = UITheme.TextMuted;
            if (GUILayout.Button("Reset Count", UITheme.ButtonStyle, GUILayout.Height(25)))
            {
                _unstuckerAgent.ResetUnstickCount();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawSettingsCard()
        {
            GUILayout.BeginVertical(UITheme.CardStyle);

            // Stuck Distance Threshold
            GUILayout.BeginHorizontal();
            GUILayout.Label("Stuck Distance:", UITheme.LabelStyle, GUILayout.Width(120));
            string distStr = GUILayout.TextField(_unstuckerAgent.StuckDistanceThreshold.ToString("F1"), UITheme.InputStyle, GUILayout.Width(UITheme.INPUT_FIELD_WIDTH));
            if (float.TryParse(distStr, out float newDist) && newDist > 0)
            {
                _unstuckerAgent.StuckDistanceThreshold = newDist;
            }
            GUILayout.Label("units", UITheme.LabelStyle, GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            // Stuck Time Threshold
            GUILayout.BeginHorizontal();
            GUILayout.Label("Stuck Time:", UITheme.LabelStyle, GUILayout.Width(120));
            string timeStr = GUILayout.TextField(_unstuckerAgent.StuckTimeThresholdSeconds.ToString("F1"), UITheme.InputStyle, GUILayout.Width(UITheme.INPUT_FIELD_WIDTH));
            if (float.TryParse(timeStr, out float newTime) && newTime > 0)
            {
                _unstuckerAgent.StuckTimeThresholdSeconds = newTime;
            }
            GUILayout.Label("sec", UITheme.LabelStyle, GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            // Unstick Duration
            GUILayout.BeginHorizontal();
            GUILayout.Label("Unstick Duration:", UITheme.LabelStyle, GUILayout.Width(120));
            string durationStr = GUILayout.TextField(_unstuckerAgent.UnstickModeDurationSeconds.ToString("F1"), UITheme.InputStyle, GUILayout.Width(UITheme.INPUT_FIELD_WIDTH));
            if (float.TryParse(durationStr, out float newDuration) && newDuration > 0)
            {
                _unstuckerAgent.UnstickModeDurationSeconds = newDuration;
            }
            GUILayout.Label("sec", UITheme.LabelStyle, GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(UITheme.SECTION_SPACING);

            // Info box
            GUILayout.BeginVertical(UITheme.CardStyle);
            GUILayout.Label("How it works:", UITheme.SubtitleStyle);
            GUILayout.Space(4);

            GUIStyle infoStyle = UITheme.CreateLabelStyle(UITheme.TextMuted, UITheme.FONT_SIZE_SMALL);
            infoStyle.wordWrap = true;

            GUILayout.Label("• If the hero moves less than 'Stuck Distance' for longer than 'Stuck Time', unstick mode activates.", infoStyle);
            GUILayout.Space(2);
            GUILayout.Label("• During unstick mode, the hero will run around randomly for 'Unstick Duration' seconds.", infoStyle);
            GUILayout.Space(2);
            GUILayout.Label("• Active actions (fighting, looting, etc.) reset the stuck timer.", infoStyle);

            GUILayout.EndVertical();
        }
    }
}
