using UnityEngine;

/// <summary>
/// Explosion handles explosion visual effects.
/// Auto-deactivates after animation completes.
/// </summary>
public class Explosion : MonoBehaviour, IPooledObject
{
    [Header("Explosion Settings")]
    [SerializeField] private float lifetime = 0.5f;
    [SerializeField] private float expandSpeed = 5f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float maxScale = 2f;

    [Header("Particle Settings")]
    [SerializeField] private bool useParticles = true;
    [SerializeField] private int particleCount = 8;

    private float timer;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialScale;
    private ParticleSystem particles;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        particles = GetComponent<ParticleSystem>();
        initialScale = transform.localScale;
    }

    public void OnObjectSpawn()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
    }

    /// <summary>
    /// Initialize explosion state
    /// </summary>
    private void Initialize()
    {
        timer = lifetime;
        transform.localScale = initialScale;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        if (particles != null && useParticles)
        {
            particles.Play();
        }
    }

    private void Update()
    {
        // Expand
        float scaleProgress = 1f - (timer / lifetime);
        float currentScale = Mathf.Lerp(1f, maxScale, scaleProgress);
        transform.localScale = initialScale * currentScale;

        // Fade out
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = timer / lifetime;
            spriteRenderer.color = color;
        }

        // Count down
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
