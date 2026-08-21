using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Weapons;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Zigzag fighter: sinusoidal horizontal movement while descending, fires an aimed bullet
    /// toward the player every 1.5 seconds and rotates to face travel direction.
    /// </summary>
    public class EnemyTypeB : EnemyBase
    {
        [Header("Type B Settings")]
        [SerializeField] private float zigzagAmplitude = 3f;
        [SerializeField] private float zigzagFrequency = 3f;
        [SerializeField] private float shootInterval = 1.5f;
        [SerializeField] private float bulletSpeed = 7f;
        [SerializeField] private int bulletDamage = 12;
        [SerializeField] private string enemyBulletTag = "EnemyBullet";

        private float shootTimer;
        private float spawnX;
        private float spawnTime;
        private Vector3 lastPosition;

        protected override void Awake()
        {
            base.Awake();
            maxHealth = 50;
            speed = 4f;
            scoreValue = 200;
            poolTag = "EnemyB";
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            spawnX = transform.position.x;
            spawnTime = Time.time;
            shootTimer = shootInterval;
            lastPosition = transform.position;
        }

        protected override void Move()
        {
            float t = Time.time - spawnTime;
            float x = spawnX + Mathf.Sin(t * zigzagFrequency) * zigzagAmplitude;
            float y = transform.position.y - speed * Time.deltaTime;
            Vector3 newPos = new Vector3(x, y, 0f);

            // Rotate to face movement direction.
            Vector3 dir = newPos - lastPosition;
            if (dir.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            lastPosition = newPos;
            transform.position = newPos;
        }

        protected override void Shoot()
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                shootTimer = shootInterval;
                BulletPattern.AimedShot(enemyBulletTag, transform, player, bulletSpeed, bulletDamage, "Player");
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("EnemyShoot");
            }
        }
    }
}
