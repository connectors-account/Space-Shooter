using UnityEngine;

/// <summary>
/// Simple explosion visual effect: scales up and fades out, then self-destructs.
/// </summary>
public class ExplosionEffect : MonoBehaviour
{
    public float duration = 0.3f;
    private float timer = 0f;
    private SpriteRenderer sr;
    private Vector3 startScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        // Scale up
        transform.localScale = startScale * (1f + t * 3f);

        // Fade out
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f - t;
            sr.color = c;
        }

        if (timer >= duration)
            Destroy(gameObject);
    }
}
