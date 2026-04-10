using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private Vector2 minBounds = new Vector2(-8f, -4.2f);
    [SerializeField] private Vector2 maxBounds = new Vector2(8f, 4.2f);

    [Header("Combat")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invincibilityDuration = 1.2f;

    [Header("Power Ups")]
    [SerializeField] private float rapidFireMultiplier = 0.45f;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    private int _currentHealth;
    private float _nextShotTime;
    private bool _isInvincible;
    private bool _isShielded;
    private float _fireRateMultiplier = 1f;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => _currentHealth;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentHealth = maxHealth;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 velocity = new Vector2(h, v).normalized * moveSpeed;
        _rb.velocity = velocity;

        Vector3 clamped = transform.position;
        clamped.x = Mathf.Clamp(clamped.x, minBounds.x, maxBounds.x);
        clamped.y = Mathf.Clamp(clamped.y, minBounds.y, maxBounds.y);
        transform.position = clamped;
    }

    private void HandleShooting()
    {
        if (!Input.GetKey(KeyCode.Space) && !Input.GetButton("Fire1"))
        {
            return;
        }

        if (Time.time < _nextShotTime)
        {
            return;
        }

        ObjectPool pool = GameManager.Instance.GetPlayerBulletPool();
        if (pool == null)
        {
            return;
        }

        PlayerBullet bullet = pool.Get<PlayerBullet>();
        if (bullet == null)
        {
            return;
        }

        bullet.transform.position = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.7f;
        bullet.Initialize(pool, Vector2.up);

        _nextShotTime = Time.time + (fireRate * _fireRateMultiplier);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(AudioCue.Shoot);
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || _isInvincible || GameManager.Instance == null || GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (_isShielded)
        {
            _isShielded = false;
            StartCoroutine(InvincibilityFlash(0.25f));
            return;
        }

        _currentHealth = Mathf.Max(0, _currentHealth - damage);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(AudioCue.PlayerHit);
        }

        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibilityFlash(invincibilityDuration));
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(maxHealth, _currentHealth + Mathf.Abs(amount));
    }

    public void EnableShield(float duration)
    {
        StartCoroutine(ShieldRoutine(duration));
    }

    public void EnableRapidFire(float duration)
    {
        StartCoroutine(RapidFireRoutine(duration));
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        _isShielded = true;
        yield return new WaitForSeconds(duration);
        _isShielded = false;
    }

    private IEnumerator RapidFireRoutine(float duration)
    {
        _fireRateMultiplier = rapidFireMultiplier;
        yield return new WaitForSeconds(duration);
        _fireRateMultiplier = 1f;
    }

    private IEnumerator InvincibilityFlash(float duration)
    {
        _isInvincible = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = !_spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(0.08f);
            elapsed += 0.08f;
        }

        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = true;
        }

        _isInvincible = false;
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpawnExplosion(transform.position);
            GameManager.Instance.OnPlayerDied();
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyController enemy))
        {
            TakeDamage(enemy.ContactDamage);
            enemy.TakeDamage(9999);
        }
    }
}
