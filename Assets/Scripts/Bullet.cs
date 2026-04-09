using UnityEngine;

/// <summary>
/// Moves the bullet upward and handles collision with enemies.
/// Attach to the Bullet prefab.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Tooltip("Bullet travel speed.")]
    public float speed = 15f;

    [Tooltip("Damage dealt to enemies on hit.")]
    public int damage = 1;

    [Tooltip("Seconds before the bullet auto-destroys if it hits nothing.")]
    public float lifetime = 3f;

    void Start()
    {
        // Auto-destroy after lifetime to prevent orphaned bullets
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move upward every frame
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// When the bullet's trigger collider hits something tagged "Enemy",
    /// deal damage and destroy the bullet.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
