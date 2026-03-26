using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player movement (WASD / Arrows), shooting (Space), invincibility frames,
/// and power-up effects. Attach to the Player GameObject.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float screenInset = 0.4f;  // keep sprite fully on-screen

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float fireRate      = 0.2f;   // seconds between shots
    public float bulletSpeed   = 14f;
    public int   bulletDamage  = 1;

    [Header("Invincibility")]
    public float invincibleDuration = 1.5f;
    public float flashInterval      = 0.1f;

    [Header("Power-up state (read-only in Inspector)")]
    public bool  hasSpreadShot;
    public bool  hasRapidFire;
    public bool  hasShield;

    private float fireCooldown;
    private bool  isInvincible;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Register bullet prefab with the static spawner
        if (bulletPrefab != null)
            BulletSpawner.SetPrefab(bulletPrefab);
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameManager.State.Playing) return;

        HandleMovement();
        HandleShooting();
    }

    // ── Movement ──────────────────────────────────────────────────────
    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 delta = new Vector3(h, v, 0f).normalized * moveSpeed * Time.deltaTime;
        transform.position += delta;

        if (ScreenBounds.Instance != null)
            transform.position = ScreenBounds.Instance.ClampToScreen(transform.position, screenInset);
    }

    // ── Shooting ──────────────────────────────────────────────────────
    private void HandleShooting()
    {
        fireCooldown -= Time.deltaTime;
        if (Input.GetKey(KeyCode.Space) && fireCooldown <= 0f)
        {
            float rate = hasRapidFire ? fireRate * 0.4f : fireRate;
            fireCooldown = rate;

            Vector3 spawnPos = transform.position + Vector3.up * 0.6f;

            if (hasSpreadShot)
                BulletSpawner.SpreadShot3(spawnPos, Vector2.up, bulletSpeed,
                                           bulletDamage, Bullet.Owner.Player);
            else
                BulletSpawner.SingleShot(spawnPos, Vector2.up, bulletSpeed,
                                          bulletDamage, Bullet.Owner.Player);

            if (AudioManager.Instance != null) AudioManager.Instance.PlayShoot();
        }
    }

    // ── Damage / invincibility ────────────────────────────────────────
    public void Hit(int damage)
    {
        if (isInvincible) return;

        if (hasShield)
        {
            hasShield = false;
            StartCoroutine(InvincibilityCoroutine());
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.TakeDamage(damage);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPlayerHit();

        StartCoroutine(InvincibilityCoroutine());
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float timer = 0f;
        while (timer < invincibleDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;  // flash
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }
        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    // ── Power-up application ─────────────────────────────────────────
    public void ApplyPowerUp(PowerUp.PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUp.PowerUpType.HealthRestore:
                if (GameManager.Instance != null) GameManager.Instance.RestoreHealth(1);
                break;
            case PowerUp.PowerUpType.SpreadShot:
                StartCoroutine(TimedPowerUp(type, duration));
                break;
            case PowerUp.PowerUpType.Shield:
                hasShield = true;   // one-hit shield, no timer
                break;
            case PowerUp.PowerUpType.RapidFire:
                StartCoroutine(TimedPowerUp(type, duration));
                break;
        }
    }

    private IEnumerator TimedPowerUp(PowerUp.PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUp.PowerUpType.SpreadShot: hasSpreadShot = true; break;
            case PowerUp.PowerUpType.RapidFire:  hasRapidFire  = true; break;
        }

        // Notify UI of active power-up
        if (UIManager.Instance != null)
            UIManager.Instance.ShowPowerUpTimer(type.ToString(), duration);

        yield return new WaitForSeconds(duration);

        switch (type)
        {
            case PowerUp.PowerUpType.SpreadShot: hasSpreadShot = false; break;
            case PowerUp.PowerUpType.RapidFire:  hasRapidFire  = false; break;
        }
    }

    // ── Collision with enemy body ────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Hit(1);
        }
    }
}
