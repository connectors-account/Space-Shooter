using System.Collections;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Abstract base shared by every enemy. Wires up health/movement, fires on a
    /// timer, and on death notifies the wave manager and rolls for a power-up drop.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Tooltip("Seconds between shots.")]
        public float shootInterval = 2f;

        [Tooltip("Chance (0-1) to drop a power-up on death.")]
        [Range(0f, 1f)] public float dropChance = 0.2f;

        [Tooltip("Possible power-up prefabs dropped on death.")]
        public PowerUpBase[] powerUpDrops;

        protected EnemyHealth Health;
        protected EnemyMovement Movement;

        private Coroutine _shootRoutine;
        private bool _deathHandled;

        protected virtual void Awake()
        {
            Health = GetComponent<EnemyHealth>();
            Movement = GetComponent<EnemyMovement>();
            Health.Configure(GetMaxHealth(), GetScoreValue());
            Health.OnDeath += HandleDeath;
        }

        protected virtual void OnEnable()
        {
            _deathHandled = false;
            _shootRoutine = StartCoroutine(ShootLoop());
        }

        protected virtual void OnDisable()
        {
            if (_shootRoutine != null) StopCoroutine(_shootRoutine);
        }

        protected virtual void OnDestroy()
        {
            if (Health != null) Health.OnDeath -= HandleDeath;
        }

        private IEnumerator ShootLoop()
        {
            // Small initial delay so freshly spawned enemies do not all fire at once.
            yield return new WaitForSeconds(Random.Range(0.2f, shootInterval));
            while (true)
            {
                if (GameManager.Instance == null || GameManager.Instance.IsPlaying)
                {
                    Shoot();
                }
                yield return new WaitForSeconds(shootInterval);
            }
        }

        private void HandleDeath()
        {
            if (_deathHandled) return;
            _deathHandled = true;

            if (WaveManager.Instance != null) WaveManager.Instance.NotifyEnemyKilled();

            TryDropPowerUp();
        }

        private void TryDropPowerUp()
        {
            if (powerUpDrops == null || powerUpDrops.Length == 0) return;
            if (Random.value > dropChance) return;

            PowerUpBase prefab = powerUpDrops[Random.Range(0, powerUpDrops.Length)];
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        /// <summary>Fires an enemy bullet from <paramref name="origin"/> in <paramref name="direction"/>.</summary>
        protected BulletBase FireBullet(Vector3 origin, Vector2 direction)
        {
            if (BulletPool.Instance == null) return null;
            BulletBase bullet = BulletPool.Instance.GetBullet(BulletType.Enemy, origin, direction.normalized);

            if (bullet != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.shootSFX);
            }
            return bullet;
        }

        /// <summary>Returns the normalized direction from this enemy toward the player.</summary>
        protected Vector2 DirectionToPlayer()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return Vector2.down;
            return ((Vector2)(player.transform.position - transform.position)).normalized;
        }

        /// <summary>Fires the enemy's weapon. Implemented per enemy type.</summary>
        public abstract void Shoot();

        /// <summary>Score awarded when this enemy dies.</summary>
        public abstract int GetScoreValue();

        /// <summary>Maximum hit points for this enemy type.</summary>
        protected abstract int GetMaxHealth();
    }
}
