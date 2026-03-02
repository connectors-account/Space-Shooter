using UnityEngine;

/// <summary>
/// PowerUp script handles collectible power-ups.
/// Currently implements health pickup, easily extendable for other types.
/// </summary>
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        Health,
        RapidFire,
        Shield
    }

    [Header("Power-Up Settings")]
    [Tooltip("Type of power-up")]
    public PowerUpType powerUpType = PowerUpType.Health;
    
    [Tooltip("Value of the power-up (health amount, duration, etc.)")]
    public int value = 25;
    
    [Tooltip("Fall speed")]
    public float fallSpeed = 2f;
    
    [Tooltip("Time before auto-destroy")]
    public float lifetime = 10f;

    [Header("Visual Settings")]
    [Tooltip("Rotation speed for visual effect")]
    public float rotationSpeed = 90f;
    
    [Tooltip("Bob amplitude")]
    public float bobAmplitude = 0.2f;
    
    [Tooltip("Bob frequency")]
    public float bobFrequency = 2f;

    [Header("Audio")]
    public AudioClip pickupSound;

    private float spawnTime;
    private float startY;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spawnTime = Time.time;
        startY = transform.position.y;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Set color based on power-up type
        SetPowerUpColor();
    }

    void Update()
    {
        // Move downward
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        
        // Add rotation for visual effect
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // Add bobbing effect (optional)
        // float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        
        // Auto-destroy after lifetime or if off-screen
        if (Time.time - spawnTime > lifetime || transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Set color based on power-up type
    /// </summary>
    void SetPowerUpColor()
    {
        if (spriteRenderer == null) return;
        
        switch (powerUpType)
        {
            case PowerUpType.Health:
                spriteRenderer.color = Color.green;
                break;
            case PowerUpType.RapidFire:
                spriteRenderer.color = Color.yellow;
                break;
            case PowerUpType.Shield:
                spriteRenderer.color = Color.blue;
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyPowerUp(other.gameObject);
            
            // Play pickup sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Apply the power-up effect to the player
    /// </summary>
    void ApplyPowerUp(GameObject player)
    {
        switch (powerUpType)
        {
            case PowerUpType.Health:
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.Heal(value);
                }
                break;
                
            case PowerUpType.RapidFire:
                // Could implement temporary fire rate increase
                PlayerController controller = player.GetComponent<PlayerController>();
                if (controller != null)
                {
                    // Implement rapid fire power-up
                    // controller.ActivateRapidFire(value); // value = duration
                }
                break;
                
            case PowerUpType.Shield:
                // Could implement temporary shield
                break;
        }
    }
}
