using UnityEngine;

/// <summary>
/// Power-up pickup behavior. Drifts downward and applies effect on player contact.
/// Types: WeaponUpgrade, Shield, Health
/// </summary>
public class PowerUp : MonoBehaviour, IPoolable
{
    public enum PowerUpType
    {
        WeaponUpgrade,
        Shield,
        Health
    }

    [Header("Settings")]
    [SerializeField] private PowerUpType type = PowerUpType.WeaponUpgrade;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float bobFrequency = 3f;

    private float spawnTime;
    private Vector2 startPos;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void OnSpawnFromPool()
    {
        spawnTime = Time.time;
        startPos = transform.position;
    }

    public void OnReturnToPool() { }

    public void SetType(PowerUpType newType)
    {
        type = newType;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        switch (type)
        {
            case PowerUpType.WeaponUpgrade:
                spriteRenderer.color = new Color(1f, 0.5f, 0f); // Orange
                break;
            case PowerUpType.Shield:
                spriteRenderer.color = new Color(0.3f, 0.5f, 1f); // Blue
                break;
            case PowerUpType.Health:
                spriteRenderer.color = new Color(0f, 1f, 0.3f); // Green
                break;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        // Drift downward with a gentle bob
        float elapsed = Time.time - spawnTime;
        float bob = Mathf.Sin(elapsed * bobFrequency) * bobAmplitude;
        float newY = startPos.y - moveSpeed * elapsed;
        transform.position = new Vector2(startPos.x + bob, newY);

        // Slight rotation for visual flair
        transform.Rotate(0f, 0f, 90f * Time.deltaTime);

        // Despawn if out of bounds
        if (GameManager.Instance != null && !GameManager.Instance.IsInBounds(transform.position, 1f))
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(Tags.Player)) return;

        ApplyEffect(other.gameObject);
        AudioManager.Instance?.PlaySFX("PowerUp");
        ReturnToPool();
    }

    private void ApplyEffect(GameObject player)
    {
        switch (type)
        {
            case PowerUpType.WeaponUpgrade:
                PlayerShooting shooting = player.GetComponent<PlayerShooting>();
                if (shooting != null) shooting.UpgradeWeapon();
                break;

            case PowerUpType.Shield:
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null) health.ActivateShield();
                break;

            case PowerUpType.Health:
                PlayerHealth hp = player.GetComponent<PlayerHealth>();
                if (hp != null) hp.Heal(2);
                break;
        }
    }

    private void ReturnToPool()
    {
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Despawn(Tags.PowerUp, gameObject);
        else
            gameObject.SetActive(false);
    }
}
