using UnityEngine;

/// <summary>
/// Controls power-up behavior including movement, collection, and effect application.
/// </summary>
public class PowerUpController : MonoBehaviour
{
    public enum PowerUpType
    {
        Shield,     // Temporary invincibility
        RapidFire,  // Increased fire rate
        Health      // Restore health
    }
    
    [Header("Power-Up Settings")]
    [SerializeField] private PowerUpType powerUpType = PowerUpType.Health;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private int healthRestoreAmount = 30;
    
    [Header("Visual")]
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseScale = 0.2f;
    
    // Colors for each type
    private readonly Color shieldColor = new Color(0.3f, 0.7f, 1f);
    private readonly Color rapidFireColor = new Color(1f, 0.8f, 0.2f);
    private readonly Color healthColor = new Color(0.3f, 1f, 0.3f);
    
    // Components
    private SpriteRenderer spriteRenderer;
    
    // State
    private float timeAlive;
    private Vector3 baseScale;
    
    public PowerUpType Type => powerUpType;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }
    
    private void Start()
    {
        UpdateVisual();
    }
    
    private void Update()
    {
        // Move down
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        
        // Rotate
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        
        // Pulse effect
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
        transform.localScale = baseScale * pulse;
        
        // Check lifetime
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
        {
            Destroy(gameObject);
        }
        
        // Blink when about to expire
        if (lifetime - timeAlive < 3f)
        {
            if (spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time * 5f, 1f);
                Color c = spriteRenderer.color;
                c.a = 0.3f + alpha * 0.7f;
                spriteRenderer.color = c;
            }
        }
        
        // Destroy if off screen
        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initialize power-up with specific type
    /// </summary>
    public void Initialize(PowerUpType type)
    {
        powerUpType = type;
        UpdateVisual();
    }
    
    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;
        
        switch (powerUpType)
        {
            case PowerUpType.Shield:
                spriteRenderer.color = shieldColor;
                break;
            case PowerUpType.RapidFire:
                spriteRenderer.color = rapidFireColor;
                break;
            case PowerUpType.Health:
                spriteRenderer.color = healthColor;
                break;
        }
    }
    
    /// <summary>
    /// Called when collected by player
    /// </summary>
    public void Collect(PlayerController player)
    {
        if (player == null) return;
        
        switch (powerUpType)
        {
            case PowerUpType.Shield:
                player.ActivateShield();
                break;
            case PowerUpType.RapidFire:
                player.ActivateRapidFire();
                break;
            case PowerUpType.Health:
                player.RestoreHealth(healthRestoreAmount);
                break;
        }
        
        AudioManager.Instance?.PlaySound("PowerUp");
        GameManager.Instance?.AddScore(50);
        
        // Visual feedback
        SpawnCollectEffect();
        
        Destroy(gameObject);
    }
    
    private void SpawnCollectEffect()
    {
        // Create a simple particle burst effect
        GameObject effect = new GameObject("CollectEffect");
        effect.transform.position = transform.position;
        
        // Add particle system or destroy after delay
        Destroy(effect, 0.5f);
    }
}
