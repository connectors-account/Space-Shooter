using UnityEngine;

namespace SpaceShooter.Weapons
{
    public enum BulletPattern
    {
        Single,
        Spread,
        Burst
    }

    public class WeaponSystem : MonoBehaviour
    {
        [Header("Bullet Setup")]
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private bool firedByPlayer = true;

        [Header("Firing")]
        [SerializeField] private BulletPattern defaultPattern = BulletPattern.Single;
        [SerializeField] private float defaultDamage = 10f;
        [SerializeField] private float spreadAngle = 18f;
        [SerializeField] private int burstCount = 3;

        public void Fire(Vector2 baseDirection)
        {
            FirePattern(defaultPattern, baseDirection);
        }

        public void FirePattern(BulletPattern pattern, Vector2 baseDirection)
        {
            if (bulletPrefab == null || firePoint == null)
            {
                return;
            }

            switch (pattern)
            {
                case BulletPattern.Single:
                    SpawnBullet(baseDirection);
                    break;
                case BulletPattern.Spread:
                    SpawnBullet(Rotate(baseDirection, -spreadAngle));
                    SpawnBullet(baseDirection);
                    SpawnBullet(Rotate(baseDirection, spreadAngle));
                    break;
                case BulletPattern.Burst:
                    for (int i = 0; i < Mathf.Max(1, burstCount); i++)
                    {
                        SpawnBullet(baseDirection);
                    }
                    break;
            }
        }

        private void SpawnBullet(Vector2 direction)
        {
            Bullet bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.Initialize(direction, firedByPlayer, defaultDamage);
        }

        private static Vector2 Rotate(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2((cos * vector.x) - (sin * vector.y), (sin * vector.x) + (cos * vector.y));
        }
    }
}
