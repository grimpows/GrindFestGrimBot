using System;
using UnityEngine;
using UnityEngine.AI;

namespace Scripts.Models.PathFinding
{
    /// <summary>
    /// NavMesh-based pathfinder implementation with waypoint system and stuck detection.
    /// </summary>
    public class NavMeshPathFinder : IPathFinder
    {
        // Configuration
        private readonly PathFinderConfig _config;

        // State
        private Vector3 _currentWaypoint = Vector3.zero;
        private DateTime _lastPathCalculation = DateTime.MinValue;
        private int _stuckCounter = 0;
        private Vector3 _lastPosition = Vector3.zero;
        private DateTime _lastPositionCheck = DateTime.MinValue;

        public Vector3 CurrentWaypoint => _currentWaypoint;
        public int StuckCounter => _stuckCounter;

        public NavMeshPathFinder() : this(new PathFinderConfig())
        {
        }

        public NavMeshPathFinder(PathFinderConfig config)
        {
            _config = config ?? new PathFinderConfig();
        }

        public void Reset()
        {
            _currentWaypoint = Vector3.zero;
            _lastPathCalculation = DateTime.MinValue;
            _stuckCounter = 0;
            _lastPosition = Vector3.zero;
            _lastPositionCheck = DateTime.MinValue;
        }

        public void Update(Vector3 currentPosition)
        {
            CheckStuckState(currentPosition);
        }

        public Vector3 CalculateNextMoveTarget(Vector3 currentPosition, Vector3 targetPosition)
        {
            float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);

            // Check if we need to recalculate the path
            bool needsRecalculation = (DateTime.Now - _lastPathCalculation).TotalSeconds > _config.PathRecalculationInterval
                                      || _currentWaypoint == Vector3.zero
                                      || _stuckCounter > _config.StuckThresholdForRecalculation;

            if (needsRecalculation)
            {
                _lastPathCalculation = DateTime.Now;

                // For short distances, move directly to target
                if (distanceToTarget <= _config.MaxDirectMoveDistance)
                {
                    _currentWaypoint = FindValidNavMeshPosition(targetPosition);
                }
                else
                {
                    // For longer distances, calculate intermediate waypoint
                    _currentWaypoint = CalculateIntermediateWaypoint(currentPosition, targetPosition, distanceToTarget);
                }
            }

            // Check if current waypoint is reached
            float distanceToWaypoint = Vector3.Distance(currentPosition, _currentWaypoint);
            if (distanceToWaypoint < _config.WaypointReachedDistance)
            {
                // Move to next waypoint or final target
                if (Vector3.Distance(_currentWaypoint, targetPosition) > _config.WaypointReachedDistance)
                {
                    _currentWaypoint = CalculateIntermediateWaypoint(currentPosition, targetPosition, distanceToTarget);
                }
                else
                {
                    _currentWaypoint = FindValidNavMeshPosition(targetPosition);
                }
            }

            return _currentWaypoint;
        }

        public bool IsStuck(Vector3 currentPosition)
        {
            return _stuckCounter > _config.StuckThreshold;
        }

        public Vector3 HandleStuckSituation(Vector3 currentPosition, Vector3 originalTarget)
        {
            _stuckCounter = 0;

            // Try moving in a random direction
            float randomAngle = UnityEngine.Random.Range(-90f, 90f);
            Vector3 direction = (originalTarget - currentPosition).normalized;
            
            // If direction is zero (at target), pick a random direction
            if (direction == Vector3.zero)
            {
                direction = UnityEngine.Random.insideUnitSphere;
                direction.y = 0;
                direction.Normalize();
            }

            Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * direction;
            Vector3 unstuckTarget = currentPosition + randomDirection * _config.UnstuckMoveDistance;

            // Sample NavMesh for valid position
            Vector3 validPosition = FindValidNavMeshPosition(unstuckTarget, _config.NavMeshSampleRadius * 2);
            
            if (validPosition != Vector3.zero)
            {
                _currentWaypoint = validPosition;
                return validPosition;
            }

            // Return a position behind the entity as last resort
            Vector3 backwardTarget = currentPosition - direction * _config.UnstuckMoveDistance;
            return FindValidNavMeshPosition(backwardTarget, _config.NavMeshSampleRadius * 2);
        }

        /// <summary>
        /// Calculates an intermediate waypoint for long-distance travel.
        /// </summary>
        private Vector3 CalculateIntermediateWaypoint(Vector3 heroPosition, Vector3 targetPosition, float totalDistance)
        {
            Vector3 direction = (targetPosition - heroPosition).normalized;
            float stepDistance = Mathf.Min(_config.MaxDirectMoveDistance, totalDistance * 0.5f);
            Vector3 intermediatePosition = heroPosition + direction * stepDistance;

            // Try to find a valid NavMesh position
            Vector3 validPosition = FindValidNavMeshPosition(intermediatePosition);
            if (validPosition != Vector3.zero)
            {
                return validPosition;
            }

            // Try alternative angles
            float[] angles = { -30f, 30f, -60f, 60f, -45f, 45f };
            foreach (float angle in angles)
            {
                Vector3 offset = Quaternion.Euler(0, angle, 0) * direction * stepDistance;
                Vector3 altPosition = heroPosition + offset;
                validPosition = FindValidNavMeshPosition(altPosition);
                if (validPosition != Vector3.zero)
                {
                    return validPosition;
                }
            }

            // Fallback: return intermediate position
            return intermediatePosition;
        }

        /// <summary>
        /// Finds a valid NavMesh position near the given position.
        /// </summary>
        private Vector3 FindValidNavMeshPosition(Vector3 position, float sampleRadius = -1f)
        {
            if (sampleRadius < 0)
                sampleRadius = _config.NavMeshSampleRadius;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, sampleRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return position; // Return original if no valid NavMesh position found
        }

        /// <summary>
        /// Checks and updates the stuck state.
        /// </summary>
        private void CheckStuckState(Vector3 currentPosition)
        {
            if ((DateTime.Now - _lastPositionCheck).TotalSeconds < _config.StuckCheckInterval)
                return;

            _lastPositionCheck = DateTime.Now;

            if (_lastPosition != Vector3.zero)
            {
                float movedDistance = Vector3.Distance(currentPosition, _lastPosition);
                if (movedDistance < _config.MinMovementThreshold)
                {
                    _stuckCounter++;
                }
                else
                {
                    _stuckCounter = Mathf.Max(0, _stuckCounter - 1);
                }
            }

            _lastPosition = currentPosition;
        }
    }
}
