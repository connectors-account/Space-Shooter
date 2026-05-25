using UnityEngine;

/// <summary>
/// Simple explosion visual effect — expands and fades out, then self-destructs.
/// </summary>
public class ExplosionEffect : MonoBehaviour
{
    private float lifetime = 0.4f;
    private float timer;
    private SpriteRenderer sr;
    private Vector3 startScale;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = Vector3.one * 0.3f;
        transform.localScale = startScale;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifetime;

        // Expand
        transform.localScale = Vector3.Lerp(startScale, Vector3.one * 1.5f, t);

        // Fade out
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(0.8f, 0f, t);
            sr.color = c;
        }

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
