using UnityEngine;

/// <summary>
/// Power-up item that drifts downward and applies an effect when collected by the player.
/// </summary>
public class PowerUp : MonoBehaviour, IPoolable
{
    public enum PowerUpType
    {
        RapidFire,
        Shield,
        SpreadShot,
        Health
    }

    [Header("Power-Up Settings")]
    public PowerUpType type = PowerUpType.RapidFire;
    public float fallSpeed = 2f;
    public string poolTag = "PowerUp";

    public void OnSpawnFromPool()
    {
        // Color is set by PowerUpSpawner based on type
    }

    /// <summary>
    /// Configure the power-up type and update visual.
    /// </summary>
    public void SetType(PowerUpType newType)
    {
        type = newType;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            switch (type)
            {
                case PowerUpType.RapidFire:
                    sr.color = Color.yellow;
                    break;
                case PowerUpType.Shield:
                    sr.color = Color.cyan;
                    break;
                case PowerUpType.SpreadShot:
                    sr.color = new Color(1f, 0.5f, 0f); // Orange
                    break;
                case PowerUpType.Health:
                    sr.color = Color.green;
                    break;
            }
        }
    }

    private void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // Rotate for visual flair
        transform.Rotate(0, 0, 90f * Time.deltaTime);

        if (GameBounds.Instance != null && GameBounds.Instance.IsOutOfBounds(transform.position, 2f))
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        ApplyEffect(other.gameObject);
        AudioManager.Instance?.PlaySound("PowerUp");
        ReturnToPool();
    }

    private void ApplyEffect(GameObject player)
    {
        switch (type)
        {
            case PowerUpType.RapidFire:
                PlayerShooting shooting = player.GetComponent<PlayerShooting>();
                if (shooting != null) shooting.ActivateRapidFire();
                break;

            case PowerUpType.Shield:
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null) health.ActivateShield();
                break;

            case PowerUpType.SpreadShot:
                PlayerShooting spread = player.GetComponent<PlayerShooting>();
                if (spread != null) spread.ActivateSpreadShot();
                break;

            case PowerUpType.Health:
                PlayerHealth hp = player.GetComponent<PlayerHealth>();
                if (hp != null) hp.Heal(1);
                break;
        }
    }

    private void ReturnToPool()
    {
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
