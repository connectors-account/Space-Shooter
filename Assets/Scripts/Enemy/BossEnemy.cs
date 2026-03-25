// =============================================================================
// BossEnemy.cs — Boss enemy with phases and special attack patterns
// =============================================================================
using UnityEngine;
using System.Collections;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Boss enemy with multiple attack phases. Health triggers phase changes.
    /// Moves side-to-side at the top of the screen.
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        [Header("Boss Settings")]
        [SerializeField] private float horizontalSpeed = 2f;
        [SerializeField] private float targetY = 3.5f;
        [SerializeField] private float approachSpeed = 2f;
        [SerializeField] private Weapons.BulletPattern phase2Pattern;
        [SerializeField] private Weapons.BulletPattern phase3Pattern;

        private enum BossPhase { Entering, Phase1, Phase2, Phase3 }
        private BossPhase currentPhase = BossPhase.Entering;
        private float horizontalDir = 1f;
        private float screenHalfWidth;

        protected override void Awake()
        {
            base.Awake();
            enemyType = EnemyType.Boss;
        }

        protected override void Start()
        {
            base.Start();
            Camera cam = Camera.main;
            if (cam != null)
                screenHalfWidth = cam.orthographicSize * cam.aspect - 1f;
        }

        protected override void Move()
        {
            switch (currentPhase)
            {
                case BossPhase.Entering:
                    // Move to target Y position
                    float newY = Mathf.MoveTowards(transform.position.y, targetY, approachSpeed * Time.deltaTime);
                    transform.position = new Vector3(transform.position.x, newY, 0f);
                    if (Mathf.Abs(transform.position.y - targetY) < 0.1f)
                        currentPhase = BossPhase.Phase1;
                    break;

                case BossPhase.Phase1:
                case BossPhase.Phase2:
                case BossPhase.Phase3:
                    // Horizontal patrol
                    float x = transform.position.x + horizontalDir * horizontalSpeed * Time.deltaTime;
                    if (x > screenHalfWidth) { x = screenHalfWidth; horizontalDir = -1f; }
                    if (x < -screenHalfWidth) { x = -screenHalfWidth; horizontalDir = 1f; }
                    transform.position = new Vector3(x, targetY, 0f);
                    break;
            }
        }

        protected override void TryShoot()
        {
            if (currentPhase == BossPhase.Entering) return;
            if (Time.time < nextFireTime) return;

            // Select pattern based on phase
            Weapons.BulletPattern activePattern = bulletPattern;
            float rate = fireRate;

            switch (currentPhase)
            {
                case BossPhase.Phase2:
                    activePattern = phase2Pattern != null ? phase2Pattern : bulletPattern;
                    rate = fireRate * 0.7f;
                    break;
                case BossPhase.Phase3:
                    activePattern = phase3Pattern != null ? phase3Pattern : bulletPattern;
                    rate = fireRate * 0.5f;
                    break;
            }

            nextFireTime = Time.time + rate;
            if (activePattern != null)
                activePattern.Fire(transform.position, Vector2.down);
        }

        public override void TakeDamage(int damage)
        {
            if (!isAlive) return;
            currentHealth -= damage;
            Managers.SoundManager.Instance?.PlaySFX("enemy_hit");

            // Update phase based on health percentage
            float healthPct = (float)currentHealth / maxHealth;
            if (healthPct <= 0.33f && currentPhase != BossPhase.Phase3)
            {
                currentPhase = BossPhase.Phase3;
                horizontalSpeed *= 1.5f;
            }
            else if (healthPct <= 0.66f && currentPhase == BossPhase.Phase1)
            {
                currentPhase = BossPhase.Phase2;
                horizontalSpeed *= 1.2f;
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected override void Die()
        {
            isAlive = false;
            Managers.GameManager.Instance?.AddScore(scoreValue);
            Managers.SoundManager.Instance?.PlaySFX("boss_explode");

            // Big explosion effect
            if (explosionPrefab != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector3 offset = (Vector3)Random.insideUnitCircle * 1.5f;
                    Instantiate(explosionPrefab, transform.position + offset, Quaternion.identity);
                }
            }

            // Drop multiple power-ups
            for (int i = 0; i < 3; i++)
            {
                Vector3 dropPos = transform.position + (Vector3)Random.insideUnitCircle * 1f;
                Managers.GameManager.Instance?.SpawnRandomPowerUp(dropPos);
            }

            // Notify spawner that boss is defeated
            Managers.GameManager.Instance?.BossDefeated();

            Destroy(gameObject);
        }

        protected override void CheckBounds()
        {
            // Boss doesn't leave the screen
        }
    }
}
