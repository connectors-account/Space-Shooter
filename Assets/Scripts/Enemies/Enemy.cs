using System;
using SpaceShooter.Core;
using SpaceShooter.Environment;
using SpaceShooter.Managers;
using SpaceShooter.PowerUps;
using SpaceShooter.Weapons;
using UnityEngine;

namespace SpaceShooter.Enemies
{
    /// <summary>
    /// A single enemy unit. The same component implements every enemy archetype
    /// (<see cref="EnemyType"/>) — movement and firing behaviour branch on the configured type.
    /// Enemies are pooled and implement <see cref="IDamageable"/> so bullets can damage them.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class Enemy : MonoBehaviour, IDamageable, IPoolable
    {
        private GameConfig _config;
        private SpriteRenderer _renderer;
        private CircleCollider2D _collider;
        private Transform _playerTransform;

        private EnemyType _type;
        private int _health;
        private int _maxHealth;
        private int _scoreValue;
        private float _moveSpeed;
        private float _fireInterval;
        private float _fireTimer;
        private float _lifeTime;

        // Movement state.
        private Vector3 _spawnPosition;
        private float _horizontalAmplitude;
        private float _horizontalFrequency;
        private float _phaseOffset;

        // Boss state.
        private int _bossPhase;
        private float _bossPatternTimer;
        private float _radialSpin;

        private Color _color;

        /// <summary>Raised when this enemy dies. Argument is the score awarded.</summary>
        public event Action<Enemy, int> Died;

        /// <inheritdoc />
        public Faction Faction => Faction.Enemy;

        /// <inheritdoc />
        public bool IsDead => _health <= 0;

        /// <inheritdoc />
        public Vector3 Position => transform.position;

        /// <summary>The archetype of this enemy.</summary>
        public EnemyType Type => _type;

        /// <summary>Maximum health, used for contact-kill calculations.</summary>
        public int MaxHealth => _maxHealth;

        /// <summary>Contact damage dealt to the player on collision.</summary>
        public int ContactDamage => _config.EnemyContactDamage;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<CircleCollider2D>();
            _collider.isTrigger = true;

            var body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.useFullKinematicContacts = true;
        }

        /// <summary>
        /// Configures the enemy when it is spawned from the pool.
        /// </summary>
        /// <param name="config">Shared configuration.</param>
        /// <param name="type">Enemy archetype.</param>
        /// <param name="health">Starting / max health.</param>
        /// <param name="scoreValue">Score awarded when destroyed.</param>
        /// <param name="moveSpeed">Vertical movement speed.</param>
        /// <param name="playerTransform">Reference to the player (for aimed fire).</param>
        public void Configure(GameConfig config, EnemyType type, int health, int scoreValue, float moveSpeed, Transform playerTransform)
        {
            _config = config;
            _type = type;
            _health = health;
            _maxHealth = health;
            _scoreValue = scoreValue;
            _moveSpeed = moveSpeed;
            _playerTransform = playerTransform;
            _spawnPosition = transform.position;
            _lifeTime = 0f;
            _fireTimer = UnityEngine.Random.Range(0.5f, 1.5f);
            _bossPhase = 0;
            _bossPatternTimer = 0f;
            _radialSpin = 0f;

            _horizontalAmplitude = UnityEngine.Random.Range(2f, 3.5f);
            _horizontalFrequency = UnityEngine.Random.Range(1.5f, 2.5f);
            _phaseOffset = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

            ApplyAppearance();
        }

