using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Simple single-layer vertical scrolling background.
    /// Works one of two ways depending on the setup:
    ///  1) If a Material with a texture is present on a Renderer, it scrolls the
    ///     material's main texture UV offset (seamless, needs a tiling texture).
    ///  2) Otherwise it moves the transform downward and loops it back to the top,
    ///     which works with two stacked background sprites parented under one object.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Tooltip("Scroll speed. For UV mode this is UV units/sec; for transform mode, world units/sec.")]
        [SerializeField] private float scrollSpeed = 0.5f;

        [Tooltip("If true, scroll the material texture UVs. If false, move the transform and loop it.")]
        [SerializeField] private bool useMaterialScroll = true;

        [Header("Transform Scroll Settings")]
        [Tooltip("World-space Y position at which the object resets back to the top.")]
        [SerializeField] private float resetThresholdY = -10f;

        [Tooltip("World-space Y position to reset to when the threshold is crossed.")]
        [SerializeField] private float resetToY = 10f;

        private Material scrollingMaterial;
        private Vector2 uvOffset;

        private void Awake()
        {
            if (useMaterialScroll)
            {
                Renderer rend = GetComponent<Renderer>();
                if (rend != null)
                {
                    // Use the instance material so we do not modify the shared asset.
                    scrollingMaterial = rend.material;
                }
                else
                {
                    // No renderer available; fall back to transform scrolling.
                    useMaterialScroll = false;
                }
            }
        }

        private void Update()
        {
            if (useMaterialScroll && scrollingMaterial != null)
            {
                uvOffset.y += scrollSpeed * Time.deltaTime;
                if (uvOffset.y > 1f)
                {
                    uvOffset.y -= 1f;
                }
                scrollingMaterial.mainTextureOffset = uvOffset;
            }
            else
            {
                transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime, Space.World);
                if (transform.position.y <= resetThresholdY)
                {
                    Vector3 pos = transform.position;
                    pos.y = resetToY;
                    transform.position = pos;
                }
            }
        }
    }
}
