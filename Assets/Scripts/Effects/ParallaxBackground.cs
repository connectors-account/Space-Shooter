// =============================================================================
// ParallaxBackground.cs — Multi-layer parallax scrolling background
// =============================================================================
using UnityEngine;

namespace SpaceShooter.Effects
{
    /// <summary>
    /// Scrolls a background layer downward and tiles it seamlessly.
    /// Multiple instances with different speeds create parallax depth.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Scroll Settings")]
        [SerializeField] private float scrollSpeed = 1f;
        [SerializeField] private float tileHeight = 20f;

        [Header("Optional: Follow Player Horizontally")]
        [SerializeField] private float horizontalParallaxFactor = 0f;

        private Vector3 startPosition;
        private Transform playerTransform;

        private void Start()
        {
            startPosition = transform.position;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        private void Update()
        {
            // Vertical scroll
            float newY = Mathf.Repeat(Time.time * scrollSpeed, tileHeight);
            transform.position = startPosition + Vector3.down * newY;

            // Optional horizontal parallax
            if (horizontalParallaxFactor != 0f && playerTransform != null)
            {
                float x = startPosition.x + playerTransform.position.x * horizontalParallaxFactor;
                transform.position = new Vector3(x, transform.position.y, transform.position.z);
            }
        }
    }
}
