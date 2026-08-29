using UnityEngine;
using SpaceShooter.Bullets;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Enemy bullet firing patterns.
    /// </summary>
    public enum BulletPattern
    {
        Single,
        Spread3,
        Spread5,
        Aimed,
        Burst3
    }

    /// <summary>
    /// Fires enemy bullets at a fixed interval using the selected pattern.
    /// </summary>
    public class EnemyShooter : MonoBehaviour
    {
        [Header("Firing")]
        [SerializeField] private BulletPattern pattern = BulletPattern.Single;
        [SerializeField] private float shootInterval = 1.5f;
        [SerializeField] private int bulletDamage = Constants.EnemyBulletDamage;
        [SerializeField] private float bulletSpeed = Constants.EnemyBulletSpeed;
        [SerializeField] private Transform firePoint;

        [Header("Spread")]
        [SerializeField] private float spreadAngle = 15f;

        [Header("Burst")]
        [SerializeField] private int burstCount = 3;
        [SerializeField] private float burstDelay = 0.12f;

        private Transform _player;
        private bool _firing;

        private void Awake()
        {
            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        /// <summary>
        /// Begins firing. Optionally overrides pattern/interval/damage from EnemyData.
        /// </summary>
        public void BeginFiring(BulletPattern newPattern, float interval, int damage)
        {
            pattern = newPattern;
            shootInterval = Mathf.Max(0.1f, interval);
            bulletDamage = damage;
            RestartFiring();
        }

        public void RestartFiring()
        {
            var playerObj = GameObject.FindGameObjectWithTag(Constants.Tags.Player);
            if (playerObj != null)
            {
                _player = playerObj.transform;
            }

            CancelInvoke(nameof(FireOnce));
            if (shootInterval > 0f)
            {
                _firing = true;
                InvokeRepeating(nameof(FireOnce), shootInterval, shootInterval);
            }
        }

        public void StopFiring()
        {
            _firing = false;
            CancelInvoke(nameof(FireOnce));
        }

        private void OnDisable()
        {
            StopFiring();
        }

        private void FireOnce()
        {
            if (!_firing || !BulletPool.HasInstance)
            {
                return;
            }

            if (GameManager.HasInstance && GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            switch (pattern)
            {
                case BulletPattern.Single:
                    FireBullet(180f);
                    break;

                case BulletPattern.Spread3:
                    FireBullet(180f - spreadAngle);
                    FireBullet(180f);
                    FireBullet(180f + spreadAngle);
                    break;

                case BulletPattern.Spread5:
                    for (int i = -2; i <= 2; i++)
                    {
                        FireBullet(180f + i * spreadAngle);
                    }
                    break;

                case BulletPattern.Aimed:
                    FireBullet(AngleToPlayer());
                    break;

                case BulletPattern.Burst3:
                    StartCoroutine(BurstRoutine());
                    break;
            }
        }

        private System.Collections.IEnumerator BurstRoutine()
        {
            for (int i = 0; i < burstCount; i++)
            {
                if (!_firing)
                {
                    yield break;
                }
                FireBullet(180f);
                yield return new WaitForSeconds(burstDelay);
            }
        }

        private float AngleToPlayer()
        {
            if (_player == null)
            {
                return 180f;
            }

            Vector2 dir = (_player.position - firePoint.position).normalized;
            // Bullet moves along its local up; compute the Z rotation so up points at the player.
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            return angle;
        }

        private void FireBullet(float zAngle)
        {
            Quaternion rot = Quaternion.Euler(0f, 0f, zAngle);
            EnemyBullet bullet = BulletPool.Instance.GetEnemyBullet(firePoint.position, rot);
            if (bullet != null)
            {
                bullet.Configure(bulletDamage, bulletSpeed);
            }
        }
    }
}
