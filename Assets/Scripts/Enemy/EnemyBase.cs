using System.Collections;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Pickups;
using SpaceShooter.Player;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Abstract base for all enemies. Handles health, damage flash, scoring,
    /// death (VFX/SFX, power-up drop, wave notification) and collision with the player.
    /// Subclasses implement Move() for their unique behaviour.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        #region Fields
        [Header("Base Stats")]
        [SerializeField] protected int _maxHealth = 2;
        [SerializeField] protected int _currentHealth;
        [SerializeField] protected int _scoreValue = 100;
        [SerializeField] protected float _moveSpeed = 3f;

        [Header("References")]
        [SerializeField] protected EnemyShooter _shooter;

        protected SpriteRenderer _renderer;
        protected Color _baseColor = Color.white;
        protected bool _isDead;

        // Formation info supplied by WaveManager.
        protected int _formationIndex;
        protected int _formationTotal = 1;
        protected float _difficultyMultiplier = 1f;
        #endregion

        #region Properties
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public int ScoreValue => _scoreValue;
        #endregion

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            if (_shooter == null) _shooter = GetComponent<EnemyShooter>();
            if (_renderer != null) _baseColor = _renderer.color;
            gameObject.tag = GameConstants.TAG_ENEMY;
            gameObject.layer = GameConstants.LAYER_ID_ENEMY;
        }

        protected virtual void OnEnable()
        {
            _isDead = false;
        }

        protected virtual void Start()
        {
            InitStats();
            if (_shooter != null) _shooter.StartShooting();
        }

        protected virtual void Update()
        {
            if (_isDead) return;
            Move();
            CullIfOffscreen();
        }
        #endregion

        #region Setup
        /// <summary>Applies difficulty scaling and formation slot from the wave manager.</summary>
        public virtual void Configure(float difficultyMultiplier, int formationIndex, int formationTotal)
        {
            _difficultyMultiplier = Mathf.Max(1f, difficultyMultiplier);
            _formationIndex = formationIndex;
            _formationTotal = Mathf.Max(1, formationTotal);
        }

        /// <summary>Sets initial health/score using difficulty scaling. Called on Start.</summary>
        protected virtual void InitStats()
        {
            _maxHealth = Mathf.CeilToInt(_maxHealth * _difficultyMultiplier);
            _currentHealth = _maxHealth;
            _scoreValue = Mathf.CeilToInt(_scoreValue * _difficultyMultiplier);
        }
        #endregion

        #region Movement (abstract)
        /// <summary>Per-frame movement logic implemented by each enemy type.</summary>
        protected abstract void Move();

        /// <summary>Removes the enemy if it drifts far past the bottom of the screen.</summary>
        protected virtual void CullIfOffscreen()
        {
            if (transform.position.y < GameConstants.CAMERA_BOTTOM - 2f)
            {
                // Count as removed but no score; still notify wave manager.
                _isDead = true;
                if (_shooter != null) _shooter.StopShooting();
                if (WaveManager.Instance != null) WaveManager.Instance.EnemyKilled();
                Destroy(gameObject);
            }
        }
        #endregion

        #region Damage & Death
        /// <summary>Applies damage; flashes red and dies at zero health.</summary>
        public virtual void TakeDamage(int dmg)
        {
            if (_isDead || dmg <= 0) return;
            _currentHealth -= dmg;

            if (_renderer != null)
            {
                StopCoroutine(nameof(FlashRed));
                StartCoroutine(FlashRed());
            }

            if (_currentHealth <= 0)
                Die();
        }

        private IEnumerator FlashRed()
        {
            if (_renderer == null) yield break;
            _renderer.color = Color.red;
            yield return new WaitForSeconds(0.06f);
            if (_renderer != null) _renderer.color = _baseColor;
        }

        /// <summary>Handles death: score, VFX/SFX, power-up drop, wave notify, cleanup.</summary>
        protected virtual void Die()
        {
            if (_isDead) return;
            _isDead = true;

            if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(_scoreValue);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.Explosion, Random.Range(0.9f, 1.1f));

            SpawnExplosion();
            PowerUpSpawner.Spawn(transform.position);

            if (_shooter != null) _shooter.StopShooting();
            if (WaveManager.Instance != null) WaveManager.Instance.EnemyKilled();

            Destroy(gameObject);
        }

        /// <summary>Spawns a short particle explosion burst at this enemy's position.</summary>
        protected void SpawnExplosion()
        {
            Sprite particle = Utilities.SpriteGenerator.GenerateStar();
            int count = 12;
            for (int i = 0; i < count; i++)
            {
                GameObject p = new GameObject("EnemyExplosionParticle");
                p.transform.position = transform.position;
                SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
                sr.sprite = particle;
                sr.color = new Color(1f, Random.Range(0.3f, 0.7f), 0.1f, 1f);
                sr.sortingOrder = 40;
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(1.5f, 4.5f);
                ExplosionParticle ep = p.AddComponent<ExplosionParticle>();
                ep.Launch(vel, Random.Range(0.3f, 0.6f));
            }
        }
        #endregion

        #region Collision
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(GameConstants.TAG_PLAYER))
            {
                PlayerHealth hp = other.GetComponent<PlayerHealth>();
                if (hp != null) hp.TakeDamage(1);
                TakeDamage(1);
            }
        }
        #endregion
    }
}
