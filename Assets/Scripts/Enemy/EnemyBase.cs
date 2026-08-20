using System;
using System.Collections;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Abstract base for all enemies. Handles health/damage, hit flashing, death (score,
    /// explosion, power-up drop chance) and defers movement + firing to subclasses.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Stats")]
        public int maxHealth = 30;
        public float speed = 2f;
        public int scoreValue = 100;
        public int contactDamage = 20;

        [Header("Firing")]
        public GameObject enemyBulletPrefab;
        public float bulletSpeed = 6f;
        public int bulletDamage = 10;

        [Header("Drops")]
        [Range(0f, 1f)] public float powerUpDropChance = 0.2f;
        public GameObject[] powerUpPrefabs; // assigned by spawner

        public int CurrentHealth { get; protected set; }
        public event Action<EnemyBase> OnDeath;

        protected SpriteRenderer Sr;
        protected Transform PlayerTransform;
        private Color _baseColor;
        private bool _isDead;

        protected virtual void Awake()
        {
            Sr = GetComponent<SpriteRenderer>();
            var col = GetComponent<Collider2D>();
            if (col == null)
            {
                var box = gameObject.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
            }
            else col.isTrigger = true;

            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;

            gameObject.tag = "Enemy";
        }

        protected virtual void Start()
        {
            CurrentHealth = maxHealth;
            if (Sr != null && Sr.sprite == null) Sr.sprite = CreateSprite();
            if (Sr != null) _baseColor = Sr.color;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) PlayerTransform = player.transform;
        }

        protected virtual void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
            Move();
            FirePattern();

            // Despawn if it drifts far below the screen.
            if (ScreenBounds.Instance != null &&
                transform.position.y < ScreenBounds.Instance.Bottom - 2f)
            {
                Destroy(gameObject);
            }
        }

        // Subclasses implement behaviour.
        protected abstract void Move();
        protected abstract void FirePattern();

        /// <summary>Runtime sprite used when none is assigned in the prefab. Override per type.</summary>
        protected virtual Sprite CreateSprite()
        {
            return SpriteGenerator.CreateShip(Color.gray, Color.white);
        }

        public virtual void TakeDamage(int amount)
        {
            if (_isDead) return;
            CurrentHealth -= amount;
            if (Sr != null) StartCoroutine(FlashRed());

            if (CurrentHealth <= 0) Die();
        }

        protected IEnumerator FlashRed()
        {
            if (Sr == null) yield break;
            Sr.color = Color.red;
            yield return new WaitForSeconds(0.06f);
            if (!_isDead && Sr != null) Sr.color = _baseColor;
        }

        protected virtual void Die()
        {
            if (_isDead) return;
            _isDead = true;

            if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(scoreValue);
            if (GameManager.Instance != null && ScoreManager.Instance != null)
                GameManager.Instance.SyncScore(ScoreManager.Instance.CurrentScore);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("explosion");

            TryDropPowerUp();
            OnDeath?.Invoke(this);
            Destroy(gameObject);
        }

        private void TryDropPowerUp()
        {
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
            if (UnityEngine.Random.value > powerUpDropChance) return;

            int i = UnityEngine.Random.Range(0, powerUpPrefabs.Length);
            if (powerUpPrefabs[i] != null)
                Instantiate(powerUpPrefabs[i], transform.position, Quaternion.identity);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var hp = other.GetComponent<Player.PlayerHealth>();
                if (hp != null) hp.TakeDamage(contactDamage);
                Die();
            }
        }
    }
}
