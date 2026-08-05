using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Abstract base for enemy/boss bullet patterns. A pattern knows how to
    /// spawn one "volley" of bullets from a given origin using the object pool.
    /// Concrete patterns override <see cref="Fire"/>.
    /// </summary>
    public abstract class BulletPattern : MonoBehaviour
    {
        [Header("Common bullet settings")]
        [SerializeField] protected float bulletSpeed = 6f;
        [SerializeField] protected int bulletDamage = 1;
        [SerializeField] protected Color bulletColour = new Color(1f, 0.5f, 0.35f);

        /// <summary>The player transform, used by aimed/homing patterns.</summary>
        public Transform PlayerTarget { get; set; }

        /// <summary>Spawn a volley of bullets originating at <paramref name="origin"/>.</summary>
        public abstract void Fire(Vector2 origin, ObjectPool pool);

        /// <summary>Helper to acquire and configure one enemy bullet.</summary>
        protected Bullet SpawnBullet(ObjectPool pool, Vector2 origin, Vector2 direction, bool homing = false)
        {
            if (pool == null) return null;
            var go = pool.Acquire(Constants.PoolEnemyBullet, origin, Quaternion.identity);
            if (go == null) return null;

            var bullet = go.GetComponent<Bullet>();
            if (bullet == null) bullet = go.AddComponent<Bullet>();

            bullet.Configure(
                direction,
                bulletSpeed,
                bulletDamage,
                Constants.TagEnemyBullet,
                bulletColour,
                homing,
                homing ? PlayerTarget : null);
            return bullet;
        }

        protected Transform ResolvePlayer()
        {
            if (PlayerTarget != null) return PlayerTarget;
            var go = GameObject.FindGameObjectWithTag(Constants.TagPlayer);
            if (go != null) PlayerTarget = go.transform;
            return PlayerTarget;
        }
    }
}
