using UnityEngine;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Controls bullet movement and lifetime.
    /// Used for both player and enemy bullets.
    /// </summary>
    public class BulletController : MonoBehaviour
    {
        [Header("Bullet Settings")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private Vector2 direction = Vector2.up;

        /// <summary>How much damage this bullet deals on hit.</summary>
        public int Damage => damage;

        /// <summary>Set bullet direction (normalized).</summary>
        public void SetDirection(Vector2 dir)
        {
            direction = dir.normalized;
        }

        /// <summary>Set bullet speed.</summary>
        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }

        /// <summary>Set bullet damage.</summary>
        public void SetDamage(int newDamage)
        {
            damage = newDamage;
        }

        private void Start()
        {
            // Auto-destroy after lifetime to prevent memory leaks
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            // Move bullet in its direction
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            // Destroy if out of screen bounds (extra safety)
            if (Mathf.Abs(transform.position.x) > 12f || Mathf.Abs(transform.position.y) > 8f)
            {
                Destroy(gameObject);
            }
        }
    }
}
