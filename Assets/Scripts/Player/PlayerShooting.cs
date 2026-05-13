// ============================================================================
// PlayerShooting.cs - Handles player weapon firing and upgrades
// ============================================================================
using UnityEngine;

/// <summary>
/// Fires bullets from the player ship. Supports three weapon levels that
/// increase the number and spread of projectiles.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Prefab for the player's bullet.")]
    [SerializeField] private GameObject bulletPrefab;
    [Tooltip("Offset from the ship center where bullets spawn.")]
    [SerializeField] private Vector3 fireOffset = new Vector3(0f, 0.8f, 0f);
    [Tooltip("Base fire rate in shots per second.")]
    [SerializeField] private float baseFireRate = 5f;

    [Header("Weapon Levels")]
    [Tooltip("Current weapon power level (1 = single shot, 2 = double, 3 = triple spread).")]
    [SerializeField] private int weaponLevel = 1;
    [Tooltip("Maximum weapon level.")]
    [SerializeField] private int maxWeaponLevel = 3;
    [Tooltip("Spread angle in degrees for triple-shot.")]
    [SerializeField] private float spreadAngle = 15f;

    private float fireCooldown;
    private float fireTimer;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Start()
    {
        fireCooldown = 1f / baseFireRate;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        fireTimer -= Time.deltaTime;

        // Fire on Space, Left Mouse, or Left Ctrl (held or pressed).
        bool fireInput = Input.GetKey(KeyCode.Space) ||
                         Input.GetMouseButton(0) ||
                         Input.GetKey(KeyCode.LeftControl);

        if (fireInput && fireTimer <= 0f)
        {
            Fire();
            fireTimer = fireCooldown;
        }
    }

    // ========================================================================
    // Firing Logic
    // ========================================================================

    /// <summary>
    /// Fires bullets based on the current weapon level.
    /// Level 1: single shot straight up.
    /// Level 2: two parallel shots.
    /// Level 3: three-way spread.
    /// </summary>
    private void Fire()
    {
        Vector3 spawnPos = transform.position + fireOffset;

        switch (weaponLevel)
        {
            case 1:
                SpawnBullet(spawnPos, Vector2.up);
                break;

            case 2:
                SpawnBullet(spawnPos + Vector3.left * 0.3f, Vector2.up);
                SpawnBullet(spawnPos + Vector3.right * 0.3f, Vector2.up);
                break;

            case 3:
            default:
                SpawnBullet(spawnPos, Vector2.up);
                SpawnBullet(spawnPos, RotateDirection(Vector2.up, spreadAngle));
                SpawnBullet(spawnPos, RotateDirection(Vector2.up, -spreadAngle));
                break;
        }

        AudioManager.Instance?.PlaySFX(AudioManager.SFX.PlayerShoot);
    }

    /// <summary>
    /// Instantiates or retrieves a bullet from the pool and initializes it.
    /// </summary>
    private void SpawnBullet(Vector3 position, Vector2 direction)
    {
        GameObject bulletObj = null;

        // Try pool first.
        if (PoolManager.Instance != null)
        {
            bulletObj = PoolManager.Instance.Get("PlayerBulletPool", position, Quaternion.identity);
        }

        // Fallback: instantiate directly.
        if (bulletObj == null && bulletPrefab != null)
        {
            bulletObj = Instantiate(bulletPrefab, position, Quaternion.identity);
        }

        if (bulletObj == null) return;

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(direction, true);
        }
    }

    // ========================================================================
    // Weapon Upgrades
    // ========================================================================

    /// <summary>
    /// Increases the weapon level by one step (capped at maxWeaponLevel).
    /// </summary>
    public void UpgradeWeapon()
    {
        if (weaponLevel < maxWeaponLevel)
        {
            weaponLevel++;
            // Slightly increase fire rate with each level.
            fireCooldown = 1f / (baseFireRate + (weaponLevel - 1) * 1.5f);
            AudioManager.Instance?.PlaySFX(AudioManager.SFX.WeaponUpgrade);
        }
    }

    /// <summary>Current weapon level (read-only).</summary>
    public int WeaponLevel => weaponLevel;

    // ========================================================================
    // Helpers
    // ========================================================================

    /// <summary>
    /// Rotates a 2D direction vector by the given angle in degrees.
    /// </summary>
    private Vector2 RotateDirection(Vector2 dir, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            dir.x * cos - dir.y * sin,
            dir.x * sin + dir.y * cos
        );
    }
}
