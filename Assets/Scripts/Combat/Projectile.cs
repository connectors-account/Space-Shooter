using SpaceShooter.PowerUps;
using UnityEngine;

namespace SpaceShooter.Combat
{
    public enum ProjectileOwner
    {
        Player,
        Enemy
    }

    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float lifeTime = 4f;
        [SerializeField] private ProjectileOwner owner = ProjectileOwner.Player;

        private Vector2 direction = Vector2.up;

        private void OnEnable()
        {
            CancelInvoke(nameof(SelfDestruct));
            Invoke(nameof(SelfDestruct), lifeTime);
        }

        private void Update()
        {
            transform.Translate(direction * (speed * Time.deltaTime), Space.World);
        }

        public void Initialize(Vector2 newDirection, ProjectileOwner projectileOwner, int projectileDamage, float projectileSpeed)
        {
            direction = newDirection.normalized;
            owner = projectileOwner;
            damage = projectileDamage;
            speed = projectileSpeed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (owner == ProjectileOwner.Player && other.CompareTag("Player"))
            {
                return;
            }

            if (owner == ProjectileOwner.Enemy && other.CompareTag("Enemy"))
            {
                return;
            }

            if (owner == ProjectileOwner.Enemy && other.TryGetComponent(out PlayerPowerUpController powerUpController))
            {
                bool absorbed = powerUpController.TryAbsorbIncomingDamage(damage);
                if (absorbed)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            if (other.TryGetComponent(out Health health))
            {
                health.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            if (other.CompareTag("Bounds"))
            {
                Destroy(gameObject);
            }
        }

        private void SelfDestruct()
        {
            Destroy(gameObject);
        }
    }
}
