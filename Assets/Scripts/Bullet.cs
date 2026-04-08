using UnityEngine;

/// <summary>
/// Simple bullet that moves in a direction and destroys itself off-screen.
/// </summary>
public class Bullet : MonoBehaviour
{
    public bool isPlayerBullet = true;
    public int  damage = 1;
    public float lifetime = 5f;

    private Vector2 direction;
    private float speed;

    void Start()
    {
        // Tag the bullet so collision detection works
        gameObject.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
}
