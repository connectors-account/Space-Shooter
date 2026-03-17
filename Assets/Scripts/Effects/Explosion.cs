using UnityEngine;

/// <summary>
/// Simple explosion effect that destroys itself after a duration.
/// Attach to an explosion prefab with a SpriteRenderer or ParticleSystem.
/// </summary>
public class Explosion : MonoBehaviour
{
    public float duration = 0.5f;
    public float expandSpeed = 3f;
    public float fadeSpeed = 2f;

    private SpriteRenderer spriteRenderer;
    private float timer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timer = 0f;
        transform.localScale = Vector3.one * 0.3f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Expand
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;

        // Fade out
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = Mathf.Lerp(1f, 0f, timer / duration);
            spriteRenderer.color = c;
        }

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}
