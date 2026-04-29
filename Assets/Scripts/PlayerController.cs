using System.Collections;
using UnityEngine;

/// <summary>
/// Handles keyboard movement, shooting, health, and temporary power-up effects.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private Vector2 screenPadding = new Vector2(0.5f, 0.5f);

    [Header("Combat")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private float fireRate = 0.25f;

    [Header("Power-up Durations")]
    [SerializeField] private float shieldDuration = 6f;
    [SerializeField] private float rapidFireDuration = 6f;
    [SerializeField] private float rapidFireMultiplier = 0.45f;

    [Header("Debug / Runtime")]
    [SerializeField] private int currentHealth;

    private float nextShotTime;
    private float defaultFireRate;
    private bool shieldActive;
    private Vector3 spawnPosition;

    private Coroutine shieldRoutine;
    private Coroutine rapidFireRoutine;

    public int CurrentHealth => currentHealth;
    public bool ShieldActive => shieldActive;

    private void Start()
    {
        spawnPosition = transform.position;
        currentHealth = maxHealth;
        defaultFireRate = fireRate;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)
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

        Vector3 moveDelta = new Vector3(horizontal, vertical, 0f).normalized * moveSpeed * Time.deltaTime;
        transform.position += moveDelta;

        ClampToScreen();
    }

    private void ClampToScreen()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1f, 1f, cam.nearClipPlane));

        float clampedX = Mathf.Clamp(transform.position.x, min.x + screenPadding.x, max.x - screenPadding.x);
        float clampedY = Mathf.Clamp(transform.position.y, min.y + screenPadding.y, max.y - screenPadding.y);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    private void HandleShooting()
    {
        if (!Input.GetKey(KeyCode.Space))
        {
            return;
        }

        if (Time.time < nextShotTime)
        {
            return;
        }

        nextShotTime = Time.time + fireRate;

        if (playerBulletPrefab != null && firePoint != null)
        {
            Instantiate(playerBulletPrefab, firePoint.position, Quaternion.identity);
        }
    }

    public void TakeDamage(int amount)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || amount <= 0 || shieldActive || gm.IsGameOver)
        {
            return;
        }

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
            gm.PlayerLostLife();

            if (!gm.IsGameOver)
            {
                ActivateShield(2f); // brief respawn invulnerability
            }
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    public void ActivateShield(float durationOverride = -1f)
    {
        float duration = durationOverride > 0 ? durationOverride : shieldDuration;

        if (shieldRoutine != null)
        {
            StopCoroutine(shieldRoutine);
        }

        shieldRoutine = StartCoroutine(ShieldTimer(duration));
    }

    private IEnumerator ShieldTimer(float duration)
    {
        shieldActive = true;
        yield return new WaitForSeconds(duration);
        shieldActive = false;
    }

    public void ActivateRapidFire(float durationOverride = -1f)
    {
        float duration = durationOverride > 0 ? durationOverride : rapidFireDuration;

        if (rapidFireRoutine != null)
        {
            StopCoroutine(rapidFireRoutine);
        }

        rapidFireRoutine = StartCoroutine(RapidFireTimer(duration));
    }

    public void ResetForNewRun()
    {
        if (shieldRoutine != null) StopCoroutine(shieldRoutine);
        if (rapidFireRoutine != null) StopCoroutine(rapidFireRoutine);

        shieldActive = false;
        fireRate = defaultFireRate;
        currentHealth = maxHealth;
        transform.position = spawnPosition;
    }

    private IEnumerator RapidFireTimer(float duration)
    {
        fireRate = defaultFireRate * rapidFireMultiplier;
        yield return new WaitForSeconds(duration);
        fireRate = defaultFireRate;
    }
}
