using UnityEngine;

namespace SpaceShooter.Environment
{
    /// <summary>
    /// Simple parallax scrolling background with two layers.
    /// Each layer scrolls at a different speed creating depth illusion.
    /// Tiles vertically for seamless infinite scrolling.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Layer 1 - Far Background (slower)")]
        [SerializeField] private SpriteRenderer layer1Renderer;
        [SerializeField] private float layer1Speed = 0.5f;

        [Header("Layer 2 - Near Background (faster)")]
        [SerializeField] private SpriteRenderer layer2Renderer;
        [SerializeField] private float layer2Speed = 1.5f;

        [Header("Scroll Settings")]
        [SerializeField] private float tileHeight = 20f;  // Height of one background tile

        // Internal tracking for each layer
        private Transform layer1A, layer1B;
        private Transform layer2A, layer2B;

        private void Start()
        {
            // Create duplicate sprites for seamless tiling
            SetupLayer(layer1Renderer, out layer1A, out layer1B, "Layer1");
            SetupLayer(layer2Renderer, out layer2A, out layer2B, "Layer2");
        }

        /// <summary>
        /// Creates two copies of a background sprite stacked vertically.
        /// </summary>
        private void SetupLayer(SpriteRenderer sourceRenderer, out Transform tileA, out Transform tileB, string name)
        {
            tileA = null;
            tileB = null;

            if (sourceRenderer == null) return;

            // Use the source as tileA
            tileA = sourceRenderer.transform;

            // Create a duplicate for tileB (positioned above tileA)
            GameObject duplicate = Instantiate(sourceRenderer.gameObject, transform);
            duplicate.name = name + "_TileB";
            tileB = duplicate.transform;
            tileB.position = tileA.position + Vector3.up * tileHeight;
        }

        private void Update()
        {
            // Scroll each layer
            ScrollLayer(layer1A, layer1B, layer1Speed);
            ScrollLayer(layer2A, layer2B, layer2Speed);
        }

        /// <summary>
        /// Scrolls two tiles downward and wraps them for infinite scrolling.
        /// </summary>
        private void ScrollLayer(Transform tileA, Transform tileB, float speed)
        {
            if (tileA == null || tileB == null) return;

            // Move both tiles downward
            float offset = speed * Time.deltaTime;
            tileA.position += Vector3.down * offset;
            tileB.position += Vector3.down * offset;

            // Wrap tiles when they go below the screen
            float resetThreshold = -tileHeight;

            if (tileA.position.y <= resetThreshold)
            {
                tileA.position = tileB.position + Vector3.up * tileHeight;
            }

            if (tileB.position.y <= resetThreshold)
            {
                tileB.position = tileA.position + Vector3.up * tileHeight;
            }
        }
    }
}
