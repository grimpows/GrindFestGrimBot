namespace Scripts.Models.PathFinding
{
    /// <summary>
    /// Configuration settings for the pathfinder.
    /// </summary>
    public class PathFinderConfig
    {
        /// <summary>
        /// How often to recalculate the path in seconds.
        /// </summary>
        public float PathRecalculationInterval { get; set; } = 2f;

        /// <summary>
        /// Distance at which a waypoint is considered reached.
        /// </summary>
        public float WaypointReachedDistance { get; set; } = 3f;

        /// <summary>
        /// Maximum distance for direct movement without intermediate waypoints.
        /// </summary>
        public float MaxDirectMoveDistance { get; set; } = 50f;

        /// <summary>
        /// How often to check if the entity is stuck in seconds.
        /// </summary>
        public float StuckCheckInterval { get; set; } = 1f;

        /// <summary>
        /// Minimum movement required to not be considered stuck.
        /// </summary>
        public float MinMovementThreshold { get; set; } = 0.5f;

        /// <summary>
        /// Number of stuck checks before considered stuck.
        /// </summary>
        public int StuckThreshold { get; set; } = 5;

        /// <summary>
        /// Number of stuck counts that triggers path recalculation.
        /// </summary>
        public int StuckThresholdForRecalculation { get; set; } = 3;

        /// <summary>
        /// Distance to try moving when stuck.
        /// </summary>
        public float UnstuckMoveDistance { get; set; } = 15f;

        /// <summary>
        /// Radius to sample NavMesh positions.
        /// </summary>
        public float NavMeshSampleRadius { get; set; } = 10f;

        /// <summary>
        /// Creates a default configuration.
        /// </summary>
        public static PathFinderConfig Default => new PathFinderConfig();

        /// <summary>
        /// Creates a configuration optimized for long-distance travel.
        /// </summary>
        public static PathFinderConfig LongDistance => new PathFinderConfig
        {
            PathRecalculationInterval = 3f,
            MaxDirectMoveDistance = 75f,
            WaypointReachedDistance = 5f,
            NavMeshSampleRadius = 15f
        };

        /// <summary>
        /// Creates a configuration optimized for precise movement.
        /// </summary>
        public static PathFinderConfig Precise => new PathFinderConfig
        {
            PathRecalculationInterval = 1f,
            MaxDirectMoveDistance = 25f,
            WaypointReachedDistance = 2f,
            NavMeshSampleRadius = 5f,
            MinMovementThreshold = 0.3f
        };
    }
}
