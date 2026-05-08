using UnityEngine;

/// <summary>
/// Handles player shooting mechanics, weapon levels, and fire rate.
/// Supports multiple weapon upgrade levels with different bullet patterns.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private float baseFireRate = 0.2f;
    [SerializeField] private Transform[] firePoints; // Assigned at runtime

    [Header("Weapon")]
    [SerializeField] private int weaponLevel = 1; // 1-4
    [SerializeField] private int maxWeaponLevel = 4;

    private float fireCooldown;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        fireCooldown -= Time.deltaTime;

        if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && fireCooldown <= 0f)
        {
            Fire();
            fireCooldown = GetFireRate();
        }
    }

    private void Fire()
    {
        if (ObjectPool.Instance == null) return;

        AudioManager.Instance?.PlaySFX("PlayerShoot");

        switch (weaponLevel)
        {
            case 1: // Single shot
                SpawnBullet(Vector2.up, 0f);
                break;
            case 2: // Double shot
                SpawnBullet(Vector2.up, -0.25f);
                SpawnBullet(Vector2.up, 0.25f);
                break;
            case 3: // Triple spread
                SpawnBullet(Vector2.up, 0f);
                SpawnBullet(RotateVector(Vector2.up, 10f), -0.2f);
                SpawnBullet(RotateVector(Vector2.up, -10f), 0.2f);
                break;
            case 4: // Quad + spread
                SpawnBullet(Vector2.up, -0.3f);
                SpawnBullet(Vector2.up, 0.3f);
                SpawnBullet(RotateVector(Vector2.up, 15f), -0.15f);
                SpawnBullet(RotateVector(Vector2.up, -15f), 0.15f);
                break;
        }
    }

    private void SpawnBullet(Vector2 direction, float xOffset)
    {
        Vector3 spawnPos = transform.position + new Vector3(xOffset, 0.5f, 0f);
        GameObject bullet = ObjectPool.Instance.Spawn(Tags.PlayerBullet, spawnPos, Quaternion.identity);
        if (bullet != null)
        {
            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null)
            {
                b.Initialize(direction, true);
            }
        }
    }

    private float GetFireRate()
    {
        // Higher weapon levels shoot slightly faster
        return baseFireRate * (1f - (weaponLevel - 1) * 0.05f);
    }

    public void UpgradeWeapon()
    {
        if (weaponLevel < maxWeaponLevel)
        {
            weaponLevel++;
            AudioManager.Instance?.PlaySFX("PowerUp");
        }
    }

    public int GetWeaponLevel() => weaponLevel;

    public void ResetWeapon()
    {
        weaponLevel = 1;
    }

    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
