// =============================================================================
// BulletPattern.cs — Fires bullets in various patterns from a point
// =============================================================================
using UnityEngine;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Enum for bullet pattern types.
    /// </summary>
    public enum PatternType
    {
        Straight,
        Spread3,
        Spread5,
        Circle,
        Aimed
    }

    /// <summary>
    /// Utility class to spawn bullet patterns from a given position.
    /// Attach to any entity that needs to fire projectiles.
    /// </summary>
    public class BulletPattern : MonoBehaviour
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private PatternType patternType = PatternType.Straight;
        [SerializeField] private float bulletSpeed = 8f;
        [SerializeField] private int bulletDamage = 1;
        [SerializeField] private bool isPlayerBullet = false;

        /// <summary>
        /// Fires the configured bullet pattern from the given position toward the given base direction.
        /// </summary>
        public void Fire(Vector3 spawnPosition, Vector2 baseDirection)
        {
            if (bulletPrefab == null) return;

            switch (patternType)
            {
                case PatternType.Straight:
                    SpawnBullet(spawnPosition, baseDirection);
                    break;

                case PatternType.Spread3:
                    FireSpread(spawnPosition, baseDirection, 3, 15f);
                    break;

                case PatternType.Spread5:
                    FireSpread(spawnPosition, baseDirection, 5, 12f);
                    break;

                case PatternType.Circle:
                    FireCircle(spawnPosition, 12);
                    break;

                case PatternType.Aimed:
                    FireAimed(spawnPosition);
                    break;
            }
        }

        /// <summary>
        /// Fires a spread of bullets in a fan around the base direction.
        /// </summary>
        private void FireSpread(Vector3 pos, Vector2 baseDir, int count, float angleBetween)
        {
            float startAngle = -((count - 1) * angleBetween) / 2f;
            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + i * angleBetween;
                Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;
                SpawnBullet(pos, dir);
            }
        }

        /// <summary>
        /// Fires bullets in all directions around the origin.
        /// </summary>
        private void FireCircle(Vector3 pos, int count)
        {
            float angleStep = 360f / count;
            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep;
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
                SpawnBullet(pos, dir);
            }
        }

        /// <summary>
        /// Fires a bullet aimed directly at the player.
        /// </summary>
        private void FireAimed(Vector3 pos)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector2 dir = ((Vector2)(player.transform.position - pos)).normalized;
                SpawnBullet(pos, dir);
            }
            else
            {
                SpawnBullet(pos, Vector2.down);
            }
        }

        /// <summary>
        /// Spawns a single bullet with the given direction.
        /// </summary>
        private void SpawnBullet(Vector3 pos, Vector2 direction)
        {
            GameObject go = Instantiate(bulletPrefab, pos, Quaternion.identity);
            Bullet b = go.GetComponent<Bullet>();
            if (b != null)
            {
                b.Initialize(direction, bulletSpeed, isPlayerBullet, bulletDamage);
            }
        }
    }
}
