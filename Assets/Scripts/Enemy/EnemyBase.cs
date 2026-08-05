using System;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Audio;
using SpaceShooter.Utilities;
using SpaceShooter.Weapons;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Abstract base for all enemies. Handles health, scoring, shooting cadence,
    /// death (explosion VFX, power-up drop chance, score award) and collision
    /// with player bullets / the player. Concrete enemies implement <see cref="Move"/>.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] protected int maxHp = 1;
        [SerializeField] protected int scoreValue = 100;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected float shootInterval = 2f;

        [Header("Drops")]
        [Range(0f, 1f)]
        [SerializeField] protected float powerUpDropChance = 0.15f;
        [SerializeField] protected bool guaranteedDrop = false;

        [Header("Contact")]
        [SerializeField] protected int contactDamage = 1;

        /// <summary>Fired when this enemy dies (score value).</summary>
        public event Action<EnemyBase> OnEnemyDied;

        protected int CurrentHp;
        protected float ShootTimer;
        protected BulletPattern Pattern;
        protected Transform PlayerTarget;
        protected float DifficultyMultiplier = 1f;

        protected SpriteRenderer Renderer;
        protected Collider2D Collider;
        protected Camera Cam;

        private string _poolKey;
        private bool _dead;

        public virtual bool IsBoss => false;
        public int ScoreValue => scoreValue;

        protected virtual void Awake()
        {
            Renderer = GetComponent<SpriteRenderer>();
            Collider = GetComponent<Collider2D>();
            Collider.isTrigger = true;

            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            Renderer.sortingOrder = 2;
            AssignSprite();
            SetupPattern();
        }

        /// <summary>Each concrete enemy assigns its procedurally-generated sprite.</summary>
        protected abstract void AssignSprite();

        /// <summary>Optionally create and configure a bullet pattern.</summary>
        protected virtual void SetupPattern() { }

        /// <summary>Concrete movement behaviour, called each frame while playing.</summary>
        protected abstract void Move();

        /// <summary>Called on spawn to (re)initialise pooled state.</summary>
        public virtual void Initialise(string poolKey, Transform player, float difficultyMultiplier)
        {
            _poolKey = poolKey;
            PlayerTarget = player;
            DifficultyMultiplier = Mathf.Max(0.5f, difficultyMultiplier);
            CurrentHp = maxHp;
            ShootTimer = shootInterval;
            _dead = false;
            gameObject.tag = IsBoss ? Constants.TagBoss : Constants.TagEnemy;
            if (Renderer != null) Renderer.color = Color.white;
            if (Collider != null) Collider.enabled = true;
            if (Pattern != null) Pattern.PlayerTarget = player;
        }

        protected virtual void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
                return;
            if (_dead) return;

            if (Cam == null) Cam = Camera.main;

            Move();
            HandleShooting();
            CheckOffScreen();
        }

        protected virtual void HandleShooting()
        {
            if (Pattern == null) return;
            ShootTimer -= Time.deltaTime;
            if (ShootTimer <= 0f)
            {
                ShootTimer = shootInterval;
                Pattern.Fire(transform.position, ObjectPool.Instance);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(Constants.SfxEnemyShoot, 0.5f);
            }
        }

        protected virtual void CheckOffScreen()
        {
            if (Cam == null || !Cam.orthographic) return;
            float halfH = Cam.orthographicSize;
            float bottom = Cam.transform.position.y - halfH - 2f;
            if (transform.position.y < bottom)
                Despawn();   // Left the play area – recycle without reward.
        }

        public virtual void TakeDamage(int amount)
        {
            if (_dead) return;
            CurrentHp -= amount;
            FlashHit();
            if (CurrentHp <= 0)
                Die();
        }

        protected virtual void FlashHit()
        {
            if (Renderer != null)
                Renderer.color = Color.Lerp(Color.white, Color.red, 0.5f);
            CancelInvoke(nameof(RestoreColour));
            Invoke(nameof(RestoreColour), 0.06f);
        }

        private void RestoreColour()
        {
            if (Renderer != null) Renderer.color = Color.white;
        }

        /// <summary>Instantly destroyed by a bomb – awards score, drops nothing.</summary>
        public virtual void KillByBomb()
        {
            if (_dead) return;
            AwardScore();
            SpawnExplosion();
            Finish();
        }

        protected virtual void Die()
        {
            if (_dead) return;
            _dead = true;

            AwardScore();
            SpawnExplosion();
            TryDropPowerUp();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(Constants.SfxExplosion);

            OnEnemyDied?.Invoke(this);
            Recycle();
        }

        protected void AwardScore()
        {
            if (Scoring.ScoreManager.Instance != null)
                Scoring.ScoreManager.Instance.AddScore(scoreValue);
        }

        protected void SpawnExplosion()
        {
            if (ObjectPool.Instance == null) return;
            var vfx = ObjectPool.Instance.Acquire(Constants.PoolExplosion, transform.position, Quaternion.identity);
            if (vfx != null)
            {
                var expl = vfx.GetComponent<ExplosionVFX>();
                if (expl == null) expl = vfx.AddComponent<ExplosionVFX>();
                expl.Play();
            }
        }

        protected void TryDropPowerUp()
        {
            if (ObjectPool.Instance == null) return;
            bool drop = guaranteedDrop || UnityEngine.Random.value < powerUpDropChance;
            if (!drop) return;

            string key = RandomPowerUpPoolKey();
            var go = ObjectPool.Instance.Acquire(key, transform.position, Quaternion.identity);
            // Power-up initialises itself on enable; nothing more to do here.
        }

        protected string RandomPowerUpPoolKey()
        {
            string[] keys =
            {
                Constants.PoolPowerUpShield,
                Constants.PoolPowerUpRapidFire,
                Constants.PoolPowerUpTripleShot,
                Constants.PoolPowerUpBomb,
                Constants.PoolPowerUpSpeed
            };
            return keys[UnityEngine.Random.Range(0, keys.Length)];
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_dead) return;
            // Direct collision with the player damages the player and this enemy.
            if (other.CompareTag(Constants.TagPlayer))
            {
                var health = other.GetComponent<Player.PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage();
                    if (!IsBoss)
                        TakeDamage(contactDamage >= CurrentHp ? CurrentHp : contactDamage);
                }
            }
        }

        /// <summary>Quietly remove without reward (e.g. flew off-screen).</summary>
        protected virtual void Despawn()
        {
            if (_dead) return;
            _dead = true;
            OnEnemyDied?.Invoke(this);
            Recycle();
        }

        protected void Recycle()
        {
            Finish();
        }

        private void Finish()
        {
            _dead = true;
            if (Collider != null) Collider.enabled = false;
            if (ObjectPool.Instance != null && !string.IsNullOrEmpty(_poolKey))
                ObjectPool.Instance.Release(_poolKey, gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
