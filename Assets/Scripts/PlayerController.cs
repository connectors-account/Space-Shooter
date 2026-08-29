using UnityEngine;

/// <summary>
/// Handles player movement and shooting.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private float minY = -4.5f;
    [SerializeField] private float maxY = 4.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 0.2f;

    private float nextShootTime;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        Vector3 moveDelta = new Vector3(inputX, inputY, 0f) * moveSpeed * Time.deltaTime;
        transform.position += moveDelta;

        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    private void HandleShooting()
    {
        if (!Input.GetKey(KeyCode.Space))
            return;

        if (Time.time < nextShootTime)
            return;

        if (bulletPrefab == null)
        {
            Debug.LogWarning("PlayerController: bulletPrefab is not assigned.");
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.6f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.Initialize(true, Vector3.up);
        }

        nextShootTime = Time.time + fireCooldown;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        if (other.CompareTag("Enemy"))
        {
            GameManager.Instance.DamagePlayer(1);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("EnemyBullet"))
        {
            GameManager.Instance.DamagePlayer(1);
            Destroy(other.gameObject);
        }
    }
}
