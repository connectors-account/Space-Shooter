using UnityEngine;

/// <summary>
/// Basic enemy movement/behavior.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float followStrength = 1.5f;
    [SerializeField] private float destroyY = -6f;

    [Header("Combat")]
    [SerializeField] private int health = 1;
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private bool canShoot = false;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shootInterval = 1.8f;

    private Transform player;
    private float nextShotTime;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        nextShotTime = Time.time + Random.Range(0.4f, shootInterval);
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        MoveTowardPlayer();
        TryShoot();

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    private void MoveTowardPlayer()
    {
        float xDirection = 0f;
        if (player != null)
        {
            xDirection = Mathf.Clamp(player.position.x - transform.position.x, -1f, 1f);
        }

        Vector3 direction = new Vector3(xDirection * followStrength, -1f, 0f).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void TryShoot()
    {
        if (!canShoot || bulletPrefab == null)
            return;

        if (Time.time < nextShotTime)
            return;

        GameObject bullet = Instantiate(bulletPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.Initialize(false, Vector3.down);
        }

        nextShotTime = Time.time + shootInterval;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DamagePlayer(1);
            }
            Destroy(gameObject);
        }
    }
}
