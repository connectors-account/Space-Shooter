using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform transform;
        public float scrollSpeed = 1f;
        public bool tileVertically = true;
        public float height = 20f;
    }

    [Header("Parallax Layers")]
    public ParallaxLayer[] layers;

    [Header("Global Settings")]
    public float baseScrollSpeed = 2f;
    public bool autoScroll = true;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused())
            return;

        foreach (var layer in layers)
        {
            if (layer.transform == null) continue;

            // Scroll the layer
            float scrollAmount = layer.scrollSpeed * baseScrollSpeed * Time.deltaTime;
            layer.transform.position += Vector3.down * scrollAmount;

            // Tile vertically (wrap around)
            if (layer.tileVertically)
            {
                if (layer.transform.position.y <= -layer.height)
                {
                    layer.transform.position += Vector3.up * (layer.height * 2);
                }
            }
        }
    }
}

// Separate component for simple single-layer scrolling
public class ScrollingBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 2f;
    public float resetHeight = 20f;
    public float startHeight = 0f;

    [Header("References")]
    public Transform secondBackground; // For seamless looping

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused())
            return;

        // Scroll down
        transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

        // Check for reset
        if (transform.position.y <= -resetHeight)
        {
            Vector3 newPos = transform.position;
            newPos.y += resetHeight * 2;
            transform.position = newPos;
        }
    }
}
