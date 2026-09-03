using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Enemy;
using SpaceShooter.Player;

namespace SpaceShooter.Bullets
{
    /// <summary>
    /// A pooled projectile. Moves in a fixed direction, damages the opposing side
    /// on trigger, and returns itself to the pool on hit or after its lifetime.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        #region Fields
        [SerializeField] private float _speed = GameConstants.PLAYER_BULLET_SPEED;
        [SerializeField] private int _damage = GameConstants.PLAYER_BULLET_DAMAGE;
        [SerializeField] private bool _isEnemyBullet = false;
        [SerializeField] private Vector2 _direction = Vector2.up;

        private float _lifeTimer;
        private SpriteRenderer _renderer;
        #endregion

        #region Properties
        public bool IsEnemyBullet => _isEnemyBullet;
        public int Damage => _damage;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _lifeTimer = 0f;
        }

        private void Update()
        {
            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= GameConstants.BULLET_LIFETIME)
            {
                ReturnToPool();
                return;
            }

            transform.Translate((Vector3)(_direction.normalized * _speed * Time.deltaTime), Space.World);

            // Cull if off-screen with margin.
            Vector3 p = transform.position;
            if (p.y > GameConstants.CAMERA_TOP + 1.5f || p.y < GameConstants.CAMERA_BOTTOM - 1.5f ||
                p.x > GameConstants.CAMERA_RIGHT + 1.5f || p.x < GameConstants.CAMERA_LEFT - 1.5f)
            {
                ReturnToPool();
            }
        }
        #endregion

        #region Configuration
        /// <summary>Configures this bullet for use after being fetched from the pool.</summary>
        public void Configure(Vector3 position, Vector2 direction, bool isEnemy, float speed, int damage)
        {
            transform.position = position;
            _direction = direction.normalized;
            _isEnemyBullet = isEnemy;
            _speed = speed;
            _damage = damage;

            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null)
                _renderer.color = isEnemy ? Color.red : Color.yellow;

            gameObject.layer = isEnemy ? GameConstants.LAYER_ID_ENEMY_BULLET : GameConstants.LAYER_ID_PLAYER_BULLET;

            // Orient sprite along travel direction.
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        #endregion

        #region Collision
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isEnemyBullet)
            {
                if (other.CompareTag(GameConstants.TAG_PLAYER))
                {
                    PlayerHealth hp = other.GetComponent<PlayerHealth>();
                    if (hp != null) hp.TakeDamage(_damage);
                    ReturnToPool();
                }
            }
            else
            {
                if (other.CompareTag(GameConstants.TAG_ENEMY) || other.CompareTag(GameConstants.TAG_BOSS))
                {
                    EnemyBase enemy = other.GetComponent<EnemyBase>();
                    if (enemy != null) enemy.TakeDamage(_damage);
                    ReturnToPool();
                }
            }
        }
        #endregion

        #region Pool
        private void ReturnToPool()
        {
            if (BulletPool.Instance != null)
                BulletPool.Instance.ReturnBullet(this);
            else
                gameObject.SetActive(false);
        }
        #endregion
    }
}
