// ============================================================================
// ParallaxBackground.cs — Infinite vertical scrolling parallax layers
// Attach to a parent object that has two child sprites stacked vertically.
// Each layer scrolls at a different speed to create depth.
// ============================================================================
using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Background
{
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Scroll Settings")]
        [SerializeField] private float scrollSpeed = 2f;

        [Header("Layer Setup — assign two vertically tiled children")]
        [SerializeField] private Transform[] layers;  // exactly 2 copies stacked

        private float _spriteHeight;

        private void Start()
        {
            if (layers == null || layers.Length < 2)
            {
                Debug.LogWarning("ParallaxBackground needs at least 2 child layers.");
                return;
            }

            // Determine height from the first layer's SpriteRenderer
            var sr = layers[0].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                _spriteHeight = sr.bounds.size.y;
            }
            else
            {
                _spriteHeight = 20f;  // fallback
            }

            // Stack the second copy directly above the first
            layers[1].position = layers[0].position + Vector3.up * _spriteHeight;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
                return;

            float delta = scrollSpeed * Time.deltaTime;

            foreach (var layer in layers)
            {
                layer.position += Vector3.down * delta;

                // When a layer scrolls fully off the bottom, teleport it above the other
                if (layer.position.y <= -_spriteHeight)
                {
                    layer.position += Vector3.up * (_spriteHeight * 2f);
                }
            }
        }
    }
}
