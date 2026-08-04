using UnityEngine;

/// <summary>
/// Player bullet: flies upward, damages enemies, self-destructs off-screen or on hit.
/// Tag this prefab "PlayerBullet". Attach a CircleCollider2D (isTrigger = true).
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Stats")]
    public float speed    = 14f;
    public int   damage   =  1;
    public float lifeTime =  3f;

    void Start() => Destroy(gameObject, lifeTime);

    void Update() => transform.Translate(Vector2.up * speed * Time.deltaTime);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
