using UnityEngine;

/// <summary>
/// Basic enemy behavior: move downward and damage player on contact.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int health = 1;
    [SerializeField] private int collisionDamage = 20;
    [SerializeField] private int scoreValue = 10;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);

        // Cleanup if enemy leaves visible play area.
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= Mathf.Abs(damage);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(collisionDamage);
            }

            Destroy(gameObject);
        }
        else if (other.CompareTag("Boundary"))
        {
            Destroy(gameObject);
        }
    }
}
