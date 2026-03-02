using UnityEngine;

/// <summary>
/// Controls bullet movement and behavior for both player and enemy bullets.
/// </summary>
public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 3f;

    private Vector3 moveDirection = Vector3.up;
    private bool isPlayerBullet = true;

    public bool IsPlayerBullet => isPlayerBullet;

    private void Start()
    {
        // Destroy bullet after lifetime expires
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Initialize the bullet with direction and ownership.
    /// </summary>
    /// <param name="playerBullet">True if fired by player, false if fired by enemy</param>
    /// <param name="direction">Direction the bullet should travel</param>
    public void Initialize(bool playerBullet, Vector3 direction)
    {
        isPlayerBullet = playerBullet;
        moveDirection = direction.normalized;

        // Set appropriate tag based on ownership
        if (playerBullet)
        {
            gameObject.tag = "PlayerBullet";
        }
        else
        {
            gameObject.tag = "EnemyBullet";
        }

        // Rotate bullet to face movement direction
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    /// <summary>
    /// Set bullet speed.
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}
