using GrindFest;
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

    public class Bot_Agent_Traveler
    {

        private AutomaticHero _hero;

        private string _targetAreaName = "";
        private string _forcedAreaName = "";
        private VectorZone _forcedVectorZone = null;

        // Pathfinding state
        private Vector3 _currentWaypoint = Vector3.zero;
        private DateTime _lastPathCalculation = DateTime.MinValue;
        private float _pathRecalculationInterval = 2f; // Recalculate path every 2 seconds
        private float _waypointReachedDistance = 3f; // Distance to consider waypoint reached
        private float _maxDirectMoveDistance = 50f; // Max distance for direct movement without waypoints
        private int _stuckCounter = 0;
        private Vector3 _lastPosition = Vector3.zero;
        private DateTime _lastPositionCheck = DateTime.MinValue;

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
                        // Reset pathfinding state
                        ResetPathfindingState();
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

        /// <summary>
        /// Resets the pathfinding state when changing zones or destinations.
        /// </summary>
        private void ResetPathfindingState()
        {
            _currentWaypoint = Vector3.zero;
            _lastPathCalculation = DateTime.MinValue;
            _stuckCounter = 0;
            _lastPosition = Vector3.zero;
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
        /// Handles travel to a forced vector zone with optimized pathfinding.
        /// Returns true if the bot is moving toward the zone.
        /// </summary>
        private bool HandleVectorZoneTravel()
        {
            if (_forcedVectorZone == null || _hero == null) return false;

            Vector3 heroPosition = _hero.transform.position;
            Vector3 targetPosition = _forcedVectorZone.Position;
            float distanceToCenter = Vector3.Distance(heroPosition, targetPosition);

            // If hero is within the radius, we're done
            if (distanceToCenter <= _forcedVectorZone.Radius)
            {
                TargetAreaName = "";
                ResetPathfindingState();
                return false;
            }

            TargetAreaName = $"Zone: {_forcedVectorZone.Name}";

            // Check if we're stuck
            CheckAndHandleStuck(heroPosition);

            // Calculate or update path
            Vector3 moveTarget = CalculateOptimalMoveTarget(heroPosition, targetPosition, distanceToCenter);

            // Execute movement using the game's MoveToSafe
            ExecuteMovement(moveTarget, distanceToCenter);

            return true;
        }

        /// <summary>
        /// Calculates the optimal move target based on distance and pathfinding.
        /// </summary>
        private Vector3 CalculateOptimalMoveTarget(Vector3 heroPosition, Vector3 targetPosition, float distanceToCenter)
        {
            // Check if we need to recalculate the path
            bool needsRecalculation = (DateTime.Now - _lastPathCalculation).TotalSeconds > _pathRecalculationInterval
                                      || _currentWaypoint == Vector3.zero
                                      || _stuckCounter > 3;

            if (needsRecalculation)
            {
                _lastPathCalculation = DateTime.Now;
                _stuckCounter = 0;

                // For short distances, move directly to target
                if (distanceToCenter <= _maxDirectMoveDistance)
                {
                    _currentWaypoint = targetPosition;
                }
                else
                {
                    // For longer distances, calculate intermediate waypoint
                    _currentWaypoint = CalculateIntermediateWaypoint(heroPosition, targetPosition, distanceToCenter);
                }
            }

            // Check if current waypoint is reached
            float distanceToWaypoint = Vector3.Distance(heroPosition, _currentWaypoint);
            if (distanceToWaypoint < _waypointReachedDistance)
            {
                // Move to next waypoint or final target
                if (Vector3.Distance(_currentWaypoint, targetPosition) > _waypointReachedDistance)
                {
                    _currentWaypoint = CalculateIntermediateWaypoint(heroPosition, targetPosition, distanceToCenter);
                }
                else
                {
                    _currentWaypoint = targetPosition;
                }
            }

            return _currentWaypoint;
        }

        /// <summary>
        /// Calculates an intermediate waypoint for long-distance travel.
        /// Uses NavMesh sampling to find valid positions.
        /// </summary>
        private Vector3 CalculateIntermediateWaypoint(Vector3 heroPosition, Vector3 targetPosition, float totalDistance)
        {
            // Calculate direction to target
            Vector3 direction = (targetPosition - heroPosition).normalized;

            // Calculate step distance (move in chunks)
            float stepDistance = Mathf.Min(_maxDirectMoveDistance, totalDistance * 0.5f);

            // Calculate intermediate position
            Vector3 intermediatePosition = heroPosition + direction * stepDistance;

            // Try to find a valid NavMesh position near the intermediate point
            NavMeshHit hit;
            if (NavMesh.SamplePosition(intermediatePosition, out hit, 10f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            // If no valid NavMesh position found, try alternative positions
            // Try slightly to the left
            Vector3 leftOffset = Quaternion.Euler(0, -30, 0) * direction * stepDistance;
            Vector3 leftPosition = heroPosition + leftOffset;
            if (NavMesh.SamplePosition(leftPosition, out hit, 10f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            // Try slightly to the right
            Vector3 rightOffset = Quaternion.Euler(0, 30, 0) * direction * stepDistance;
            Vector3 rightPosition = heroPosition + rightOffset;
            if (NavMesh.SamplePosition(rightPosition, out hit, 10f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            // Fallback: return intermediate position and let MoveToSafe handle it
            return intermediatePosition;
        }

        /// <summary>
        /// Checks if the hero is stuck and handles it.
        /// </summary>
        private void CheckAndHandleStuck(Vector3 currentPosition)
        {
            if ((DateTime.Now - _lastPositionCheck).TotalSeconds < 1f)
                return;

            _lastPositionCheck = DateTime.Now;

            if (_lastPosition != Vector3.zero)
            {
                float movedDistance = Vector3.Distance(currentPosition, _lastPosition);
                if (movedDistance < 0.5f) // Hasn't moved much
                {
                    _stuckCounter++;

                    if (_stuckCounter > 5)
                    {
                        // We're really stuck, try a random offset
                        HandleStuckSituation(currentPosition);
                    }
                }
                else
                {
                    _stuckCounter = 0;
                }
            }

            _lastPosition = currentPosition;
        }

        /// <summary>
        /// Handles stuck situations by trying alternative movement strategies.
        /// </summary>
        private void HandleStuckSituation(Vector3 currentPosition)
        {
            _stuckCounter = 0;

            // Try moving in a random direction first
            float randomAngle = UnityEngine.Random.Range(-90f, 90f);
            Vector3 direction = (_forcedVectorZone.Position - currentPosition).normalized;
            Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * direction;

            Vector3 unstuckTarget = currentPosition + randomDirection * 15f;

            // Sample NavMesh for valid position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(unstuckTarget, out hit, 20f, NavMesh.AllAreas))
            {
                _currentWaypoint = hit.position;
            }
            else
            {
                // Use RunAroundInArea as last resort
                _hero?.RunAroundInArea();
            }
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
        /// Gets the current waypoint being navigated to (for debugging/UI).
        /// </summary>
        public Vector3 CurrentWaypoint => _currentWaypoint;

        /// <summary>
        /// Gets the stuck counter (for debugging/UI).
        /// </summary>
        public int StuckCounter => _stuckCounter;

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
            ResetPathfindingState();
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