        private void ApplyAppearance()
        {
            switch (_type)
            {
                case EnemyType.Basic:
                    _color = new Color(1f, 0.4f, 0.4f);
                    _renderer.sprite = SpriteFactory.CreateShipSprite(_color, 64);
                    transform.localScale = Vector3.one * 0.7f;
                    _collider.radius = 0.3f;
                    _fireInterval = 2.2f;
                    break;
                case EnemyType.Zigzag:
                    _color = new Color(1f, 0.75f, 0.2f);
                    _renderer.sprite = SpriteFactory.CreateSquareSprite(_color, 48);
                    transform.localScale = Vector3.one * 0.6f;
                    _collider.radius = 0.3f;
                    _fireInterval = 2.6f;
                    break;
                case EnemyType.Circular:
                    _color = new Color(0.7f, 0.4f, 1f);
                    _renderer.sprite = SpriteFactory.CreateCircleSprite(_color, 48);
                    transform.localScale = Vector3.one * 0.7f;
                    _collider.radius = 0.32f;
                    _fireInterval = 2.0f;
                    break;
                case EnemyType.Boss:
                    _color = new Color(0.9f, 0.1f, 0.3f);
                    _renderer.sprite = SpriteFactory.CreateCircleSprite(_color, 128);
                    transform.localScale = Vector3.one * 2.4f;
                    _collider.radius = 1.0f;
                    _fireInterval = 1.4f;
                    break;
            }

            // Point ship-shaped enemies downward.
            transform.rotation = (_type == EnemyType.Basic)
                ? Quaternion.Euler(0f, 0f, 180f)
                : Quaternion.identity;

            _renderer.color = _color;
            _renderer.sortingOrder = 6;
        }

        private void Update()
        {
            if (_config == null || IsDead)
            {
                return;
            }

            _lifeTime += Time.deltaTime;
            Move();
            HandleFiring();
            DespawnIfOffScreen();
        }

        private void Move()
        {
            switch (_type)
            {
                case EnemyType.Basic:
                    transform.position += Vector3.down * (_moveSpeed * Time.deltaTime);
                    break;

                case EnemyType.Zigzag:
                {
                    float x = Mathf.PingPong(_lifeTime * _horizontalFrequency, 2f) - 1f; // -1..1 triangle wave
                    Vector3 pos = transform.position;
                    pos.y -= _moveSpeed * Time.deltaTime;
                    pos.x = _spawnPosition.x + x * _horizontalAmplitude;
                    transform.position = _config.ClampToPlayfield(pos, 0.3f);
                    break;
                }

                case EnemyType.Circular:
                {
                    Vector3 pos = transform.position;
                    pos.y -= _moveSpeed * Time.deltaTime;
                    pos.x = _spawnPosition.x + Mathf.Sin(_lifeTime * _horizontalFrequency + _phaseOffset) * _horizontalAmplitude;
                    transform.position = _config.ClampToPlayfield(pos, 0.3f);
                    break;
                }

                case EnemyType.Boss:
                    MoveBoss();
                    break;
            }
        }

        private void MoveBoss()
        {
            // Descend to a hover line near the top, then sweep horizontally.
            float hoverY = _config.HalfHeight - 1.5f;
            Vector3 pos = transform.position;
            if (pos.y > hoverY)
            {
                pos.y -= _moveSpeed * Time.deltaTime;
            }
            else
            {
                pos.y = hoverY;
                pos.x = Mathf.Sin(_lifeTime * 0.8f) * (_config.HalfWidth - 2f);
            }
            transform.position = pos;

            // Health based phase changes.
            float ratio = (float)_health / _maxHealth;
            _bossPhase = ratio > 0.66f ? 0 : (ratio > 0.33f ? 1 : 2);
        }

        private void HandleFiring()
        {
            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f)
            {
                return;
            }

            if (_type == EnemyType.Boss)
            {
                FireBossPattern();
                _fireTimer = Mathf.Lerp(1.4f, 0.7f, _bossPhase / 2f);
                return;
            }

            // Don't fire until the enemy is on screen.
            if (transform.position.y > _config.HalfHeight)
            {
                _fireTimer = 0.2f;
                return;
            }

            Vector3 muzzle = transform.position + Vector3.down * 0.4f;
            Color bulletColor = new Color(1f, 0.5f, 0.5f);

