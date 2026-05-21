using UnityEngine;

namespace SpaceShooter.Effects
{
    /// <summary>
    /// Scrolls a background layer downward and tiles it seamlessly.
    /// Attach to each background layer sprite. Use different speeds
    /// for each layer to achieve a parallax effect.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [SerializeField] private float scrollSpeed = 1f;
        [SerializeField] private float tileHeight = 10f; // height of one tile in world units

        private Vector3 startPos;
        private float offset;

        private void Start()
        {
            startPos = transform.position;

            // Auto-detect tile height from sprite if available
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                tileHeight = sr.bounds.size.y;
            }
        }

        private void Update()
        {
            offset += scrollSpeed * Time.deltaTime;

            // Wrap around when one full tile has scrolled past
            if (offset >= tileHeight)
            {
                offset -= tileHeight;
            }

            transform.position = startPos + Vector3.down * offset;
        }
    }
}
