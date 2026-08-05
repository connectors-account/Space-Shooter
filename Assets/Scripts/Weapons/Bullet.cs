using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// A pooled projectile. Configured on spawn with a direction, speed, damage
    /// and owner tag (Player or Enemy). Damages targets carrying the opposite
    /// tag on trigger, then returns itself to the pool. Also self-releases when
    /// its lifetime expires or it leaves the screen. Supports optional homing
    /// (used by the boss's second phase).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private float homingTurnRate = 180f; // degrees/sec

        private Vector2 _direction = Vector2.up;
        private float _speed = 12f;
        private int _damage = 1;
        private string _ownerTag = Constants.TagPlayerBullet;
        private bool _homing;
        private Transform _homingTarget;

        private float _life;
        private SpriteRenderer _sr;
        private Collider2D _collider;
        private string _poolKey;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sortingOrder = 3;

            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;

            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        /// <summary>Configure a freshly-acquired bullet.</summary>
        public void Configure(Vector2 direction, float speed, int damage, string ownerTag, Color colour, bool homing = false, Transform homingTarget = null)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _ownerTag = ownerTag;
            _homing = homing;
            _homingTarget = homingTarget;
            _life = lifetime;

            _poolKey = ownerTag == Constants.TagPlayerBullet
                ? Constants.PoolPlayerBullet
                : Constants.PoolEnemyBullet;

            gameObject.tag = ownerTag;
            if (_sr.sprite == null || colour != _sr.color)
                _sr.sprite = SpriteGenerator.CreateBulletSprite(colour);
            _sr.color = Color.white;

            // Face travel direction.
            float ang = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, ang);
        }

        private void OnEnable()
        {
            _life = lifetime;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
                return;

            float dt = Time.deltaTime;

            if (_homing && _homingTarget != null && _homingTarget.gameObject.activeInHierarchy)
            {
                Vector2 desired = ((Vector2)_homingTarget.position - (Vector2)transform.position).normalized;
                _direction = Vector3.RotateTowards(_direction, desired, homingTurnRate * Mathf.Deg2Rad * dt, 0f);
                float ang = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, ang);
            }

            transform.position += (Vector3)(_direction * (_speed * dt));

            _life -= dt;
            if (_life <= 0f || IsOffScreen())
                ReleaseSelf();
        }

        private bool IsOffScreen()
        {
            var cam = Camera.main;
            if (cam == null || !cam.orthographic) return false;
            float halfH = cam.orthographicSize + 1.5f;
            float halfW = halfH * cam.aspect + 1.5f;
            Vector3 c = cam.transform.position;
            Vector3 p = transform.position;
            return p.y > c.y + halfH || p.y < c.y - halfH || p.x > c.x + halfW || p.x < c.x - halfW;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Player bullets hit enemies; enemy bullets hit the player.
            if (_ownerTag == Constants.TagPlayerBullet)
            {
                if (other.CompareTag(Constants.TagEnemy) || other.CompareTag(Constants.TagBoss))
                {
                    var enemy = other.GetComponent<Enemy.EnemyBase>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(_damage);
                        ReleaseSelf();
                    }
                }
            }
            else // enemy bullet
            {
                if (other.CompareTag(Constants.TagPlayer))
                {
                    var health = other.GetComponent<Player.PlayerHealth>();
                    if (health != null)
                    {
                        health.TakeDamage();
                        ReleaseSelf();
                    }
                }
            }
        }

        private void ReleaseSelf()
        {
            _homing = false;
            _homingTarget = null;
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.Release(_poolKey, gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
