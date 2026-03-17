using UnityEngine;

namespace SpaceShooter.Effects
{
    /// <summary>
    /// Explosion effect with particles and sound
    /// </summary>
    public class ExplosionEffect : MonoBehaviour
    {
        [Header("Effect Settings")]
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private float expandSpeed = 5f;
        [SerializeField] private float fadeSpeed = 2f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip explosionSound;
        [SerializeField] private float volume = 0.7f;
        
        [Header("Particle Settings")]
        [SerializeField] private bool createParticles = true;
        [SerializeField] private int particleCount = 8;
        [SerializeField] private float particleSpeed = 3f;
        [SerializeField] private Color[] particleColors = { Color.yellow, Color.red, Color.orange };
        
        private SpriteRenderer spriteRenderer;
        private float startTime;
        
        private void Start()
        {
            startTime = Time.time;
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(explosionSound, transform.position, volume);
            }
            
            if (createParticles)
            {
                CreateParticles();
            }
            
            Destroy(gameObject, lifetime);
        }
        
        private void Update()
        {
            float elapsed = Time.time - startTime;
            float progress = elapsed / lifetime;
            
            // Expand
            float scale = 1f + (progress * expandSpeed);
            transform.localScale = Vector3.one * scale;
            
            // Fade
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1f, 0f, progress);
                spriteRenderer.color = color;
            }
        }
        
        private void CreateParticles()
        {
            float angleStep = 360f / particleCount;
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = i * angleStep + Random.Range(-10f, 10f);
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
                
                CreateParticle(direction);
            }
        }
        
        private void CreateParticle(Vector2 direction)
        {
            GameObject particle = new GameObject("Particle");
            particle.transform.position = transform.position;
            particle.transform.SetParent(transform);
            
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = CreateParticleSprite();
            sr.color = particleColors[Random.Range(0, particleColors.Length)];
            sr.sortingOrder = 100;
            
            float scale = Random.Range(0.1f, 0.3f);
            particle.transform.localScale = Vector3.one * scale;
            
            ExplosionParticle ep = particle.AddComponent<ExplosionParticle>();
            ep.Initialize(direction * particleSpeed * Random.Range(0.5f, 1.5f), lifetime);
        }
        
        private Sprite CreateParticleSprite()
        {
            Texture2D texture = new Texture2D(4, 4);
            Color[] colors = new Color[16];
            for (int i = 0; i < 16; i++)
            {
                colors[i] = Color.white;
            }
            texture.SetPixels(colors);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            
            return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }
    }
    
    public class ExplosionParticle : MonoBehaviour
    {
        private Vector2 velocity;
        private float lifetime;
        private float startTime;
        private SpriteRenderer spriteRenderer;
        
        public void Initialize(Vector2 vel, float life)
        {
            velocity = vel;
            lifetime = life;
            startTime = Time.time;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        private void Update()
        {
            float elapsed = Time.time - startTime;
            float progress = elapsed / lifetime;
            
            // Move
            transform.position += (Vector3)velocity * Time.deltaTime;
            
            // Slow down
            velocity *= 0.95f;
            
            // Shrink and fade
            float scale = Mathf.Lerp(1f, 0f, progress);
            transform.localScale = Vector3.one * scale * 0.2f;
            
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1f, 0f, progress);
                spriteRenderer.color = color;
            }
        }
    }
}
