using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Owns the projectile object pool and exposes high-level firing helpers that implement the
    /// various bullet patterns (single, spread, aimed and radial). All gameplay code fires bullets
    /// through this singleton so projectiles are always recycled.
    /// </summary>
    public class BulletManager : MonoBehaviour
    {
        /// <summary>Global access point.</summary>
        public static BulletManager Instance { get; private set; }

        private ObjectPool _pool;
        private GameConfig _config;
        private Transform _container;

        /// <summary>
        /// Initialises the bullet pool. Must be called once by the bootstrap before any firing.
        /// </summary>
        /// <param name="config">Shared game configuration.</param>
        public void Initialize(GameConfig config)
        {
            Instance = this;
            _config = config;

            _container = new GameObject("Bullets").transform;
            _container.SetParent(transform, false);

            GameObject template = CreateBulletTemplate();
            _pool = new ObjectPool(template, _container, prewarm: 64);
            template.SetActive(false);
        }

        private GameObject CreateBulletTemplate()
        {
            var go = new GameObject("Bullet");
            go.SetActive(false);
            go.layer = gameObject.layer;
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<Bullet>();
            return go;
        }

        /// <summary>
        /// Fires a single straight bullet in the supplied direction.
        /// </summary>
        public Bullet FireStraight(Vector3 origin, Vector2 direction, float speed, int damage, Faction faction, Color color, float radius = 0.12f)
        {
            GameObject go = _pool.Get(origin, Quaternion.identity);
            Bullet bullet = go.GetComponent<Bullet>();
            bullet.Launch(_config, direction, speed, damage, faction, color, radius);
            return bullet;
        }

        /// <summary>
        /// Fires <paramref name="count"/> bullets fanned symmetrically around <paramref name="direction"/>.
        /// </summary>
        /// <param name="spreadDegrees">Total angular spread covered by the fan.</param>
        public void FireSpread(Vector3 origin, Vector2 direction, int count, float spreadDegrees, float speed, int damage, Faction faction, Color color)
        {
            if (count <= 1)
            {
                FireStraight(origin, direction, speed, damage, faction, color);
                return;
            }

            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float step = spreadDegrees / (count - 1);
            float start = baseAngle - spreadDegrees * 0.5f;
            for (int i = 0; i < count; i++)
            {
                float angle = (start + step * i) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                FireStraight(origin, dir, speed, damage, faction, color);
            }
        }

        /// <summary>
        /// Fires a single bullet aimed at <paramref name="target"/> from <paramref name="origin"/>.
        /// </summary>
        public void FireAimed(Vector3 origin, Vector3 target, float speed, int damage, Faction faction, Color color)
        {
            Vector2 dir = (target - origin);
            if (dir.sqrMagnitude < 0.0001f)
            {
                dir = Vector2.down;
            }
            FireStraight(origin, dir.normalized, speed, damage, faction, color);
        }

        /// <summary>
        /// Fires <paramref name="count"/> bullets evenly distributed around a full circle.
        /// </summary>
        /// <param name="angleOffset">Rotational offset applied to the whole ring (useful for spinning sprays).</param>
        public void FireRadial(Vector3 origin, int count, float speed, int damage, Faction faction, Color color, float angleOffset = 0f)
        {
            if (count <= 0)
            {
                return;
            }

            float step = 360f / count;
            for (int i = 0; i < count; i++)
            {
                float angle = (angleOffset + step * i) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                FireStraight(origin, dir, speed, damage, faction, color);
            }
        }

        /// <summary>Returns a bullet instance to the pool.</summary>
        public void Release(GameObject bullet)
        {
            _pool?.Release(bullet);
        }

        /// <summary>Recycles every active bullet (used when restarting or clearing the field).</summary>
        public void ReleaseAll()
        {
            _pool?.ReleaseAll();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
