using UnityEngine;

/// <summary>
/// Creates a parallax scrolling background with multiple layers.
/// Each layer scrolls at a different speed for depth effect.
/// Tiles vertically for infinite scrolling.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public string name;
        public SpriteRenderer spriteRenderer;
        public float scrollSpeed;
        public bool tile = true;
    }

    public ParallaxLayer[] layers;

    private float[] layerOffsets;

    private void Start()
    {
        layerOffsets = new float[layers.Length];
    }

    private void Update()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].spriteRenderer == null) continue;

            layerOffsets[i] += layers[i].scrollSpeed * Time.deltaTime;

            if (layers[i].tile)
            {
                // Use material offset for seamless tiling
                if (layers[i].spriteRenderer.material != null)
                {
                    layers[i].spriteRenderer.material.mainTextureOffset = new Vector2(0, layerOffsets[i]);
                }
            }
            else
            {
                // Move the transform
                layers[i].spriteRenderer.transform.Translate(Vector3.down * layers[i].scrollSpeed * Time.deltaTime, Space.World);

                // Reset position when off screen
                float spriteHeight = layers[i].spriteRenderer.bounds.size.y;
                if (layers[i].spriteRenderer.transform.position.y <= -spriteHeight)
                {
                    layers[i].spriteRenderer.transform.position = new Vector3(
                        layers[i].spriteRenderer.transform.position.x,
                        layers[i].spriteRenderer.transform.position.y + spriteHeight * 2f,
                        layers[i].spriteRenderer.transform.position.z
                    );
                }
            }
        }
    }
}
