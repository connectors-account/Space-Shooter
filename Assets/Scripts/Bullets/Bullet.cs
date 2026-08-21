using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Bullets
{
    public enum BulletOwner
    {
        Player,
        Enemy
    }

    /// <summary>
    /// A pooled projectile. Moves in a set direction, deals damage on layer-based
    /// collision, and returns itself to the pool after a lifetime or when off-screen.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float maxLifetime = 5f;
        [SerializeField] private BulletOwner owner = BulletOwner.Player;

        private Vector2 _direction = Vector2.up;
        private float _speed = 12f;
        private int _damage = 10;
        private float _lifeTimer;
        private Rigidbody2D _rb;

        public BulletOwner Owner => owner;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }

        /// <summary>Configures the bullet when spawned from the pool.</summary>
        public void Initialize(Vector2 direction, float speed, int damage, BulletOwner bulletOwner)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            owner = bulletOwner;
            _lifeTimer = 0f;

            // Orient sprite to travel direction.
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void OnEnable()
        {
            _lifeTimer = 0f;
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * _speed * Time.deltaTime);

            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= maxLifetime)
            {
                ReturnToPool();
                return;
            }

            if (ScreenBounds.Instance != null && ScreenBounds.Instance.IsOutside(transform.position, 1.5f))
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Tag-based routing: player bullets hit enemies/boss; enemy bullets hit player.
            bool validTarget =
                (owner == BulletOwner.Player && (other.CompareTag("Enemy") || other.CompareTag("Boss"))) ||
                (owner == BulletOwner.Enemy && other.CompareTag("Player"));

            if (!validTarget) return;

            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(_damage);
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            if (BulletPool.Instance != null)
            {
                BulletPool.Instance.ReturnBullet(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>Interface implemented by anything that can take bullet damage.</summary>
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }
}
