// =============================================================================
// TankEnemy.cs — Slow, heavily armored enemy
// =============================================================================
using UnityEngine;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Tank enemy: slow movement, high health, fires spread shots.
    /// Pauses periodically to fire.
    /// </summary>
    public class TankEnemy : EnemyBase
    {
        [Header("Tank Settings")]
        [SerializeField] private float pauseDuration = 1.5f;
        [SerializeField] private float moveDuration = 3f;

        private float stateTimer;
        private bool isPaused;

        protected override void Awake()
        {
            base.Awake();
            enemyType = EnemyType.Tank;
        }

        protected override void Start()
        {
            base.Start();
            stateTimer = moveDuration;
            isPaused = false;
        }

        protected override void Move()
        {
            stateTimer -= Time.deltaTime;

            if (isPaused)
            {
                // Stationary, fires more aggressively (handled by TryShoot)
                if (stateTimer <= 0f)
                {
                    isPaused = false;
                    stateTimer = moveDuration;
                }
            }
            else
            {
                // Move slowly downward
                transform.Translate(Vector2.down * moveSpeed * Time.deltaTime, Space.World);
                if (stateTimer <= 0f)
                {
                    isPaused = true;
                    stateTimer = pauseDuration;
                }
            }
        }

        protected override void TryShoot()
        {
            if (bulletPattern == null) return;

            // Fire more frequently when paused
            float rate = isPaused ? fireRate * 0.5f : fireRate;
            if (Time.time < nextFireTime) return;
            nextFireTime = Time.time + rate;

            bulletPattern.Fire(transform.position, Vector2.down);
        }
    }
}
