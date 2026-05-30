using UnityEngine;

/// <summary>
/// Enemy – Moves downward. Damages the player on contact.
/// Attach to the Enemy prefab. Requires Rigidbody2D, BoxCollider2D (Is Trigger).
/// </summary>
public class Enemy : MonoBehaviour
{
    [Tooltip("Downward movement speed")]
    public float speed = 3f;

    [Tooltip("Optional: slight horizontal sway amplitude (0 = straight line)")]
    public float swayAmplitude = 0f;

    [Tooltip("Sway frequency")]
    public float swayFrequency = 2f;

    // Auto-destroy boundary
    private float destroyY;
    private float startX;
    private float elapsedTime;

    void Start()
    {
        destroyY = -(Camera.main.orthographicSize + 1f);
        startX = transform.position.x;
        elapsedTime = Random.Range(0f, Mathf.PI * 2); // random phase offset
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Move downward
        Vector3 pos = transform.position;
        pos.y -= speed * Time.deltaTime;

        // Optional horizontal sway for variety
        if (swayAmplitude > 0f)
        {
            pos.x = startX + Mathf.Sin(elapsedTime * swayFrequency) * swayAmplitude;
        }

        transform.position = pos;

        // Destroy when off-screen bottom
        if (pos.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}
