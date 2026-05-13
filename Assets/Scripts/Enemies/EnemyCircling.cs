// ============================================================================
// EnemyCircling.cs - Enemy that enters from the top, then circles at a fixed Y
// A tougher enemy that lingers on-screen and fires at the player.
// ============================================================================
using UnityEngine;

/// <summary>
/// Descends to a target altitude, then orbits in a circle while shooting.
/// After a set duration, resumes descending off-screen.
/// </summary>
public class EnemyCircling : EnemyBase
{
    [Header("Circling Settings")]
    [Tooltip("Y position where the enemy stops descending and begins circling.")]
    [SerializeField] private float orbitAltitude = 2f;
    [Tooltip("Radius of the circular orbit.")]
    [SerializeField] private float orbitRadius = 2f;
    [Tooltip("Angular speed in degrees per second.")]
    [SerializeField] private float orbitSpeed = 120f;
    [Tooltip("Seconds spent circling before retreating.")]
    [SerializeField] private float orbitDuration = 5f;

    private enum Phase { Entering, Orbiting, Retreating }
    private Phase currentPhase;
    private float orbitTimer;
    private float angle;
    private Vector3 orbitCenter;

    protected override void OnEnable()
    {
        base.OnEnable();
        currentPhase = Phase.Entering;
        orbitTimer = orbitDuration;
        angle = 0f;
        // Randomize the orbit altitude slightly so multiple circlers don't stack.
        orbitAltitude = Random.Range(1f, 3.5f);
    }

    protected override void MovementPattern()
    {
        switch (currentPhase)
        {
            case Phase.Entering:
                // Descend toward the orbit altitude.
                transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
                if (transform.position.y <= orbitAltitude)
                {
                    orbitCenter = transform.position;
                    currentPhase = Phase.Orbiting;
                }
                break;

            case Phase.Orbiting:
                // Circle around the orbit center.
                angle += orbitSpeed * Time.deltaTime;
                float rad = angle * Mathf.Deg2Rad;
                float x = orbitCenter.x + Mathf.Cos(rad) * orbitRadius;
                float y = orbitCenter.y + Mathf.Sin(rad) * orbitRadius;
                transform.position = new Vector3(x, y, transform.position.z);

                orbitTimer -= Time.deltaTime;
                if (orbitTimer <= 0f)
                {
                    currentPhase = Phase.Retreating;
                }
                break;

            case Phase.Retreating:
                // Fly downward off-screen.
                transform.Translate(Vector3.down * moveSpeed * 1.5f * Time.deltaTime, Space.World);
                break;
        }
    }

    /// <summary>
    /// Circling enemies always aim at the player when shooting.
    /// </summary>
    protected override void Shoot()
    {
        if (bulletPrefab == null || playerTransform == null) return;

        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
        GameObject bulletObj = null;

        if (PoolManager.Instance != null)
        {
            bulletObj = PoolManager.Instance.Get("EnemyBulletPool", spawnPos, Quaternion.identity);
        }
        if (bulletObj == null)
        {
            bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        }

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            Vector2 direction = ((Vector2)(playerTransform.position - transform.position)).normalized;
            bullet.Initialize(direction, false);
        }

        AudioManager.Instance?.PlaySFX(AudioManager.SFX.EnemyShoot);
    }
}
