using UnityEngine;

namespace SpaceShooter.Weapons
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float lifeTime = 4f;
        [SerializeField] private bool destroyOnHit = true;

        private Vector2 direction = Vector2.up;
        private bool fromPlayer;

        public float Damage => damage;
        public bool FromPlayer => fromPlayer;

        private void OnEnable()
        {
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.Translate(direction * (speed * Time.deltaTime), Space.World);
        }

        public void Initialize(Vector2 bulletDirection, bool spawnedByPlayer, float overrideDamage = -1f)
        {
            direction = bulletDirection.normalized;
            fromPlayer = spawnedByPlayer;
            if (overrideDamage > 0f)
            {
                damage = overrideDamage;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (fromPlayer && other.CompareTag("Player"))
            {
                return;
            }

            if (!fromPlayer && other.CompareTag("Enemy"))
            {
                return;
            }

            if (other.isTrigger && !other.CompareTag("Bounds"))
            {
                return;
            }

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}
