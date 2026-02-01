using GrindFest;
using System;
using UnityEngine;

namespace Scripts.Models
{
    public class Bot_Agent_Unstucker
    {
        private AutomaticHero _hero;

        /// <summary>
        /// Distance threshold to consider the hero as "stuck".
        /// If the hero moves less than this distance, they might be stuck.
        /// </summary>
        public float StuckDistanceThreshold { get; set; } = 2f;

        /// <summary>
        /// Time in seconds before considering the hero as stuck.
        /// </summary>
        public float StuckTimeThresholdSeconds { get; set; } = 3f;

        /// <summary>
        /// Duration in seconds for the unstick mode.
        /// </summary>
        public float UnstickModeDurationSeconds { get; set; } = 5f;

        /// <summary>
        /// Last recorded position of the hero.
        /// </summary>
        public Vector3 LastHeroPosition { get; set; } = Vector3.zero;

        /// <summary>
        /// Time when the last position was recorded.
        /// </summary>
        public DateTime LastHeroPositionTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Time when unstick mode started.
        /// </summary>
        public DateTime UnstickStartTime { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Number of times the bot has entered unstick mode.
        /// </summary>
        public int UnstickCount { get; private set; } = 0;

        private bool _isOnUnstickMode = false;

        /// <summary>
        /// Whether the bot is currently in unstick mode.
        /// </summary>
        public bool IsOnUnstickMode
        {
            get => _isOnUnstickMode;
            private set
            {
                if (_isOnUnstickMode != value)
                {
                    _isOnUnstickMode = value;
                    if (_isOnUnstickMode)
                    {
                        UnstickStartTime = DateTime.Now;
                        UnstickCount++;
                        _hero?.Say("Entering Unstick Mode");
                    }
                    else
                    {
                        _hero?.Say("Exiting Unstick Mode");
                        ResetPosition();
                    }
                }
            }
        }

        /// <summary>
        /// Time elapsed since unstick mode started.
        /// </summary>
        public TimeSpan TimeSinceUnstickStarted => IsOnUnstickMode ? DateTime.Now - UnstickStartTime : TimeSpan.Zero;

        /// <summary>
        /// Time elapsed since last position change.
        /// </summary>
        public TimeSpan TimeSinceLastMove => DateTime.Now - LastHeroPositionTime;

        /// <summary>
        /// Distance from last recorded position.
        /// </summary>
        public float DistanceFromLastPosition => _hero != null ? Vector3.Distance(_hero.Character.transform.position, LastHeroPosition) : 0f;

        public Bot_Agent_Unstucker(AutomaticHero hero)
        {
            _hero = hero;
        }

        /// <summary>
        /// Initialize the position tracking. Call this when the bot starts.
        /// </summary>
        public void Initialize()
        {
            if (_hero != null && LastHeroPosition == Vector3.zero)
            {
                ResetPosition();
            }
        }

        /// <summary>
        /// Reset the last known position to the current position.
        /// </summary>
        public void ResetPosition()
        {
            if (_hero != null)
            {
                LastHeroPosition = _hero.Character.transform.position;
                LastHeroPositionTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Call this when the bot is actively doing something (fighting, looting, etc.)
        /// to prevent false stuck detection.
        /// </summary>
        public void NotifyActivity()
        {
            LastHeroPositionTime = DateTime.Now;
        }

        /// <summary>
        /// Check if the hero has moved enough since last check.
        /// Updates the position if moved beyond threshold.
        /// </summary>
        public void UpdatePositionTracking()
        {
            if (_hero == null) return;

            float distance = Vector3.Distance(LastHeroPosition, _hero.Character.transform.position);
            if (distance > StuckDistanceThreshold)
            {
                LastHeroPosition = _hero.Character.transform.position;
                LastHeroPositionTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Check if the hero appears to be stuck and should enter unstick mode.
        /// </summary>
        public bool CheckIfStuck()
        {
            if (_hero == null) return false;

            return TimeSinceLastMove.TotalSeconds > StuckTimeThresholdSeconds;
        }

        /// <summary>
        /// Main method to check and handle unstick logic.
        /// Returns true if the agent is acting (in unstick mode).
        /// </summary>
        public bool IsActing()
        {
            if (_hero == null) return false;

            // If already in unstick mode
            if (IsOnUnstickMode)
            {
                _hero.RunAroundInArea();

                // Check if unstick duration has elapsed
                if (TimeSinceUnstickStarted.TotalSeconds > UnstickModeDurationSeconds)
                {
                    IsOnUnstickMode = false;
                }

                return true;
            }

            // Check if we should enter unstick mode
            if (CheckIfStuck())
            {
                IsOnUnstickMode = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Force exit from unstick mode.
        /// </summary>
        public void ForceExitUnstickMode()
        {
            IsOnUnstickMode = false;
        }

        /// <summary>
        /// Force enter unstick mode.
        /// </summary>
        public void ForceEnterUnstickMode()
        {
            IsOnUnstickMode = true;
        }

        /// <summary>
        /// Reset the unstick counter.
        /// </summary>
        public void ResetUnstickCount()
        {
            UnstickCount = 0;
        }
    }
}
