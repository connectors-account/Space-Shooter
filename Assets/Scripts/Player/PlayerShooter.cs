using System.Collections;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Weapons;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Player weapon controller. Supports single/double/triple/rapid/laser modes and power-up timers.
    /// Uses the ObjectPool for bullets.
    /// </summary>
    public class PlayerShooter : MonoBehaviour
    {
        public enum WeaponMode { Single, Double, Triple, Rapid, Laser }

        [Header("Fire Settings")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private string bulletPoolTag = "PlayerBullet";
        [SerializeField] private float defaultFireRate = 0.2f;
        [SerializeField] private float rapidFireRate = 0.08f;
        [SerializeField] private float bulletSpeed = 14f;
        [SerializeField] private int bulletDamage = 10;
        [SerializeField] private float spreadAngle = 15f;

        [Header("Laser")]
        [SerializeField] private LineRenderer laserLine;
        [SerializeField] private float laserRange = 20f;
        [SerializeField] private int laserDamagePerTick = 4;
        [SerializeField] private float laserTickInterval = 0.05f;
        [SerializeField] private LayerMask laserHitMask = ~0;

        [Header("Muzzle Flash")]
        [SerializeField] private float muzzlePulseScale = 1.4f;

        private WeaponMode currentMode = WeaponMode.Single;
        private float fireTimer;
        private float laserTickTimer;
        private Coroutine powerupRoutine;
        private Vector3 firePointBaseScale = Vector3.one;
        private bool canShoot = true;

        public WeaponMode CurrentMode => currentMode;
        public float PowerUpTimeRemaining { get; private set; }
        public float PowerUpTotalDuration { get; private set; }

        private void Awake()
        {
            if (firePoint == null) firePoint = transform;
            firePointBaseScale = firePoint.localScale;
            if (laserLine != null) laserLine.enabled = false;
        }

        private void Update()
        {
            if (!canShoot) return;

            bool firing = Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

            if (currentMode == WeaponMode.Laser)
            {
                HandleLaser(firing);
                return;
            }

            if (laserLine != null && laserLine.enabled) laserLine.enabled = false;

            fireTimer -= Time.deltaTime;
            if (firing && fireTimer <= 0f)
            {
                Fire();
                fireTimer = GetCurrentFireRate();
            }
        }

        private float GetCurrentFireRate()
        {
            return currentMode == WeaponMode.Rapid ? rapidFireRate : defaultFireRate;
        }

        private void Fire()
        {
            Vector2 up = Vector2.up;
            switch (currentMode)
            {
                case WeaponMode.Single:
                case WeaponMode.Rapid:
                    BulletPattern.SingleShot(bulletPoolTag, firePoint, up, bulletSpeed, bulletDamage, "Enemy");
                    break;
                case WeaponMode.Double:
                    BulletPattern.DoubleShot(bulletPoolTag, firePoint, up, bulletSpeed, bulletDamage, "Enemy", 0.3f);
                    break;
                case WeaponMode.Triple:
                    BulletPattern.TripleSpread(bulletPoolTag, firePoint, up, bulletSpeed, bulletDamage, "Enemy", spreadAngle);
                    break;
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("PlayerShoot");
            StartCoroutine(MuzzleFlash());
        }

        private IEnumerator MuzzleFlash()
        {
            firePoint.localScale = firePointBaseScale * muzzlePulseScale;
            yield return new WaitForSeconds(0.04f);
            firePoint.localScale = firePointBaseScale;
        }

        private void HandleLaser(bool firing)
        {
            if (laserLine == null)
            {
                // Fallback to rapid single shots if no line renderer assigned.
                fireTimer -= Time.deltaTime;
                if (firing && fireTimer <= 0f)
                {
                    BulletPattern.SingleShot(bulletPoolTag, firePoint, Vector2.up, bulletSpeed * 1.5f, bulletDamage, "Enemy");
                    fireTimer = rapidFireRate;
                }
                return;
            }

            if (!firing)
            {
                laserLine.enabled = false;
                return;
            }

            laserLine.enabled = true;
            Vector3 origin = firePoint.position;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, laserRange, laserHitMask);

            Vector3 endPoint = origin + Vector3.up * laserRange;
            if (hit.collider != null)
            {
                endPoint = hit.point;
                laserTickTimer -= Time.deltaTime;
                if (laserTickTimer <= 0f)
                {
                    var enemy = hit.collider.GetComponent<SpaceShooter.Enemy.EnemyBase>();
                    if (enemy != null) enemy.TakeDamage(laserDamagePerTick);
                    laserTickTimer = laserTickInterval;
                }
            }

            laserLine.positionCount = 2;
            laserLine.SetPosition(0, origin);
            laserLine.SetPosition(1, endPoint);
        }

        // ---------------- Power-up management ----------------

        public void SetWeaponMode(WeaponMode mode, float duration)
        {
            currentMode = mode;
            if (powerupRoutine != null) StopCoroutine(powerupRoutine);
            if (duration > 0f)
            {
                powerupRoutine = StartCoroutine(PowerUpTimer(duration));
            }
        }

        private IEnumerator PowerUpTimer(float duration)
        {
            PowerUpTotalDuration = duration;
            PowerUpTimeRemaining = duration;
            while (PowerUpTimeRemaining > 0f)
            {
                PowerUpTimeRemaining -= Time.deltaTime;
                yield return null;
            }
            PowerUpTimeRemaining = 0f;
            currentMode = WeaponMode.Single;
            if (laserLine != null) laserLine.enabled = false;
            powerupRoutine = null;
        }

        public void SetCanShoot(bool value)
        {
            canShoot = value;
            if (!value && laserLine != null) laserLine.enabled = false;
        }

        public void ResetWeapon()
        {
            currentMode = WeaponMode.Single;
            PowerUpTimeRemaining = 0f;
            if (powerupRoutine != null) StopCoroutine(powerupRoutine);
            if (laserLine != null) laserLine.enabled = false;
        }
    }
}
