using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Pooled projectile. Moves in a direction, damages the correct target tag and
    /// returns itself to the pool on hit or after a lifetime. Supports homing.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Bullet : MonoBehaviour
    {
        [Header("Runtime (set on spawn)")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float lifetime = 5f;

        [Header("Homing")]
        [SerializeField] private bool homing = false;
        [SerializeField] private float homingStrength = 3f;

        [Header("Colors")]
        [SerializeField] private Color playerColor = Color.cyan;
        [SerializeField] private Color enemyColor = new Color(1f, 0.4f, 0.1f);

        private Vector2 direction = Vector2.up;
        private string targetTag = "Enemy";
        private string poolTag;
        private float timer;
        private SpriteRenderer sr;
        private Transform homingTarget;

        public int Damage => damage;
        public string PoolTag => poolTag;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        /// <summary>Configure a freshly pooled bullet.</summary>
        public void Initialize(Vector2 dir, float spd, int dmg, string targetTag, string poolTag, bool homing = false)
        {
            this.direction = dir.normalized;
            this.speed = spd;
            this.damage = dmg;
            this.targetTag = targetTag;
            this.poolTag = poolTag;
            this.homing = homing;
            this.timer = lifetime;

            if (sr != null)
            {
                sr.color = targetTag == "Enemy" ? playerColor : enemyColor;
            }

            // Orient sprite toward travel direction.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (homing)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                homingTarget = player != null ? player.transform : null;
            }
        }

        private void Update()
        {
            if (homing && homingTarget != null && homingTarget.gameObject.activeInHierarchy)
            {
                Vector2 toTarget = ((Vector2)homingTarget.position - (Vector2)transform.position).normalized;
                direction = Vector2.Lerp(direction, toTarget, homingStrength * Time.deltaTime).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            transform.position += (Vector3)(direction * speed * Time.deltaTime);

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(targetTag)) return;

            if (targetTag == "Enemy")
            {
                var enemy = other.GetComponent<SpaceShooter.Enemy.EnemyBase>();
                if (enemy != null) enemy.TakeDamage(damage);
            }
            else if (targetTag == "Player")
            {
                var health = other.GetComponent<SpaceShooter.Player.PlayerHealth>();
                if (health != null) health.TakeDamage(damage);
            }

            ReturnToPool();
        }

        public void ReturnToPool()
        {
            if (ObjectPool.Instance != null && !string.IsNullOrEmpty(poolTag))
            {
                ObjectPool.Instance.ReturnObject(poolTag, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
