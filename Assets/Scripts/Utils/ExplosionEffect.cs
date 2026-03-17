using UnityEngine;

namespace SpaceShooter.Utils
{
    /// <summary>
    /// Simple explosion visual effect using scaled sprite animation.
    /// Spawned when enemies or the player are destroyed.
    /// </summary>
    public class ExplosionEffect : MonoBehaviour
    {
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float maxScale = 2f;
        [SerializeField] private Color startColor = Color.yellow;
        [SerializeField] private Color endColor = new Color(1f, 0.3f, 0f, 0f); // fading orange

        private SpriteRenderer spriteRenderer;
        private float elapsed;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                // Create a circle-ish sprite
                Texture2D tex = new Texture2D(16, 16);
                for (int x = 0; x < 16; x++)
                {
                    for (int y = 0; y < 16; y++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(8, 8));
                        float alpha = Mathf.Clamp01(1f - dist / 8f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }
                tex.Apply();
                spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            }

            spriteRenderer.sortingOrder = 10;
            Destroy(gameObject, duration);
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Scale up
            float scale = Mathf.Lerp(0.5f, maxScale, t);
            transform.localScale = new Vector3(scale, scale, 1f);

            // Fade color
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(startColor, endColor, t);
        }
    }
}
