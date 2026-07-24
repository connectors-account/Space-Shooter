// ============================================================
//  PlayerShooter.cs  –  Single / double / triple shot + rapid fire
// ============================================================
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public enum FireMode { Single, Double, Triple }

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float      fireRate    = 0.18f;  // seconds between shots
    public float      bulletSpeed = 14f;
    public int        bulletDmg   = 1;

    [Header("Spread (Triple)")]
    public float spreadAngle = 18f;

    public FireMode Mode { get; private set; } = FireMode.Single;

    float _nextFire;
    bool  _rapidFire;
    float _rapidTimer;

    // ── Unity lifecycle ──────────────────────────────────────

    void Update()
    {
        if (GameManager.Instance?.State != GameState.Playing) return;

        // Countdown rapid-fire timer
        if (_rapidFire)
        {
            _rapidTimer -= Time.deltaTime;
            if (_rapidTimer <= 0f)
            {
                _rapidFire = false;
                // fireRate restores automatically (stored externally)
            }
        }

        float rate = _rapidFire ? fireRate * 0.35f : fireRate;

        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Z))
        {
            if (Time.time >= _nextFire)
            {
                _nextFire = Time.time + rate;
                Fire();
            }
        }
    }

    // ── Fire logic ───────────────────────────────────────────

    void Fire()
    {
        AudioManager.Instance?.PlayShoot();

        switch (Mode)
        {
            case FireMode.Single:
                SpawnBullet(transform.position, Vector2.up);
                break;

            case FireMode.Double:
                SpawnBullet(transform.position + Vector3.left  * 0.25f, Vector2.up);
                SpawnBullet(transform.position + Vector3.right * 0.25f, Vector2.up);
                break;

            case FireMode.Triple:
                SpawnBullet(transform.position, Vector2.up);
                SpawnBullet(transform.position, Quaternion.Euler(0,0, spreadAngle) * Vector2.up);
                SpawnBullet(transform.position, Quaternion.Euler(0,0,-spreadAngle) * Vector2.up);
                break;
        }
    }

    void SpawnBullet(Vector3 pos, Vector2 dir)
    {
        if (bulletPrefab == null) return;
        var go = Instantiate(bulletPrefab, pos, Quaternion.identity);
        var b  = go.GetComponent<Bullet>();
        if (b != null)
        {
            b.Init(dir.normalized, bulletSpeed, bulletDmg, isPlayer: true);
        }
    }

    // ── Power-up callbacks ───────────────────────────────────

    public void SetFireMode(FireMode mode) => Mode = mode;

    public void ApplyRapidFire(float duration)
    {
        _rapidFire  = true;
        _rapidTimer = duration;
    }

    public void UpgradeFireMode()
    {
        if (Mode < FireMode.Triple)
            Mode++;
    }
}
