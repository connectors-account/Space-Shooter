using UnityEngine;

namespace SpaceShooter.Projectiles
{
    /// <summary>
    /// Static utility for spawning bullets in various patterns.
    ///
    /// Bullets are drawn from pools when available (register them via SetPools), otherwise
    /// the supplied prefab is instantiated directly. Every spawned object is expected to have
    /// a <see cref="Bullet"/> component.
    /// </summary>
    public static class BulletPattern
    {
        private static ObjectPool _playerPool;
        private static ObjectPool _enemyPool;

        /// <summary>Register the pools the game uses (call once at scene start).</summary>
        public static void SetPools(ObjectPool playerPool, ObjectPool enemyPool)
        {
            _playerPool = playerPool;
            _enemyPool = enemyPool;
        }

        public static void RegisterPlayerPool(ObjectPool p) { _playerPool = p; }
        public static void RegisterEnemyPool(ObjectPool p) { _enemyPool = p; }

        /// <summary>Clear pool references (e.g. on scene unload) so stale objects aren't reused.</summary>
        public static void ClearPools()
        {
            _playerPool = null;
            _enemyPool = null;
        }

        private static Bullet Spawn(Vector2 origin, Vector2 dir, GameObject prefab,
            float speed, int damage, bool isPlayer)
        {
            GameObject go;
            ObjectPool pool = isPlayer ? _playerPool : _enemyPool;

            if (pool != null)
            {
                go = pool.Get();
            }
            else if (prefab != null)
            {
                go = Object.Instantiate(prefab);
            }
            else
            {
                return null;
            }

            var bullet = go.GetComponent<Bullet>();
            if (bullet == null) bullet = go.AddComponent<Bullet>();
            bullet.Launch(origin, dir, speed, damage, isPlayer, pool);
            return bullet;
        }

        /// <summary>Fire one bullet in a direction.</summary>
        public static void FireSingle(Vector2 origin, Vector2 direction, GameObject prefab,
            float speed, int damage, bool isPlayer = true)
        {
            Spawn(origin, direction, prefab, speed, damage, isPlayer);
        }

        /// <summary>Fire <paramref name="count"/> bullets fanned across <paramref name="spreadAngle"/> degrees.</summary>
        public static void FireSpread(Vector2 origin, int count, float spreadAngle, Vector2 baseDirection,
            GameObject prefab, float speed, int damage, bool isPlayer = true)
        {
            if (count <= 0) return;
            float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
            float start = baseAngle - spreadAngle * 0.5f;
            float step = count > 1 ? spreadAngle / (count - 1) : 0f;

            for (int i = 0; i < count; i++)
            {
                float a = (start + step * i) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                Spawn(origin, dir, prefab, speed, damage, isPlayer);
            }
        }

        /// <summary>Fire bullets radially outward in a full circle.</summary>
        public static void FireCircle(Vector2 origin, int count, GameObject prefab,
            float speed, int damage, bool isPlayer = false)
        {
            if (count <= 0) return;
            float step = 360f / count;
            for (int i = 0; i < count; i++)
            {
                float a = (step * i) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                Spawn(origin, dir, prefab, speed, damage, isPlayer);
            }
        }

        /// <summary>
        /// Fire a spiral arm. Call repeatedly, incrementing <paramref name="angleOffset"/> each time
        /// to sweep the spiral around the origin.
        /// </summary>
        public static void FireSpiral(Vector2 origin, int count, float angleOffset, GameObject prefab,
            float speed, int damage, bool isPlayer = false)
        {
            if (count <= 0) return;
            float step = 360f / count;
            for (int i = 0; i < count; i++)
            {
                float a = (step * i + angleOffset) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                Spawn(origin, dir, prefab, speed, damage, isPlayer);
            }
        }

        /// <summary>Fire one bullet aimed at a target position.</summary>
        public static void FireAimed(Vector2 origin, Vector2 target, GameObject prefab,
            float speed, int damage, bool isPlayer = false)
        {
            Vector2 dir = (target - origin);
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.down;
            Spawn(origin, dir.normalized, prefab, speed, damage, isPlayer);
        }
    }
}
