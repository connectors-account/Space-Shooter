using UnityEngine;

/// <summary>
/// Controls player ship movement and shooting.
/// Handles input for WASD/Arrow keys movement and Space to fire.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float padding = 0.5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    public float bulletSpeed = 12f;

    [Header("Power-Up State")]
    public bool hasRapidFire = false;
    public bool hasSpreadShot = false;
    public bool hasShield = false;
    public float rapidFireRate = 0.08f;
    public int spreadShotCount = 5;
    public float spreadAngle = 30f;

    private float nextFireTime = 0f;
    private float minX, maxX, minY, maxY;
    private PlayerHealth playerHealth;
    private GameObject shieldVisual;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        CalculateBounds();
        CreateShieldVisual();
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive()) return;

        HandleMovement();
        HandleShooting();
        UpdateShieldVisual();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, vertical, 0f) * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // Clamp position to screen bounds
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            float currentFireRate = hasRapidFire ? rapidFireRate : fireRate;
            nextFireTime = Time.time + currentFireRate;
            Fire();
        }
    }

    void Fire()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.6f;

        if (hasSpreadShot)
        {
            float angleStep = spreadAngle / (spreadShotCount - 1);
            float startAngle = -spreadAngle / 2f;

            for (int i = 0; i < spreadShotCount; i++)
            {
                float angle = startAngle + (angleStep * i);
                Quaternion rotation = Quaternion.Euler(0, 0, angle);
                GameObject bullet = Instantiate(bulletPrefab, spawnPos, rotation);
                Bullet b = bullet.GetComponent<Bullet>();
                if (b != null)
                {
                    b.speed = bulletSpeed;
                    b.direction = rotation * Vector3.up;
                    b.isPlayerBullet = true;
                }
            }
        }
        else
        {
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null)
            {
                b.speed = bulletSpeed;
                b.direction = Vector3.up;
                b.isPlayerBullet = true;
            }
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayShoot();
    }

    void CalculateBounds()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            minX = cam.ViewportToWorldPoint(Vector3.zero).x + padding;
            maxX = cam.ViewportToWorldPoint(Vector3.one).x - padding;
            minY = cam.ViewportToWorldPoint(Vector3.zero).y + padding;
            maxY = cam.ViewportToWorldPoint(Vector3.one).y - padding;
        }
    }

    void CreateShieldVisual()
    {
        shieldVisual = new GameObject("ShieldVisual");
        shieldVisual.transform.SetParent(transform);
        shieldVisual.transform.localPosition = Vector3.zero;
        SpriteRenderer sr = shieldVisual.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(32, new Color(0.3f, 0.7f, 1f, 0.3f));
        sr.sortingOrder = 5;
        shieldVisual.transform.localScale = Vector3.one * 2.5f;
        shieldVisual.SetActive(false);
    }

    void UpdateShieldVisual()
    {
        if (shieldVisual != null)
            shieldVisual.SetActive(hasShield);
    }

    Sprite CreateCircleSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        float radius = size / 2f;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                if (dist < radius && dist > radius - 2)
                    tex.SetPixel(x, y, color);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }

    public void ActivatePowerUp(PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUpType.RapidFire:
                hasRapidFire = true;
                CancelInvoke(nameof(DeactivateRapidFire));
                Invoke(nameof(DeactivateRapidFire), duration);
                break;
            case PowerUpType.SpreadShot:
                hasSpreadShot = true;
                CancelInvoke(nameof(DeactivateSpreadShot));
                Invoke(nameof(DeactivateSpreadShot), duration);
                break;
            case PowerUpType.Shield:
                hasShield = true;
                if (playerHealth != null) playerHealth.SetShield(true);
                CancelInvoke(nameof(DeactivateShield));
                Invoke(nameof(DeactivateShield), duration);
                break;
            case PowerUpType.Health:
                if (playerHealth != null) playerHealth.Heal(1);
                break;
        }
    }

    void DeactivateRapidFire() { hasRapidFire = false; }
    void DeactivateSpreadShot() { hasSpreadShot = false; }
    void DeactivateShield()
    {
        hasShield = false;
        if (playerHealth != null) playerHealth.SetShield(false);
    }
}
