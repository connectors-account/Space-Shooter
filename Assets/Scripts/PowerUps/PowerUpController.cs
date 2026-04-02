using UnityEngine;

/// <summary>
/// Controls power-up behavior: floating downward, bobbing, and applying effects on collection.
/// </summary>
public class PowerUpController : MonoBehaviour
{
    public enum PowerUpType { WeaponUpgrade, Shield, Health, SpeedBoost, ScoreBonus }

    [Header("Power-Up Config")]
    [SerializeField] private PowerUpType powerUpType = PowerUpType.WeaponUpgrade;

    [Header("Movement")]
    [SerializeField] private float fallSpeed = 1.5f;
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float bobFrequency = 2f;

    [Header("Effect Values")]
    [SerializeField] private float effectDuration = 8f;
    [SerializeField] private int healAmount = 2;
    [SerializeField] private int scoreBonusAmount = 500;

    [Header("Visual")]
    [SerializeField] private float rotateSpeed = 90f;

    private float startY;
    private float spawnTime;

    private void Start()
    {
        startY = transform.position.y;
        spawnTime = Time.time;

        // Set color based on type
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            switch (powerUpType)
            {
                case PowerUpType.WeaponUpgrade:
                    sr.color = new Color(1f, 0.5f, 0f); // Orange
                    break;
                case PowerUpType.Shield:
                    sr.color = new Color(0.3f, 0.7f, 1f); // Light Blue
                    break;
                case PowerUpType.Health:
                    sr.color = new Color(0f, 1f, 0.3f); // Green
                    break;
                case PowerUpType.SpeedBoost:
                    sr.color = new Color(1f, 1f, 0f); // Yellow
                    break;
                case PowerUpType.ScoreBonus:
                    sr.color = new Color(1f, 0f, 1f); // Magenta
                    break;
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

        float elapsed = Time.time - spawnTime;

        // Move downward with bobbing
        Vector3 pos = transform.position;
        pos.y -= fallSpeed * Time.deltaTime;
        pos.x += Mathf.Sin(elapsed * bobFrequency) * bobAmplitude * Time.deltaTime;
        transform.position = pos;

        // Gentle rotation
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        // Destroy if off-screen
        if (pos.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        ApplyEffect(player);
        AudioManager.Instance?.PlaySFX("PowerUp");
        Destroy(gameObject);
    }

    private void ApplyEffect(PlayerController player)
    {
        switch (powerUpType)
        {
            case PowerUpType.WeaponUpgrade:
                player.UpgradeWeapon(effectDuration);
                break;

            case PowerUpType.Shield:
                player.ActivateShield(effectDuration);
                break;

            case PowerUpType.Health:
                player.Heal(healAmount);
                break;

            case PowerUpType.SpeedBoost:
                // Speed boost - could be implemented as a separate system
                // For now, give weapon upgrade as a bonus
                player.UpgradeWeapon(effectDuration * 0.5f);
                break;

            case PowerUpType.ScoreBonus:
                GameManager.Instance?.AddScore(scoreBonusAmount);
                break;
        }
    }
}
