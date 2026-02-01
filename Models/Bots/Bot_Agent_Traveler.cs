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
        private TravelMode _travelMode = TravelMode.VectorBased;

        // RunAround state for vector mode
        private Vector3 _currentRunAroundTarget = Vector3.zero;
        private DateTime _lastRunAroundTargetTime = DateTime.MinValue;
        private float _runAroundTargetInterval = 5f; // Change target every 5 seconds
        private float _runAroundReachedDistance = 5f; // Distance to consider target reached

        // Path waypoint tracking
        private LevelVectorZone _currentPathWaypoint = null;
        private const float WAYPOINT_REACHED_DISTANCE = 5f; // Distance to consider close to a waypoint

        // Visited waypoints tracking for path navigation
        private HashSet<int> _visitedWaypointIndices = new HashSet<int>();
        private int _currentWaypointIndex = -1;
        private bool _pathInitialized = false;

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

        /// <summary>
        /// Gets the current hero position (for UI/debugging).
        /// </summary>
        public Vector3 HeroPosition => _hero?.transform?.position ?? Vector3.zero;

        /// <summary>
        /// Returns true if the hero reference is valid.
        /// </summary>
        public bool HasValidHero => _hero != null;

        /// <summary>
        /// Gets the set of visited waypoint indices (for UI/debugging).
        /// </summary>
        public IReadOnlyCollection<int> VisitedWaypointIndices => _visitedWaypointIndices;

        /// <summary>
        /// Gets the current waypoint index being navigated to (for UI/debugging).
        /// </summary>
        public int CurrentWaypointIndex => _currentWaypointIndex;

        /// <summary>
        /// Gets whether the path has been initialized.
        /// </summary>
        public bool IsPathInitialized => _pathInitialized;

        /// <summary>
        /// Gets the total number of waypoints in the path.
        /// </summary>
        public int TotalWaypointsCount => MinLevelVectorDictionary?.Count ?? 0;

        /// <summary>
        /// Gets the number of visited waypoints.
        /// </summary>
        public int VisitedWaypointsCount => _visitedWaypointIndices.Count;

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
                
                // Mark this zone as visited
                var orderedZones = MinLevelVectorDictionary.OrderBy(z => z.MinLevel).ToList();
                int bestIndex = orderedZones.IndexOf(bestZone);
                if (bestIndex >= 0)
                    MarkWaypointVisited(bestIndex);
                
                return false;
            }

            // Find the next waypoint to navigate to along the path
            var nextWaypoint = FindNextPathWaypointWithTracking(heroPosition, bestZone);
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
        /// Finds the next waypoint using the tracking system.
        /// </summary>
        private LevelVectorZone FindNextPathWaypointWithTracking(Vector3 heroPosition, LevelVectorZone targetZone)
        {
            if (MinLevelVectorDictionary == null || MinLevelVectorDictionary.Count < 2)
                return targetZone;

            var orderedZones = MinLevelVectorDictionary.OrderBy(z => z.MinLevel).ToList();
            int targetIndex = orderedZones.IndexOf(targetZone);

            if (targetIndex < 0)
                return targetZone;

            // Initialize path if not done yet
            if (!_pathInitialized)
            {
                InitializePath(heroPosition, orderedZones, targetIndex);
            }

            // Check if we've reached the current waypoint
            if (_currentWaypointIndex >= 0 && _currentWaypointIndex < orderedZones.Count)
            {
                var currentZone = orderedZones[_currentWaypointIndex];
                float distToCurrent = Vector3.Distance(heroPosition, currentZone.Position);
                
                if (distToCurrent <= currentZone.Radius)
                {
                    // Mark as visited and move to next
                    MarkWaypointVisited(_currentWaypointIndex);
                    _currentWaypointIndex = FindNextWaypointIndex(orderedZones, targetIndex);
                }
            }

            // If current waypoint index is invalid or we've reached target, recalculate
            if (_currentWaypointIndex < 0 || _currentWaypointIndex >= orderedZones.Count)
            {
                _currentWaypointIndex = FindNextWaypointIndex(orderedZones, targetIndex);
            }

            // If still invalid or equals target, return target
            if (_currentWaypointIndex < 0 || _currentWaypointIndex == targetIndex)
            {
                return targetZone;
            }

            return orderedZones[_currentWaypointIndex];
        }

        /// <summary>
        /// Initializes the path by finding the closest waypoint to start from.
        /// </summary>
        private void InitializePath(Vector3 heroPosition, List<LevelVectorZone> orderedZones, int targetIndex)
        {
            _pathInitialized = true;
            _visitedWaypointIndices.Clear();

            // Find the closest waypoint to the hero
            float closestDistance = float.MaxValue;
            int closestIndex = -1;

            for (int i = 0; i < orderedZones.Count; i++)
            {
                float dist = Vector3.Distance(heroPosition, orderedZones[i].Position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestIndex = i;
                }
            }

            // Check if hero is already at a waypoint
            for (int i = 0; i < orderedZones.Count; i++)
            {
                float dist = Vector3.Distance(heroPosition, orderedZones[i].Position);
                if (dist <= orderedZones[i].Radius)
                {
                    MarkWaypointVisited(i);
                    closestIndex = i;
                }
            }

            // Set current waypoint based on direction to target
            if (closestIndex >= 0)
            {
                if (closestIndex < targetIndex)
                {
                    // Need to go forward, start from closest
                    _currentWaypointIndex = closestIndex;
                }
                else if (closestIndex > targetIndex)
                {
                    // Need to go backward, start from closest
                    _currentWaypointIndex = closestIndex;
                }
                else
                {
                    // We're at target
                    _currentWaypointIndex = targetIndex;
                }
            }
            else
            {
                // No waypoint found, start from first
                _currentWaypointIndex = 0;
            }
        }

        /// <summary>
        /// Finds the next waypoint index to navigate to.
        /// </summary>
        private int FindNextWaypointIndex(List<LevelVectorZone> orderedZones, int targetIndex)
        {
            if (_currentWaypointIndex < 0)
                return targetIndex;

            // Determine direction
            if (_currentWaypointIndex < targetIndex)
            {
                // Moving forward
                for (int i = _currentWaypointIndex + 1; i <= targetIndex; i++)
                {
                    if (!_visitedWaypointIndices.Contains(i) || i == targetIndex)
                        return i;
                }
            }
            else if (_currentWaypointIndex > targetIndex)
            {
                // Moving backward
                for (int i = _currentWaypointIndex - 1; i >= targetIndex; i--)
                {
                    if (!_visitedWaypointIndices.Contains(i) || i == targetIndex)
                        return i;
                }
            }

            return targetIndex;
        }

        /// <summary>
        /// Marks a waypoint as visited.
        /// </summary>
        private void MarkWaypointVisited(int index)
        {
            if (index >= 0)
                _visitedWaypointIndices.Add(index);
        }

        /// <summary>
        /// Clears the visited waypoints and resets path tracking.
        /// </summary>
        public void ClearVisitedWaypoints()
        {
            _visitedWaypointIndices.Clear();
            _currentWaypointIndex = -1;
            _pathInitialized = false;
            _currentPathWaypoint = null;
            _hero?.Say("Cleared waypoint history");
        }

        /// <summary>
        /// Checks if a waypoint index has been visited.
        /// </summary>
        public bool IsWaypointVisited(int index)
        {
            return _visitedWaypointIndices.Contains(index);
        }

        /// <summary>
        /// Gets the waypoint at the specified index.
        /// </summary>
        public LevelVectorZone GetWaypointAtIndex(int index)
        {
            var orderedZones = MinLevelVectorDictionary?.OrderBy(z => z.MinLevel).ToList();
            if (orderedZones == null || index < 0 || index >= orderedZones.Count)
                return null;
            return orderedZones[index];
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
        /// Gets the distance to the current path waypoint (if navigating through path).
        /// Returns -1 if no waypoint or hero is null.
        /// </summary>
        public float GetDistanceToCurrentPathWaypoint()
        {
            if (_hero == null || _currentPathWaypoint == null) return -1f;
            return Vector3.Distance(_hero.transform.position, _currentPathWaypoint.Position);
        }

        /// <summary>
        /// Returns true if the hero is within the current path waypoint radius.
        /// </summary>
        public bool IsHeroAtCurrentPathWaypoint()
        {
            if (_currentPathWaypoint == null || _hero == null) return false;
            return GetDistanceToCurrentPathWaypoint() <= _currentPathWaypoint.Radius;
        }

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
            new LevelVectorZone(1, "Lv1", -192, 11, 675, 100),
            new LevelVectorZone(3, "Lv3", -196, 11, 862, 100),
            new LevelVectorZone(3, "Wp5", -59.5f, 11, 981, 30),
            new LevelVectorZone(4, "Lv5", 63.6f, 11, 817, 100),
            new LevelVectorZone(4, "Wp7", 158.8f, 11f, 650, 30),
            new LevelVectorZone(7, "Lv7", 299.3f, 11, 646, 100),
            new LevelVectorZone(8, "Lv8", 502.7f, 11, 755.2f, 100),
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
