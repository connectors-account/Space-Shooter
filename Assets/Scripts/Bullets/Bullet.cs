using UnityEngine;

/// <summary>
/// Generic bullet script for both player and enemy bullets.
/// Moves in a direction and auto-destroys when off screen.
/// </summary>
public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public Vector2 direction = Vector2.up;
    public int damage = 1;
    public float lifetime = 5f;

    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

        // Destroy if off screen or exceeded lifetime
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (Mathf.Abs(transform.position.x) > 12f || Mathf.Abs(transform.position.y) > 8f)
        {
            Destroy(gameObject);
        }
    }
}
