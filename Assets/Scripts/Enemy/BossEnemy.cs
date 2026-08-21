using System.Collections;
using UnityEngine;
using SpaceShooter.Bullets;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Boss enemy with a dramatic entrance and two combat phases.
    /// Phase 1 (>50% HP): Spread5 pattern, slow horizontal patrol.
    /// Phase 2 (&lt;=50% HP): Spiral pattern, faster patrol, periodic minion spawning.
    /// 500 HP.
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        [Header("Boss Config")]
        [SerializeField] private float entranceDuration = 2.5f;
        [SerializeField] private float patrolSpeedPhase1 = 2f;
        [SerializeField] private float patrolSpeedPhase2 = 4f;
        [SerializeField] private float targetYRatio = 0.65f; // fraction of top half to settle at

        [Header("Attack")]
        [SerializeField] private float fireIntervalPhase1 = 1.6f;
        [SerializeField] private float fireIntervalPhase2 = 0.15f;
        [SerializeField] private float bulletSpeed = 6.5f;
        [SerializeField] private int bulletDamage = 12;
        [SerializeField] private float spiralStep = 22f;

        [Header("Minions")]
        [SerializeField] private float minionSpawnInterval = 4f;
        [SerializeField] private EnemyType minionType = EnemyType.Fast;

        private bool _entered;
        private bool _phase2;
        private int _patrolDir = 1;
        private float _fireTimer;
        private float _spiralAngle;
        private float _minionTimer;
        private float _settleY;

        protected override void Awake()
        {
            base.Awake();
            maxHealth = 500;
            scoreValue = 5000;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _entered = false;
            _phase2 = false;
            _patrolDir = 1;
            _fireTimer = fireIntervalPhase1;
            _minionTimer = minionSpawnInterval;
            _spiralAngle = 0f;
            StartCoroutine(EntranceRoutine());
        }

        private IEnumerator EntranceRoutine()
        {
            float topY = ScreenBounds.Instance != null ? ScreenBounds.Instance.MaxY : 5f;
            _settleY = ScreenBounds.Instance != null
                ? Mathf.Lerp(0f, ScreenBounds.Instance.MaxY, targetYRatio)
                : 3f;

            Vector3 start = new Vector3(0f, topY + 2f, 0f);
            Vector3 end = new Vector3(0f, _settleY, 0f);
            transform.position = start;

            float t = 0f;
            while (t < entranceDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / entranceDuration);
                transform.position = Vector3.Lerp(start, end, k);
                yield return null;
            }

            transform.position = end;
            _entered = true;

            AudioManager.Instance?.PlaySFX("boss_music");
        }

        protected override void OnDamaged()
        {
            base.OnDamaged();
            if (!_phase2 && currentHealth <= maxHealth * 0.5f)
            {
                EnterPhase2();
            }
        }

        private void EnterPhase2()
        {
            _phase2 = true;
            _fireTimer = fireIntervalPhase2;
            if (spriteRenderer != null)
            {
                // Shift tint to signal enrage.
                spriteRenderer.color = Color.Lerp(originalColor, Color.red, 0.4f);
                originalColor = spriteRenderer.color;
            }
        }

        private void Update()
        {
            if (!_entered || isDead) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            Patrol();
            HandleFiring();

            if (_phase2)
            {
                HandleMinions();
            }
        }

        private void Patrol()
        {
            float speed = _phase2 ? patrolSpeedPhase2 : patrolSpeedPhase1;
            Vector3 pos = transform.position;
            pos.x += _patrolDir * speed * Time.deltaTime;

            if (ScreenBounds.Instance != null)
            {
                float limit = ScreenBounds.Instance.MaxX - 2f;
                if (pos.x >= limit)
                {
                    pos.x = limit;
                    _patrolDir = -1;
                }
                else if (pos.x <= -limit)
                {
                    pos.x = -limit;
                    _patrolDir = 1;
                }
            }
            transform.position = pos;
        }

        private void HandleFiring()
        {
            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f) return;

            Vector3 origin = transform.position + Vector3.down * 1f;
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            Vector3 target = playerObj != null ? playerObj.transform.position : origin + Vector3.down * 5f;

            if (!_phase2)
            {
                BulletPattern.Fire(PatternType.Spread5, origin, Vector2.down, target, bulletSpeed, bulletDamage);
                _fireTimer = fireIntervalPhase1;
            }
            else
            {
                BulletPattern.Fire(PatternType.Spiral, origin, Vector2.down, target, bulletSpeed, bulletDamage, _spiralAngle);
                _spiralAngle += spiralStep;
                if (_spiralAngle >= 360f) _spiralAngle -= 360f;
                _fireTimer = fireIntervalPhase2;
            }
        }

        private void HandleMinions()
        {
            _minionTimer -= Time.deltaTime;
            if (_minionTimer > 0f) return;

            _minionTimer = minionSpawnInterval;
            var spawner = FindObjectOfType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.SpawnEnemy(minionType, 1.2f);
            }
        }
    }
}
