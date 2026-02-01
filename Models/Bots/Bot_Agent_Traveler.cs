using GrindFest;
using Scripts.Models.PathFinding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

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

    /// <summary>
    /// Enum to define the travel mode used for automatic area selection.
    /// </summary>
    public enum TravelMode
    {
        /// <summary>
        /// Use named areas (MinLevelAreaDictionary) - default game behavior.
        /// </summary>
        AreaBased,

        /// <summary>
        /// Use Vector3 positions (MinLevelVectorDictionary) - custom positions.
        /// </summary>
        VectorBased
    }

    public class Bot_Agent_Traveler
    {
        private AutomaticHero _hero;
        private IPathFinder _pathFinder;

        private string _targetAreaName = "";
        private string _forcedAreaName = "";
        private VectorZone _forcedVectorZone = null;

        // Travel mode for automatic selection
        private TravelMode _travelMode = TravelMode.AreaBased;

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
        /// Gets or sets the travel mode for automatic area selection.
        /// </summary>
        public TravelMode TravelMode
        {
            get => _travelMode;
            set
            {
                if (_travelMode != value)
                {
                    _travelMode = value;
                    _hero?.Say($"Travel mode changed to {_travelMode}");
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
                        // Reset pathfinding state
                        _pathFinder?.Reset();
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

        /// <summary>
        /// Gets the current waypoint from the pathfinder.
        /// </summary>
        public Vector3 CurrentWaypoint => _pathFinder?.CurrentWaypoint ?? Vector3.zero;

        /// <summary>
        /// Gets the stuck counter from the pathfinder.
        /// </summary>
        public int StuckCounter => _pathFinder?.StuckCounter ?? 0;

        public Bot_Agent_Traveler(AutomaticHero hero)
        {
            _hero = hero;
            _pathFinder = new NavMeshPathFinder(PathFinderConfig.Default);
        }

        public bool IsActing()
        {
            if (_hero == null) return false;

            // Priority 1: Forced vector zone (custom zones)
            if (IsForcedVectorZoneEnabled)
            {
                return HandleVectorZoneTravel(_forcedVectorZone);
            }

            // Priority 2: Forced area name (custom areas)
            if (IsForcedAreaEnabled)
            {
                if (_hero.CurrentArea?.Root?.name != _forcedAreaName)
                {
                    TargetAreaName = _forcedAreaName;
                    _hero.GoToArea(_forcedAreaName);
                    return true;
                }
                TargetAreaName = "";
                return false;
            }

            // Priority 3: Automatic travel based on TravelMode
            if (_travelMode == TravelMode.VectorBased)
            {
                return HandleAutomaticVectorTravel();
            }
            else
            {
                return HandleAutomaticAreaTravel();
            }
        }

        /// <summary>
        /// Handles automatic travel using area names (default behavior).
        /// </summary>
        private bool HandleAutomaticAreaTravel()
        {
            string bestArea = GetBestAreaForLevel();
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
        /// Handles automatic travel using vector zones.
        /// </summary>
        private bool HandleAutomaticVectorTravel()
        {
            var bestZone = GetBestVectorZoneForLevel();
            if (bestZone == null)
            {
                // Fallback to area-based if no vector zones configured
                return HandleAutomaticAreaTravel();
            }

            // Create a temporary VectorZone for travel
            var targetZone = new VectorZone(bestZone.Name, bestZone.Position, bestZone.Radius);
            return HandleVectorZoneTravel(targetZone);
        }

        /// <summary>
        /// Handles travel to a vector zone with optimized pathfinding.
        /// Returns true if the bot is moving toward the zone.
        /// </summary>
        private bool HandleVectorZoneTravel(VectorZone zone)
        {
            if (zone == null || _hero == null) return false;

            Vector3 heroPosition = _hero.transform.position;
            Vector3 targetPosition = zone.Position;
            float distanceToCenter = Vector3.Distance(heroPosition, targetPosition);

            // If hero is within the radius, we're done
            if (distanceToCenter <= zone.Radius)
            {
                TargetAreaName = "";
                return false;
            }

            TargetAreaName = $"Zone: {zone.Name}";

            // Update pathfinder state
            _pathFinder.Update(heroPosition);

            // Check if stuck and handle
            if (_pathFinder.IsStuck(heroPosition))
            {
                Vector3 unstuckTarget = _pathFinder.HandleStuckSituation(heroPosition, targetPosition);
                ExecuteMovement(unstuckTarget, distanceToCenter);
                return true;
            }

            // Calculate optimal move target
            Vector3 moveTarget = _pathFinder.CalculateNextMoveTarget(heroPosition, targetPosition);

            // Execute movement
            ExecuteMovement(moveTarget, distanceToCenter);

            return true;
        }

        /// <summary>
        /// Executes the movement command to the target position.
        /// </summary>
        private void ExecuteMovement(Vector3 moveTarget, float totalDistance)
        {
            // Use different movement ranges based on distance
            int moveRange = totalDistance > 100 ? 200 : (totalDistance > 50 ? 100 : 50);

            // Primary method: Use Character.MoveToSafe
            _hero.Character.MoveToSafe(moveTarget, moveRange);

            // Also try to use NavMeshAgent if available for better pathfinding
            var navAgent = _hero.Character?.GetComponent<NavMeshAgent>();
            if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
            {
                navAgent.SetDestination(moveTarget);
            }
        }

        /// <summary>
        /// Gets the best area name for the current hero level.
        /// </summary>
        public string GetBestAreaForLevel()
        {
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
        /// Gets the best vector zone for the current hero level.
        /// </summary>
        public LevelVectorZone GetBestVectorZoneForLevel()
        {
            if (MinLevelVectorDictionary == null || MinLevelVectorDictionary.Count == 0)
                return null;

            int heroLevel = _hero.Level;
            var zoneForLevel = MinLevelVectorDictionary
                .Where(z => z.MinLevel <= heroLevel)
                .OrderByDescending(z => z.MinLevel)
                .FirstOrDefault();

            if (zoneForLevel == null)
                return MinLevelVectorDictionary.OrderBy(z => z.MinLevel).FirstOrDefault();

            var lowerZone = MinLevelVectorDictionary
                .Where(z => z.MinLevel < zoneForLevel.MinLevel)
                .OrderByDescending(z => z.MinLevel)
                .FirstOrDefault() ?? zoneForLevel;

            // Check if we need to go to lower level zone for potions
            if (_hero.HealthPotionCount() < 5)
            {
                Vector3 heroPos = _hero.transform.position;
                float distToCurrentZone = Vector3.Distance(heroPos, zoneForLevel.Position);
                if (distToCurrentZone <= zoneForLevel.Radius)
                {
                    return lowerZone;
                }
            }

            return zoneForLevel;
        }

        /// <summary>
        /// Gets the area to travel (for compatibility with existing code).
        /// </summary>
        public string GetAreaToTravel()
        {
            if (IsForcedAreaEnabled)
                return _forcedAreaName;

            return GetBestAreaForLevel();
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

        /// <summary>
        /// Gets the distance to the current automatic vector zone (if in vector mode).
        /// </summary>
        public float GetDistanceToCurrentVectorZone()
        {
            if (_hero == null) return -1f;

            if (IsForcedVectorZoneEnabled)
                return GetDistanceToForcedVectorZone();

            if (_travelMode == TravelMode.VectorBased)
            {
                var zone = GetBestVectorZoneForLevel();
                if (zone != null)
                    return Vector3.Distance(_hero.transform.position, zone.Position);
            }

            return -1f;
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
            _pathFinder?.Reset();
            _hero?.Say("Disabled all forced modes, returning to auto mode");
        }

        #region Area Dictionaries

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
        };

        /// <summary>
        /// List of level-based vector zones for automatic vector-based travel.
        /// </summary>
        public List<LevelVectorZone> MinLevelVectorDictionary = new List<LevelVectorZone>()
        {
            // Example entries (users can add their own):
            // new LevelVectorZone(1, "Starting Area", 0, 0, 0, 50),
            // new LevelVectorZone(3, "Mid Level Zone", 100, 0, 100, 75),
            new LevelVectorZone(1, "Stony Plains", -185, -11, 590, 50),
            new LevelVectorZone(5, "10", 150,11,970, 50),
            new LevelVectorZone(7, "20", 147,11,616, 50),
            new LevelVectorZone(9, "30", 504,11,1030, 50),
            new LevelVectorZone(11, "40", 269,9,17780, 50),
            new LevelVectorZone(13, "50", 28,9,2502, 50),
            new LevelVectorZone(15, "54", 28,9,2502, 50),
        };

        #endregion

        #region Custom Areas/Zones Lists

        /// <summary>
        /// List of custom areas that can be forced but are not part of the level-based area selection.
        /// </summary>
        public List<string> CustomAreaList = new List<string>()
        {
        };

        /// <summary>
        /// List of custom vector zones that can be forced.
        /// </summary>
        public List<VectorZone> CustomVectorZoneList = new List<VectorZone>()
        {
        };

        #endregion

        #region Custom Area Management

        public void AddCustomArea(string areaName)
        {
            if (!string.IsNullOrEmpty(areaName) && !CustomAreaList.Contains(areaName))
            {
                CustomAreaList.Add(areaName);
            }
        }

        public void RemoveCustomArea(string areaName)
        {
            CustomAreaList.Remove(areaName);
        }

        #endregion

        #region Custom Vector Zone Management

        public void AddCustomVectorZone(string name, float x, float y, float z, float radius)
        {
            if (!string.IsNullOrEmpty(name) && !CustomVectorZoneList.Any(z => z.Name == name))
            {
                CustomVectorZoneList.Add(new VectorZone(name, x, y, z, radius));
            }
        }

        public void AddCustomVectorZone(VectorZone zone)
        {
            if (zone != null && !string.IsNullOrEmpty(zone.Name) && !CustomVectorZoneList.Any(z => z.Name == zone.Name))
            {
                CustomVectorZoneList.Add(zone);
            }
        }

        public void RemoveCustomVectorZone(string zoneName)
        {
            var zone = CustomVectorZoneList.FirstOrDefault(z => z.Name == zoneName);
            if (zone != null)
            {
                CustomVectorZoneList.Remove(zone);
            }
        }

        public void RemoveCustomVectorZone(VectorZone zone)
        {
            if (zone != null)
            {
                CustomVectorZoneList.Remove(zone);
            }
        }

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

        #endregion

        #region Level Vector Zone Management

        public void AddLevelVectorZone(int minLevel, string name, float x, float y, float z, float radius)
        {
            if (!string.IsNullOrEmpty(name) && !MinLevelVectorDictionary.Any(z => z.MinLevel == minLevel))
            {
                MinLevelVectorDictionary.Add(new LevelVectorZone(minLevel, name, x, y, z, radius));
                MinLevelVectorDictionary = MinLevelVectorDictionary.OrderBy(z => z.MinLevel).ToList();
            }
        }

        public void AddLevelVectorZone(LevelVectorZone zone)
        {
            if (zone != null && !MinLevelVectorDictionary.Any(z => z.MinLevel == zone.MinLevel))
            {
                MinLevelVectorDictionary.Add(zone);
                MinLevelVectorDictionary = MinLevelVectorDictionary.OrderBy(z => z.MinLevel).ToList();
            }
        }

        public void RemoveLevelVectorZone(int minLevel)
        {
            var zone = MinLevelVectorDictionary.FirstOrDefault(z => z.MinLevel == minLevel);
            if (zone != null)
            {
                MinLevelVectorDictionary.Remove(zone);
            }
        }

        public void UpdateLevelVectorZone(int minLevel, string name, float x, float y, float z, float radius)
        {
            var zone = MinLevelVectorDictionary.FirstOrDefault(z => z.MinLevel == minLevel);
            if (zone != null)
            {
                zone.Name = name;
                zone.Position = new Vector3(x, y, z);
                zone.Radius = radius;
            }
            else
            {
                AddLevelVectorZone(minLevel, name, x, y, z, radius);
            }
        }

        #endregion
    }
}