            switch (_type)
            {
                case EnemyType.Basic:
                    BulletManager.Instance.FireStraight(muzzle, Vector2.down,
                        _config.EnemyBulletSpeed, 10, Faction.Enemy, bulletColor);
                    break;
                case EnemyType.Zigzag:
                    if (_playerTransform != null)
                    {
                        BulletManager.Instance.FireAimed(muzzle, _playerTransform.position,
                            _config.EnemyBulletSpeed, 10, Faction.Enemy, bulletColor);
                    }
                    else
                    {
                        BulletManager.Instance.FireStraight(muzzle, Vector2.down,
                            _config.EnemyBulletSpeed, 10, Faction.Enemy, bulletColor);
                    }
                    break;
                case EnemyType.Circular:
                    BulletManager.Instance.FireRadial(transform.position, 6,
                        _config.EnemyBulletSpeed * 0.8f, 10, Faction.Enemy, bulletColor, _lifeTime * 40f);
                    break;
            }

            _fireTimer = _fireInterval;
            AudioManager.Instance?.PlayEnemyShot();
        }

        private void FireBossPattern()
        {
            Color bulletColor = new Color(1f, 0.3f, 0.4f);
            Vector3 muzzle = transform.position + Vector3.down * 0.6f;
            _radialSpin += 17f;

            switch (_bossPhase)
            {
                case 0:
                    // Phase 1: triple aimed burst.
                    if (_playerTransform != null)
                    {
                        BulletManager.Instance.FireSpread(muzzle,
                            (_playerTransform.position - muzzle).normalized, 3, 24f,
                            _config.EnemyBulletSpeed, 12, Faction.Enemy, bulletColor);
                    }
                    break;
                case 1:
                    // Phase 2: radial ring.
                    BulletManager.Instance.FireRadial(transform.position, 12,
                        _config.EnemyBulletSpeed, 12, Faction.Enemy, bulletColor, _radialSpin);
                    break;
                case 2:
                    // Phase 3: dense spinning ring + aimed shot.
                    BulletManager.Instance.FireRadial(transform.position, 16,
                        _config.EnemyBulletSpeed, 14, Faction.Enemy, bulletColor, _radialSpin);
                    if (_playerTransform != null)
                    {
                        BulletManager.Instance.FireAimed(muzzle, _playerTransform.position,
                            _config.EnemyBulletSpeed * 1.3f, 14, Faction.Enemy, bulletColor);
                    }
                    break;
            }

            AudioManager.Instance?.PlayEnemyShot();
        }

        private void DespawnIfOffScreen()
        {
            // Bosses never leave; others despawn once well below the screen.
            if (_type != EnemyType.Boss && transform.position.y < -_config.HalfHeight - 1.5f)
            {
                // Counts as defeated for wave progression but awards no score.
                Died?.Invoke(this, 0);
                SpawnManager.Instance?.ReleaseEnemy(gameObject);
            }
        }

        /// <inheritdoc />
        public void TakeDamage(int amount)
        {
            if (IsDead)
            {
                return;
            }

            _health -= Mathf.Abs(amount);
            // Brief hit flash.
            _renderer.color = Color.white;
            CancelInvoke(nameof(RestoreColor));
            Invoke(nameof(RestoreColor), 0.05f);

            if (_health <= 0)
            {
                Kill();
            }
        }

        private void RestoreColor()
        {
            if (!IsDead)
            {
                _renderer.color = _color;
            }
        }

        private void Kill()
        {
            _health = 0;
            float scale = _type == EnemyType.Boss ? 3f : 1f;
            ExplosionManager.Instance?.Spawn(transform.position, _color, scale);
            AudioManager.Instance?.PlayExplosion();

            // Bosses always drop a power-up; others roll the dice.
            PowerUpManager.Instance?.TryDrop(transform.position, guaranteed: _type == EnemyType.Boss);

            Died?.Invoke(this, _scoreValue);
            SpawnManager.Instance?.ReleaseEnemy(gameObject);
        }

        /// <inheritdoc />
        public void OnSpawned()
        {
            CancelInvoke();
        }

        /// <inheritdoc />
        public void OnDespawned()
        {
            Died = null;
            CancelInvoke();
        }
    }
}
