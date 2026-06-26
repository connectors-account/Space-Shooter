using System;
using SpaceShooter.Core;
using SpaceShooter.Environment;
using SpaceShooter.Managers;
using SpaceShooter.Weapons;
using UnityEngine;

namespace SpaceShooter.Player
{
    /// <summary>
    /// The player ship. Handles movement (WASD / arrow keys), shooting (Space / left mouse),
    /// health and lives, temporary power-up effects, and collision responses. Raises events that the
    /// <see cref="UIManager"/> subscribes to for HUD updates.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class PlayerController : MonoBehaviour, IDamageable
    {
        private GameConfig _config;
        private SpriteRenderer _renderer;
        private CircleCollider2D _collider;

        private int _health;
        private int _lives;
        private float _fireTimer;
        private float _invulnerabilityTimer;
        private float _flickerTimer;
        private bool _controllable;

        // Power-up timers (seconds remaining; <= 0 means inactive).
        private float _shieldTimer;
        private float _rapidFireTimer;
        private float _spreadShotTimer;
        private float _scoreMultiplierTimer;

        private readonly Color _shipColor = new Color(0.35f, 0.85f, 1f);
        private readonly Color _shieldColor = new Color(0.4f, 0.8f, 1f, 0.5f);
        private SpriteRenderer _shieldRenderer;

        /// <summary>Raised when current/max health changes. Args: (current, max).</summary>
        public event Action<int, int> HealthChanged;

        /// <summary>Raised when the remaining life count changes.</summary>
        public event Action<int> LivesChanged;

        /// <summary>Raised when the player runs out of lives.</summary>
        public event Action PlayerDied;

        /// <summary>Raised when a power-up state changes. Args: (type, secondsRemaining).</summary>
        public event Action<PowerUpType, float> PowerUpStateChanged;

        /// <inheritdoc />
        public Faction Faction => Faction.Player;

        /// <inheritdoc />
        public bool IsDead => _lives <= 0;

        /// <inheritdoc />
        public Vector3 Position => transform.position;

        /// <summary>Current health of the ship.</summary>
        public int Health => _health;

        /// <summary>Maximum health of the ship.</summary>
        public int MaxHealth => _config.PlayerMaxHealth;

        /// <summary>Remaining lives.</summary>
        public int Lives => _lives;

        /// <summary>True while the shield power-up is active (invincible).</summary>
        public bool ShieldActive => _shieldTimer > 0f;

        /// <summary>The score multiplier currently granted by power-ups (1 when none).</summary>
        public int ScoreMultiplier => _scoreMultiplierTimer > 0f ? _config.ScoreMultiplierFactor : 1;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<CircleCollider2D>();
            _collider.isTrigger = true;
            _collider.radius = 0.32f;

            var body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.useFullKinematicContacts = true;
        }

        /// <summary>
        /// Initialises the ship with shared configuration. Called by the bootstrap once the scene is built.
        /// </summary>
        /// <param name="config">Shared game configuration.</param>
        public void Initialize(GameConfig config)
        {
            _config = config;
            _renderer.sprite = SpriteFactory.CreateShipSprite(_shipColor, 64);
            _renderer.color = _shipColor;
            _renderer.sortingOrder = 6;
            transform.localScale = Vector3.one * 0.8f;

            CreateShieldVisual();

            _lives = config.PlayerStartingLives;
            ResetForNewLife(true);
            LivesChanged?.Invoke(_lives);
        }

        private void CreateShieldVisual()
        {
            var shieldGo = new GameObject("Shield");
            shieldGo.transform.SetParent(transform, false);
            shieldGo.transform.localScale = Vector3.one * 1.6f;
            _shieldRenderer = shieldGo.AddComponent<SpriteRenderer>();
            _shieldRenderer.sprite = SpriteFactory.CreateCircleSprite(Color.white, 64);
            _shieldRenderer.color = _shieldColor;
            _shieldRenderer.sortingOrder = 7;
            _shieldRenderer.enabled = false;
        }

        /// <summary>
        /// Enables or disables player control (used while paused / on game over).
        /// </summary>
        public void SetControllable(bool value)
        {
            _controllable = value;
        }

