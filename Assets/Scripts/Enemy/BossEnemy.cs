using System.Collections;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.UI;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Multi-phase boss. Enters from the top, stops at a hold position, then attacks.
    /// Three phases scale movement speed and firing based on remaining health percentage.
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        [Header("Entry")]
        [SerializeField] private float entrySpeed = 3f;
        [SerializeField] private float holdY = 3f;

        [Header("Sideways Movement")]
        [SerializeField] private float sideSpeedPhase1 = 1.5f;
        [SerializeField] private float sideSpeedPhase2 = 3f;
        [SerializeField] private float sideSpeedPhase3 = 5f;
        [SerializeField] private float sideRange = 3.5f;

        [Header("Side Cannons")]
        [SerializeField] private EnemyShooter[] sideCannons;

        private int _currentPhase;
        private bool _active;
        private float _centerX;
        private float _sideSpeed;

        public override void InitializeEnemy()
        {
            ApplyCommonData();

            // Disable the mover; the boss handles its own movement.
            if (mover != null)
            {
                mover.enabled = false;
            }

            _centerX = transform.position.x;
            _currentPhase = 0;
            _active = false;

            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.bossAlertSFX);
            }

            if (health != null)
            {
                health.OnHealthChanged += HandleHealthChanged;
            }

            if (UIManager.HasInstance)
            {
                UIManager.Instance.ShowBossHealthBar(1f);
            }

            StartCoroutine(EntryRoutine());
        }

        private IEnumerator EntryRoutine()
        {
            // Fly in from the top until reaching the hold position.
            while (transform.position.y > holdY)
            {
                transform.position += Vector3.down * (entrySpeed * Time.deltaTime);
                yield return null;
            }

            Vector3 pos = transform.position;
            pos.y = holdY;
            transform.position = pos;

            _active = true;
            EnterPhase(1);
        }

        private void Update()
        {
            if (!_active)
            {
                return;
            }

            if (GameManager.HasInstance && GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            // Oscillate horizontally around the centre.
            float x = _centerX + Mathf.Sin(Time.time * _sideSpeed) * sideRange;
            Vector3 pos = transform.position;
            pos.x = x;
            transform.position = pos;
        }

        private void HandleHealthChanged(int current, int max)
        {
            float percent = max > 0 ? (float)current / max : 0f;

            if (UIManager.HasInstance)
            {
                UIManager.Instance.ShowBossHealthBar(percent);
            }

            if (percent > 0.66f)
            {
                EnterPhase(1);
            }
            else if (percent > 0.33f)
            {
                EnterPhase(2);
            }
            else
            {
                EnterPhase(3);
            }
        }

        private void EnterPhase(int phase)
        {
            if (phase == _currentPhase || !_active)
            {
                return;
            }
            _currentPhase = phase;

            switch (phase)
            {
                case 1:
                    _sideSpeed = sideSpeedPhase1;
                    if (shooter != null) shooter.BeginFiring(BulletPattern.Single, 1.2f, DamageOrDefault());
                    SetSideCannons(false, BulletPattern.Single, 1.5f);
                    break;

                case 2:
                    _sideSpeed = sideSpeedPhase2;
                    if (shooter != null) shooter.BeginFiring(BulletPattern.Spread3, 0.9f, DamageOrDefault());
                    SetSideCannons(true, BulletPattern.Single, 1.0f);
                    break;

                case 3:
                    _sideSpeed = sideSpeedPhase3;
                    if (shooter != null) shooter.BeginFiring(BulletPattern.Spread5, 0.45f, DamageOrDefault());
                    SetSideCannons(true, BulletPattern.Aimed, 0.7f);
                    break;
            }
        }

        private int DamageOrDefault()
        {
            return data != null ? data.bulletDamage : Constants.EnemyBulletDamage;
        }

        private void SetSideCannons(bool active, BulletPattern pattern, float interval)
        {
            if (sideCannons == null)
            {
                return;
            }

            foreach (var cannon in sideCannons)
            {
                if (cannon == null)
                {
                    continue;
                }
                cannon.gameObject.SetActive(active);
                if (active)
                {
                    cannon.BeginFiring(pattern, interval, DamageOrDefault());
                }
                else
                {
                    cannon.StopFiring();
                }
            }
        }

        public override void OnDeath()
        {
            _active = false;

            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
            }

            if (shooter != null)
            {
                shooter.StopFiring();
            }
            SetSideCannons(false, BulletPattern.Single, 1f);

            if (UIManager.HasInstance)
            {
                UIManager.Instance.HideBossHealthBar();
            }

            base.OnDeath();
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}
