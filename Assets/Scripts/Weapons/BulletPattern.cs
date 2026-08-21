using System.Collections;
using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Static utility for spawning bullet patterns from the ObjectPool.
    /// Every method pulls bullets from the pool and initializes them.
    /// </summary>
    public static class BulletPattern
    {
        private static Bullet Spawn(string poolTag, Vector3 position, Vector2 direction, float speed, int damage, string targetTag, bool homing = false)
        {
            if (ObjectPool.Instance == null) return null;
            GameObject obj = ObjectPool.Instance.GetObject(poolTag, position, Quaternion.identity);
            if (obj == null) return null;
            Bullet bullet = obj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(direction, speed, damage, targetTag, poolTag, homing);
            }
            return bullet;
        }

        public static void SingleShot(string poolTag, Transform origin, Vector2 direction, float speed, int damage, string targetTag, bool homing = false)
        {
            Spawn(poolTag, origin.position, direction, speed, damage, targetTag, homing);
        }

        public static void DoubleShot(string poolTag, Transform origin, Vector2 direction, float speed, int damage, string targetTag, float separation)
        {
            Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized * (separation * 0.5f);
            Spawn(poolTag, origin.position + (Vector3)perpendicular, direction, speed, damage, targetTag);
            Spawn(poolTag, origin.position - (Vector3)perpendicular, direction, speed, damage, targetTag);
        }

        public static void TripleSpread(string poolTag, Transform origin, Vector2 direction, float speed, int damage, string targetTag, float spreadAngle)
        {
            Spawn(poolTag, origin.position, direction, speed, damage, targetTag);
            Spawn(poolTag, origin.position, Rotate(direction, spreadAngle), speed, damage, targetTag);
            Spawn(poolTag, origin.position, Rotate(direction, -spreadAngle), speed, damage, targetTag);
        }

        public static void FiveWaySpread(string poolTag, Transform origin, Vector2 direction, float speed, int damage, string targetTag, float spreadAngle)
        {
            for (int i = -2; i <= 2; i++)
            {
                Spawn(poolTag, origin.position, Rotate(direction, spreadAngle * i), speed, damage, targetTag);
            }
        }

        public static void CirclePattern(string poolTag, Transform origin, float speed, int damage, string targetTag, int count)
        {
            float step = 360f / Mathf.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Rotate(Vector2.up, step * i);
                Spawn(poolTag, origin.position, dir, speed, damage, targetTag);
            }
        }

        /// <summary>Coroutine-based spiral. Call from a MonoBehaviour with StartCoroutine.</summary>
        public static IEnumerator SpiralPattern(string poolTag, Transform origin, float speed, int damage, string targetTag, int count, float delay, float angleStep = 24f)
        {
            float angle = 0f;
            for (int i = 0; i < count; i++)
            {
                if (origin == null) yield break;
                Vector2 dir = Rotate(Vector2.up, angle);
                Spawn(poolTag, origin.position, dir, speed, damage, targetTag);
                angle += angleStep;
                yield return new WaitForSeconds(delay);
            }
        }

        public static void AimedShot(string poolTag, Transform origin, Transform target, float speed, int damage, string targetTag, bool homing = false)
        {
            Vector2 dir = target != null
                ? ((Vector2)target.position - (Vector2)origin.position).normalized
                : Vector2.down;
            Spawn(poolTag, origin.position, dir, speed, damage, targetTag, homing);
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
