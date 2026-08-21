using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using SpaceShooter.Bullets;
using SpaceShooter.Core;

namespace SpaceShooter.Player
{
    public enum ShootMode
    {
        Single,
        Triple,
        Rapid
    }

    /// <summary>
    /// Handles player firing: auto-fire on hold, multiple shoot modes, bullet pooling,
    /// and a muzzle flash effect. Base fire rate 0.2s.
    /// </summary>
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Firing")]
        [SerializeField] private float baseFireRate = 0.2f;
        [SerializeField] private float bulletSpeed = 14f;
        [SerializeField] private int bulletDamage = 10;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float tripleSpreadAngle = 12f;

        [Header("Muzzle Flash")]
        [SerializeField] private SpriteRenderer muzzleFlash;
        [SerializeField] private float muzzleFlashTime = 0.05f;

        private ShootMode _mode = ShootMode.Single;
        private float _fireRateMultiplier = 1f;
        private float _cooldown;
        private bool _fireHeld;

        private PlayerInputActions _inputActions;

        public ShootMode Mode => _mode;

        private void Awake()
        {
            if (firePoint == null)
            {
                firePoint = transform;
            }
            if (muzzleFlash != null)
            {
                muzzleFlash.enabled = false;
            }
            _inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
            _inputActions.Gameplay.Fire.performed += OnFirePerformed;
            _inputActions.Gameplay.Fire.canceled += OnFireCanceled;
        }

        private void OnDisable()
        {
            _inputActions.Gameplay.Fire.performed -= OnFirePerformed;
            _inputActions.Gameplay.Fire.canceled -= OnFireCanceled;
            _inputActions.Disable();
        }

        private void OnFirePerformed(InputAction.CallbackContext ctx) => _fireHeld = true;
        private void OnFireCanceled(InputAction.CallbackContext ctx) => _fireHeld = false;

        public void SetMode(ShootMode mode) => _mode = mode;
        public void ResetMode() => _mode = ShootMode.Single;

        public void SetFireRateMultiplier(float multiplier)
        {
            _fireRateMultiplier = Mathf.Max(0.05f, multiplier);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            _cooldown -= Time.deltaTime;

            if (_fireHeld && _cooldown <= 0f)
            {
                Fire();
                _cooldown = CurrentFireRate();
            }
        }

        private float CurrentFireRate()
        {
            // Rapid mode fires roughly twice as fast; power-up multiplier reduces interval.
            float rate = baseFireRate * _fireRateMultiplier;
            if (_mode == ShootMode.Rapid)
            {
                rate *= 0.5f;
            }
            return Mathf.Max(0.03f, rate);
        }

        private void Fire()
        {
            if (BulletPool.Instance == null) return;

            Vector3 origin = firePoint.position;

            switch (_mode)
            {
                case ShootMode.Single:
                case ShootMode.Rapid:
                    SpawnBullet(origin, Vector2.up);
                    break;

                case ShootMode.Triple:
                    SpawnBullet(origin, Vector2.up);
                    SpawnBullet(origin, RotateVector(Vector2.up, tripleSpreadAngle));
                    SpawnBullet(origin, RotateVector(Vector2.up, -tripleSpreadAngle));
                    break;
            }

            AudioManager.Instance?.PlaySFX("shoot");
            if (muzzleFlash != null)
            {
                StartCoroutine(MuzzleFlashRoutine());
            }
        }

        private void SpawnBullet(Vector3 origin, Vector2 direction)
        {
            BulletPool.Instance.GetPlayerBullet(origin, direction, bulletSpeed, bulletDamage);
        }

        private static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        private IEnumerator MuzzleFlashRoutine()
        {
            muzzleFlash.enabled = true;
            yield return new WaitForSeconds(muzzleFlashTime);
            muzzleFlash.enabled = false;
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();
        }
    }
}
