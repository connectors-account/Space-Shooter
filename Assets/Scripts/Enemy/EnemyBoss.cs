using System;
using System.Collections;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Weapons;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Two-phase boss. Enters from the top, then moves side to side firing spreads.
    /// Below 50% HP it enrages: faster, red tint, denser fire and spiral attacks.
    /// </summary>
    public class EnemyBoss : EnemyBase
    {
        [Header("Boss Settings")]
        [SerializeField] private float entrySpeed = 3f;
        [SerializeField] private float stopY = 3f;
        [SerializeField] private float phase1MoveSpeed = 2f;
        [SerializeField] private float phase2MoveSpeed = 4f;
        [SerializeField] private float phase1ShootInterval = 1f;
        [SerializeField] private float phase2ShootInterval = 0.5f;
        [SerializeField] private float bulletSpeed = 6f;
        [SerializeField] private int bulletDamage = 15;
        [SerializeField] private float spreadAngle = 18f;
        [SerializeField] private string enemyBulletTag = "EnemyBullet";

        [Header("Enrage Visual")]
        [SerializeField] private Color enragedTint = new Color(1f, 0.4f, 0.4f);

        // Events for UI boss health bar.
        public static event Action<EnemyBoss> OnBossSpawned;
        public static event Action<int, int> OnBossHealthChanged; // current, max
        public static event Action OnBossDefeated;

        private enum Phase { Entering, Phase1, Phase2 }
        private Phase phase = Phase.Entering;

        private float shootTimer;
        private int moveDir = 1;
        private SpriteRenderer sr;
        private Color baseColor;
        private bool spiralActive;

        protected override void Awake()
        {
            base.Awake();
            IsBoss = true;
            maxHealth = 500;
            speed = phase1MoveSpeed;
            scoreValue = 5000;
            poolTag = "EnemyBoss";
            contactDamage = 40;
            powerUpDropChance = 1f; // Boss always drops.
            sr = GetComponent<SpriteRenderer>();
            if (sr != null) baseColor = sr.color;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            phase = Phase.Entering;
            moveDir = 1;
            shootTimer = phase1ShootInterval;
            spiralActive = false;
            if (sr != null) sr.color = baseColor;

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("BossAlert");
            OnBossSpawned?.Invoke(this);
            OnBossHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public override void ApplyDifficulty(float healthMultiplier, float speedBonus)
        {
            base.ApplyDifficulty(healthMultiplier, speedBonus);
            OnBossHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        protected override void Move()
        {
            switch (phase)
            {
                case Phase.Entering:
                    transform.position += Vector3.down * entrySpeed * Time.deltaTime;
                    if (transform.position.y <= stopY)
                    {
                        Vector3 p = transform.position;
                        p.y = stopY;
                        transform.position = p;
                        phase = Phase.Phase1;
                    }
                    break;

                case Phase.Phase1:
                case Phase.Phase2:
                    float moveSpeed = phase == Phase.Phase1 ? phase1MoveSpeed : phase2MoveSpeed;
                    transform.position += Vector3.right * moveDir * moveSpeed * Time.deltaTime;
                    float halfWidth = ScreenHalfWidth() - 1.5f;
                    if (transform.position.x > halfWidth) { moveDir = -1; }
                    else if (transform.position.x < -halfWidth) { moveDir = 1; }
                    break;
            }
        }

        protected override void Shoot()
        {
            if (phase == Phase.Entering) return;

            shootTimer -= Time.deltaTime;
            float interval = phase == Phase.Phase1 ? phase1ShootInterval : phase2ShootInterval;
            if (shootTimer <= 0f)
            {
                shootTimer = interval;
                if (phase == Phase.Phase1)
                {
                    BulletPattern.TripleSpread(enemyBulletTag, transform, Vector2.down, bulletSpeed, bulletDamage, "Player", spreadAngle);
                }
                else
                {
                    BulletPattern.FiveWaySpread(enemyBulletTag, transform, Vector2.down, bulletSpeed, bulletDamage, "Player", spreadAngle);
                    if (!spiralActive)
                    {
                        StartCoroutine(SpiralBurst());
                    }
                }
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("EnemyShoot");
            }
        }

        private IEnumerator SpiralBurst()
        {
            spiralActive = true;
            yield return BulletPattern.SpiralPattern(enemyBulletTag, transform, bulletSpeed * 0.8f, bulletDamage, "Player", 12, 0.05f, 30f);
            spiralActive = false;
        }

        public override void TakeDamage(int amount)
        {
            if (isDead) return;
            currentHealth -= amount;
            OnBossHealthChanged?.Invoke(Mathf.Max(0, currentHealth), maxHealth);

            if (phase == Phase.Phase1 && currentHealth <= maxHealth * 0.5f && currentHealth > 0)
            {
                EnterPhase2();
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void EnterPhase2()
        {
            phase = Phase.Phase2;
            speed = phase2MoveSpeed;
            shootTimer = 0.1f;
            if (sr != null) sr.color = enragedTint;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("BossAlert");
        }

        protected override void Die()
        {
            if (isDead) return;
            OnBossDefeated?.Invoke();
            if (Effects.CameraShake.Instance != null) Effects.CameraShake.Instance.Shake(0.8f, 0.6f);
            base.Die();
            if (sr != null) sr.color = baseColor;
        }

        protected override void CheckBounds()
        {
            // Boss never leaves the screen; ignore bottom bounds.
        }
    }
}
