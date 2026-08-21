using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Systems;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Abstract base for all enemies. Handles health, damage, death, scoring,
    /// power-up drops and pooling. Subclasses implement Move/Shoot behaviour.
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] protected int maxHealth = 30;
        [SerializeField] protected float speed = 3f;
        [SerializeField] protected int scoreValue = 100;
        [SerializeField] protected int contactDamage = 25;

        [Header("Drops")]
        [SerializeField] protected float powerUpDropChance = 0.2f;
        [SerializeField] protected string[] powerUpPoolTags = {
            "PowerUp_Speed", "PowerUp_Rapid", "PowerUp_Triple",
            "PowerUp_Shield", "PowerUp_Health", "PowerUp_Bomb"
        };

        [Header("Pooling / Effects")]
        [SerializeField] protected string poolTag = "EnemyA";
        [SerializeField] protected string explosionPoolTag = "Explosion";
        [SerializeField] protected float offScreenPadding = 1.5f;

        protected int currentHealth;
        protected Transform player;
        protected Camera cam;
        protected bool isDead;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public bool IsBoss { get; protected set; }

        protected virtual void Awake()
        {
            cam = Camera.main;
        }

        protected virtual void OnEnable()
        {
            currentHealth = maxHealth;
            isDead = false;
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            player = p != null ? p.transform : null;
        }

        /// <summary>Apply difficulty scaling from the wave manager.</summary>
        public virtual void ApplyDifficulty(float healthMultiplier, float speedBonus)
        {
            maxHealth = Mathf.RoundToInt(maxHealth * healthMultiplier);
            speed += speedBonus;
            currentHealth = maxHealth;
        }

        protected virtual void Update()
        {
            if (isDead) return;
            Move();
            Shoot();
            CheckBounds();
        }

        // Abstract behaviour hooks.
        protected abstract void Move();
        protected abstract void Shoot();

        public virtual void TakeDamage(int amount)
        {
            if (isDead) return;
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            if (isDead) return;
            isDead = true;

            SpawnExplosion();

            if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(scoreValue);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Explosion");

            TryDropPowerUp();
            NotifySpawner();
            ReturnToPool();
        }

        protected void SpawnExplosion()
        {
            if (ObjectPool.Instance != null && ObjectPool.Instance.HasPool(explosionPoolTag))
            {
                GameObject fx = ObjectPool.Instance.GetObject(explosionPoolTag, transform.position, Quaternion.identity);
                var effect = fx != null ? fx.GetComponent<Effects.ExplosionEffect>() : null;
                if (effect != null) effect.Play(IsBoss ? 3f : 1f, false, explosionPoolTag);
            }
        }

        protected void TryDropPowerUp()
        {
            if (powerUpPoolTags == null || powerUpPoolTags.Length == 0) return;
            if (Random.value > powerUpDropChance) return;

            string tag = powerUpPoolTags[Random.Range(0, powerUpPoolTags.Length)];
            if (ObjectPool.Instance != null && ObjectPool.Instance.HasPool(tag))
            {
                ObjectPool.Instance.GetObject(tag, transform.position, Quaternion.identity);
            }
        }

        protected void NotifySpawner()
        {
            if (EnemySpawner.Instance != null) EnemySpawner.Instance.OnEnemyDestroyed(this);
        }

        protected virtual void CheckBounds()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            Vector3 bottom = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, Mathf.Abs(cam.transform.position.z)));
            if (transform.position.y < bottom.y - offScreenPadding)
            {
                // Left the screen: count as removed but no score.
                NotifySpawner();
                ReturnToPool();
            }
        }

        protected void ReturnToPool()
        {
            if (ObjectPool.Instance != null && ObjectPool.Instance.HasPool(poolTag))
            {
                ObjectPool.Instance.ReturnObject(poolTag, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        protected bool IsOffScreenTop()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return false;
            Vector3 top = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, Mathf.Abs(cam.transform.position.z)));
            return transform.position.y > top.y;
        }

        protected float ScreenHalfWidth()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return 8f;
            Vector3 right = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(cam.transform.position.z)));
            return right.x;
        }

        /// <summary>Used by the bomb power-up to clear the screen.</summary>
        public virtual void KillFromBomb()
        {
            TakeDamage(currentHealth + 9999);
        }
    }
}
