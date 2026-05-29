using UnityEngine;

/// <summary>
/// Simple explosion effect that scales up and fades out, then self-destructs.
/// Attach to an Explosion prefab with a SpriteRenderer.
/// </summary>
public class ExplosionEffect : MonoBehaviour
{
    public float duration = 0.5f;
    public float maxScale = 2f;
    public Color startColor = new Color(1f, 0.8f, 0f, 1f);  // Yellow-orange
    public Color endColor = new Color(1f, 0.2f, 0f, 0f);     // Red, transparent

    private float timer;
    private SpriteRenderer sr;
    private Vector3 initialScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
        timer = 0f;

        if (sr != null)
            sr.color = startColor;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Scale up
        float scale = Mathf.Lerp(1f, maxScale, t);
        transform.localScale = initialScale * scale;

        // Fade color
        if (sr != null)
        {
            sr.color = Color.Lerp(startColor, endColor, t);
        }
    }
}
