using UnityEngine;

/// <summary>
/// Simple explosion effect that destroys itself after animation/duration.
/// </summary>
public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float expandSpeed = 5f;
    [SerializeField] private float fadeSpeed = 3f;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private Vector3 initialScale;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timer = duration;
        initialScale = transform.localScale;

        // Set a bright explosion color if no sprite is set
        if (spriteRenderer != null && spriteRenderer.color == Color.white)
        {
            spriteRenderer.color = new Color(1f, 0.6f, 0.1f, 1f); // Orange-yellow
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        // Expand
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;

        // Fade out
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a -= fadeSpeed * Time.deltaTime;
            spriteRenderer.color = c;
        }

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
