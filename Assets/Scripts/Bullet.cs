using UnityEngine;

/// <summary>
/// Generic bullet used by both the player and enemies.
/// Moves in a given direction at a given speed and self-destructs off-screen.
/// </summary>
public class Bullet : MonoBehaviour
{
    public enum BulletType { Normal, Spread, Laser }
    public enum Owner { Player, Enemy }

    [Header("Config – set by BulletSpawner at spawn time")]
    public BulletType bulletType = BulletType.Normal;
    public Owner      owner     = Owner.Player;
    public Vector2    direction = Vector2.up;
    public float      speed     = 12f;
    public int        damage    = 1;

    private void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

        // Destroy when off-screen
        if (ScreenBounds.Instance != null && ScreenBounds.Instance.IsOffScreen(transform.position))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player bullet hits enemy
        if (owner == Owner.Player && other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
        // Enemy bullet hits player
        else if (owner == Owner.Enemy && other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null) player.Hit(damage);
            Destroy(gameObject);
        }
    }
}
