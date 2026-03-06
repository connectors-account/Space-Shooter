using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized collision handling system.
/// Manages collision layers and provides utility methods for collision detection.
/// </summary>
public class CollisionHandler : MonoBehaviour
{
    public static CollisionHandler Instance { get; private set; }
    
    [Header("Collision Settings")]
    [SerializeField] private bool debugCollisions = false;
    
    // Collision layer names
    public const string LAYER_PLAYER = "Player";
    public const string LAYER_ENEMY = "Enemy";
    public const string LAYER_PLAYER_BULLET = "PlayerBullet";
    public const string LAYER_ENEMY_BULLET = "EnemyBullet";
    public const string LAYER_POWERUP = "PowerUp";
    
    // Layer mask cache
    private int playerLayerMask;
    private int enemyLayerMask;
    private int playerBulletLayerMask;
    private int enemyBulletLayerMask;
    private int powerUpLayerMask;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        InitializeLayerMasks();
    }
    
    private void InitializeLayerMasks()
    {
        playerLayerMask = LayerMask.GetMask(LAYER_PLAYER);
        enemyLayerMask = LayerMask.GetMask(LAYER_ENEMY);
        playerBulletLayerMask = LayerMask.GetMask(LAYER_PLAYER_BULLET);
        enemyBulletLayerMask = LayerMask.GetMask(LAYER_ENEMY_BULLET);
        powerUpLayerMask = LayerMask.GetMask(LAYER_POWERUP);
    }
    
    /// <summary>
    /// Check if point overlaps with any enemies
    /// </summary>
    public bool CheckEnemyAtPoint(Vector2 point, float radius = 0.1f)
    {
        Collider2D hit = Physics2D.OverlapCircle(point, radius, enemyLayerMask);
        return hit != null;
    }
    
    /// <summary>
    /// Get all enemies within radius
    /// </summary>
    public List<EnemyController> GetEnemiesInRadius(Vector2 center, float radius)
    {
        List<EnemyController> enemies = new List<EnemyController>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, enemyLayerMask);
        
        foreach (var hit in hits)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemies.Add(enemy);
            }
        }
        
        return enemies;
    }
    
    /// <summary>
    /// Check if position is clear for spawning
    /// </summary>
    public bool IsSpawnPositionClear(Vector2 position, float radius, int excludeLayer = 0)
    {
        int layerMask = ~excludeLayer;
        Collider2D hit = Physics2D.OverlapCircle(position, radius, layerMask);
        return hit == null;
    }
    
    /// <summary>
    /// Raycast to find nearest enemy
    /// </summary>
    public EnemyController FindNearestEnemy(Vector2 origin, Vector2 direction, float maxDistance = 50f)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, enemyLayerMask);
        
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<EnemyController>();
        }
        
        return null;
    }
    
    /// <summary>
    /// Get closest enemy to position
    /// </summary>
    public EnemyController GetClosestEnemy(Vector2 position, float maxDistance = float.MaxValue)
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        EnemyController closest = null;
        float closestDist = maxDistance;
        
        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = enemy;
            }
        }
        
        return closest;
    }
    
    /// <summary>
    /// Apply damage to all enemies in radius (for explosions)
    /// </summary>
    public void DamageEnemiesInRadius(Vector2 center, float radius, int damage)
    {
        List<EnemyController> enemies = GetEnemiesInRadius(center, radius);
        
        foreach (var enemy in enemies)
        {
            HealthSystem health = enemy.GetComponent<HealthSystem>();
            if (health != null)
            {
                // Damage falloff based on distance
                float dist = Vector2.Distance(center, enemy.transform.position);
                float falloff = 1f - (dist / radius);
                int actualDamage = Mathf.RoundToInt(damage * falloff);
                
                health.TakeDamage(actualDamage);
            }
        }
        
        if (debugCollisions)
        {
            Debug.Log($"Explosion at {center} damaged {enemies.Count} enemies");
        }
    }
    
    /// <summary>
    /// Setup collision matrix (call from editor or startup)
    /// </summary>
    public static void SetupCollisionMatrix()
    {
        // This would typically be configured in Unity's Physics2D settings
        // The collision matrix determines which layers collide with each other
        
        // Player collides with: Enemy, EnemyBullet, PowerUp
        // Enemy collides with: Player, PlayerBullet
        // PlayerBullet collides with: Enemy
        // EnemyBullet collides with: Player
        // PowerUp collides with: Player
        
        Debug.Log("Collision matrix should be configured in Edit > Project Settings > Physics 2D");
    }
    
    /// <summary>
    /// Log collision for debugging
    /// </summary>
    public void LogCollision(GameObject a, GameObject b, string type)
    {
        if (debugCollisions)
        {
            Debug.Log($"Collision [{type}]: {a.name} <-> {b.name}");
        }
    }
}
