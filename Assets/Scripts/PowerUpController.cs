using UnityEngine;

/// <summary>
/// A collectible power-up that drifts downward and is picked up by the player.
/// Supports three types: WeaponUpgrade (spread shot), Health, and Shield.
/// </summary>
[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class PowerUpController : MonoBehaviour
{
    public enum PowerUpType { WeaponUpgrade, Health, Shield }

    [Header("Power-Up Config")]
    [SerializeField] private PowerUpType type = PowerUpType.WeaponUpgrade;
    [SerializeField] private float fallSpeed  = 2f;
    [SerializeField] private int   healAmount = 2;

    [Header("Visual")]
    [SerializeField] private Color weaponColor = new Color(1f, 0.6f, 0f);   // orange
    [SerializeField] private Color healthColor = new Color(0f, 1f, 0.3f);   // green
    [SerializeField] private Color shieldColor = new Color(0.3f, 0.6f, 1f); // blue

    private void Start()
    {
        // Tint sprite based on type
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            switch (type)
            {
                case PowerUpType.WeaponUpgrade: sr.color = weaponColor; break;
                case PowerUpType.Health:        sr.color = healthColor; break;
                case PowerUpType.Shield:        sr.color = shieldColor; break;
            }
        }

        // Auto-destroy after 10 seconds
        Destroy(gameObject, 10f);
    }

    private void Update()
    {
        // Bob slightly while falling
        float bob = Mathf.Sin(Time.time * 4f) * 0.3f;
        transform.position += new Vector3(bob * Time.deltaTime, -fallSpeed * Time.deltaTime, 0f);

        // Destroy if off-screen
        if (transform.position.y < -7f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        switch (type)
        {
            case PowerUpType.WeaponUpgrade:
                player.ActivateSpreadShot();
                break;
            case PowerUpType.Health:
                player.HealPlayer(healAmount);
                break;
            case PowerUpType.Shield:
                player.ActivateShield();
                break;
        }

        AudioManager.Instance?.PlaySFX("PowerUp");
        Destroy(gameObject);
    }
}
