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

        // RunAround state for vector mode
        private Vector3 _currentRunAroundTarget = Vector3.zero;
        private DateTime _lastRunAroundTargetTime = DateTime.MinValue;
        private float _runAroundTargetInterval = 3f; // Change target every 3 seconds
        private float _runAroundReachedDistance = 5f; // Distance to consider target reached

        // Path waypoint tracking
        private LevelVectorZone _currentPathWaypoint = null;
        private const float WAYPOINT_REACHED_DISTANCE = 5f; // Distance to consider close to a waypoint

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

        /// <summary>
        /// Gets the current path waypoint being navigated to (for UI/debugging).
        /// </summary>
        public LevelVectorZone CurrentPathWaypoint => _currentPathWaypoint;

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
        /// Handles automatic travel using vector zones with path-based navigation.
        /// The list of zones is treated as a path, navigating through intermediate waypoints.
        /// </summary>
        private bool HandleAutomaticVectorTravel()
        {
            var bestZone = GetBestVectorZoneForLevel();
            if (bestZone == null)
            {
                // Fallback to area-based if no vector zones configured
                return HandleAutomaticAreaTravel();
            }

            Vector3 heroPosition = _hero.transform.position;
            float distanceToBestZone = Vector3.Distance(heroPosition, bestZone.Position);

            // If hero is within the best zone's radius, we're done
            if (distanceToBestZone <= bestZone.Radius)
            {
                TargetAreaName = "";
                _currentPathWaypoint = null;
                return false;
            }

            // Find the next waypoint to navigate to along the path
            var nextWaypoint = FindNextPathWaypoint(heroPosition, bestZone);
            _currentPathWaypoint = nextWaypoint;

            if (nextWaypoint == null)
            {
                // No waypoint found, go directly to best zone
                var targetZone = new VectorZone(bestZone.Name, bestZone.Position, bestZone.Radius);
                return HandleVectorZoneTravel(targetZone);
            }

            // Navigate to the waypoint
            var waypointZone = new VectorZone(nextWaypoint.Name, nextWaypoint.Position, nextWaypoint.Radius);
            return HandleVectorZoneTravel(waypointZone);
        }

        /// <summary>
        /// Finds the next waypoint to navigate to based on hero position and target zone.
        /// Uses the zone list as a path, finding the appropriate intermediate waypoint.
        /// </summary>
        private LevelVectorZone FindNextPathWaypoint(Vector3 heroPosition, LevelVectorZone targetZone)
        {
            if (MinLevelVectorDictionary == null || MinLevelVectorDictionary.Count < 2)
                return targetZone;

            // Get the ordered list of zones
            var orderedZones = MinLevelVectorDictionary.OrderBy(z => z.MinLevel).ToList();
            int targetIndex = orderedZones.IndexOf(targetZone);

            if (targetIndex < 0)
                return targetZone;

            // Find the two closest zones to the hero
            var closestZones = FindTwoClosestZones(heroPosition, orderedZones);
            if (closestZones.Item1 == null || closestZones.Item2 == null)
                return targetZone;

            var zoneA = closestZones.Item1;
            var zoneB = closestZones.Item2;

            int indexA = orderedZones.IndexOf(zoneA);
            int indexB = orderedZones.IndexOf(zoneB);

            // Ensure indexA < indexB (A is lower in the list)
            if (indexA > indexB)
            {
                (zoneA, zoneB) = (zoneB, zoneA);
                (indexA, indexB) = (indexB, indexA);
            }

            float distToZoneA = Vector3.Distance(heroPosition, zoneA.Position);
            float distToZoneB = Vector3.Distance(heroPosition, zoneB.Position);

            // Check if hero is "between" the two zones (in the parallel corridor)
            bool isInCorridor = IsInZoneCorridor(heroPosition, zoneA, zoneB);

            LevelVectorZone nextWaypoint;

            if (isInCorridor)
            {
                // Hero is in the corridor between zones, determine direction based on target
                if (targetIndex >= indexB)
                {
                    // Target is ahead, go to the higher index zone (zoneB) to progress
                    nextWaypoint = zoneB;
                }
                else if (targetIndex <= indexA)
                {
                    // Target is behind, go to the lower index zone (zoneA) to go back
                    nextWaypoint = zoneA;
                }
                else
                {
                    // Target is between A and B, go to lower one first
                    nextWaypoint = zoneA;
                }
            }
            else
            {
                // Hero is not in corridor
                // If very close to the higher zone (< 5f), go to lower zone
                if (distToZoneB < WAYPOINT_REACHED_DISTANCE)
                {
                    // We're at zone B, check if we need to continue or go back
                    if (targetIndex > indexB)
                    {
                        // Need to go further, find the next zone after B
                        if (indexB + 1 < orderedZones.Count)
                            nextWaypoint = orderedZones[indexB + 1];
                        else
                            nextWaypoint = targetZone;
                    }
                    else if (targetIndex < indexB)
                    {
                        // Need to go back
                        nextWaypoint = zoneA;
                    }
                    else
                    {
                        nextWaypoint = targetZone;
                    }
                }
                else if (distToZoneA < WAYPOINT_REACHED_DISTANCE)
                {
                    // We're at zone A, check direction
                    if (targetIndex > indexA)
                    {
                        // Need to go forward
                        nextWaypoint = zoneB;
                    }
                    else if (targetIndex < indexA)
                    {
                        // Need to go back, find previous zone
                        if (indexA - 1 >= 0)
                            nextWaypoint = orderedZones[indexA - 1];
                        else
                            nextWaypoint = targetZone;
                    }
                    else
                    {
                        nextWaypoint = targetZone;
                    }
                }
                else
                {
                    // Not close to either, go to the closest one that progresses toward target
                    if (targetIndex >= indexB)
                    {
                        // Target is ahead or at B, prefer going to B
                        nextWaypoint = zoneB;
                    }
                    else if (targetIndex <= indexA)
                    {
                        // Target is behind or at A, prefer going to A
                        nextWaypoint = zoneA;
                    }
                    else
                    {
                        // Target is between, go to the closer one
                        nextWaypoint = distToZoneA < distToZoneB ? zoneA : zoneB;
                    }
                }
            }

            // If the waypoint is the target zone, return it
            if (nextWaypoint == targetZone)
                return targetZone;

            // Check if we're already at this waypoint
            float distToWaypoint = Vector3.Distance(heroPosition, nextWaypoint.Position);
            if (distToWaypoint <= nextWaypoint.Radius)
            {
                // We're at this waypoint, find the next one toward target
                int waypointIndex = orderedZones.IndexOf(nextWaypoint);
                if (targetIndex > waypointIndex && waypointIndex + 1 < orderedZones.Count)
                {
                    return orderedZones[waypointIndex + 1];
                }
                else if (targetIndex < waypointIndex && waypointIndex - 1 >= 0)
                {
                    return orderedZones[waypointIndex - 1];
                }
            }

            return nextWaypoint;
        }

        /// <summary>
        /// Finds the two closest zones to the hero position.
        /// </summary>
        private (LevelVectorZone, LevelVectorZone) FindTwoClosestZones(Vector3 heroPosition, List<LevelVectorZone> zones)
        {
            if (zones == null || zones.Count < 2)
                return (null, null);

            var sortedByDistance = zones
                .Select(z => new { Zone = z, Distance = Vector3.Distance(heroPosition, z.Position) })
                .OrderBy(x => x.Distance)
                .Take(2)
                .ToList();

            if (sortedByDistance.Count < 2)
                return (sortedByDistance.FirstOrDefault()?.Zone, null);

            return (sortedByDistance[0].Zone, sortedByDistance[1].Zone);
        }

        /// <summary>
        /// Checks if the hero is in the "corridor" between two zones.
        /// A corridor is defined as the area between the perpendicular planes at each zone position.
        /// </summary>
        private bool IsInZoneCorridor(Vector3 heroPosition, LevelVectorZone zoneA, LevelVectorZone zoneB)
        {
            Vector3 posA = zoneA.Position;
            Vector3 posB = zoneB.Position;

            // Direction from A to B
            Vector3 dirAB = (posB - posA).normalized;

            // Project hero position onto the line AB
            Vector3 heroRelativeToA = heroPosition - posA;
            float projectionLength = Vector3.Dot(heroRelativeToA, dirAB);
            float totalLength = Vector3.Distance(posA, posB);

            // Hero is in corridor if projection is between 0 and totalLength
            // with some margin for the zone radii
            float marginA = zoneA.Radius * 0.5f;
            float marginB = zoneB.Radius * 0.5f;

            return projectionLength > -marginA && projectionLength < (totalLength + marginB);
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
        /// Handles the "run around" behavior based on the current travel mode.
        /// In Area mode, uses the default hero.RunAroundInArea().
        /// In Vector mode, moves randomly within the current zone's radius.
        /// </summary>
        public void RunAround()
        {
            if (_hero == null) return;

            // Determine which zone to use for running around
            VectorZone activeZone = GetActiveVectorZone();

            // If no vector zone is active or we're in area mode, use default behavior
            if (activeZone == null || (_travelMode == TravelMode.AreaBased && !IsForcedVectorZoneEnabled))
            {
                _hero.RunAroundInArea();
                return;
            }

            // Vector mode: run around within the zone radius
            RunAroundInVectorZone(activeZone);
        }

        /// <summary>
        /// Gets the currently active vector zone (forced or automatic).
        /// </summary>
        private VectorZone GetActiveVectorZone()
        {
            // Priority 1: Forced vector zone
            if (IsForcedVectorZoneEnabled)
            {
                return _forcedVectorZone;
            }

            // Priority 2: Auto vector mode - get best zone for level
            if (_travelMode == TravelMode.VectorBased)
            {
                var bestZone = GetBestVectorZoneForLevel();
                if (bestZone != null)
                {
                    return new VectorZone(bestZone.Name, bestZone.Position, bestZone.Radius);
                }
            }

            return null;
        }

        /// <summary>
        /// Runs around within a vector zone by picking random points inside the radius.
        /// </summary>
        private void RunAroundInVectorZone(VectorZone zone)
        {
            if (zone == null || _hero == null) return;

            Vector3 heroPosition = _hero.transform.position;
            float distanceToTarget = Vector3.Distance(heroPosition, _currentRunAroundTarget);

            // Check if we need a new target
            bool needsNewTarget = _currentRunAroundTarget == Vector3.zero
                                  || distanceToTarget < _runAroundReachedDistance
                                  || (DateTime.Now - _lastRunAroundTargetTime).TotalSeconds > _runAroundTargetInterval;

            if (needsNewTarget)
            {
                _currentRunAroundTarget = CalculateRandomPointInZone(zone);
                _lastRunAroundTargetTime = DateTime.Now;
            }

            // Move towards the target
            float distanceToCenter = Vector3.Distance(heroPosition, zone.Position);
            
            // If we're outside the zone, move back towards center first
            if (distanceToCenter > zone.Radius)
            {
                _currentRunAroundTarget = CalculatePointTowardsCenter(heroPosition, zone);
            }

            // Execute movement to the run around target
            _hero.Character.MoveToSafe(_currentRunAroundTarget, 50);

            // Also use NavMeshAgent if available
            var navAgent = _hero.Character?.GetComponent<NavMeshAgent>();
            if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
            {
                navAgent.SetDestination(_currentRunAroundTarget);
            }
        }

        /// <summary>
        /// Calculates a random point within the zone radius.
        /// </summary>
        private Vector3 CalculateRandomPointInZone(VectorZone zone)
        {
            // Generate random angle and distance
            float randomAngle = UnityEngine.Random.Range(0f, 360f);
            float randomDistance = UnityEngine.Random.Range(0f, zone.Radius * 0.8f); // Stay within 80% of radius

            // Calculate offset from center
            Vector3 offset = new Vector3(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad) * randomDistance,
                0f,
                Mathf.Sin(randomAngle * Mathf.Deg2Rad) * randomDistance
            );

            Vector3 targetPosition = zone.Position + offset;

            // Try to find a valid NavMesh position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 10f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            // Fallback: return the calculated position
            return targetPosition;
        }

        /// <summary>
        /// Calculates a point towards the zone center when the hero is outside the radius.
        /// </summary>
        private Vector3 CalculatePointTowardsCenter(Vector3 heroPosition, VectorZone zone)
        {
            Vector3 directionToCenter = (zone.Position - heroPosition).normalized;
            float distanceToMove = Mathf.Min(30f, Vector3.Distance(heroPosition, zone.Position));
            Vector3 targetPosition = heroPosition + directionToCenter * distanceToMove;

            // Try to find a valid NavMesh position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 10f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return targetPosition;
        }

        /// <summary>
        /// Gets the current run around target position (for debugging/UI).
        /// </summary>
        public Vector3 CurrentRunAroundTarget => _currentRunAroundTarget;

        /// <summary>
        /// Resets the run around state.
        /// </summary>
        public void ResetRunAroundState()
        {
            _currentRunAroundTarget = Vector3.zero;
            _lastRunAroundTargetTime = DateTime.MinValue;
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
        /// These zones are treated as a PATH - the bot will navigate through intermediate
        /// waypoints to reach the target zone.
        /// </summary>
        public List<LevelVectorZone> MinLevelVectorDictionary = new List<LevelVectorZone>()
        {
            new LevelVectorZone(1, "Stony Plains", -185, -11, 590, 50),
            new LevelVectorZone(5, "10", 150, 11, 970, 50),
            new LevelVectorZone(7, "20", 147, 11, 616, 50),
            new LevelVectorZone(9, "30", 504, 11, 1030, 50),
            new LevelVectorZone(11, "40", 269, 9, 17780, 50),
            new LevelVectorZone(13, "50", 28, 9, 2502, 50),
            new LevelVectorZone(15, "54", 28, 9, 2502, 50),
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
