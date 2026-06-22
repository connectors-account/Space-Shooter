using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple object pool for bullets. Reusing bullet GameObjects avoids the
/// performance cost (and garbage collection spikes) of constantly
/// Instantiating and Destroying projectiles during heavy fire.
///
/// Attach this to an empty "BulletPool" GameObject and assign the bullet
/// prefab in the inspector. Both PlayerController and Enemy reference it.
/// </summary>
public class BulletPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [Tooltip("Bullet prefab (must have the Bullet component).")]
    public Bullet bulletPrefab;

    [Tooltip("How many bullets to create up front.")]
    public int initialSize = 30;

    // Inactive bullets waiting to be reused.
    private readonly Queue<Bullet> pool = new Queue<Bullet>();

    private void Awake()
    {
        // Pre-instantiate a batch of bullets so the first shots are cheap.
        for (int i = 0; i < initialSize; i++)
        {
            Bullet b = CreateBullet();
            b.gameObject.SetActive(false);
            pool.Enqueue(b);
        }
    }

    /// <summary>Instantiate one bullet parented to this pool.</summary>
    private Bullet CreateBullet()
    {
        Bullet b = Instantiate(bulletPrefab, transform);
        return b;
    }

    /// <summary>
    /// Fetch a bullet from the pool (or grow the pool if empty), position it,
    /// initialize it, and activate it.
    /// </summary>
    public Bullet GetBullet(Vector3 position, Vector2 direction, string targetTag, int damage)
    {
        Bullet b = pool.Count > 0 ? pool.Dequeue() : CreateBullet();

        b.transform.position = position;
        b.gameObject.SetActive(true);
        b.Initialize(direction, targetTag, damage, this);
        return b;
    }

    /// <summary>Deactivate a bullet and put it back in the queue for reuse.</summary>
    public void ReturnBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
        pool.Enqueue(bullet);
    }
}