        private void Update()
        {
            if (_config == null || !_controllable || IsDead)
            {
                return;
            }

            HandleMovement();
            HandleShooting();
            UpdateTimers();
        }

        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 input = new Vector3(h, v, 0f);
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            Vector3 next = transform.position + input * (_config.PlayerMoveSpeed * Time.deltaTime);
            transform.position = _config.ClampToPlayfield(next);
        }

        private void HandleShooting()
        {
            _fireTimer -= Time.deltaTime;

            bool firing = Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
            if (!firing || _fireTimer > 0f)
            {
                return;
            }

            float cooldown = _rapidFireTimer > 0f ? _config.PlayerRapidFireCooldown : _config.PlayerFireCooldown;
            _fireTimer = cooldown;

            Vector3 muzzle = transform.position + Vector3.up * 0.5f;
            Color bulletColor = new Color(0.7f, 1f, 1f);

            if (_spreadShotTimer > 0f)
            {
                BulletManager.Instance.FireSpread(muzzle, Vector2.up, 3, 30f,
                    _config.PlayerBulletSpeed, _config.PlayerBulletDamage, Faction.Player, bulletColor);
            }
            else
            {
                BulletManager.Instance.FireStraight(muzzle, Vector2.up,
                    _config.PlayerBulletSpeed, _config.PlayerBulletDamage, Faction.Player, bulletColor);
            }

            AudioManager.Instance?.PlayPlayerShot();
        }

        private void UpdateTimers()
        {
            if (_invulnerabilityTimer > 0f)
            {
                _invulnerabilityTimer -= Time.deltaTime;
                _flickerTimer += Time.deltaTime;
                // Flicker the ship while briefly invulnerable after a hit (but not while shielded).
                if (!ShieldActive)
                {
                    _renderer.enabled = Mathf.Repeat(_flickerTimer, 0.2f) > 0.1f;
                }
                if (_invulnerabilityTimer <= 0f)
                {
                    _renderer.enabled = true;
                }
            }

            TickPowerUp(ref _shieldTimer, PowerUpType.Shield);
            TickPowerUp(ref _rapidFireTimer, PowerUpType.RapidFire);
            TickPowerUp(ref _spreadShotTimer, PowerUpType.SpreadShot);
            TickPowerUp(ref _scoreMultiplierTimer, PowerUpType.ScoreMultiplier);

            _shieldRenderer.enabled = ShieldActive;
        }

        private void TickPowerUp(ref float timer, PowerUpType type)
        {
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
                PowerUpStateChanged?.Invoke(type, Mathf.Max(0f, timer));
                if (timer <= 0f)
                {
                    PowerUpStateChanged?.Invoke(type, 0f);
                }
            }
        }

        /// <summary>
        /// Applies a collected power-up effect.
        /// </summary>
        /// <param name="type">The power-up type collected.</param>
        public void ApplyPowerUp(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Health:
                    _health = Mathf.Min(MaxHealth, _health + _config.HealthPickupAmount);
                    HealthChanged?.Invoke(_health, MaxHealth);
                    break;
                case PowerUpType.Shield:
                    _shieldTimer = _config.ShieldDuration;
                    break;
                case PowerUpType.RapidFire:
                    _rapidFireTimer = _config.RapidFireDuration;
                    break;
                case PowerUpType.SpreadShot:
                    _spreadShotTimer = _config.SpreadShotDuration;
                    break;
                case PowerUpType.ScoreMultiplier:
                    _scoreMultiplierTimer = _config.ScoreMultiplierDuration;
                    break;
            }

            PowerUpStateChanged?.Invoke(type, PowerUpDuration(type));
            AudioManager.Instance?.PlayPowerUp();
        }

        private float PowerUpDuration(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Shield: return _config.ShieldDuration;
                case PowerUpType.RapidFire: return _config.RapidFireDuration;
                case PowerUpType.SpreadShot: return _config.SpreadShotDuration;
                case PowerUpType.ScoreMultiplier: return _config.ScoreMultiplierDuration;
                default: return 0f;
            }
        }

        /// <inheritdoc />
        public void TakeDamage(int amount)
        {
            if (IsDead || _invulnerabilityTimer > 0f || ShieldActive || !_controllable)
            {
                return;
            }

            _health -= Mathf.Abs(amount);
            _invulnerabilityTimer = _config.PlayerInvulnerabilityAfterHit;
            _flickerTimer = 0f;
            AudioManager.Instance?.PlayPlayerHit();

            if (_health <= 0)
            {
                _health = 0;
                LoseLife();
            }

            HealthChanged?.Invoke(_health, MaxHealth);
        }

        private void LoseLife()
        {
            _lives = Mathf.Max(0, _lives - 1);
            LivesChanged?.Invoke(_lives);
            ExplosionManager.Instance?.Spawn(transform.position, _shipColor, 1.4f);
            AudioManager.Instance?.PlayExplosion();

            if (_lives <= 0)
            {
                _controllable = false;
                _renderer.enabled = false;
                _shieldRenderer.enabled = false;
                PlayerDied?.Invoke();
            }
            else
            {
                ResetForNewLife(false);
            }
        }

        private void ResetForNewLife(bool fullReset)
        {
            _health = MaxHealth;
            transform.position = new Vector3(0f, -_config.HalfHeight + 1f, 0f);
            _renderer.enabled = true;
            _invulnerabilityTimer = _config.PlayerInvulnerabilityAfterHit;
            _flickerTimer = 0f;
            _fireTimer = 0f;

            if (fullReset)
            {
                _shieldTimer = 0f;
                _rapidFireTimer = 0f;
                _spreadShotTimer = 0f;
                _scoreMultiplierTimer = 0f;
            }

            HealthChanged?.Invoke(_health, MaxHealth);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            CollisionHandler.HandlePlayerContact(this, other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            // Allows continuous power-up/contact resolution if overlap persists across frames.
            CollisionHandler.HandlePlayerContact(this, other);
        }
    }
}
