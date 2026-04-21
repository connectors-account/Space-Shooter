using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Moves a bullet in a direction, applies damage on hit, and self-destructs when out of bounds.
    /// Works for both player and enemy projectiles.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BulletController : MonoBehaviour
    {
        [Header("Bullet Stats")]
        [SerializeField] private float speed = 14f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifetime = 3f;

        [Header("Runtime (Debug)")]
        [SerializeField] private DamageTeam ownerTeam = DamageTeam.Player;
        [SerializeField] private Vector2 moveDirection = Vector2.up;

        private bool initialized;

        public void Initialize(DamageTeam team, Vector2 direction, int bulletDamage, float bulletSpeed)
        {
            ownerTeam = team;
            moveDirection = direction.normalized;
            damage = Mathf.Max(1, bulletDamage);
            speed = Mathf.Max(0.1f, bulletSpeed);
            initialized = true;

            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (!initialized)
            {
                // Safe default for inspector testing.
                Initialize(ownerTeam, moveDirection, damage, speed);
            }

            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

            if (Mathf.Abs(transform.position.x) > 14f || Mathf.Abs(transform.position.y) > 9f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null)
            {
                return;
            }

            bool hitApplied = CollisionHandler.TryApplyDamage(other, damage, ownerTeam);
            if (hitApplied)
            {
                AudioManager.Instance?.PlaySfx(AudioManager.SfxType.Hit);
                Destroy(gameObject);
                return;
            }

            // Bullets should still be removed if they hit environment bounds.
            if (other.CompareTag("Boundary"))
            {
                Destroy(gameObject);
            }
        }
    }
}
