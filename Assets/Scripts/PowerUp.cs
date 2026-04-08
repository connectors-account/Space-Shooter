using UnityEngine;

/// <summary>
/// A collectible power-up that drifts downward. Applies its effect on pickup.
/// </summary>
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { Shield, RapidFire, Health }

    public PowerUpType type = PowerUpType.Health;
    public float driftSpeed = 2f;
    public float lifetime = 10f;

    void Start()
    {
        gameObject.tag = "PowerUp";
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += Vector3.down * driftSpeed * Time.deltaTime;

        // Slight bobbing
        float bob = Mathf.Sin(Time.time * 4f) * 0.3f;
        transform.localScale = Vector3.one * (1f + bob * 0.1f);

        if (transform.position.y < -7f)
            Destroy(gameObject);
    }

    /// <summary>Apply this power-up to the given player.</summary>
    public void Apply(PlayerController player)
    {
        switch (type)
        {
            case PowerUpType.Shield:
                player.ActivateShield();
                break;
            case PowerUpType.RapidFire:
                player.ActivateRapidFire(8f);
                break;
            case PowerUpType.Health:
                player.Heal(2);
                break;
        }

        AudioManager.PlaySfx("PowerUp");
        Destroy(gameObject);
    }
}
