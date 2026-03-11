using UnityEngine;

/// <summary>
/// Base class for power-up items.
/// Attach to power-up GameObjects.
/// </summary>
public class Powerup : MonoBehaviour
{
    /// <summary>
    /// Types of power-ups available.
    /// </summary>
    public enum PowerupType
    {
        Health,         // Restore health
        SpeedBoost,     // Increase movement speed
        RapidFire,      // Increase fire rate
        SpreadShot,     // Fire multiple bullets
        Shield,         // Temporary invincibility
        ScoreBonus      // Bonus points
    }
    
    [Header("Powerup Settings")]
    [Tooltip("Type of power-up")]
    [SerializeField] private PowerupType type = PowerupType.Health;
    
    [Tooltip("Value/amount of the power-up effect")]
    [SerializeField] private float value = 25f;
    
    [Tooltip("Duration of temporary effects (seconds)")]
    [SerializeField] private float duration = 5f;
    
    [Header("Movement")]
    [Tooltip("Speed at which power-up moves down")]
    [SerializeField] private float fallSpeed = 2f;
    
    [Tooltip("Bobbing amplitude")]
    [SerializeField] private float bobAmplitude = 0.2f;
    
    [Tooltip("Bobbing frequency")]
    [SerializeField] private float bobFrequency = 2f;
    
    [Header("Lifetime")]
    [Tooltip("Time before power-up disappears")]
    [SerializeField] private float lifetime = 10f;
    
    // Internal state
    private float spawnY;
    private float timeAlive;
    
    /// <summary>
    /// Initialize on start.
    /// </summary>
    private void Start()
    {
        spawnY = transform.position.y;
        timeAlive = 0f;
        
        // Set color based on type
        SetColorByType();
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    /// <summary>
    /// Update movement.
    /// </summary>
    private void Update()
    {
        timeAlive += Time.deltaTime;
        
        // Move down with bobbing
        float newY = spawnY - (fallSpeed * timeAlive);
        float bob = Mathf.Sin(timeAlive * bobFrequency) * bobAmplitude;
        
        transform.position = new Vector3(
            transform.position.x + bob * Time.deltaTime,
            newY,
            transform.position.z
        );
        
        // Rotate for visual effect
        transform.Rotate(0f, 0f, 90f * Time.deltaTime);
        
        // Destroy if off screen
        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Handle collision with player.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyEffect(other.gameObject);
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Apply the power-up effect to the player.
    /// </summary>
    private void ApplyEffect(GameObject player)
    {
        switch (type)
        {
            case PowerupType.Health:
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.Heal(Mathf.RoundToInt(value));
                }
                break;
                
            case PowerupType.SpeedBoost:
                PlayerController controller = player.GetComponent<PlayerController>();
                if (controller != null)
                {
                    StartCoroutine(TemporarySpeedBoost(controller));
                }
                break;
                
            case PowerupType.RapidFire:
                PlayerShooting shooting = player.GetComponent<PlayerShooting>();
                if (shooting != null)
                {
                    StartCoroutine(TemporaryRapidFire(shooting));
                }
                break;
                
            case PowerupType.SpreadShot:
                PlayerShooting shootingSpread = player.GetComponent<PlayerShooting>();
                if (shootingSpread != null)
                {
                    StartCoroutine(TemporarySpreadShot(shootingSpread));
                }
                break;
                
            case PowerupType.Shield:
                PlayerHealth healthShield = player.GetComponent<PlayerHealth>();
                if (healthShield != null)
                {
                    StartCoroutine(TemporaryShield(healthShield));
                }
                break;
                
            case PowerupType.ScoreBonus:
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(Mathf.RoundToInt(value));
                }
                break;
        }
        
        Debug.Log($"Power-up collected: {type}");
    }
    
    /// <summary>
    /// Set the power-up color based on its type.
    /// </summary>
    private void SetColorByType()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        
        Color color = Color.white;
        
        switch (type)
        {
            case PowerupType.Health:
                color = Color.green;
                break;
            case PowerupType.SpeedBoost:
                color = Color.yellow;
                break;
            case PowerupType.RapidFire:
                color = Color.red;
                break;
            case PowerupType.SpreadShot:
                color = Color.magenta;
                break;
            case PowerupType.Shield:
                color = Color.cyan;
                break;
            case PowerupType.ScoreBonus:
                color = new Color(1f, 0.84f, 0f); // Gold
                break;
        }
        
        if (renderer != null)
        {
            renderer.material.color = color;
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
    
    // Temporary effect coroutines
    
    private System.Collections.IEnumerator TemporarySpeedBoost(PlayerController controller)
    {
        float originalSpeed = controller.GetMoveSpeed();
        controller.SetMoveSpeed(originalSpeed * value);
        yield return new WaitForSeconds(duration);
        controller.SetMoveSpeed(originalSpeed);
    }
    
    private System.Collections.IEnumerator TemporaryRapidFire(PlayerShooting shooting)
    {
        shooting.SetFireRate(0.05f);
        yield return new WaitForSeconds(duration);
        shooting.SetFireRate(0.2f);
    }
    
    private System.Collections.IEnumerator TemporarySpreadShot(PlayerShooting shooting)
    {
        shooting.SetBulletCount(3);
        yield return new WaitForSeconds(duration);
        shooting.SetBulletCount(1);
    }
    
    private System.Collections.IEnumerator TemporaryShield(PlayerHealth health)
    {
        // The invincibility is handled by PlayerHealth internally
        // This is a simplified version
        yield return new WaitForSeconds(duration);
    }
    
    /// <summary>
    /// Static factory method to create a power-up.
    /// </summary>
    public static GameObject CreatePowerup(PowerupType type, Vector3 position)
    {
        GameObject powerupObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        powerupObj.name = $"Powerup_{type}";
        powerupObj.transform.position = position;
        powerupObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        powerupObj.tag = "Powerup";
        
        // Remove 3D collider
        Object.Destroy(powerupObj.GetComponent<SphereCollider>());
        
        // Add 2D collider
        CircleCollider2D collider = powerupObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.25f;
        
        // Add Powerup component
        Powerup powerup = powerupObj.AddComponent<Powerup>();
        powerup.type = type;
        
        return powerupObj;
    }
}
