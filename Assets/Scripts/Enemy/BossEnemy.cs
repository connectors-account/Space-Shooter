using System.Collections;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Bullets;
using SpaceShooter.Pickups;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Two-phase boss. Phase 1: side-to-side movement, Spread(5) + Circle(12).
    /// Phase 2 (at 50% HP): faster, adds a permanent rotating Spiral. Big death sequence.
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        #region Fields
        [SerializeField] private int _phase = 1;
        private float _direction = 1f;
        private float _currentMoveSpeed;

        private Coroutine _spreadRoutine;
        private Coroutine _circleRoutine;
        private Coroutine _spiralRoutine;
        #endregion

        #region Properties
        public int Phase => _phase;
        #endregion

        #region Setup
        protected override void Awake()
        {
            base.Awake();
            _maxHealth = GameConstants.BOSS_MAX_HEALTH;
            _scoreValue = GameConstants.BOSS_SCORE;
            _moveSpeed = GameConstants.BOSS_MOVE_SPEED;
            gameObject.tag = GameConstants.TAG_BOSS;
        }

        protected override void InitStats()
        {
            _maxHealth = Mathf.CeilToInt(GameConstants.BOSS_MAX_HEALTH * _difficultyMultiplier);
            _currentHealth = _maxHealth;
            _scoreValue = Mathf.CeilToInt(GameConstants.BOSS_SCORE * _difficultyMultiplier);
        }

        protected override void Start()
        {
            InitStats();
            _phase = 1;
            _currentMoveSpeed = _moveSpeed;
            // Boss manages its own firing; do not use the generic EnemyShooter loop.
            StartPhase1();
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.BossRoar);
        }
        #endregion

        #region Movement
        protected override void Move()
        {
            // Move into position first, then patrol side-to-side.
            if (transform.position.y > GameConstants.BOSS_Y_POSITION)
            {
                transform.Translate(Vector3.down * _currentMoveSpeed * Time.deltaTime, Space.World);
                return;
            }

            float x = transform.position.x + _direction * _currentMoveSpeed * Time.deltaTime;
            float limit = GameConstants.CAMERA_RIGHT - 2f;
            if (x > limit) { x = limit; _direction = -1f; }
            else if (x < -limit) { x = -limit; _direction = 1f; }
            transform.position = new Vector3(x, GameConstants.BOSS_Y_POSITION, 0f);
        }
        #endregion

        #region Phases
        private void StartPhase1()
        {
            _spreadRoutine = StartCoroutine(SpreadLoop());
            _circleRoutine = StartCoroutine(CircleLoop());
        }

        private void EnterPhase2()
        {
            _phase = 2;
            _currentMoveSpeed = _moveSpeed * GameConstants.BOSS_PHASE2_SPEED_MULTIPLIER;
            _spiralRoutine = StartCoroutine(
                BulletPattern.Spiral(this, () => transform.position, 3, 140f, true, GameConstants.BOSS_SPIRAL_FIRE_RATE));
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.BossRoar, 0.8f);
        }

        private IEnumerator SpreadLoop()
        {
            WaitForSeconds wait = new WaitForSeconds(GameConstants.BOSS_SPREAD_FIRE_RATE);
            while (!_isDead)
            {
                BulletPattern.Spread(transform.position, 5, 70f, true);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.EnemyShoot, 0.8f, 0.5f);
                yield return wait;
            }
        }

        private IEnumerator CircleLoop()
        {
            WaitForSeconds wait = new WaitForSeconds(GameConstants.BOSS_CIRCLE_FIRE_RATE);
            while (!_isDead)
            {
                yield return wait;
                if (_isDead) break;
                BulletPattern.Circle(transform.position, 12, true);
            }
        }
        #endregion

        #region Damage & Death
        public override void TakeDamage(int dmg)
        {
            if (_isDead || dmg <= 0) return;
            base.TakeDamage(dmg);

            if (_phase == 1 && _currentHealth > 0 && _currentHealth <= _maxHealth * 0.5f)
                EnterPhase2();
        }

        protected override void Die()
        {
            if (_isDead) return;
            _isDead = true;

            StopAllFiring();

            if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(_scoreValue);
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.BossRoar);

            StartCoroutine(DeathSequence());
        }

        private void StopAllFiring()
        {
            if (_spreadRoutine != null) StopCoroutine(_spreadRoutine);
            if (_circleRoutine != null) StopCoroutine(_circleRoutine);
            if (_spiralRoutine != null) StopCoroutine(_spiralRoutine);
        }

        private IEnumerator DeathSequence()
        {
            // Multiple staggered explosions across the boss body.
            for (int i = 0; i < 8; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1f, 1f), 0f);
                SpawnExplosionAt(transform.position + offset);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.Explosion, Random.Range(0.7f, 1.2f));
                if (Environment.CameraShake.Instance != null)
                    Environment.CameraShake.Instance.Shake(0.15f, 0.25f);
                yield return new WaitForSeconds(0.15f);
            }

            // Drop three power-ups around the death location.
            PowerUpSpawner.SpawnGuaranteed(transform.position + Vector3.left);
            PowerUpSpawner.SpawnGuaranteed(transform.position);
            PowerUpSpawner.SpawnGuaranteed(transform.position + Vector3.right);

            if (WaveManager.Instance != null) WaveManager.Instance.BossDefeated();

            Destroy(gameObject);
        }

        private void SpawnExplosionAt(Vector3 pos)
        {
            Sprite particle = Utilities.SpriteGenerator.GenerateStar();
            for (int i = 0; i < 16; i++)
            {
                GameObject p = new GameObject("BossExplosionParticle");
                p.transform.position = pos;
                SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
                sr.sprite = particle;
                sr.color = new Color(1f, Random.Range(0.3f, 0.7f), 0.1f, 1f);
                sr.sortingOrder = 45;
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(2f, 6f);
                Player.ExplosionParticle ep = p.AddComponent<Player.ExplosionParticle>();
                ep.Launch(vel, Random.Range(0.5f, 1f));
            }
        }
        #endregion
    }
}
