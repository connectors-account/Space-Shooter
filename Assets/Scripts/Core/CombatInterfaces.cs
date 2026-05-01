using UnityEngine;

namespace SpaceShooter.Core
{
    public enum Faction
    {
        Player,
        Enemy,
        Neutral
    }

    public interface IDamageable
    {
        Faction Faction { get; }
        void ApplyDamage(int amount, Vector3 hitPosition);
    }
}
