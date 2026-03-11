using UnityEngine;

public enum PowerUpType
{
    Shield,
    RapidFire,
    Health,
    ExtraLife,
    ScoreBonus
}

public class PowerUp : MonoBehaviour, IPooledObject
{
    [Header("Power-Up Settings")]
    public PowerUpType powerUpType;
    public float duration = 5f;
    public int amount = 25; // For health or score
    public float moveSpeed = 2f;
    public float lifetime = 10f;

    [Header("Visual")]
    public Color shieldColor = Color.cyan;
    public Color rapidFireColor = Color.yellow;
    public Color healthColor = Color.green;
    public Color extraLifeColor = Color.magenta;
    public Color scoreBonusColor = Color.white;

    [Header("Audio")]
    public AudioClip pickupSound;

    private SpriteRenderer spriteRenderer;
    private float spawnTime;
    private bool isActive = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (spawnTime == 0)
        {
            OnObjectSpawn();
        }
    }

    public void OnObjectSpawn()
    {
        isActive = true;
        spawnTime = Time.time;
        UpdateVisual();
    }

    public void Initialize(PowerUpType type)
    {
        powerUpType = type;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        Color color = powerUpType switch
        {
            PowerUpType.Shield => shieldColor,
            PowerUpType.RapidFire => rapidFireColor,
            PowerUpType.Health => healthColor,
            PowerUpType.ExtraLife => extraLifeColor,
            PowerUpType.ScoreBonus => scoreBonusColor,
            _ => Color.white
        };

        spriteRenderer.color = color;
    }

    private void Update()
    {
        if (!isActive) return;

        // Move down slowly
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

        // Rotate for visual effect
        transform.Rotate(Vector3.forward * 90f * Time.deltaTime);

        // Check lifetime
        if (Time.time - spawnTime > lifetime)
        {
            Deactivate();
        }

        // Check bounds
        if (transform.position.y < -6f)
        {
            Deactivate();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            ApplyPowerUp(other.gameObject);
            Deactivate();
        }
    }

    private void ApplyPowerUp(GameObject player)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController == null) return;

        switch (powerUpType)
        {
            case PowerUpType.Shield:
                playerController.ActivateShield(duration);
                break;

            case PowerUpType.RapidFire:
                playerController.ActivateRapidFire(duration);
                break;

            case PowerUpType.Health:
                playerController.RestoreHealth(amount);
                break;

            case PowerUpType.ExtraLife:
                playerController.AddLife();
                break;

            case PowerUpType.ScoreBonus:
                ScoreManager.Instance?.AddScore(amount * 10);
                break;
        }

        // Play pickup sound
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    private void Deactivate()
    {
        isActive = false;

        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool("PowerUp", gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
