using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    public enum ObjectType
    {
        Player,
        Enemy,
        PlayerBullet,
        EnemyBullet,
        PowerUp
    }

    [Header("Object Settings")]
    public ObjectType objectType;
    public int damageAmount = 1;

    private PlayerController playerController;
    private EnemyController enemyController;
    private AudioManager audioManager;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();

        switch (objectType)
        {
            case ObjectType.Player:
                playerController = GetComponent<PlayerController>();
                break;
            case ObjectType.Enemy:
                enemyController = GetComponent<EnemyController>();
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        CollisionHandler otherHandler = other.GetComponent<CollisionHandler>();

        if (otherHandler == null) return;

        switch (objectType)
        {
            case ObjectType.Player:
                HandlePlayerCollision(otherHandler);
                break;
            case ObjectType.Enemy:
                HandleEnemyCollision(otherHandler);
                break;
            case ObjectType.PlayerBullet:
                HandlePlayerBulletCollision(otherHandler);
                break;
            case ObjectType.EnemyBullet:
                HandleEnemyBulletCollision(otherHandler);
                break;
        }
    }

    void HandlePlayerCollision(CollisionHandler other)
    {
        if (other.objectType == ObjectType.Enemy || other.objectType == ObjectType.EnemyBullet)
        {
            if (playerController != null)
            {
                playerController.TakeDamage(other.damageAmount);
            }
        }
        else if (other.objectType == ObjectType.PowerUp)
        {
            PowerUpController powerUp = other.GetComponent<PowerUpController>();
            if (powerUp != null && playerController != null)
            {
                if (audioManager != null)
                    audioManager.PlayPowerUpSound();
            }
        }
    }

    void HandleEnemyCollision(CollisionHandler other)
    {
        if (other.objectType == ObjectType.PlayerBullet)
        {
            if (enemyController != null)
            {
                enemyController.TakeDamage(other.damageAmount);
            }
        }
        else if (other.objectType == ObjectType.Player)
        {
            if (enemyController != null)
            {
                enemyController.TakeDamage(enemyController.health);
            }
        }
    }

    void HandlePlayerBulletCollision(CollisionHandler other)
    {
        if (other.objectType == ObjectType.Enemy)
        {
            Destroy(gameObject);
        }
    }

    void HandleEnemyBulletCollision(CollisionHandler other)
    {
        if (other.objectType == ObjectType.Player)
        {
            Destroy(gameObject);
        }
    }

    public static void SetupCollisionLayers()
    {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerBullet"), LayerMask.NameToLayer("Player"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("EnemyBullet"), LayerMask.NameToLayer("Enemy"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerBullet"), LayerMask.NameToLayer("PlayerBullet"), true);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("EnemyBullet"), LayerMask.NameToLayer("EnemyBullet"), true);
    }
}
