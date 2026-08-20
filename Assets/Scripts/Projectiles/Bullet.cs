using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Projectiles
{
    /// <summary>
    /// A single projectile. Travels in a fixed direction, damages the opposing side on
    /// collision, and returns itself to its pool when it leaves the screen or hits something.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        public float Speed = 10f;
        public int Damage = 10;
        public bool IsPlayerBullet = true;

        private Vector2 _direction = Vector2.up;
        private ObjectPool _pool;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.bodyType = RigidbodyType2D.Kinematic;
            // Required so trigger callbacks fire against other kinematic/static bodies.
            _rb.useFullKinematicContacts = true;
            _sr = GetComponent<SpriteRenderer>();
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        /// <summary>Configure the bullet as it is fetched from the pool.</summary>
        public void Launch(Vector2 origin, Vector2 direction, float speed, int damage, bool isPlayerBullet, ObjectPool pool)
        {
            transform.position = origin;
            _direction = direction.normalized;
            Speed = speed;
            Damage = damage;
            IsPlayerBullet = isPlayerBullet;
            _pool = pool;

            // Rotate sprite to face travel direction (sprites drawn pointing up).
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            gameObject.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";
            if (_sr != null)
            {
                if (_sr.sprite == null)
                    _sr.sprite = SpriteGenerator.CreateRect(4, 14, Color.white);
                _sr.color = isPlayerBullet ? new Color(0.3f, 1f, 1f) : new Color(1f, 0.45f, 0.1f);
            }
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * Speed * Time.deltaTime);

            if (ScreenBounds.Instance != null && ScreenBounds.Instance.IsOutOfBounds(transform.position))
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsPlayerBullet)
            {
                // Player bullets hurt enemies only.
                if (other.CompareTag("Enemy"))
                {
                    var enemy = other.GetComponent<Enemy.EnemyBase>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(Damage);
                        ReturnToPool();
                    }
                }
            }
            else
            {
                // Enemy bullets hurt the player only.
                if (other.CompareTag("Player"))
                {
                    var health = other.GetComponent<Player.PlayerHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(Damage);
                        ReturnToPool();
                    }
                }
            }
        }

        public void ReturnToPool()
        {
            if (_pool != null) _pool.Return(gameObject);
            else Destroy(gameObject);
        }
    }
}
