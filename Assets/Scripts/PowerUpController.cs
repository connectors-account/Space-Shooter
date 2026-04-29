using UnityEngine;

/// <summary>
/// Defines pickup behavior and effect values for each power-up type.
/// </summary>
public class PowerUpController : MonoBehaviour
{
    public enum PowerUpType
    {
        Shield,
        RapidFire,
        Health
    }

    [Header("Power-up Config")]
    [SerializeField] private PowerUpType powerUpType;
    [SerializeField] private float fallSpeed = 2.5f;
    [SerializeField] private float lifeTime = 12f;
    [SerializeField] private int healthAmount = 25;
    [SerializeField] private float shieldDuration = 6f;
    [SerializeField] private float rapidFireDuration = 6f;

    public PowerUpType Type => powerUpType;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.down * (fallSpeed * Time.deltaTime), Space.World);
    }

    public void ApplyTo(PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        switch (powerUpType)
        {
            case PowerUpType.Shield:
                player.ActivateShield(shieldDuration);
                break;
            case PowerUpType.RapidFire:
                player.ActivateRapidFire(rapidFireDuration);
                break;
            case PowerUpType.Health:
                player.Heal(healthAmount);
                break;
        }
    }
}
