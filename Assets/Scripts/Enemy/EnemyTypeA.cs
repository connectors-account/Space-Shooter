using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Weapons;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Basic drone: flies straight down with a subtle sine drift, fires a single bullet
    /// downward every 2 seconds.
    /// </summary>
    public class EnemyTypeA : EnemyBase
    {
        [Header("Type A Settings")]
        [SerializeField] private float driftAmplitude = 1.2f;
        [SerializeField] private float driftFrequency = 2f;
        [SerializeField] private float shootInterval = 2f;
        [SerializeField] private float bulletSpeed = 6f;
        [SerializeField] private int bulletDamage = 10;
        [SerializeField] private string enemyBulletTag = "EnemyBullet";

        private float shootTimer;
        private float spawnX;
        private float spawnTime;

        protected override void Awake()
        {
            base.Awake();
            maxHealth = 30;
            speed = 3f;
            scoreValue = 100;
            poolTag = "EnemyA";
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            spawnX = transform.position.x;
            spawnTime = Time.time;
            shootTimer = shootInterval;
        }

        protected override void Move()
        {
            float t = Time.time - spawnTime;
            float x = spawnX + Mathf.Sin(t * driftFrequency) * driftAmplitude;
            float y = transform.position.y - speed * Time.deltaTime;
            transform.position = new Vector3(x, y, 0f);
        }

        protected override void Shoot()
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                shootTimer = shootInterval;
                BulletPattern.SingleShot(enemyBulletTag, transform, Vector2.down, bulletSpeed, bulletDamage, "Player");
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("EnemyShoot");
            }
        }
    }
}
