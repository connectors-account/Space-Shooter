using UnityEngine;
using System.Collections;
using SpaceShooter.Managers;
using SpaceShooter.Combat;

namespace SpaceShooter.Enemy
{
    public class BossEnemy : EnemyBase
    {
        [Header("Boss Settings")]
        [SerializeField] private float entrySpeed = 2f;
        [SerializeField] private float targetYPosition = 3f;
        [SerializeField] private bool hasEnteredArena = false;

        [Header("Attack Patterns")]
        [SerializeField] private float patternDuration = 5f;
        [SerializeField] private int currentPattern = 0;
        [SerializeField] private int totalPatterns = 3;

        [Header("Spread Attack")]
        [SerializeField] private int spreadBulletCount = 8;
        [SerializeField] private float spreadFireRate = 0.5f;

        [Header("Laser Attack")]
        [SerializeField] private Transform[] laserPoints;
        [SerializeField] private float laserFireRate = 0.1f;

        [Header("Charge Attack")]
        [SerializeField] private float chargeSpeed = 8f;
        [SerializeField] private float chargeReturnSpeed = 3f;

        private float patternTimer;
        private Vector3 originalPosition;
        private bool isAttacking = false;

        public event System.Action<float> OnHealthChanged;
        public event System.Action OnBossDefeated;

        protected override void Awake()
        {
            base.Awake();
            enemyType = EnemyType.Boss;
            maxHealth = 500;
            currentHealth = maxHealth;
            moveSpeed = 2f;
            scoreValue = 5000;
            canShoot = false;
            damage = 25;
            powerUpDropChance = 1f;
        }

        protected override void Start()
        {
            base.Start();
            originalPosition = new Vector3(0, targetYPosition, 0);
        }

        protected override void Update()
        {
            if (GameManager.Instance != null && (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver))
                return;

            if (!hasEnteredArena)
            {
                EnterArena();
            }
            else
            {
                timeAlive += Time.deltaTime;
                patternTimer += Time.deltaTime;

                if (patternTimer >= patternDuration && !isAttacking)
                {
                    patternTimer = 0f;
                    currentPattern = (currentPattern + 1) % totalPatterns;
                    StartCoroutine(ExecutePattern(currentPattern));
                }
            }
        }

        private void EnterArena()
        {
            Vector3 targetPos = new Vector3(transform.position.x, targetYPosition, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, entrySpeed * Time.deltaTime);

            if (Mathf.Abs(transform.position.y - targetYPosition) < 0.1f)
            {
                hasEnteredArena = true;
                originalPosition = transform.position;
                StartCoroutine(ExecutePattern(0));
            }
        }

        private IEnumerator ExecutePattern(int pattern)
        {
            isAttacking = true;

            switch (pattern)
            {
                case 0:
                    yield return StartCoroutine(SpreadAttack());
                    break;
                case 1:
                    yield return StartCoroutine(LaserBarrage());
                    break;
                case 2:
                    yield return StartCoroutine(ChargeAttack());
                    break;
            }

            isAttacking = false;
        }

        private IEnumerator SpreadAttack()
        {
            for (int burst = 0; burst < 5; burst++)
            {
                float angleStep = 360f / spreadBulletCount;
                for (int i = 0; i < spreadBulletCount; i++)
                {
                    float angle = angleStep * i + (burst * 15f);
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;
                    SpawnBossBullet(transform.position, direction);
                }
                AudioManager.Instance?.PlaySound("EnemyShoot");
                yield return new WaitForSeconds(spreadFireRate);
            }
        }

        private IEnumerator LaserBarrage()
        {
            int shots = 20;
            for (int i = 0; i < shots; i++)
            {
                Vector2 direction = Vector2.down;
                SpawnBossBullet(transform.position + Vector3.left * 2f, direction);
                SpawnBossBullet(transform.position + Vector3.right * 2f, direction);
                SpawnBossBullet(transform.position, direction);
                AudioManager.Instance?.PlaySound("EnemyShoot");
                yield return new WaitForSeconds(laserFireRate);
            }
        }

        private IEnumerator ChargeAttack()
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = new Vector3(transform.position.x, -2f, transform.position.z);

            float progress = 0f;
            while (progress < 1f)
            {
                progress += Time.deltaTime * chargeSpeed / Vector3.Distance(startPos, targetPos);
                transform.position = Vector3.Lerp(startPos, targetPos, progress);
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            progress = 0f;
            while (progress < 1f)
            {
                progress += Time.deltaTime * chargeReturnSpeed / Vector3.Distance(targetPos, originalPosition);
                transform.position = Vector3.Lerp(targetPos, originalPosition, progress);
                yield return null;
            }

            transform.position = originalPosition;
        }

        private void SpawnBossBullet(Vector3 position, Vector2 direction)
        {
            if (bulletPrefab == null) return;

            GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Initialize(direction, false, damage);
            }
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            OnHealthChanged?.Invoke((float)currentHealth / maxHealth);
        }

        protected override void Die()
        {
            OnBossDefeated?.Invoke();
            
            for (int i = 0; i < 5; i++)
            {
                Vector3 explosionPos = transform.position + new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0
                );
                EffectsManager.Instance?.SpawnExplosion(explosionPos, 1.5f);
            }

            base.Die();
        }

        protected override void Move()
        {
            if (!hasEnteredArena || isAttacking) return;

            float newX = originalPosition.x + Mathf.Sin(timeAlive * 0.5f) * 3f;
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        protected override void CheckBounds() { }
    }
}
