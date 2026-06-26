using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Controls a single projectile fired by either the player or an enemy. Bullets are pooled;
    /// movement, lifetime and collision are all handled here. The owning <see cref="Faction"/>
    /// determines what the bullet is allowed to damage.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class Bullet : MonoBehaviour, IPoolable
    {
        private Vector2 _velocity;
        private float _lifeTimer;
        private float _maxLifetime = 4f;
        private int _damage;
        private Faction _faction;
        private SpriteRenderer _renderer;
        private Rigidbody2D _body;
        private CircleCollider2D _collider;
        private GameConfig _config;

        /// <summary>The team that fired this bullet.</summary>
        public Faction Faction => _faction;

        /// <summary>Damage applied to a valid target on impact.</summary>
        public int Damage => _damage;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CircleCollider2D>();

            _body.gravityScale = 0f;
            _body.bodyType = RigidbodyType2D.Kinematic;
            // Required so kinematic-vs-kinematic trigger callbacks are raised.
            _body.useFullKinematicContacts = true;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _collider.isTrigger = true;
            _collider.radius = 0.12f;
            _renderer.sortingOrder = 5;
        }

        /// <summary>
        /// Configures and launches the bullet. Call immediately after obtaining it from a pool.
        /// </summary>
        /// <param name="config">Shared game configuration (for world bounds).</param>
        /// <param name="direction">Normalised travel direction.</param>
        /// <param name="speed">Travel speed in world units per second.</param>
        /// <param name="damage">Damage dealt on impact.</param>
        /// <param name="faction">Owning faction (player or enemy).</param>
        /// <param name="color">Render colour of the bullet.</param>
        /// <param name="radius">Logical collision/visual radius in world units.</param>
        public void Launch(GameConfig config, Vector2 direction, float speed, int damage, Faction faction, Color color, float radius = 0.12f)
        {
            _config = config;
            _velocity = direction.normalized * speed;
            _damage = damage;
            _faction = faction;
            _lifeTimer = 0f;
            _collider.radius = radius;

            _renderer.sprite = SpriteFactory.CreateCircleSprite(color, 24);
            float worldDiameter = radius * 2f;
            // Sprite is 24px at 100 PPU = 0.24 world units; scale to requested radius.
            float scale = worldDiameter / 0.24f;
            transform.localScale = new Vector3(scale, scale, 1f);

            // Orient bullet so elongated art (if any) points in travel direction.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        private void Update()
        {
            transform.position += (Vector3)(_velocity * Time.deltaTime);

            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= _maxLifetime || IsOutOfBounds())
            {
                ReturnToPool();
            }
        }

        private bool IsOutOfBounds()
        {
            if (_config == null)
            {
                return false;
            }

            Vector3 p = transform.position;
            float margin = 1.5f;
            return p.x < -_config.HalfWidth - margin || p.x > _config.HalfWidth + margin
                || p.y < -_config.HalfHeight - margin || p.y > _config.HalfHeight + margin;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (SpaceShooter.Managers.CollisionHandler.HandleBulletHit(this, other))
            {
                ReturnToPool();
            }
        }

        /// <summary>
        /// Called by the collision system when the bullet has hit a valid target so it can be recycled.
        /// </summary>
        public void ReturnToPool()
        {
            BulletManager.Instance?.Release(gameObject);
        }

        /// <inheritdoc />
        public void OnSpawned()
        {
            _lifeTimer = 0f;
        }

        /// <inheritdoc />
        public void OnDespawned()
        {
            _velocity = Vector2.zero;
        }
    }
}
