using GrindFest;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Models
{
    /// <summary>
    /// Represents a zone defined by a Vector3 position and a radius.
    /// </summary>
    public class VectorZone
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float Radius { get; set; }

        public VectorZone(string name, Vector3 position, float radius)
        {
            Name = name;
            Position = position;
            Radius = radius;
        }

        public VectorZone(string name, float x, float y, float z, float radius)
        {
            Name = name;
            Position = new Vector3(x, y, z);
            Radius = radius;
        }

        public override string ToString()
        {
            return $"{Name} ({Position.x:F1}, {Position.y:F1}, {Position.z:F1}) R:{Radius:F1}";
        }
    }

    public class Bot_Agent_Traveler
    {

        private AutomaticHero _hero;

        private string _targetAreaName = "";
        private string _forcedAreaName = "";
        private VectorZone _forcedVectorZone = null;

        public string TargetAreaName
        {
            get => _targetAreaName;
            set
            {
                if (_targetAreaName != value)
                {
                    _targetAreaName = value;

                    if (!string.IsNullOrEmpty(value))
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
                    {
                        // Clear vector zone when forcing an area name
                        _forcedVectorZone = null;
                        _hero?.Say($"Forcing travel to {_forcedAreaName}");
                    }
                    else
                        _hero?.Say("Disabled forced area, returning to auto mode");
                }
            }
        }

        /// <summary>
        /// Gets or sets the forced Vector3 zone. When set, the bot will travel to this position.
        /// </summary>
        public VectorZone ForcedVectorZone
        {
            get => _forcedVectorZone;
            set
            {
                if (_forcedVectorZone != value)
                {
                    _forcedVectorZone = value;

                    if (_forcedVectorZone != null)
                    {
                        // Clear area name when forcing a vector zone
                        _forcedAreaName = "";
                        _hero?.Say($"Forcing travel to zone: {_forcedVectorZone.Name}");
                    }
                    else
                        _hero?.Say("Disabled forced vector zone, returning to auto mode");
                }
            }
        }

        /// <summary>
        /// Returns true if a forced area is set.
        /// </summary>
        public bool IsForcedAreaEnabled => !string.IsNullOrEmpty(_forcedAreaName);

        /// <summary>
        /// Returns true if a forced vector zone is set.
        /// </summary>
        public bool IsForcedVectorZoneEnabled => _forcedVectorZone != null;

        /// <summary>
        /// Returns true if any forced mode is enabled (area or vector zone).
        /// </summary>
        public bool IsAnyForcedModeEnabled => IsForcedAreaEnabled || IsForcedVectorZoneEnabled;

        public Bot_Agent_Traveler(AutomaticHero hero)
        {
            _hero = hero;
        }

        public bool IsActing()
        {
            if (_hero == null) return false;

            // Check if forced vector zone is active
            if (IsForcedVectorZoneEnabled)
            {
                return HandleVectorZoneTravel();
            }

            string bestArea = GetAreaToTravel();
            if (_hero.CurrentArea?.Root?.name != null && _hero.CurrentArea.Root.name != bestArea)
            {
                TargetAreaName = bestArea;
                _hero.GoToArea(bestArea);
                return true;
            }

            TargetAreaName = "";

            return false;
        }

        /// <summary>
        /// Handles travel to a forced vector zone. Returns true if the bot is moving toward the zone.
        /// </summary>
        private bool HandleVectorZoneTravel()
        {
            if (_forcedVectorZone == null || _hero == null) return false;

            Vector3 heroPosition = _hero.transform.position;
            Vector3 targetPosition = _forcedVectorZone.Position;
            float distanceToCenter = Vector3.Distance(heroPosition, targetPosition);

            // If hero is outside the radius, move toward the center
            if (distanceToCenter > _forcedVectorZone.Radius)
            {
                TargetAreaName = $"Zone: {_forcedVectorZone.Name}";
                _hero.Character.MoveToSafe(targetPosition, 100);
                return true;
            }

            TargetAreaName = "";
            return false;
        }

        /// <summary>
        /// Gets the distance from the hero to the forced vector zone center.
        /// Returns -1 if no vector zone is forced or hero is null.
        /// </summary>
        public float GetDistanceToForcedVectorZone()
        {
            if (_forcedVectorZone == null || _hero == null) return -1f;
            return Vector3.Distance(_hero.transform.position, _forcedVectorZone.Position);
        }

        /// <summary>
        /// Returns true if the hero is within the forced vector zone radius.
        /// </summary>
        public bool IsHeroInForcedVectorZone()
        {
            if (_forcedVectorZone == null || _hero == null) return false;
            return GetDistanceToForcedVectorZone() <= _forcedVectorZone.Radius;
        }

        public string GetAreaToTravel()
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

        /// <summary>
        /// Clears the forced vector zone and returns to automatic mode.
        /// </summary>
        public void ClearForcedVectorZone()
        {
            ForcedVectorZone = null;
        }

        /// <summary>
        /// Clears all forced modes (area and vector zone) and returns to automatic mode.
        /// </summary>
        public void ClearAllForcedModes()
        {
            _forcedAreaName = "";
            _forcedVectorZone = null;
            _hero?.Say("Disabled all forced modes, returning to auto mode");
        }

        /// <summary>
        /// Dictionary of minimum hero level to area name for automatic area selection.
        /// </summary>
        public Dictionary<int, string> MinLevelAreaDictionary = new Dictionary<int, string>()
        {
            {1, "Stony Plains" },
            {3, "Crimson Meadows" },
            {5, "Rotten Burrows" },
            {7, "Ashen Pastures" },
            {9, "Canyon of Death" },
            //{11, "Endless Desert" },

        };

        /// <summary>
        /// List of custom areas that can be forced but are not part of the level-based area selection.
        /// These areas are for special purposes like farming specific resources, bosses, etc.
        /// </summary>
        public List<string> CustomAreaList = new List<string>()
        {

        };

        /// <summary>
        /// List of custom vector zones that can be forced.
        /// These zones are defined by a position (Vector3) and a radius.
        /// </summary>
        public List<VectorZone> CustomVectorZoneList = new List<VectorZone>()
        {

        };

        /// <summary>
        /// Adds a custom area to the list.
        /// </summary>
        public void AddCustomArea(string areaName)
        {
            if (!string.IsNullOrEmpty(areaName) && !CustomAreaList.Contains(areaName))
            {
                CustomAreaList.Add(areaName);
            }
        }

        /// <summary>
        /// Removes a custom area from the list.
        /// </summary>
        public void RemoveCustomArea(string areaName)
        {
            CustomAreaList.Remove(areaName);
        }

        /// <summary>
        /// Adds a custom vector zone to the list.
        /// </summary>
        public void AddCustomVectorZone(string name, float x, float y, float z, float radius)
        {
            if (!string.IsNullOrEmpty(name) && !CustomVectorZoneList.Any(z => z.Name == name))
            {
                CustomVectorZoneList.Add(new VectorZone(name, x, y, z, radius));
            }
        }

        /// <summary>
        /// Adds a custom vector zone to the list.
        /// </summary>
        public void AddCustomVectorZone(VectorZone zone)
        {
            if (zone != null && !string.IsNullOrEmpty(zone.Name) && !CustomVectorZoneList.Any(z => z.Name == zone.Name))
            {
                CustomVectorZoneList.Add(zone);
            }
        }

        /// <summary>
        /// Removes a custom vector zone from the list by name.
        /// </summary>
        public void RemoveCustomVectorZone(string zoneName)
        {
            var zone = CustomVectorZoneList.FirstOrDefault(z => z.Name == zoneName);
            if (zone != null)
            {
                CustomVectorZoneList.Remove(zone);
            }
        }

        /// <summary>
        /// Removes a custom vector zone from the list.
        /// </summary>
        public void RemoveCustomVectorZone(VectorZone zone)
        {
            if (zone != null)
            {
                CustomVectorZoneList.Remove(zone);
            }
        }

        /// <summary>
        /// Forces a vector zone by name from the CustomVectorZoneList.
        /// </summary>
        public bool ForceVectorZoneByName(string zoneName)
        {
            var zone = CustomVectorZoneList.FirstOrDefault(z => z.Name == zoneName);
            if (zone != null)
            {
                ForcedVectorZone = zone;
                return true;
            }
            return false;
        }
    }
}
