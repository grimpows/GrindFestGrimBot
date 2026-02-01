using UnityEngine;

namespace Scripts.Models.PathFinding
{
    /// <summary>
    /// Represents a level-based vector zone for automatic area selection.
    /// </summary>
    public class LevelVectorZone
    {
        /// <summary>
        /// The minimum hero level required to use this zone.
        /// </summary>
        public int MinLevel { get; set; }

        /// <summary>
        /// The name of the zone.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The center position of the zone.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// The radius of the zone. Hero will stay within this radius.
        /// </summary>
        public float Radius { get; set; }

        public LevelVectorZone(int minLevel, string name, Vector3 position, float radius)
        {
            MinLevel = minLevel;
            Name = name;
            Position = position;
            Radius = radius;
        }

        public LevelVectorZone(int minLevel, string name, float x, float y, float z, float radius)
        {
            MinLevel = minLevel;
            Name = name;
            Position = new Vector3(x, y, z);
            Radius = radius;
        }

        public override string ToString()
        {
            return $"L{MinLevel}: {Name} ({Position.x:F0}, {Position.y:F0}, {Position.z:F0}) R:{Radius:F0}";
        }
    }
}
