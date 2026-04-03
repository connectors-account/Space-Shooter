using UnityEngine;

/// <summary>
/// Handles visual effects like explosions and particles using simple sprites.
/// Static methods allow easy spawning from anywhere.
/// </summary>
public class EffectsManager : MonoBehaviour
{
    public static void SpawnExplosion(Vector3 position, Color color)
    {
        // Create explosion parent
        GameObject explosion = new GameObject("Explosion");
        explosion.transform.position = position;

        // Create multiple particles
        int particleCount = 8;
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = new GameObject("Particle");
            particle.transform.position = position;
            particle.transform.SetParent(explosion.transform);

            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite(4, color);
            sr.sortingOrder = 10;

            ExplosionParticle ep = particle.AddComponent<ExplosionParticle>();
            float angle = (360f / particleCount) * i;
            ep.velocity = Quaternion.Euler(0, 0, angle) * Vector3.up * Random.Range(3f, 6f);
            ep.lifetime = Random.Range(0.3f, 0.6f);
            ep.startScale = Random.Range(0.3f, 0.6f);
        }

        Destroy(explosion, 1f);
    }

    static Sprite CreateSquareSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, color);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }
}

/// <summary>
/// Individual explosion particle that moves outward and fades.
/// </summary>
public class ExplosionParticle : MonoBehaviour
{
    public Vector3 velocity;
    public float lifetime = 0.5f;
    public float startScale = 0.5f;

    private float timer;
    private SpriteRenderer sr;

    void Start()
    {
        timer = lifetime;
        sr = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * startScale;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        float t = 1f - (timer / lifetime);

        transform.position += velocity * Time.deltaTime;
        velocity *= 0.95f; // Slow down

        // Fade out and shrink
        transform.localScale = Vector3.one * startScale * (1f - t);
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f - t;
            sr.color = c;
        }

        if (timer <= 0f)
            Destroy(gameObject);
    }
}
