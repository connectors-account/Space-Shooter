using UnityEngine;

/// <summary>
/// Static helper that instantiates bullet patterns.
/// Requires a "BulletPrefab" loaded from Resources or assigned elsewhere.
/// Call the static methods from PlayerController and Enemy scripts.
/// </summary>
public static class BulletSpawner
{
    private static GameObject bulletPrefab;

    /// <summary>Must be called once (e.g. from GameManager or PlayerController.Start)
    /// to cache the prefab reference.</summary>
    public static void SetPrefab(GameObject prefab)
    {
        bulletPrefab = prefab;
    }

    // ── Pattern: Single Shot ──────────────────────────────────────────
    public static void SingleShot(Vector3 origin, Vector2 direction, float speed,
                                   int damage, Bullet.Owner owner,
                                   Bullet.BulletType type = Bullet.BulletType.Normal)
    {
        SpawnOneBullet(origin, direction, speed, damage, owner, type);
    }

    // ── Pattern: Spread Shot (3-way) ──────────────────────────────────
    public static void SpreadShot3(Vector3 origin, Vector2 baseDir, float speed,
                                    int damage, Bullet.Owner owner)
    {
        float spreadAngle = 15f; // degrees between each ray
        for (int i = -1; i <= 1; i++)
        {
            Vector2 dir = RotateVector(baseDir, i * spreadAngle);
            SpawnOneBullet(origin, dir, speed, damage, owner, Bullet.BulletType.Spread);
        }
    }

    // ── Pattern: Spread Shot (5-way) ──────────────────────────────────
    public static void SpreadShot5(Vector3 origin, Vector2 baseDir, float speed,
                                    int damage, Bullet.Owner owner)
    {
        float spreadAngle = 12f;
        for (int i = -2; i <= 2; i++)
        {
            Vector2 dir = RotateVector(baseDir, i * spreadAngle);
            SpawnOneBullet(origin, dir, speed, damage, owner, Bullet.BulletType.Spread);
        }
    }

    // ── Pattern: Burst (ring of bullets) ──────────────────────────────
    public static void BurstPattern(Vector3 origin, int count, float speed,
                                     int damage, Bullet.Owner owner)
    {
        float step = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * step;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                                      Mathf.Sin(angle * Mathf.Deg2Rad));
            SpawnOneBullet(origin, dir, speed, damage, owner, Bullet.BulletType.Normal);
        }
    }

    // ── Internal ──────────────────────────────────────────────────────
    private static void SpawnOneBullet(Vector3 origin, Vector2 direction, float speed,
                                        int damage, Bullet.Owner owner, Bullet.BulletType type)
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("BulletSpawner: bulletPrefab is null. Call SetPrefab first.");
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        GameObject go = Object.Instantiate(bulletPrefab, origin, rot);
        Bullet b = go.GetComponent<Bullet>();
        if (b == null) b = go.AddComponent<Bullet>();

        b.direction  = direction;
        b.speed      = speed;
        b.damage     = damage;
        b.owner      = owner;
        b.bulletType = type;

        // Set tag/layer so collisions route correctly
        go.tag = owner == Bullet.Owner.Player ? "PlayerBullet" : "EnemyBullet";
    }

    private static Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
