using UnityEngine;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Generic projectile logic for player and enemy bullets.
    /// </summary>
    public class BulletController : MonoBehaviour
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private Vector2 direction = Vector2.up;

        private bool initialized;
        private bool firedByPlayer = true;

        public int Damage => damage;
        public bool FiredByPlayer => firedByPlayer;

        private void Start()
        {
            Destroy(gameObject, lifetime);
            if (!initialized)
            {
                direction = direction.normalized;
                ApplyRotationFromDirection();
            }
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x) > 15f || Mathf.Abs(transform.position.y) > 10f)
            {
                Destroy(gameObject);
            }
        }

        public void Configure(Vector2 moveDirection, float moveSpeed, int bulletDamage, bool fromPlayer)
        {
            direction = moveDirection.sqrMagnitude < 0.0001f ? Vector2.up : moveDirection.normalized;
            speed = Mathf.Max(0.1f, moveSpeed);
            damage = Mathf.Max(1, bulletDamage);
            firedByPlayer = fromPlayer;
            initialized = true;
            ApplyRotationFromDirection();
        }

        public void SetDirection(Vector2 moveDirection)
        {
            direction = moveDirection.sqrMagnitude < 0.0001f ? Vector2.up : moveDirection.normalized;
            ApplyRotationFromDirection();
        }

        public void SetSpeed(float moveSpeed)
        {
            speed = Mathf.Max(0.1f, moveSpeed);
        }

        public void SetDamage(int bulletDamage)
        {
            damage = Mathf.Max(1, bulletDamage);
        }

        private void ApplyRotationFromDirection()
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
