using UnityEngine;

/// <summary>
/// Utility component that can be placed on boundary objects (e.g. screen edges)
/// to destroy bullets or enemies that leave the play area.
/// Also provides a static helper to configure Physics2D layer collision matrix.
/// </summary>
public class CollisionHandler : MonoBehaviour
{
    [Header("Destroy Settings")]
    [SerializeField] private bool destroyEnemies     = true;
    [SerializeField] private bool destroyBullets     = true;
    [SerializeField] private bool destroyPowerUps    = true;

    /// <summary>
    /// Call once at game start to set up the 2D layer collision matrix so
    /// friendly bullets don't hit the player, enemy bullets don't hit enemies, etc.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void SetupCollisionMatrix()
    {
        // These layers must exist in the project (see TagsAndLayers setup).
        // If they don't exist yet we skip silently.
        int playerLayer      = LayerMask.NameToLayer("Player");
        int enemyLayer       = LayerMask.NameToLayer("Enemy");
        int playerBullet     = LayerMask.NameToLayer("PlayerBullet");
        int enemyBullet      = LayerMask.NameToLayer("EnemyBullet");
        int powerUpLayer     = LayerMask.NameToLayer("PowerUp");

        if (playerLayer < 0 || enemyLayer < 0) return; // layers not set up yet

        // Player bullets should NOT collide with the player or other player bullets
        if (playerBullet >= 0)
        {
            Physics2D.IgnoreLayerCollision(playerBullet, playerLayer, true);
            Physics2D.IgnoreLayerCollision(playerBullet, playerBullet, true);
            Physics2D.IgnoreLayerCollision(playerBullet, powerUpLayer >= 0 ? powerUpLayer : playerBullet, true);
        }

        // Enemy bullets should NOT collide with enemies or other enemy bullets
        if (enemyBullet >= 0)
        {
            Physics2D.IgnoreLayerCollision(enemyBullet, enemyLayer, true);
            Physics2D.IgnoreLayerCollision(enemyBullet, enemyBullet, true);
            Physics2D.IgnoreLayerCollision(enemyBullet, powerUpLayer >= 0 ? powerUpLayer : enemyBullet, true);
        }

        // Player bullets and enemy bullets should ignore each other
        if (playerBullet >= 0 && enemyBullet >= 0)
            Physics2D.IgnoreLayerCollision(playerBullet, enemyBullet, true);

        // Power-ups should only interact with the player
        if (powerUpLayer >= 0 && enemyLayer >= 0)
            Physics2D.IgnoreLayerCollision(powerUpLayer, enemyLayer, true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (destroyBullets && other.GetComponent<BulletController>() != null)
        {
            Destroy(other.gameObject);
            return;
        }
        if (destroyEnemies && other.GetComponent<EnemyController>() != null)
        {
            SpawnManager.Instance?.OnEnemyDestroyed();
            Destroy(other.gameObject);
            return;
        }
        if (destroyPowerUps && other.GetComponent<PowerUpController>() != null)
        {
            Destroy(other.gameObject);
            return;
        }
    }
}
