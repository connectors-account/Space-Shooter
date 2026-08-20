using UnityEngine;
using SpaceShooter.Projectiles;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Basic enemy: descends straight down, fires a single bullet downward every 2 seconds.
    /// Health 30, Speed 2, Score 100.
    /// </summary>
    public class EnemyTypeA : EnemyBase
    {
        [Header("Type A")]
        public float fireInterval = 2f;
        private float _nextFire;

        protected override void Start()
        {
            maxHealth = 30;
            speed = 2f;
            scoreValue = 100;
            base.Start();
            transform.rotation = Quaternion.Euler(0, 0, 180f); // point down toward the player
            _nextFire = Time.time + Random.Range(0.5f, fireInterval);
        }

        protected override Sprite CreateSprite()
        {
            return SpriteGenerator.CreateShip(new Color(0.9f, 0.3f, 0.3f), new Color(1f, 0.8f, 0.4f));
        }

        protected override void Move()
        {
            transform.position += Vector3.down * speed * Time.deltaTime;
        }

        protected override void FirePattern()
        {
            if (Time.time >= _nextFire)
            {
                _nextFire = Time.time + fireInterval;
                BulletPattern.FireSingle(transform.position + Vector3.down * 0.5f, Vector2.down,
                    enemyBulletPrefab, bulletSpeed, bulletDamage, false);
            }
        }
    }
}
