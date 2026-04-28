using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private Vector2 playAreaMin = new Vector2(-8.8f, -4.6f);
    [SerializeField] private Vector2 playAreaMax = new Vector2(8.8f, 4.6f);

    [Header("Combat")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 0.2f;
    [SerializeField] private int maxHealth = 100;

    [Header("Runtime")]
    [SerializeField] private bool invulnerableDebug = false;

    private int currentHealth;
    private float nextFireTime;
    private bool isDead;
    private bool shieldActive;
    private Coroutine rapidFireCoroutine;
    private Coroutine shieldCoroutine;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsShieldActive => shieldActive;

    private void Start()
    {
        currentHealth = maxHealth;
        UIManager.Instance?.SetHealth(currentHealth, maxHealth);
    }

    private void Update()
    {
        if (isDead || GameManager.Instance == null || !GameManager.Instance.IsGameplayActive)
        {
            return;
        }

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(horizontal, vertical, 0f).normalized;
        transform.position += input * (moveSpeed * Time.deltaTime);

        float clampedX = Mathf.Clamp(transform.position.x, playAreaMin.x, playAreaMax.x);
        float clampedY = Mathf.Clamp(transform.position.y, playAreaMin.y, playAreaMax.y);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    private void HandleShooting()
    {
        if (!Input.GetKey(KeyCode.Space) || Time.time < nextFireTime)
        {
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.8f;
        BulletSystem.SpawnPlayerBullet(spawnPosition);
        AudioManager.Instance?.PlaySfx(AudioSfx.Shoot);
        nextFireTime = Time.time + fireCooldown;
    }

    public void ApplyDamage(int damage)
    {
        if (isDead || invulnerableDebug)
        {
            return;
        }

        if (shieldActive)
        {
            shieldActive = false;
            UIManager.Instance?.SetShield(false);
            AudioManager.Instance?.PlaySfx(AudioSfx.PowerUpCollected);
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        UIManager.Instance?.SetHealth(currentHealth, maxHealth);
        AudioManager.Instance?.PlaySfx(AudioSfx.PlayerHit);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void RestoreHealth(int amount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIManager.Instance?.SetHealth(currentHealth, maxHealth);
    }

    public void ApplyShield(float duration)
    {
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }

        shieldCoroutine = StartCoroutine(ShieldRoutine(duration));
    }

    public void ApplyRapidFire(float duration, float newCooldown)
    {
        if (rapidFireCoroutine != null)
        {
            StopCoroutine(rapidFireCoroutine);
        }

        rapidFireCoroutine = StartCoroutine(RapidFireRoutine(duration, newCooldown));
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        shieldActive = true;
        UIManager.Instance?.SetShield(true);
        yield return new WaitForSeconds(duration);
        shieldActive = false;
        UIManager.Instance?.SetShield(false);
        shieldCoroutine = null;
    }

    private IEnumerator RapidFireRoutine(float duration, float newCooldown)
    {
        float originalCooldown = fireCooldown;
        fireCooldown = newCooldown;
        UIManager.Instance?.SetRapidFire(true);
        yield return new WaitForSeconds(duration);
        fireCooldown = originalCooldown;
        UIManager.Instance?.SetRapidFire(false);
        rapidFireCoroutine = null;
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        AudioManager.Instance?.PlaySfx(AudioSfx.Explosion);
        GameManager.Instance?.OnPlayerDied();
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead)
        {
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            ApplyDamage(25);
            EnemyManager.EnemyRuntime enemy = other.GetComponent<EnemyManager.EnemyRuntime>();
            if (enemy != null)
            {
                enemy.TakeDamage(9999);
            }
            return;
        }

        if (other.CompareTag("EnemyBullet"))
        {
            BulletSystem.BulletRuntime bullet = other.GetComponent<BulletSystem.BulletRuntime>();
            int damage = bullet != null ? bullet.Damage : 10;
            ApplyDamage(damage);
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("PowerUp"))
        {
            PowerUpSystem.PowerUpRuntime powerUp = other.GetComponent<PowerUpSystem.PowerUpRuntime>();
            powerUp?.ApplyTo(this);
        }
    }
}
