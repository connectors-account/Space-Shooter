using UnityEngine;

/// <summary>
/// Enemy bullet: flies downward toward the player.
/// Tag this prefab "EnemyBullet". Attach a CircleCollider2D (isTrigger = true).
/// The PlayerController OnTriggerEnter2D handles the player-side damage logic.
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    [Header("Stats")]
    public float speed    = 6f;
    public float lifeTime = 5f;

    void Start() => Destroy(gameObject, lifeTime);

    void Update() => transform.Translate(Vector2.down * speed * Time.deltaTime);

    // Destroy the bullet when it hits the player (damage already handled by PlayerController)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Destroy(gameObject);
    }
}
