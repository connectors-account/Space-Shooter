using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Fires a single bullet straight down (default enemy) – or upward if used
    /// by the player. Direction is configurable via <see cref="fireDirection"/>.
    /// </summary>
    public class BulletPatternStraight : BulletPattern
    {
        [SerializeField] private Vector2 fireDirection = Vector2.down;

        public override void Fire(Vector2 origin, ObjectPool pool)
        {
            SpawnBullet(pool, origin, fireDirection.normalized);
        }
    }
}
