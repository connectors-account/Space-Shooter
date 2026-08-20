using UnityEngine;
using SpaceShooter.Projectiles;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Zigzag enemy: descends while weaving horizontally with a sine wave, and fires an
    /// aimed bullet at the player every 1.5 seconds. Health 50, Speed 3, Score 200.
    /// </summary>
    public class EnemyTypeB : EnemyBase
    {
        [Header("Type B")]
        public float fireInterval = 1.5f;
        public float zigzagAmplitude = 2.5f;
        public float zigzagFrequency = 2f;

        private float _nextFire;
        private float _startX;
        private float _phase;

        protected override void Start()
        {
            maxHealth = 50;
            speed = 3f;
            scoreValue = 200;
            base.Start();
            transform.rotation = Quaternion.Euler(0, 0, 180f);
            _startX = transform.position.x;
            _phase = Random.Range(0f, Mathf.PI * 2f);
            _nextFire = Time.time + Random.Range(0.5f, fireInterval);
        }

        protected override Sprite CreateSprite()
        {
            return SpriteGenerator.CreateShip(new Color(1f, 0.6f, 0.15f), new Color(1f, 0.9f, 0.5f));
        }

        protected override void Move()
        {
            _phase += zigzagFrequency * Time.deltaTime;
            float x = _startX + Mathf.Sin(_phase) * zigzagAmplitude;
            float y = transform.position.y - speed * Time.deltaTime;
            transform.position = new Vector3(x, y, transform.position.z);
        }

        protected override void FirePattern()
        {
            if (Time.time >= _nextFire)
            {
                _nextFire = Time.time + fireInterval;
                Vector2 target = PlayerTransform != null
                    ? (Vector2)PlayerTransform.position
                    : (Vector2)transform.position + Vector2.down;
                BulletPattern.FireAimed(transform.position, target, enemyBulletPrefab,
                    bulletSpeed, bulletDamage, false);
            }
        }
    }
}
