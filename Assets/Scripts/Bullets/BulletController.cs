using UnityEngine;

namespace SpaceShooter
{
    public enum BulletOwner
    {
        Player,
        Enemy
    }

    /// <summary>
    /// Handles projectile movement, lifetime and collision damage.
    /// Works for both player and enemy bullets via the <see cref="owner"/> field.
    /// Requires a Rigidbody2D (set to Kinematic) and a trigger Collider2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class BulletController : MonoBehaviour
    {
        [Header("Bullet Settings")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float lifeTime = 4f;
        [SerializeField] private BulletOwner owner = BulletOwner.Player;
        [SerializeField] private GameObject hitEffect;

        private Vector2 direction = Vector2.up;
        private float timer;

        public BulletOwner Owner => owner;

        public void Configure(BulletOwner bulletOwner, Vector2 dir, float bulletSpeed, float bulletDamage)
        {
            owner = bulletOwner;
            direction = dir.normalized;
            speed = bulletSpeed;
            damage = bulletDamage;
        }

        private void OnEnable()
        {
            timer = lifeTime;
        }

        private void Update()
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Player bullets only hit enemies, enemy bullets only hit the player.
            if (owner == BulletOwner.Player)
            {
                if (other.CompareTag("Enemy"))
                {
                    ApplyDamage(other);
                    Despawn();
                }
            }
            else // Enemy bullet
            {
                if (other.CompareTag("Player"))
                {
                    ApplyDamage(other);
                    Despawn();
                }
            }
        }

        private void ApplyDamage(Collider2D other)
        {
            HealthSystem health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
        }

        private void Despawn()
        {
            Destroy(gameObject);
        }
    }
}
