using UnityEngine;

/// <summary>
/// BulletController - Moves a bullet in a direction and destroys it after its lifetime.
/// Attach to bullet prefabs. Tag player bullets as "PlayerBullet", enemy bullets as "EnemyBullet".
/// Needs a Collider2D (trigger) and Rigidbody2D (kinematic).
/// </summary>
public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float bulletSpeed = 12f;
    public Vector3 direction = Vector3.up;
    public float lifetime = 3f;

    private float aliveTime = 0f;

    private void Update()
    {
        transform.Translate(direction * bulletSpeed * Time.deltaTime, Space.World);

        aliveTime += Time.deltaTime;
        if (aliveTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
