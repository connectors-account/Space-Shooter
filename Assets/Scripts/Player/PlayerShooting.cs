using UnityEngine;

/// <summary>
/// Handles player shooting mechanics including fire rate, rapid fire, and spread shot power-ups.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float fireRate = 0.25f;
    public float bulletSpeed = 12f;
    public string bulletPoolTag = "PlayerBullet";

    [Header("Power-Up Durations")]
    public float rapidFireDuration = 5f;
    public float spreadShotDuration = 5f;

    // Power-up states
    public bool HasRapidFire { get; private set; }
    public bool HasSpreadShot { get; private set; }

    private float nextFireTime;
    private float rapidFireTimer;
    private float spreadShotTimer;
    private float currentFireRate;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        UpdatePowerUps();
        HandleShooting();
    }

    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + currentFireRate;
        }
    }

    private void Fire()
    {
        AudioManager.Instance?.PlaySound("PlayerShoot");

        if (HasSpreadShot)
        {
            // Fire three bullets in a spread pattern
            SpawnBullet(transform.position + Vector3.up * 0.5f, 0f);
            SpawnBullet(transform.position + Vector3.up * 0.3f, 15f);
            SpawnBullet(transform.position + Vector3.up * 0.3f, -15f);
        }
        else
        {
            SpawnBullet(transform.position + Vector3.up * 0.5f, 0f);
        }
    }

    private void SpawnBullet(Vector3 position, float angleOffset)
    {
        if (ObjectPool.Instance == null) return;

        GameObject bullet = ObjectPool.Instance.Spawn(bulletPoolTag, position, Quaternion.identity);
        if (bullet != null)
        {
            Bullet bulletComp = bullet.GetComponent<Bullet>();
            if (bulletComp != null)
            {
                Vector2 direction = Quaternion.Euler(0, 0, angleOffset) * Vector2.up;
                bulletComp.Initialize(direction, bulletSpeed, 1, true);
            }
        }
    }

    private void UpdatePowerUps()
    {
        // Rapid fire
        if (HasRapidFire)
        {
            rapidFireTimer -= Time.deltaTime;
            if (rapidFireTimer <= 0f)
            {
                HasRapidFire = false;
            }
        }

        // Spread shot
        if (HasSpreadShot)
        {
            spreadShotTimer -= Time.deltaTime;
            if (spreadShotTimer <= 0f)
            {
                HasSpreadShot = false;
            }
        }

        currentFireRate = HasRapidFire ? fireRate * 0.3f : fireRate;
    }

    public void ActivateRapidFire()
    {
        HasRapidFire = true;
        rapidFireTimer = rapidFireDuration;
    }

    public void ActivateSpreadShot()
    {
        HasSpreadShot = true;
        spreadShotTimer = spreadShotDuration;
    }

    /// <summary>
    /// Reset shooting state for a new game.
    /// </summary>
    public void ResetShooting()
    {
        HasRapidFire = false;
        HasSpreadShot = false;
        nextFireTime = 0f;
    }
}
