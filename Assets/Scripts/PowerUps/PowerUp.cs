// ============================================================================
// PowerUp.cs — Collectible power-up items
// ============================================================================
using UnityEngine;

public enum PowerUpType
{
    WeaponUpgrade,
    Shield,
    Health,
    SpeedBoost,
    RapidFire,
    ExtraLife,
    Bomb        // clears all enemies on screen
}

public class PowerUp : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private PowerUpType type = PowerUpType.WeaponUpgrade;
    [SerializeField] private float fallSpeed = 1.5f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float rotateSpeed = 90f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    // Runtime
    private float spawnTime;
    private Vector3 startPos;

    // =========================================================================
    private void Start()
    {
        spawnTime = Time.time;
        startPos = transform.position;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Fall downward with gentle bobbing
        float bob = Mathf.Sin((Time.time - spawnTime) * bobFrequency) * bobAmplitude;
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
        
        // Rotate for visual flair
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

        // Destroy if off screen
        if (transform.position.y < -6f)
            Destroy(gameObject);

        // Pulsing scale for visibility
        float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.1f;
        transform.localScale = Vector3.one * pulse;
    }

    // =========================================================================
    // Collection
    // =========================================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        ApplyEffect(other.gameObject);

        if (SoundManager.Instance != null && pickupSound != null)
            SoundManager.Instance.PlaySFX(pickupSound, 0.7f);

        Destroy(gameObject);
    }

    private void ApplyEffect(GameObject player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        PlayerShooting shooting = player.GetComponent<PlayerShooting>();
        PlayerController controller = player.GetComponent<PlayerController>();

        switch (type)
        {
            case PowerUpType.WeaponUpgrade:
                if (shooting != null) shooting.UpgradeWeapon();
                break;

            case PowerUpType.Shield:
                if (health != null) health.AddShield(2);
                break;

            case PowerUpType.Health:
                if (health != null) health.Heal(1);
                break;

            case PowerUpType.SpeedBoost:
                if (controller != null)
                {
                    controller.SetSpeedMultiplier(1.5f);
                    // Reset after duration
                    StartCoroutine(ResetSpeedAfterDelay(controller, 8f));
                }
                break;

            case PowerUpType.RapidFire:
                if (shooting != null)
                {
                    shooting.SetFireRateMultiplier(2f);
                    StartCoroutine(ResetFireRateAfterDelay(shooting, 8f));
                }
                break;

            case PowerUpType.ExtraLife:
                if (GameManager.Instance != null)
                    GameManager.Instance.AddLife(1);
                break;

            case PowerUpType.Bomb:
                ActivateBomb();
                break;
        }

        // Score bonus for collecting
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(50);
    }

    // =========================================================================
    // Bomb: destroys all enemies on screen
    // =========================================================================
    private void ActivateBomb()
    {
        EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(999);
            }
        }

        // Also destroy all enemy bullets
        GameObject[] enemyBullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        foreach (var bullet in enemyBullets)
        {
            Destroy(bullet);
        }
    }

    // =========================================================================
    // Timed resets
    // =========================================================================
    private System.Collections.IEnumerator ResetSpeedAfterDelay(PlayerController ctrl, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ctrl != null) ctrl.SetSpeedMultiplier(1f);
    }

    private System.Collections.IEnumerator ResetFireRateAfterDelay(PlayerShooting shoot, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (shoot != null) shoot.SetFireRateMultiplier(1f);
    }

    // =========================================================================
    public PowerUpType Type => type;
}
