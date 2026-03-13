using UnityEngine;

/// <summary>
/// Manages visual effects like explosions and hit flashes.
/// Creates procedural particle-like effects at runtime.
/// </summary>
public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Spawn an explosion effect at the given position with the given scale.
    /// </summary>
    public void SpawnExplosion(Vector3 position, float scale = 1f)
    {
        GameObject explosion = new GameObject("Explosion");
        explosion.transform.position = position;

        // Create multiple particles
        int particleCount = Mathf.RoundToInt(8 * scale);
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = CreateParticle(explosion.transform);
            ExplosionParticle ep = particle.AddComponent<ExplosionParticle>();
            ep.Initialize(scale);
        }

        // Create flash
        GameObject flash = new GameObject("Flash");
        flash.transform.position = position;
        flash.transform.SetParent(explosion.transform);
        SpriteRenderer flashSR = flash.AddComponent<SpriteRenderer>();
        flashSR.sprite = CreateCircleSprite();
        flashSR.color = new Color(1f, 0.9f, 0.5f, 0.8f);
        flash.transform.localScale = Vector3.one * scale * 1.5f;
        flashSR.sortingLayerName = "Foreground";
        flashSR.sortingOrder = 100;
        ExplosionFlash ef = flash.AddComponent<ExplosionFlash>();
        ef.Initialize(scale);

        Destroy(explosion, 1.5f);
    }

    /// <summary>
    /// Spawn a small hit effect (bullet impact).
    /// </summary>
    public void SpawnHitEffect(Vector3 position)
    {
        GameObject hit = new GameObject("HitEffect");
        hit.transform.position = position;

        for (int i = 0; i < 4; i++)
        {
            GameObject particle = CreateParticle(hit.transform);
            particle.transform.localScale = Vector3.one * 0.05f;
            SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.yellow;
            ExplosionParticle ep = particle.AddComponent<ExplosionParticle>();
            ep.Initialize(0.3f);
        }

        Destroy(hit, 0.5f);
    }

    private GameObject CreateParticle(Transform parent)
    {
        GameObject particle = new GameObject("Particle");
        particle.transform.SetParent(parent);
        particle.transform.position = parent.position;

        SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.sortingLayerName = "Foreground";
        sr.sortingOrder = 99;

        return particle;
    }

    private Sprite CreateCircleSprite()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(1f - dist / radius);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}

/// <summary>
/// Animates individual explosion particles - flying outward and fading.
/// </summary>
public class ExplosionParticle : MonoBehaviour
{
    private Vector3 velocity;
    private float lifetime;
    private float maxLifetime;
    private SpriteRenderer sr;

    public void Initialize(float scale)
    {
        sr = GetComponent<SpriteRenderer>();
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float speed = Random.Range(2f, 6f) * scale;
        velocity = new Vector3(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed, 0f);

        maxLifetime = Random.Range(0.3f, 0.8f);
        lifetime = maxLifetime;

        transform.localScale = Vector3.one * Random.Range(0.05f, 0.15f) * scale;

        if (sr != null)
        {
            float r = Random.Range(0.8f, 1f);
            float g = Random.Range(0.3f, 0.7f);
            sr.color = new Color(r, g, 0f, 1f);
        }
    }

    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += velocity * Time.deltaTime;
        velocity *= 0.95f;

        float t = 1f - (lifetime / maxLifetime);
        transform.localScale *= 0.98f;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f - t;
            sr.color = c;
        }
    }
}

/// <summary>
/// Animates the explosion flash - quick scale up and fade out.
/// </summary>
public class ExplosionFlash : MonoBehaviour
{
    private float lifetime;
    private SpriteRenderer sr;
    private float initialScale;

    public void Initialize(float scale)
    {
        sr = GetComponent<SpriteRenderer>();
        lifetime = 0.15f;
        initialScale = scale * 1.5f;
    }

    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (sr != null)
        {
            Color c = sr.color;
            c.a = lifetime / 0.15f;
            sr.color = c;
        }

        transform.localScale = Vector3.one * initialScale * (1f + (0.15f - lifetime) * 3f);
    }
}
