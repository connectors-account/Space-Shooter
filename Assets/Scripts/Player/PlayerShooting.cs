// ============================================================================
// PlayerShooting.cs — Handles player weapon firing and power-up weapon states
// ============================================================================
using UnityEngine;

public enum WeaponType
{
    Single,
    Double,
    Triple,
    Spread,
    Laser
}

public class PlayerShooting : MonoBehaviour
{
    [Header("Bullet Prefabs")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject laserPrefab;

    [Header("Fire Points")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform firePointLeft;
    [SerializeField] private Transform firePointRight;

    [Header("Settings")]
    [SerializeField] private float fireRate = 0.15f;
    [SerializeField] private float bulletSpeed = 12f;
    [SerializeField] private float spreadAngle = 15f;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;

    // Runtime
    private WeaponType currentWeapon = WeaponType.Single;
    private float nextFireTime;
    private float weaponTimer; // power-up weapons expire
    private bool canShoot = true;

    // =========================================================================
    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
        if (!canShoot) return;

        // Weapon timer countdown
        if (currentWeapon != WeaponType.Single && weaponTimer > 0)
        {
            weaponTimer -= Time.deltaTime;
            if (weaponTimer <= 0)
            {
                currentWeapon = WeaponType.Single;
            }
        }

        // Auto-fire when holding space/left-click, or manual tap
        if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    // =========================================================================
    // Firing
    // =========================================================================
    private void Fire()
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;

        switch (currentWeapon)
        {
            case WeaponType.Single:
                SpawnBullet(spawnPos, Vector2.up);
                break;

            case WeaponType.Double:
                Vector3 leftPos = firePointLeft != null ? firePointLeft.position : spawnPos + Vector3.left * 0.25f;
                Vector3 rightPos = firePointRight != null ? firePointRight.position : spawnPos + Vector3.right * 0.25f;
                SpawnBullet(leftPos, Vector2.up);
                SpawnBullet(rightPos, Vector2.up);
                break;

            case WeaponType.Triple:
                SpawnBullet(spawnPos, Vector2.up);
                SpawnBullet(spawnPos, RotateDirection(Vector2.up, spreadAngle));
                SpawnBullet(spawnPos, RotateDirection(Vector2.up, -spreadAngle));
                break;

            case WeaponType.Spread:
                for (int i = -2; i <= 2; i++)
                {
                    SpawnBullet(spawnPos, RotateDirection(Vector2.up, i * 12f));
                }
                break;

            case WeaponType.Laser:
                SpawnLaser(spawnPos);
                break;
        }

        // Play sound
        if (SoundManager.Instance != null && shootSound != null)
            SoundManager.Instance.PlaySFX(shootSound, 0.3f);
    }

    private void SpawnBullet(Vector3 position, Vector2 direction)
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(direction, bulletSpeed, true);
        }
        else
        {
            // Fallback: just set velocity on Rigidbody2D
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = direction.normalized * bulletSpeed;
        }
    }

    private void SpawnLaser(Vector3 position)
    {
        GameObject prefab = laserPrefab != null ? laserPrefab : bulletPrefab;
        if (prefab == null) return;

        GameObject laser = Instantiate(prefab, position, Quaternion.identity);
        Bullet bulletScript = laser.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(Vector2.up, bulletSpeed * 1.5f, true, 3); // 3x damage
        }
        laser.transform.localScale = new Vector3(0.5f, 2f, 1f); // elongated look
    }

    // =========================================================================
    // Weapon Upgrades (called by PowerUp system)
    // =========================================================================
    public void SetWeapon(WeaponType weapon, float duration = 10f)
    {
        currentWeapon = weapon;
        weaponTimer = duration;
    }

    public void UpgradeWeapon()
    {
        if (currentWeapon < WeaponType.Spread)
        {
            currentWeapon++;
            weaponTimer = 15f;
        }
    }

    public void SetFireRateMultiplier(float multiplier)
    {
        fireRate = 0.15f / multiplier;
    }

    public void SetCanShoot(bool value) => canShoot = value;

    public WeaponType CurrentWeapon => currentWeapon;

    // =========================================================================
    // Utility
    // =========================================================================
    private Vector2 RotateDirection(Vector2 dir, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(dir.x * cos - dir.y * sin, dir.x * sin + dir.y * cos);
    }
}
