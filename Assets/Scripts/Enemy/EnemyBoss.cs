using System;
using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Audio;
using SpaceShooter.Utilities;
using SpaceShooter.Weapons;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Three-phase boss. Phases are chosen by remaining-HP thresholds:
    ///   Phase 1 (100%-66%): spiral fire + gentle side-to-side movement.
    ///   Phase 2 (66%-33%):  spiral + aimed homing bullets + faster movement.
    ///   Phase 3 (33%-0%):   all patterns at once + periodically spawns drones.
    /// HP 200, score 5000. Broadcasts health for the HUD boss health bar.
    /// </summary>
    public class EnemyBoss : EnemyBase
    {
        public static EnemyBoss ActiveBoss { get; private set; }

        /// <summary>Normalised health 0..1 for the HUD.</summary>
        public event Action<float> OnBossHealthChanged;
        public event Action OnBossDefeated;

        [Header("Boss movement")]
        [SerializeField] private float sideSpeed = 2.5f;
        [SerializeField] private float entryTargetY = 3f;

        private BulletPatternSpiral _spiral;
        private BulletPatternAimed _homingAimed;
        private BulletPatternSpread _spread;

        private int _phase = 1;
        private int _sideDir = 1;
        private bool _entering = true;
        private float _droneSpawnTimer;
        private float _homingTimer;
        private float _spreadTimer;

        public override bool IsBoss => true;

        protected override void Awake()
        {
            maxHp = 200;
            scoreValue = 5000;
            moveSpeed = 2.5f;
            shootInterval = 0.35f;
            powerUpDropChance = 1f;
            guaranteedDrop = true;
            base.Awake();
        }

        protected override void AssignSprite()
        {
            if (Renderer.sprite == null)
                Renderer.sprite = SpriteGenerator.CreateBossSprite();
        }

        protected override void SetupPattern()
        {
            _spiral = gameObject.AddComponent<BulletPatternSpiral>();
            _spiral.Configure(6, 15f);

            _homingAimed = gameObject.AddComponent<BulletPatternAimed>();
            _homingAimed.SetHoming(true);

            _spread = gameObject.AddComponent<BulletPatternSpread>();
            _spread.Configure(5, 70f);

            Pattern = _spiral;
        }

        public override void Initialise(string poolKey, Transform player, float difficultyMultiplier)
        {
            base.Initialise(poolKey, player, difficultyMultiplier);
            ActiveBoss = this;
            _phase = 1;
            _entering = true;
            _sideDir = 1;
            _droneSpawnTimer = 4f;
            _homingTimer = 1.5f;
            _spreadTimer = 2f;
            _spiral.ResetSpiral();

            _spiral.PlayerTarget = player;
            _homingAimed.PlayerTarget = player;
            _spread.PlayerTarget = player;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(Constants.SfxBossSpawn);
                AudioManager.Instance.PlayMusic(Constants.MusicBoss);
            }
            OnBossHealthChanged?.Invoke(1f);
        }

        protected override void Move()
        {
            if (_entering)
            {
                // Drop into position from above.
                float y = Mathf.MoveTowards(transform.position.y, entryTargetY, moveSpeed * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
                if (Mathf.Abs(transform.position.y - entryTargetY) < 0.05f)
                    _entering = false;
                return;
            }

            float phaseSpeedBoost = _phase == 1 ? 1f : (_phase == 2 ? 1.5f : 2f);
            float x = transform.position.x + _sideDir * sideSpeed * phaseSpeedBoost * Time.deltaTime;

            if (Cam != null && Cam.orthographic)
            {
                float halfW = Cam.orthographicSize * Cam.aspect;
                float cx = Cam.transform.position.x;
                if (x > cx + halfW - 1.2f) _sideDir = -1;
                else if (x < cx - halfW + 1.2f) _sideDir = 1;
            }
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }

        protected override void HandleShooting()
        {
            if (_entering) return;

            // Primary spiral fire on the base cadence.
            ShootTimer -= Time.deltaTime;
            if (ShootTimer <= 0f)
            {
                ShootTimer = shootInterval;
                _spiral.Fire(transform.position, ObjectPool.Instance);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(Constants.SfxEnemyShoot, 0.4f);
            }

            // Phase 2+ : aimed homing shots.
            if (_phase >= 2)
            {
                _homingTimer -= Time.deltaTime;
                if (_homingTimer <= 0f)
                {
                    _homingTimer = 1.5f;
                    _homingAimed.Fire(transform.position, ObjectPool.Instance);
                }
            }

            // Phase 3 : add spreads + spawn drones.
            if (_phase >= 3)
            {
                _spreadTimer -= Time.deltaTime;
                if (_spreadTimer <= 0f)
                {
                    _spreadTimer = 1.5f;
                    _spread.Fire(transform.position, ObjectPool.Instance);
                }

                _droneSpawnTimer -= Time.deltaTime;
                if (_droneSpawnTimer <= 0f)
                {
                    _droneSpawnTimer = 5f;
                    SpawnEscortDrone();
                }
            }
        }

        private void SpawnEscortDrone()
        {
            if (ObjectPool.Instance == null) return;
            float x = transform.position.x + UnityEngine.Random.Range(-2f, 2f);
            var go = ObjectPool.Instance.Acquire(Constants.PoolEnemyDrone,
                new Vector3(x, transform.position.y - 1f, 0f), Quaternion.identity);
            if (go == null) return;
            var drone = go.GetComponent<EnemyDrone>();
            if (drone != null)
                drone.Initialise(Constants.PoolEnemyDrone, PlayerTarget, DifficultyMultiplier);
        }

        public override void TakeDamage(int amount)
        {
            if (CurrentHp <= 0) return;
            CurrentHp -= amount;
            FlashHit();

            float normalised = Mathf.Clamp01((float)CurrentHp / maxHp);
            OnBossHealthChanged?.Invoke(normalised);

            UpdatePhase(normalised);

            if (CurrentHp <= 0)
                Die();
        }

        private void UpdatePhase(float normalised)
        {
            int newPhase = normalised > 0.66f ? 1 : (normalised > 0.33f ? 2 : 3);
            if (newPhase != _phase)
            {
                _phase = newPhase;
                CameraShake.ShakeStatic(0.4f, 0.4f);
            }
        }

        protected override void Die()
        {
            OnBossDefeated?.Invoke();
            if (ActiveBoss == this) ActiveBoss = null;
            OnBossHealthChanged?.Invoke(0f);

            // Big multi-explosion flourish.
            for (int i = 0; i < 6; i++)
            {
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-1.2f, 1.2f), UnityEngine.Random.Range(-1.2f, 1.2f), 0f);
                if (ObjectPool.Instance != null)
                {
                    var vfx = ObjectPool.Instance.Acquire(Constants.PoolExplosion, transform.position + offset, Quaternion.identity);
                    if (vfx != null)
                    {
                        var e = vfx.GetComponent<ExplosionVFX>();
                        if (e == null) e = vfx.AddComponent<ExplosionVFX>();
                        e.Play();
                    }
                }
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(Constants.SfxBomb);
                AudioManager.Instance.PlayMusic(Constants.MusicGame);
            }

            // Drop several power-ups as a reward.
            for (int i = 0; i < 3; i++)
                TryDropPowerUp();

            base.Die();
        }

        private void OnDisable()
        {
            if (ActiveBoss == this) ActiveBoss = null;
        }
    }
}
