using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrindFest;
using UnityEngine;

namespace Scripts.Models
{
    public class Bot_Agent_Traveler
    {

        private AutomaticHero _hero;

        private string _targetAreaName = "";
        private string _forcedAreaName = "";

        public string TargetAreaName { 
            get => _targetAreaName; 
            set
            {
                if (_targetAreaName != value)
                {
                    _targetAreaName = value;

                    if(!string.IsNullOrEmpty(value))
                        _hero?.Say($"Traveling to {_targetAreaName}");
                }
            }
        }

        /// <summary>
        /// Forces the bot to always travel to this area, ignoring the automatic best area calculation.
        /// Set to empty string or null to disable forced area.
        /// </summary>
        public string ForcedAreaName
        {
            get => _forcedAreaName;
            set
            {
                if (_forcedAreaName != value)
                {
                    _forcedAreaName = value ?? "";

                    if (!string.IsNullOrEmpty(_forcedAreaName))
                        _hero?.Say($"Forcing travel to {_forcedAreaName}");
                    else
                        _hero?.Say("Disabled forced area, returning to auto mode");
                }
            }
        }

        /// <summary>
        /// Returns true if a forced area is set.
        /// </summary>
        public bool IsForcedAreaEnabled => !string.IsNullOrEmpty(_forcedAreaName);

        public Bot_Agent_Traveler(AutomaticHero hero)
        {
            _hero = hero;
        }

        public bool IsActing()
        {
            if (_hero == null) return false;

            string bestArea = GetBestArea();
            if (_hero.CurrentArea?.Root?.name != null && _hero.CurrentArea.Root.name != bestArea)
            {
                TargetAreaName = bestArea;
                _hero.GoToArea(bestArea);
                return true;
            }

            TargetAreaName = "";

            return false;
        }

        public string GetBestArea()
        {
            // If forced area is set, always return it
            if (IsForcedAreaEnabled)
            {
                return _forcedAreaName;
            }

            int heroLevel = _hero.Level;
            var areaForLevel = MinLevelAreaDictionary
                .Where(kv => kv.Key <= heroLevel)
                .OrderByDescending(kv => kv.Key)
                .First();

            var lowerAreaForLevel = MinLevelAreaDictionary.First();

            if (areaForLevel.Key > 1)
            {
                lowerAreaForLevel = MinLevelAreaDictionary
                    .Where(kv => kv.Key < areaForLevel.Key)
                    .OrderByDescending(kv => kv.Key)
                    .First();

            }

            if (_hero.HealthPotionCount() < 5 && _hero.CurrentArea?.Root.name == areaForLevel.Value)
            {

                return lowerAreaForLevel.Value;
            }

            if (_hero.CurrentArea?.Root.name == lowerAreaForLevel.Value && _hero.HealthPotionCount() < 20)
            {
                return lowerAreaForLevel.Value;
            }

            return areaForLevel.Value;
           
        }

        /// <summary>
        /// Clears the forced area and returns to automatic mode.
        /// </summary>
        public void ClearForcedArea()
        {
            ForcedAreaName = "";
        }

        public Dictionary<int, string> MinLevelAreaDictionary = new Dictionary<int, string>()
        {
            {1, "Stony Plains" },
            {5, "Crimson Meadows" },
            {8, "Rotten Burrows" },
            //{10, "Depths of Despair" },
            {9, "Ashen Pastures" },
            {11, "Canyon of Death" },
            {12, "Endless Desert" },
        };
    }
}
