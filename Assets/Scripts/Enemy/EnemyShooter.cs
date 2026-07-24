// ============================================================
//  EnemyShooter.cs  –  Enemy bullet patterns
//  Patterns: Single, Spread3, Spread5, Aimed, Circle8, Burst
// ============================================================
using System.Collections;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public enum ShotPattern { Single, Spread3, Spread5, Aimed, Circle8, Burst }

    [Header("Shooting")]
    public ShotPattern pattern      = ShotPattern.Single;
    public GameObject  bulletPrefab;
    public float       fireRate     = 1.8f;
    public float       bulletSpeed  = 5f;
    public int         bulletDmg    = 1;
    public float       startDelay   = 0.5f;

    void Start() => StartCoroutine(ShootLoop());

    IEnumerator ShootLoop()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            if (GameManager.Instance?.State == GameState.Playing)
                Shoot();
            yield return new WaitForSeconds(fireRate);
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;
        AudioManager.Instance?.PlayEnemyShoot();

        switch (pattern)
        {
            case ShotPattern.Single:   FireDir(Vector2.down);            break;
            case ShotPattern.Spread3:  FireSpread(3, 20f);               break;
            case ShotPattern.Spread5:  FireSpread(5, 15f);               break;
            case ShotPattern.Aimed:    FireAimed();                      break;
            case ShotPattern.Circle8:  FireCircle(8);                    break;
            case ShotPattern.Burst:    StartCoroutine(FireBurst(3, 0.1f)); break;
        }
    }

    // ── Pattern helpers ──────────────────────────────────────

    void FireDir(Vector2 dir)
    {
        SpawnBullet(transform.position, dir.normalized);
    }

    void FireSpread(int count, float angleBetween)
    {
        float startAngle = -(count - 1) * angleBetween * 0.5f;
        for (int i = 0; i < count; i++)
        {
            float a   = startAngle + i * angleBetween;
            var   dir = Quaternion.Euler(0, 0, a) * Vector2.down;
            SpawnBullet(transform.position, dir);
        }
    }

    void FireAimed()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        Vector2 dir = go
            ? ((go.transform.position - transform.position).normalized)
            : Vector2.down;
        SpawnBullet(transform.position, dir);
    }

    void FireCircle(int count)
    {
        float step = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float a   = i * step;
            var   dir = new Vector2(Mathf.Sin(a * Mathf.Deg2Rad),
                                    Mathf.Cos(a * Mathf.Deg2Rad));
            SpawnBullet(transform.position, dir);
        }
    }

    IEnumerator FireBurst(int shots, float delay)
    {
        for (int i = 0; i < shots; i++)
        {
            FireDir(Vector2.down);
            yield return new WaitForSeconds(delay);
        }
    }

    void SpawnBullet(Vector3 pos, Vector2 dir)
    {
        var go = Instantiate(bulletPrefab, pos, Quaternion.identity);
        var b  = go.GetComponent<Bullet>();
        b?.Init(dir, bulletSpeed, bulletDmg, isPlayer: false);
    }
}
