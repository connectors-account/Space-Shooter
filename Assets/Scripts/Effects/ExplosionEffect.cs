using UnityEngine;
using System.Collections;

/// <summary>
/// Procedural explosion visual effect using particles rendered as sprites.
/// </summary>
public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private int particleCount = 12;
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float maxRadius = 1.5f;
    [SerializeField] private Color startColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color endColor = new Color(1f, 0.2f, 0f, 0f);

    private void Start()
    {
        StartCoroutine(PlayExplosion());
    }

    private IEnumerator PlayExplosion()
    {
        // Create particles
        Transform[] particles = new Transform[particleCount];
        SpriteRenderer[] renderers = new SpriteRenderer[particleCount];
        Vector2[] directions = new Vector2[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            GameObject p = new GameObject($"Particle_{i}");
            p.transform.parent = transform;
            p.transform.position = transform.position;

            SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCircleSprite(6, Color.white);
            sr.color = startColor;
            sr.sortingOrder = 15;

            float angle = (360f / particleCount) * i + Random.Range(-15f, 15f);
            directions[i] = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            float size = Random.Range(0.1f, 0.3f);
            p.transform.localScale = Vector3.one * size;

            particles[i] = p.transform;
            renderers[i] = sr;
        }

        // Animate
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;

            for (int i = 0; i < particleCount; i++)
            {
                if (particles[i] == null) continue;

                // Move outward
                float speed = maxRadius * (1f - t * 0.5f);
                particles[i].position += (Vector3)(directions[i] * speed * Time.deltaTime * 3f);

                // Fade and shrink
                renderers[i].color = Color.Lerp(startColor, endColor, t);
                float scale = Mathf.Lerp(0.3f, 0f, t);
                particles[i].localScale = Vector3.one * scale;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
