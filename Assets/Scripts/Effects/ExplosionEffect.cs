using UnityEngine;
using System.Collections;

/// <summary>
/// Simple explosion visual effect using scaling and fading sprites.
/// Can be spawned at any position via the static SpawnExplosion method.
/// </summary>
public class ExplosionEffect : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float maxScale = 2f;
    [SerializeField] private Color startColor = new Color(1f, 0.8f, 0f, 1f);
    [SerializeField] private Color endColor = new Color(1f, 0.2f, 0f, 0f);

    private static GameObject explosionPrefab;

    /// <summary>
    /// Spawns an explosion effect at the given position.
    /// Creates the effect dynamically if no prefab is set.
    /// </summary>
    public static void SpawnExplosion(Vector3 position, float scale = 1f)
    {
        GameObject explosionObj = new GameObject("Explosion");
        explosionObj.transform.position = position;

        // Create multiple expanding circles for the explosion
        for (int i = 0; i < 3; i++)
        {
            GameObject ring = new GameObject($"ExplosionRing_{i}");
            ring.transform.SetParent(explosionObj.transform);
            ring.transform.localPosition = Vector3.zero;

            SpriteRenderer sr = ring.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = Color.Lerp(Color.yellow, Color.red, i * 0.3f);
            sr.sortingOrder = 100 + i;

            ring.transform.localScale = Vector3.one * 0.1f * scale;
        }

        ExplosionEffect effect = explosionObj.AddComponent<ExplosionEffect>();
        effect.maxScale = 2f * scale;
        effect.StartCoroutine(effect.AnimateExplosion());
    }

    /// <summary>
    /// Animates the explosion: scales up while fading out.
    /// </summary>
    private IEnumerator AnimateExplosion()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;

                // Each ring expands at a slightly different rate
                float ringDelay = i * 0.1f;
                float ringT = Mathf.Clamp01((t - ringDelay) / (1f - ringDelay));

                float currentScale = Mathf.Lerp(0.1f, maxScale * (1f - i * 0.2f), ringT);
                renderers[i].transform.localScale = Vector3.one * currentScale;

                Color c = Color.Lerp(startColor, endColor, ringT);
                c.a = 1f - ringT;
                renderers[i].color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Creates a simple circle sprite at runtime for the explosion rings.
    /// </summary>
    private static Sprite CreateCircleSprite()
    {
        int resolution = 32;
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color clear = new Color(1f, 1f, 1f, 0f);
        float center = resolution / 2f;
        float radius = center - 1;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                    tex.SetPixel(x, y, Color.white);
                else
                    tex.SetPixel(x, y, clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), Vector2.one * 0.5f, resolution);
    }
}
