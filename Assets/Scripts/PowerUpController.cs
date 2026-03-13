using UnityEngine;

public enum PowerUpType
{
    RapidFire,
    Shield,
    Health
}

public class PowerUpController : MonoBehaviour
{
    [Header("Power-up Settings")]
    public PowerUpType powerUpType = PowerUpType.RapidFire;
    public float moveSpeed = 2f;
    public float duration = 5f;
    public int healAmount = 1;
    public float destroyYPosition = -6f;

    [Header("Visual Settings")]
    public float bobAmplitude = 0.2f;
    public float bobFrequency = 3f;
    public float rotationSpeed = 90f;

    private float startY;
    private float elapsedTime = 0f;
    private SpriteRenderer spriteRenderer;
    private AudioManager audioManager;

    void Start()
    {
        startY = transform.position.y;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioManager = FindObjectOfType<AudioManager>();

        RandomizePowerUpType();
        UpdateVisual();
    }

    void RandomizePowerUpType()
    {
        int randomType = Random.Range(0, 3);
        powerUpType = (PowerUpType)randomType;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        float bobOffset = Mathf.Sin(elapsedTime * bobFrequency) * bobAmplitude;
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        if (transform.position.y < destroyYPosition)
        {
            Destroy(gameObject);
        }
    }

    void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        switch (powerUpType)
        {
            case PowerUpType.RapidFire:
                spriteRenderer.color = Color.yellow;
                break;
            case PowerUpType.Shield:
                spriteRenderer.color = Color.blue;
                break;
            case PowerUpType.Health:
                spriteRenderer.color = Color.green;
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyPowerUp(player);
            }

            if (audioManager != null)
                audioManager.PlayPowerUpSound();

            Destroy(gameObject);
        }
    }

    void ApplyPowerUp(PlayerController player)
    {
        switch (powerUpType)
        {
            case PowerUpType.RapidFire:
                player.ActivateRapidFire(duration);
                break;
            case PowerUpType.Shield:
                player.ActivateShield(duration);
                break;
            case PowerUpType.Health:
                player.Heal(healAmount);
                break;
        }
    }
}
