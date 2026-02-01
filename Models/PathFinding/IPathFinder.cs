using UnityEngine;

namespace Scripts.Models.PathFinding
{
    /// <summary>
    /// Interface for pathfinding implementations.
    /// </summary>
    public interface IPathFinder
    {
        /// <summary>
        /// Calculates the next move target from current position to destination.
        /// </summary>
        /// <param name="currentPosition">The current position of the entity.</param>
        /// <param name="targetPosition">The final destination.</param>
        /// <returns>The next position to move to.</returns>
        Vector3 CalculateNextMoveTarget(Vector3 currentPosition, Vector3 targetPosition);

        /// <summary>
        /// Checks if the entity is stuck and needs unsticking.
        /// </summary>
        /// <param name="currentPosition">The current position of the entity.</param>
        /// <returns>True if the entity appears to be stuck.</returns>
        bool IsStuck(Vector3 currentPosition);

        /// <summary>
        /// Handles a stuck situation and returns a new target to try.
        /// </summary>
        /// <param name="currentPosition">The current position of the entity.</param>
        /// <param name="originalTarget">The original target position.</param>
        /// <returns>A new position to try moving to.</returns>
        Vector3 HandleStuckSituation(Vector3 currentPosition, Vector3 originalTarget);

        /// <summary>
        /// Resets the pathfinding state.
        /// </summary>
        void Reset();

        /// <summary>
        /// Gets the current waypoint being navigated to.
        /// </summary>
        Vector3 CurrentWaypoint { get; }

        /// <summary>
        /// Gets the stuck counter for debugging.
        /// </summary>
        int StuckCounter { get; }

        /// <summary>
        /// Updates the pathfinder state. Should be called each frame/tick.
        /// </summary>
        /// <param name="currentPosition">The current position of the entity.</param>
        void Update(Vector3 currentPosition);
    }
}
