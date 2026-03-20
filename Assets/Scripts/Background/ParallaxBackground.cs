using UnityEngine;

/// <summary>
/// Creates a scrolling parallax starfield background.
/// Attach to a background quad/sprite. Tiles vertically for seamless scrolling.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Scrolling")]
    [SerializeField] private float scrollSpeed = 1f;
    [SerializeField] private float resetPositionY = -12f;
    [SerializeField] private float startPositionY = 12f;

    [Header("Parallax Layers (optional)")]
    [Tooltip("Additional background objects for parallax depth effect")]
    [SerializeField] private Transform[] layers;
    [SerializeField] private float[] layerSpeeds;

    private void Update()
    {
        // Scroll the main background
        transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

        // Reset position for seamless tiling
        if (transform.position.y <= resetPositionY)
        {
            transform.position = new Vector3(transform.position.x, startPositionY, transform.position.z);
        }

        // Scroll parallax layers at different speeds
        if (layers != null && layerSpeeds != null)
        {
            for (int i = 0; i < layers.Length && i < layerSpeeds.Length; i++)
            {
                if (layers[i] == null) continue;

                layers[i].position += Vector3.down * layerSpeeds[i] * Time.deltaTime;

                if (layers[i].position.y <= resetPositionY)
                {
                    layers[i].position = new Vector3(
                        layers[i].position.x,
                        startPositionY,
                        layers[i].position.z
                    );
                }
            }
        }
    }
}
