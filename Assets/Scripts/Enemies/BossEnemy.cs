using UnityEngine;
using System.Collections;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Boss enemy with multiple attack patterns
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        [Header("Boss Settings")]
        [SerializeField] private float horizontalMoveSpeed = 2f;
        [SerializeField] private float targetYPosition = 3.5f;
        [SerializeField] private int phase2HealthPercent = 60;
        [SerializeField] private int phase3HealthPercent = 30;
        
        [Header("Attack Patterns")]
        [SerializeField] private float spreadShotCount = 5;
        [SerializeField] private float spreadAngle = 60f;
        [SerializeField] private float burstFireRate = 0.1f;
        [SerializeField] private int burstCount = 5;
        
        [Header("Special Bullets")]
        [SerializeField] private GameObject spreadBulletPrefab;
        
        private int currentPhase = 1;
        private bool isAttacking = false;
        private float moveDirection = 1f;
        private Camera mainCamera;
        private float screenBoundX;
        private bool isInPosition = false;
        
        public int CurrentPhase => currentPhase;
        
        protected override void Start()
        {
            base.Start();
            mainCamera = Camera.main;
            
            if (mainCamera != null)
            {
                Vector3 screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
                screenBoundX = screenBounds.x - 2f;
            }
            else
            {
                screenBoundX = 6f;
            }
            
            StartCoroutine(AttackRoutine());
        }
        
        protected override void Move()
        {
            // Move to target Y position first
            if (!isInPosition)
            {
                if (transform.position.y > targetYPosition)
                {
                    transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                }
                else
                {
                    isInPosition = true;
                }
                return;
            }
            
            // Horizontal movement
            float speed = horizontalMoveSpeed * (currentPhase * 0.5f + 0.5f);
            transform.position += Vector3.right * moveDirection * speed * Time.deltaTime;
            
            // Bounce off screen edges
            if (Mathf.Abs(transform.position.x) >= screenBoundX)
            {
                moveDirection *= -1f;
            }
        }
        
        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            
            // Check phase transitions
            float healthPercent = (float)currentHealth / maxHealth * 100f;
            
            if (currentPhase == 1 && healthPercent <= phase2HealthPercent)
            {
                currentPhase = 2;
                OnPhaseChange();
            }
            else if (currentPhase == 2 && healthPercent <= phase3HealthPercent)
            {
                currentPhase = 3;
                OnPhaseChange();
            }
        }
        
        private void OnPhaseChange()
        {
            // Visual feedback for phase change
            StartCoroutine(PhaseChangeEffect());
        }
        
        private IEnumerator PhaseChangeEffect()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color originalColor = sr.color;
                sr.color = Color.red;
                yield return new WaitForSeconds(0.5f);
                sr.color = originalColor;
            }
        }
        
        private IEnumerator AttackRoutine()
        {
            yield return new WaitForSeconds(1f); // Initial delay
            
            while (isAlive)
            {
                if (!isInPosition || GameManager.Instance?.CurrentState != GameState.Playing)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }
                
                isAttacking = true;
                
                switch (currentPhase)
                {
                    case 1:
                        yield return StartCoroutine(Phase1Attack());
                        break;
                    case 2:
                        yield return StartCoroutine(Phase2Attack());
                        break;
                    case 3:
                        yield return StartCoroutine(Phase3Attack());
                        break;
                }
                
                isAttacking = false;
                yield return new WaitForSeconds(fireRate / currentPhase);
            }
        }
        
        private IEnumerator Phase1Attack()
        {
            // Single straight shot
            Shoot();
            yield return null;
        }
        
        private IEnumerator Phase2Attack()
        {
            // Spread shot
            ShootSpread();
            yield return null;
        }
        
        private IEnumerator Phase3Attack()
        {
            // Burst fire + spread
            for (int i = 0; i < burstCount; i++)
            {
                ShootSpread();
                yield return new WaitForSeconds(burstFireRate);
            }
        }
        
        private void ShootSpread()
        {
            GameObject bullet = spreadBulletPrefab != null ? spreadBulletPrefab : bulletPrefab;
            if (bullet == null) return;
            
            float startAngle = -spreadAngle / 2f;
            float angleStep = spreadAngle / (spreadShotCount - 1);
            
            for (int i = 0; i < spreadShotCount; i++)
            {
                float angle = startAngle + (angleStep * i);
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;
                
                GameObject bulletObj = Instantiate(bullet, transform.position + Vector3.down * 0.5f, Quaternion.identity);
                Bullet bulletScript = bulletObj.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.SetDirection(direction);
                }
            }
            
            if (shootSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shootSound, 0.3f);
            }
        }
    }
}
