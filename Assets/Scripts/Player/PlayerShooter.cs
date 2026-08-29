using System.Collections;
using UnityEngine;
using SpaceShooter.Bullets;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SpaceShooter.Player
{
    /// <summary>
    /// Handles player shooting with four upgradeable weapon levels and a muzzle flash effect.
    /// Level 1: single | Level 2: double | Level 3: triple spread | Level 4: quad + side cannons.
    /// </summary>
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Firing")]
        [SerializeField] private float fireRate = Constants.PlayerFireRate;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float bulletSpeed = Constants.PlayerBulletSpeed;
        [SerializeField] private int bulletDamage = Constants.PlayerBulletDamage;

        [Header("Weapon Level")]
        [SerializeField] private int weaponLevel = Constants.MinWeaponLevel;

        [Header("Spread")]
        [SerializeField] private float spreadAngle = 12f;
        [SerializeField] private float sideOffset = 0.35f;

        [Header("Muzzle Flash")]
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private float muzzleFlashDuration = 0.05f;

        private float _nextFireTime;

        public int WeaponLevel => weaponLevel;

        private void Awake()
        {
            if (firePoint == null)
            {
                firePoint = transform;
            }
            if (muzzleFlash != null)
            {
                muzzleFlash.SetActive(false);
            }
        }

        private void Update()
        {
            if (GameManager.HasInstance && GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            if (IsFirePressed() && Time.time >= _nextFireTime)
            {
                _nextFireTime = Time.time + fireRate;
                Fire();
            }
        }

        private bool IsFirePressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            bool keyPressed = keyboard != null && (keyboard.spaceKey.isPressed || keyboard.leftCtrlKey.isPressed);
            Gamepad gamepad = Gamepad.current;
            bool padPressed = gamepad != null && gamepad.buttonSouth.isPressed;
            return keyPressed || padPressed;
#else
            return Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space);
#endif
        }

        private void Fire()
        {
            switch (weaponLevel)
            {
                case 1:
                    SpawnBullet(firePoint.position, 0f);
                    break;

                case 2:
                    SpawnBullet(firePoint.position + firePoint.right * -sideOffset, 0f);
                    SpawnBullet(firePoint.position + firePoint.right * sideOffset, 0f);
                    break;

                case 3:
                    SpawnBullet(firePoint.position, 0f);
                    SpawnBullet(firePoint.position, -spreadAngle);
                    SpawnBullet(firePoint.position, spreadAngle);
                    break;

                default: // Level 4+
                    SpawnBullet(firePoint.position + firePoint.right * -sideOffset, -spreadAngle);
                    SpawnBullet(firePoint.position + firePoint.right * -sideOffset * 0.5f, 0f);
                    SpawnBullet(firePoint.position + firePoint.right * sideOffset * 0.5f, 0f);
                    SpawnBullet(firePoint.position + firePoint.right * sideOffset, spreadAngle);
                    // Side cannons firing straight up.
                    SpawnBullet(firePoint.position + firePoint.right * -sideOffset * 2f, 0f);
                    SpawnBullet(firePoint.position + firePoint.right * sideOffset * 2f, 0f);
                    break;
            }

            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.shootSFX, 0.6f);
            }

            ShowMuzzleFlash();
        }

        private void SpawnBullet(Vector3 position, float angleDegrees)
        {
            if (!BulletPool.HasInstance)
            {
                return;
            }

            Quaternion rotation = Quaternion.Euler(0f, 0f, angleDegrees);
            Bullet bullet = BulletPool.Instance.GetPlayerBullet(position, rotation);
            if (bullet != null)
            {
                bullet.Configure(bulletDamage, bulletSpeed);
            }
        }

        private void ShowMuzzleFlash()
        {
            if (muzzleFlash == null)
            {
                return;
            }
            StopCoroutine(nameof(MuzzleFlashRoutine));
            StartCoroutine(MuzzleFlashRoutine());
        }

        private IEnumerator MuzzleFlashRoutine()
        {
            muzzleFlash.SetActive(true);
            yield return new WaitForSeconds(muzzleFlashDuration);
            muzzleFlash.SetActive(false);
        }

        public void UpgradeWeapon()
        {
            weaponLevel = Mathf.Min(Constants.MaxWeaponLevel, weaponLevel + 1);
        }

        public void DowngradeWeapon()
        {
            weaponLevel = Mathf.Max(Constants.MinWeaponLevel, weaponLevel - 1);
        }

        public void ResetWeapon()
        {
            weaponLevel = Constants.MinWeaponLevel;
        }
    }
}
