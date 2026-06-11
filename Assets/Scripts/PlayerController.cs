using UnityEngine;

/// <summary>
/// Handles player ship movement (WASD / arrow keys), screen clamping, and
/// shooting (Space). Bullets are spawned from a configurable muzzle point.
/// Requires a Health component on the same GameObject.
/// </summary>
[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in units per second.")]
    public float moveSpeed = 8f;

    [Tooltip("Padding from the screen edges so the ship stays fully visible.")]
    public float screenPadding = 0.5f;

    [Header("Shooting")]
    [Tooltip("Bullet prefab (must have a BulletController).")]
    public GameObject bulletPrefab;

    [Tooltip("Point from which bullets spawn. If null, uses the ship position.")]
    public Transform muzzle;

    [Tooltip("Seconds between shots.")]
    public float fireRate = 0.25f;

    [Tooltip("Damage each player bullet deals.")]
    public int bulletDamage = 25;

    [Tooltip("Speed of player bullets.")]
    public float bulletSpeed = 14f;

    private float nextFireTime = 0f;
    private Camera mainCamera;
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.isPlayer = true;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Only respond to input while the game is actively being played.
        if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Playing)
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

        Vector3 movement = new Vector3(horizontal, vertical, 0f).normalized;
        transform.position += movement * moveSpeed * Time.deltaTime;

        ClampToScreen();
    }

    /// <summary>
    /// Keeps the ship inside the camera's visible area.
    /// </summary>
    private void ClampToScreen()
    {
        if (mainCamera == null)
        {
            return;
        }

        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, min.x + screenPadding, max.x - screenPadding);
        pos.y = Mathf.Clamp(pos.y, min.y + screenPadding, max.y - screenPadding);
        transform.position = pos;
    }

    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null)
        {
            return;
        }

        Vector3 spawnPos = muzzle != null ? muzzle.position : transform.position;
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        BulletController bullet = bulletObj.GetComponent<BulletController>();
        if (bullet != null)
        {
            // Player bullets travel upward.
            bullet.Initialize(Vector2.up, BulletController.Owner.Player, bulletDamage, bulletSpeed);
        }
    }
}
