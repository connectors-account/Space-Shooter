using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the player spaceship movement and firing behavior.
/// Attach this to the Player GameObject.
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float baseFireCooldown = 0.25f;
    [SerializeField] private float rapidFireCooldownMultiplier = 0.4f;

    private HealthSystem healthSystem;
    private Vector3 startPosition;
    private float nextFireTime;
    private Coroutine rapidFireCoroutine;
    private bool rapidFireActive;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        startPosition = transform.position;
    }

    private void Start()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= HandleDeath;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive)
        {
            return;
        }

        if (healthSystem != null && healthSystem.IsDead)
        {
            return;
        }

        HandleMovement();
        HandleShooting();
    }

    public void ResetPlayerState()
    {
        transform.position = startPosition;
        nextFireTime = 0f;

        if (rapidFireCoroutine != null)
        {
            StopCoroutine(rapidFireCoroutine);
            rapidFireCoroutine = null;
        }

        rapidFireActive = false;

        if (healthSystem != null)
        {
            healthSystem.ResetHealth();
        }
    }

    public void ApplyRapidFire(float duration)
    {
        if (rapidFireCoroutine != null)
        {
            StopCoroutine(rapidFireCoroutine);
        }

        rapidFireCoroutine = StartCoroutine(RapidFireRoutine(duration));
    }

    private void HandleMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float deltaX = inputX * moveSpeed * Time.deltaTime;

        Vector3 newPosition = transform.position + new Vector3(deltaX, 0f, 0f);
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        transform.position = newPosition;
    }

    private void HandleShooting()
    {
        bool shootInput = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        if (!shootInput || Time.time < nextFireTime)
        {
            return;
        }

        Shoot();
        float cooldown = rapidFireActive ? baseFireCooldown * rapidFireCooldownMultiplier : baseFireCooldown;
        nextFireTime = Time.time + cooldown;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("PlayerController: bulletPrefab or firePoint is not assigned.");
            return;
        }

        Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
    }

    private IEnumerator RapidFireRoutine(float duration)
    {
        rapidFireActive = true;
        yield return new WaitForSeconds(duration);
        rapidFireActive = false;
        rapidFireCoroutine = null;
    }

    private void HandleDeath()
    {
        // GameOver is triggered by HealthSystem; this callback is reserved for future VFX/SFX hooks.
    }
}
