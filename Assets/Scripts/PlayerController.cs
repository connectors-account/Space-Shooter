using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private Vector2 minBounds = new Vector2(-8.5f, -4.5f);
    [SerializeField] private Vector2 maxBounds = new Vector2(8.5f, 4.5f);

    [Header("Shooting")]
    [SerializeField] private BulletController playerBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float normalFireCooldown = 0.25f;
    [SerializeField] private float rapidFireCooldown = 0.1f;

    [Header("Health / Lives")]
    [SerializeField] private int maxLives = 5;
    [SerializeField] private float invulnerableDuration = 1f;

    [Header("Power-up Durations")]
    [SerializeField] private float shieldDuration = 6f;
    [SerializeField] private float rapidFireDuration = 6f;

    [Header("Visual")]
    [SerializeField] private GameObject shieldVisual;

    private int currentLives;
    private float nextShootTime;
    private bool shieldActive;
    private bool rapidFireActive;
    private bool invulnerable;

    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;
    public bool ShieldActive => shieldActive;
    public bool RapidFireActive => rapidFireActive;

    private void Start()
    {
        ResetPlayer();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
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

        Vector3 move = new Vector3(horizontal, vertical, 0f).normalized * moveSpeed * Time.deltaTime;
        transform.position += move;

        float clampedX = Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x);
        float clampedY = Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    private void HandleShooting()
    {
        if (!Input.GetKey(KeyCode.Space) || Time.time < nextShootTime)
        {
            return;
        }

        if (playerBulletPrefab == null || firePoint == null)
        {
            return;
        }

        float cooldown = rapidFireActive ? rapidFireCooldown : normalFireCooldown;
        nextShootTime = Time.time + cooldown;

        BulletController bullet = Instantiate(playerBulletPrefab, firePoint.position, Quaternion.identity);
        bullet.Initialize(BulletController.BulletOwner.Player, Vector2.up, 1);
        AudioManager.Instance?.PlayShoot();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || invulnerable || shieldActive)
        {
            return;
        }

        currentLives -= damage;
        currentLives = Mathf.Clamp(currentLives, 0, maxLives);
        UIManager.Instance?.UpdateLives(currentLives, maxLives);
        AudioManager.Instance?.PlayPlayerHit();

        if (currentLives <= 0)
        {
            GameManager.Instance?.OnPlayerDeath();
            gameObject.SetActive(false);
            return;
        }

        StartCoroutine(InvulnerableRoutine());
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentLives = Mathf.Clamp(currentLives + amount, 0, maxLives);
        UIManager.Instance?.UpdateLives(currentLives, maxLives);
    }

    public void ActivateShield()
    {
        StartCoroutine(ShieldRoutine());
    }

    public void ActivateRapidFire()
    {
        StartCoroutine(RapidFireRoutine());
    }

    public void ResetPlayer()
    {
        StopAllCoroutines();

        currentLives = maxLives;
        shieldActive = false;
        rapidFireActive = false;
        invulnerable = false;
        nextShootTime = 0f;

        transform.position = new Vector3(0f, -3.8f, 0f);
        gameObject.SetActive(true);

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        UIManager.Instance?.UpdateLives(currentLives, maxLives);
        UIManager.Instance?.UpdatePowerUpStatus(shieldActive, rapidFireActive);
    }

    private IEnumerator ShieldRoutine()
    {
        shieldActive = true;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }

        UIManager.Instance?.UpdatePowerUpStatus(shieldActive, rapidFireActive);
        yield return new WaitForSeconds(shieldDuration);

        shieldActive = false;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        UIManager.Instance?.UpdatePowerUpStatus(shieldActive, rapidFireActive);
    }

    private IEnumerator RapidFireRoutine()
    {
        rapidFireActive = true;
        UIManager.Instance?.UpdatePowerUpStatus(shieldActive, rapidFireActive);

        yield return new WaitForSeconds(rapidFireDuration);

        rapidFireActive = false;
        UIManager.Instance?.UpdatePowerUpStatus(shieldActive, rapidFireActive);
    }

    private IEnumerator InvulnerableRoutine()
    {
        invulnerable = true;
        yield return new WaitForSeconds(invulnerableDuration);
        invulnerable = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }
}
