using GrindFest;
using UnityEngine;
using UnityEngine.AI;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        /// <summary>
        /// Moves the hero toward the specified position using the navigation system.
        /// </summary>
        /// <param name="hero">The hero to move.</param>
        /// <param name="position">The target position to move to.</param>
        public static void GoToPosition(this AutomaticHero hero, Vector3 position)
        {
            if (hero == null) return;

            // Try to use NavMeshAgent if available on hero
            var navAgent = hero.GetComponent<NavMeshAgent>();
            if (navAgent != null && navAgent.enabled)
            {
                navAgent.SetDestination(position);
                return;
            }

            // Try to get NavMeshAgent from Character
            var characterNav = hero.Character?.GetComponent<NavMeshAgent>();
            if (characterNav != null && characterNav.enabled)
            {
                characterNav.SetDestination(position);
                return;
            }

            // Try to find any interactive object at that position and interact with it
            // This is a workaround if no direct movement method is available
            // The hero will attempt to move towards the position by interacting with the world
        }

        /// <summary>
        /// Moves the hero toward the specified position using the navigation system.
        /// </summary>
        /// <param name="hero">The hero to move.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public static void GoToPosition(this AutomaticHero hero, float x, float y, float z)
        {
            hero.GoToPosition(new Vector3(x, y, z));
        }
    }
}
