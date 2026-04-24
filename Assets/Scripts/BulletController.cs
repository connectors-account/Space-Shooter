using UnityEngine;

/// <summary>
/// Bullet movement and collision handling.
/// </summary>
public class BulletController : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 4f;

    private bool isPlayerBullet;
    private Vector3 moveDirection = Vector3.up;

    public void Initialize(bool firedByPlayer, Vector3 direction)
    {
        isPlayerBullet = firedByPlayer;
        moveDirection = direction.normalized;

        gameObject.tag = firedByPlayer ? "PlayerBullet" : "EnemyBullet";
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet && other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(1);
            }

            Destroy(gameObject);
            return;
        }

        if (!isPlayerBullet && other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DamagePlayer(1);
            }

            Destroy(gameObject);
        }
    }
}
