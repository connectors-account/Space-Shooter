using System.Collections;
using UnityEngine;
using SpaceShooter.Utilities;
using SpaceShooter.Player;

namespace SpaceShooter.Bullets
{
    /// <summary>
    /// Enemy bullet. Travels along its local up axis (which is pointed downward / toward
    /// the player when fired), damages the player on contact and returns to the pool.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class EnemyBullet : MonoBehaviour
    {
        [SerializeField] private float speed = Constants.EnemyBulletSpeed;
        [SerializeField] private int damage = Constants.EnemyBulletDamage;
        [SerializeField] private float lifetime = Constants.BulletLifetime;

        private Coroutine _lifetimeRoutine;

        public int Damage => damage;

        public void Configure(int newDamage, float newSpeed)
        {
            damage = newDamage;
            speed = newSpeed;
        }

        private void OnEnable()
        {
            _lifetimeRoutine = StartCoroutine(LifetimeCountdown());
        }

        private void OnDisable()
        {
            if (_lifetimeRoutine != null)
            {
                StopCoroutine(_lifetimeRoutine);
                _lifetimeRoutine = null;
            }
        }

        private void Update()
        {
            // Enemy bullets are spawned rotated 180° so their "up" faces down.
            transform.position += transform.up * (speed * Time.deltaTime);
        }

        private IEnumerator LifetimeCountdown()
        {
            yield return new WaitForSeconds(lifetime);
            ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constants.Tags.Player))
            {
                var playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth == null)
                {
                    playerHealth = other.GetComponentInParent<PlayerHealth>();
                }

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }

                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            if (BulletPool.HasInstance)
            {
                BulletPool.Instance.ReturnEnemyBullet(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
