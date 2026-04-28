using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyController : MonoBehaviour
{
    private enum MovementPattern
    {
        Straight,
        Sine,
        ZigZag
    }

    [Header("Stats")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int contactDamage = 25;
    [SerializeField] private int scoreValue = 100;

    [Header("Shooting")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float minFireCooldown = 1.4f;
    [SerializeField] private float maxFireCooldown = 2.8f;
    [SerializeField] private int bulletDamage = 1;

    [Header("Movement Pattern")]
    [SerializeField] private float patternAmplitude = 1.2f;
    [SerializeField] private float patternFrequency = 2.2f;

    private MovementPattern movementPattern;
    private int currentHealth;
    private float nextShotTime;
    private float spawnX;

    private void Start()
    {
        currentHealth = maxHealth;
        spawnX = transform.position.x;
        movementPattern = (MovementPattern)Random.Range(0, 3);
        nextShotTime = Time.time + Random.Range(minFireCooldown, maxFireCooldown);
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        HandleMovement();
        HandleShooting();
        CheckOutOfBounds();
    }

    private void HandleMovement()
    {
        Vector3 pos = transform.position;
        pos.y -= moveSpeed * Time.deltaTime;

        switch (movementPattern)
        {
            case MovementPattern.Sine:
                pos.x = spawnX + Mathf.Sin(Time.time * patternFrequency) * patternAmplitude;
                break;
            case MovementPattern.ZigZag:
                pos.x = spawnX + Mathf.PingPong(Time.time * patternFrequency, patternAmplitude * 2f) - patternAmplitude;
                break;
        }

        transform.position = pos;
    }

    private void HandleShooting()
    {
        if (enemyBulletPrefab == null || firePoint == null)
        {
            return;
        }

        if (Time.time < nextShotTime)
        {
            return;
        }

        GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.Initialize(Vector2.down, true, bulletDamage);
        }

        AudioManager.Instance?.PlayEnemyShoot(); // Add enemy shoot SFX clip in AudioManager inspector
        nextShotTime = Time.time + Random.Range(minFireCooldown, maxFireCooldown);
    }

    private void CheckOutOfBounds()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane));
        if (transform.position.y < min.y - 2f)
        {
            GameManager.Instance?.NotifyEnemyDestroyed(false);
            Destroy(gameObject);
        }
    }

    public void ConfigureForWave(int waveNumber)
    {
        moveSpeed += waveNumber * 0.15f;
        maxHealth += Mathf.FloorToInt(waveNumber * 0.3f);
        scoreValue += waveNumber * 12;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        AudioManager.Instance?.PlayEnemyDeath(); // Add enemy death SFX clip in AudioManager inspector
        GameManager.Instance?.AddScore(scoreValue);
        GameManager.Instance?.NotifyEnemyDestroyed(true);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(contactDamage);
            GameManager.Instance?.NotifyEnemyDestroyed(false);
            Destroy(gameObject);
        }
    }
}
