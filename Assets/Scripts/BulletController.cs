using UnityEngine;

/// <summary>
/// Controls bullet movement and lifetime.
/// Used by both player and enemy bullets.
/// </summary>
public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 12f;
    public int damage = 10;
    public Vector2 direction = Vector2.up;
    public bool isPlayerBullet = true;
    public float lifetime = 5f;

    private float aliveTime = 0f;

    void Start()
    {
        // Set the correct tag based on ownership
        if (isPlayerBullet)
        {
            gameObject.tag = "PlayerBullet";
        }
        else
        {
            gameObject.tag = "EnemyBullet";
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused)
            return;

        // Move in the assigned direction
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);

        // Destroy after lifetime expires
        aliveTime += Time.deltaTime;
        if (aliveTime >= lifetime)
        {
            Destroy(gameObject);
        }

        // Also destroy if way off screen
        if (Mathf.Abs(transform.position.x) > 12f || Mathf.Abs(transform.position.y) > 8f)
        {
            Destroy(gameObject);
        }
    }
}
