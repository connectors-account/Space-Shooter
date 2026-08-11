using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// A straight-moving projectile. Bullets know who fired them so player bullets
    /// only hurt enemies and enemy bullets only hurt the player.
    /// Auto-destroys when leaving the screen or after a max lifetime.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        public enum Owner
        {
            Player,
            Enemy
        }

        [Tooltip("Seconds before the bullet self-destructs if it never hits anything.")]
        [SerializeField] private float maxLifetime = 5f;

        [Tooltip("Extra world-space margin beyond the screen before despawning.")]
        [SerializeField] private float offscreenMargin = 1f;

        private Vector2 velocity;
        private Owner owner;
        private Camera mainCamera;
        private float despawnTime;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        /// <summary>Configure direction/speed and which side fired this bullet.</summary>
        public void Initialize(Vector2 initialVelocity, Owner bulletOwner)
        {
            velocity = initialVelocity;
            owner = bulletOwner;
            despawnTime = Time.time + maxLifetime;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = velocity;
            }
        }

        private void Update()
        {
            // Fallback movement in case Rigidbody2D velocity is not used.
            transform.Translate(velocity * Time.deltaTime, Space.World);

            if (Time.time >= despawnTime)
            {
                Destroy(gameObject);
                return;
            }

            if (mainCamera != null)
            {
                Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
                float marginX = offscreenMargin / Mathf.Max(1f, mainCamera.orthographicSize * mainCamera.aspect);
                float marginY = offscreenMargin / Mathf.Max(1f, mainCamera.orthographicSize);
                if (viewPos.y > 1f + marginY || viewPos.y < -marginY ||
                    viewPos.x > 1f + marginX || viewPos.x < -marginX)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (owner == Owner.Player)
            {
                if (other.CompareTag("Enemy"))
                {
                    EnemyController enemy = other.GetComponent<EnemyController>();
                    if (enemy != null)
                    {
                        enemy.DestroyEnemy(true);
                    }
                    Destroy(gameObject);
                }
            }
            else // Enemy bullet
            {
                if (other.CompareTag("Player"))
                {
                    PlayerController player = other.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        player.TakeDamage();
                    }
                    Destroy(gameObject);
                }
            }
        }
    }
}
