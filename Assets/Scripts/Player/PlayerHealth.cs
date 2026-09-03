using System;
using System.Collections;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Handles player health, shield, invincibility frames and death.
    /// Broadcasts health/shield changes for the HUD.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerHealth : MonoBehaviour
    {
        #region Events
        /// <summary>Fired when health changes. Args: current, max.</summary>
        public static event Action<int, int> OnHealthChanged;
        /// <summary>Fired when shield changes. Args: current, max.</summary>
        public static event Action<int, int> OnShieldChanged;
        /// <summary>Fired when the player dies.</summary>
        public static event Action OnPlayerDied;
        #endregion

        #region Fields
        [Header("Health")]
        [SerializeField] private int _maxHealth = GameConstants.PLAYER_MAX_HEALTH;
        [SerializeField] private int _currentHealth;

        [Header("Shield")]
        [SerializeField] private int _maxShield = GameConstants.PLAYER_MAX_SHIELD;
        [SerializeField] private int _shieldHP;

        [Header("State")]
        [SerializeField] private bool _isInvincible;

        private SpriteRenderer _renderer;
        private bool _isDead;
        #endregion

        #region Properties
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public int ShieldHP => _shieldHP;
        public int MaxShield => _maxShield;
        public bool IsInvincible => _isInvincible;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _currentHealth = _maxHealth;
            _shieldHP = 0;
            _isDead = false;
            _isInvincible = false;
            BroadcastAll();
        }

        private void Start()
        {
            BroadcastAll();
        }
        #endregion

        #region Damage
        /// <summary>Applies damage; shield absorbs first, then health. Triggers i-frames.</summary>
        public void TakeDamage(int dmg)
        {
            if (_isDead || _isInvincible || dmg <= 0) return;

            if (_shieldHP > 0)
            {
                _shieldHP = Mathf.Max(0, _shieldHP - dmg);
                OnShieldChanged?.Invoke(_shieldHP, _maxShield);
            }
            else
            {
                _currentHealth = Mathf.Max(0, _currentHealth - dmg);
                OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.PlayerHit);

            if (SpaceShooter.Environment.CameraShake.Instance != null)
                SpaceShooter.Environment.CameraShake.Instance.Shake(0.2f, 0.15f);

            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvincibilityBlink());
            }
        }
        #endregion

        #region Healing / Shield
        /// <summary>Restores health up to the maximum.</summary>
        public void Heal(int amount)
        {
            if (_isDead) return;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        /// <summary>Refills the shield to maximum.</summary>
        public void AddShield()
        {
            _shieldHP = _maxShield;
            OnShieldChanged?.Invoke(_shieldHP, _maxShield);
        }
        #endregion

        #region Invincibility
        private IEnumerator InvincibilityBlink()
        {
            _isInvincible = true;
            float elapsed = 0f;
            float blink = 0.12f;
            while (elapsed < GameConstants.PLAYER_INVINCIBILITY_TIME)
            {
                if (_renderer != null)
                    _renderer.enabled = !_renderer.enabled;
                yield return new WaitForSeconds(blink);
                elapsed += blink;
            }
            if (_renderer != null) _renderer.enabled = true;
            _isInvincible = false;
        }
        #endregion

        #region Death
        /// <summary>Kills the player: explosion VFX + SFX, then game over.</summary>
        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            SpawnExplosionBurst(transform.position);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.Explosion);

            OnPlayerDied?.Invoke();

            if (GameManager.Instance != null)
                GameManager.Instance.TriggerGameOver();

            if (_renderer != null) _renderer.enabled = false;
            // Disable player controls/collider; leave object for GameManager cleanup.
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        private void SpawnExplosionBurst(Vector3 pos)
        {
            // Procedural particle burst using tiny quads (SpriteGenerator star sprite).
            Sprite particle = SpriteGenerator.GenerateStar();
            int count = 18;
            for (int i = 0; i < count; i++)
            {
                GameObject p = new GameObject("ExplosionParticle");
                p.transform.position = pos;
                SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
                sr.sprite = particle;
                sr.color = new Color(1f, Random.Range(0.4f, 0.8f), 0.1f, 1f);
                sr.sortingOrder = 50;
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(2f, 6f);
                ExplosionParticle ep = p.AddComponent<ExplosionParticle>();
                ep.Launch(vel, Random.Range(0.4f, 0.8f));
            }
        }
        #endregion
    }

    /// <summary>Self-contained fading particle used for explosion bursts.</summary>
    public class ExplosionParticle : MonoBehaviour
    {
        private Vector2 _velocity;
        private float _life;
        private float _maxLife;
        private SpriteRenderer _sr;

        /// <summary>Launches the particle with a velocity and lifetime.</summary>
        public void Launch(Vector2 velocity, float life)
        {
            _velocity = velocity;
            _maxLife = life;
            _life = 0f;
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _life += Time.deltaTime;
            transform.Translate((Vector3)(_velocity * Time.deltaTime), Space.World);
            _velocity *= 0.94f;

            if (_sr != null)
            {
                float t = 1f - Mathf.Clamp01(_life / _maxLife);
                Color c = _sr.color;
                c.a = t;
                _sr.color = c;
                transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 1.2f, 1f - t);
            }

            if (_life >= _maxLife) Destroy(gameObject);
        }
    }
}
