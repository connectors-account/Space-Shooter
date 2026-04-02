using UnityEngine;

/// <summary>
/// Handles collision events between game objects.
/// Attach to objects that need collision response beyond bullets.
/// Handles player-enemy collisions and boundary cleanup.
/// </summary>
public class CollisionHandler : MonoBehaviour
{
    public enum CollisionObjectType { Player, Enemy, PowerUp, Boundary }

    [SerializeField] private CollisionObjectType objectType = CollisionObjectType.Player;
    [SerializeField] private int collisionDamage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (objectType)
        {
            case CollisionObjectType.Player:
                HandlePlayerCollision(other);
                break;
            case CollisionObjectType.Enemy:
                HandleEnemyCollision(other);
                break;
            case CollisionObjectType.Boundary:
                HandleBoundaryCollision(other);
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (objectType)
        {
            case CollisionObjectType.Player:
                HandlePlayerCollision(collision.collider);
                break;
            case CollisionObjectType.Enemy:
                HandleEnemyCollision(collision.collider);
                break;
        }
    }

    private void HandlePlayerCollision(Collider2D other)
    {
        // Player touching an enemy = take damage
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            PlayerController player = GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(collisionDamage);
            }
            enemy.TakeDamage(999); // Destroy enemy on contact
        }
    }

    private void HandleEnemyCollision(Collider2D other)
    {
        // Enemy touching player
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(collisionDamage);
            }
            EnemyController self = GetComponent<EnemyController>();
            if (self != null)
            {
                self.TakeDamage(999);
            }
        }
    }

    private void HandleBoundaryCollision(Collider2D other)
    {
        // Destroy anything that touches the boundary
        if (other.GetComponent<BulletController>() != null ||
            other.GetComponent<EnemyController>() != null ||
            other.GetComponent<PowerUpController>() != null)
        {
            Destroy(other.gameObject);
        }
    }
}
