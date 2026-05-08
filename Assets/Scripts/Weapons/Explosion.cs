using UnityEngine;

/// <summary>
/// Simple explosion visual effect that animates scale and opacity, then returns to pool.
/// </summary>
public class Explosion : MonoBehaviour, IPoolable
{
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private float maxScale = 1.5f;

    private float timer;
    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        baseScale = Vector3.one * 0.3f;
    }

    public void OnSpawnFromPool()
    {
        timer = 0f;
        transform.localScale = baseScale;
        if (spriteRenderer != null)
        {
            Color c = new Color(1f, 0.6f, 0f, 1f); // Orange
            spriteRenderer.color = c;
        }
    }

    public void OnReturnToPool() { }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        if (t >= 1f)
        {
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.Despawn(Tags.Explosion, gameObject);
            else
                gameObject.SetActive(false);
            return;
        }

        // Scale up
        float scale = Mathf.Lerp(0.3f, maxScale, t);
        transform.localScale = Vector3.one * scale;

        // Fade out and shift color from orange to red
        if (spriteRenderer != null)
        {
            float alpha = 1f - t;
            float green = Mathf.Lerp(0.6f, 0f, t);
            spriteRenderer.color = new Color(1f, green, 0f, alpha);
        }
    }
}
