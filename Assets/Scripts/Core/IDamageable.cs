using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Implemented by any object that can receive damage (the player and all enemies). Allows the
    /// collision system to deal damage generically without knowing concrete types.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>The faction this object belongs to.</summary>
        Faction Faction { get; }

        /// <summary>True once the object's health has reached zero.</summary>
        bool IsDead { get; }

        /// <summary>World position of the object (for spawning effects).</summary>
        Vector3 Position { get; }

        /// <summary>
        /// Applies damage to the object.
        /// </summary>
        /// <param name="amount">Amount of damage to apply (already positive).</param>
        void TakeDamage(int amount);
    }
}
