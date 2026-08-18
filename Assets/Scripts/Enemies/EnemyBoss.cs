using System;
using System.Collections;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// The wave-5 boss. 50 HP, worth 5000 points. Sweeps horizontally and fires in
    /// three escalating phases keyed to its remaining health, becomes invincible for
    /// 3 seconds when it first drops to 50% HP, and charges the player in phase 3.
    /// </summary>
    public class EnemyBoss : EnemyBase
    {
        [Header("Movement")]
        [SerializeField] private float sweepSpeed = 2f;
        [SerializeField] private float sweepHalfWidth = 5f;
        [SerializeField] private float chargeSpeed = 6f;

        [Header("Combat")]
        [SerializeField] private float bulletSpread = 30f;

        /// <summary>Raised (statically) whenever a boss spawns, for the HUD boss bar.</summary>
        public static event Action<EnemyBoss> OnBossSpawn;

        private float _centerX;
        private float _startY;
        private bool _shieldTriggered;
        private bool _charging;
        private float _nextChargeTime;

        protected override void Awake()
        {
            shootInterval = 1f;
            dropChance = 0f; // bosses do not drop power-ups
            base.Awake();
            Health.OnHealthChanged += OnBossHealthChanged;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _centerX = transform.position.x;
            _startY = transform.position.y;
            _nextChargeTime = Time.time + 4f;
            OnBossSpawn?.Invoke(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Health != null) Health.OnHealthChanged -= OnBossHealthChanged;
        }

        private float HealthPercent =>
            Health.maxHealth > 0 ? (float)Health.CurrentHealth / Health.maxHealth : 0f;

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            if (_charging)
            {
                // Charge downward toward the player, then reset upward.
                transform.position += Vector3.down * chargeSpeed * Time.deltaTime;
                if (transform.position.y <= _startY - 2.5f)
                {
                    _charging = false;
                }
                return;
            }

            // Horizontal sweep around the spawn X, easing back to start height.
            float x = _centerX + Mathf.Sin(Time.time * sweepSpeed) * sweepHalfWidth;
            float y = Mathf.MoveTowards(transform.position.y, _startY, sweepSpeed * Time.deltaTime);
            transform.position = new Vector3(x, y, transform.position.z);

            // Phase-3 charge behaviour.
            if (HealthPercent < 0.3f && Time.time >= _nextChargeTime)
            {
                _charging = true;
                _nextChargeTime = Time.time + 5f;
            }
        }

        public override void Shoot()
        {
            Vector3 muzzle = transform.position + Vector3.down * 0.8f;

            if (HealthPercent >= 0.6f)
            {
                // Phase 1: 3-way spread.
                FireSpread(muzzle, 3, bulletSpread);
                shootInterval = 1f;
            }
            else if (HealthPercent >= 0.3f)
            {
                // Phase 2: 5-way spread plus two side bullets.
                FireSpread(muzzle, 5, bulletSpread * 1.5f);
                FireBullet(transform.position + Vector3.left * 1.2f, Vector2.left);
                FireBullet(transform.position + Vector3.right * 1.2f, Vector2.right);
                shootInterval = 0.8f;
            }
            else
            {
                // Phase 3: 8-way radial burst.
                FireRadial(muzzle, 8);
                shootInterval = 0.6f;
            }
        }

        private void FireSpread(Vector3 origin, int count, float totalSpread)
        {
            if (count <= 1)
            {
                FireBullet(origin, Vector2.down);
                return;
            }

            float step = totalSpread / (count - 1);
            float start = -totalSpread * 0.5f;
            for (int i = 0; i < count; i++)
            {
                float angle = start + step * i;
                Vector2 dir = Rotate(Vector2.down, angle);
                FireBullet(origin, dir);
            }
        }

        private void FireRadial(Vector3 origin, int count)
        {
            float step = 360f / count;
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Rotate(Vector2.down, step * i);
                FireBullet(origin, dir);
            }
        }

        private void OnBossHealthChanged(int _)
        {
            if (!_shieldTriggered && HealthPercent <= 0.5f)
            {
                _shieldTriggered = true;
                if (isActiveAndEnabled) StartCoroutine(ShieldPhase());
            }
        }

        private IEnumerator ShieldPhase()
        {
            Health.Invincible = true;
            yield return new WaitForSeconds(3f);
            Health.Invincible = false;
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        public override int GetScoreValue() => 5000;

        protected override int GetMaxHealth() => 50;
    }
}
