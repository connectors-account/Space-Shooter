using System.Collections;
using UnityEngine;

namespace SpaceShooter.Bullets
{
    /// <summary>
    /// Static helpers that emit bullets in common patterns using the BulletPool.
    /// All angles are in degrees, measured from straight up (0 = up).
    /// </summary>
    public static class BulletPattern
    {
        #region Helpers
        private static Vector2 DirFromAngle(float degreesFromUp, bool isEnemy)
        {
            // Enemies generally fire down, players up. angle 0 => that base direction.
            float baseAngle = isEnemy ? -90f : 90f; // world degrees
            float world = baseAngle - degreesFromUp; // positive offset rotates clockwise
            float rad = world * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        private static void Emit(Vector3 origin, Vector2 dir, bool isEnemy)
        {
            if (BulletPool.Instance == null) return;
            if (isEnemy) BulletPool.Instance.GetEnemyBullet(origin, dir);
            else BulletPool.Instance.GetPlayerBullet(origin, dir);
        }
        #endregion

        #region Patterns
        /// <summary>Fires a single bullet with an angular offset from the base direction.</summary>
        public static void Straight(Vector3 origin, float angleOffset, bool isEnemy)
        {
            Emit(origin, DirFromAngle(angleOffset, isEnemy), isEnemy);
        }

        /// <summary>Fans <paramref name="count"/> bullets evenly across an arc.</summary>
        public static void Spread(Vector3 origin, int count, float totalArc, bool isEnemy)
        {
            if (count <= 0) return;
            if (count == 1)
            {
                Straight(origin, 0f, isEnemy);
                return;
            }
            float step = totalArc / (count - 1);
            float start = -totalArc * 0.5f;
            for (int i = 0; i < count; i++)
            {
                float offset = start + step * i;
                Emit(origin, DirFromAngle(offset, isEnemy), isEnemy);
            }
        }

        /// <summary>Fires a full 360-degree ring of bullets.</summary>
        public static void Circle(Vector3 origin, int count, bool isEnemy)
        {
            if (count <= 0) return;
            float step = 360f / count;
            for (int i = 0; i < count; i++)
            {
                float deg = step * i;
                float rad = deg * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                Emit(origin, dir, isEnemy);
            }
        }

        /// <summary>
        /// Coroutine emitting rotating spiral arms over time. Run via owner.StartCoroutine.
        /// Runs indefinitely until the owning coroutine is stopped.
        /// </summary>
        public static IEnumerator Spiral(MonoBehaviour owner, System.Func<Vector3> originProvider, int arms, float rotSpeed, bool isEnemy, float tickInterval = 0.15f)
        {
            float angle = 0f;
            WaitForSeconds wait = new WaitForSeconds(tickInterval);
            while (true)
            {
                Vector3 origin = originProvider != null ? originProvider() : Vector3.zero;
                float armStep = 360f / Mathf.Max(1, arms);
                for (int i = 0; i < arms; i++)
                {
                    float deg = angle + armStep * i;
                    float rad = deg * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                    Emit(origin, dir, isEnemy);
                }
                angle += rotSpeed * tickInterval;
                if (angle >= 360f) angle -= 360f;
                yield return wait;
            }
        }

        /// <summary>Overload of Spiral taking a fixed origin.</summary>
        public static IEnumerator Spiral(MonoBehaviour owner, Vector3 origin, int arms, float rotSpeed, bool isEnemy, float tickInterval = 0.15f)
        {
            return Spiral(owner, () => origin, arms, rotSpeed, isEnemy, tickInterval);
        }

        /// <summary>Fires one bullet aimed directly at a target position.</summary>
        public static void Aimed(Vector3 origin, Vector3 targetPos, bool isEnemy)
        {
            Vector2 dir = (targetPos - origin);
            if (dir.sqrMagnitude < 0.0001f)
                dir = isEnemy ? Vector2.down : Vector2.up;
            Emit(origin, dir.normalized, isEnemy);
        }
        #endregion
    }
}
