using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Controls bullet movement and collision
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [Header("Bullet Settings")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private bool isPlayerBullet = true;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject hitEffectPrefab;
        
        private Vector2 direction = Vector2.up;
        private Rigidbody2D rb;
        
        public int Damage => damage;
        public bool IsPlayerBullet => isPlayerBullet;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        private void Start()
        {
            if (rb != null)
            {
                rb.velocity = direction * speed;
            }
            
            Destroy(gameObject, lifetime);
        }
        
        private void Update()
        {
            if (rb == null)
            {
                transform.Translate(direction * speed * Time.deltaTime, Space.World);
            }
        }
        
        public void SetDirection(Vector2 newDirection)
        {
            direction = newDirection.normalized;
            if (rb != null)
            {
                rb.velocity = direction * speed;
            }
            
            // Rotate bullet to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
            if (rb != null)
            {
                rb.velocity = direction * speed;
            }
        }
        
        public void SetDamage(int newDamage)
        {
            damage = newDamage;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPlayerBullet && other.CompareTag("Enemy"))
            {
                EnemyBase enemy = other.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                
                SpawnHitEffect();
                Destroy(gameObject);
            }
            else if (!isPlayerBullet && other.CompareTag("Player"))
            {
                // Player collision handled in PlayerController
            }
            else if (other.CompareTag("Boundary"))
            {
                Destroy(gameObject);
            }
        }
        
        private void SpawnHitEffect()
        {
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }
    }
}
