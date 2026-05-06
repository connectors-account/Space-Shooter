using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private Vector2 minBounds = new Vector2(-8.5f, -4.5f);
    [SerializeField] private Vector2 maxBounds = new Vector2(8.5f, 4.5f);

    [Header("Combat")]
    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.25f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Power Ups")]
    [SerializeField] private float rapidFireDuration = 6f;
    [SerializeField] private float rapidFireMultiplier = 0.45f;
    [SerializeField] private float shieldDuration = 5f;

    private int currentHealth;
    private float nextFireTime;
    private bool rapidFireActive;
    private bool shieldActive;

    private Coroutine rapidFireRoutine;
    private Coroutine shieldRoutine;

    public bool IsShieldActive => shieldActive;
    public int CurrentHealth => currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        UIManager.Instance?.UpdateHealth(currentHealth, maxHealth);
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
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

        Vector3 direction = new Vector3(horizontal, vertical, 0f).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y),
            0f
        );
    }

    private void HandleShooting()
    {
        if (!Input.GetKey(KeyCode.Space))
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

        float actualFireRate = rapidFireActive ? fireRate * rapidFireMultiplier : fireRate;
        nextFireTime = Time.time + actualFireRate;

        Instantiate(playerBulletPrefab, firePoint.position, Quaternion.identity);
        AudioManager.Instance?.PlaySfx(AudioManager.Instance.PlayerShootClip);
    }

    public void TakeDamage(int amount)
    {
        if (shieldActive || GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        UIManager.Instance?.UpdateHealth(currentHealth, maxHealth);
        AudioManager.Instance?.PlaySfx(AudioManager.Instance.PlayerHitClip);

        if (currentHealth <= 0)
        {
            AudioManager.Instance?.PlaySfx(AudioManager.Instance.PlayerDeathClip);
            GameManager.Instance.TriggerGameOver();
            gameObject.SetActive(false);
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UIManager.Instance?.UpdateHealth(currentHealth, maxHealth);
    }

    public void ActivateRapidFire()
    {
        if (rapidFireRoutine != null)
        {
            StopCoroutine(rapidFireRoutine);
        }

        rapidFireRoutine = StartCoroutine(RapidFireTimer());
    }

    public void ActivateShield()
    {
        if (shieldRoutine != null)
        {
            StopCoroutine(shieldRoutine);
        }

        shieldRoutine = StartCoroutine(ShieldTimer());
    }

    private IEnumerator RapidFireTimer()
    {
        rapidFireActive = true;
        UIManager.Instance?.SetRapidFireIndicator(true);
        yield return new WaitForSeconds(rapidFireDuration);
        rapidFireActive = false;
        UIManager.Instance?.SetRapidFireIndicator(false);
    }

    private IEnumerator ShieldTimer()
    {
        shieldActive = true;
        UIManager.Instance?.SetShieldIndicator(true);
        yield return new WaitForSeconds(shieldDuration);
        shieldActive = false;
        UIManager.Instance?.SetShieldIndicator(false);
    }

}
