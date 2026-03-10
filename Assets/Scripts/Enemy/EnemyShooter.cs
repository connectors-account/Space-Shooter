using UnityEngine;

namespace SpaceShooter.Enemy
{
    public class EnemyShooter : EnemyBase
    {
        [Header("Shooter Settings")]
        [SerializeField] private bool spreadShot = false;
        [SerializeField] private int bulletCount = 3;
        [SerializeField] private float spreadAngle = 30f;

        protected override void Awake()
        {
            base.Awake();
            enemyType = EnemyType.Shooter;
            maxHealth = 30;
            currentHealth = maxHealth;
            moveSpeed = 2f;
            scoreValue = 200;
            canShoot = true;
            fireRate = 1f;
            damage = 10;
            useWaveMovement = true;
            horizontalAmplitude = 1f;
            horizontalFrequency = 0.5f;
        }

        protected override void Fire()
        {
            if (firePoint == null || bulletPrefab == null) return;

            if (spreadShot)
            {
                float startAngle = -spreadAngle / 2f;
                float angleStep = spreadAngle / (bulletCount - 1);

                for (int i = 0; i < bulletCount; i++)
                {
                    float angle = startAngle + (angleStep * i);
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;
                    SpawnBullet(direction);
                }
            }
            else
            {
                SpawnBullet(Vector2.down);
            }

            AudioManager.Instance?.PlaySound("EnemyShoot");
        }

        private void SpawnBullet(Vector2 direction)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Combat.Bullet bulletComponent = bullet.GetComponent<Combat.Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Initialize(direction, false, damage);
            }
        }
    }
}
