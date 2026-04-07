using UnityEngine;

/// <summary>
/// ParallaxBackground - Creates an infinite scrolling background with multiple layers.
/// Attach to a parent GameObject; each child with a SpriteRenderer becomes a parallax layer.
/// Alternatively, configure layers manually via the inspector.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float baseScrollSpeed = 1f;

    [Header("Layers (auto-populated from children if empty)")]
    public ParallaxLayer[] layers;

    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform transform1;
        public Transform transform2;
        public float speedMultiplier = 1f;
        [HideInInspector] public float spriteHeight;
    }

    private void Start()
    {
        // Auto-setup if layers not configured
        if (layers == null || layers.Length == 0)
        {
            AutoSetupLayers();
        }
        else
        {
            // Calculate sprite heights for configured layers
            foreach (var layer in layers)
            {
                if (layer.transform1 != null)
                {
                    SpriteRenderer sr = layer.transform1.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        layer.spriteHeight = sr.bounds.size.y;
                }
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        foreach (var layer in layers)
        {
            if (layer.transform1 == null || layer.transform2 == null) continue;

            float speed = baseScrollSpeed * layer.speedMultiplier * Time.deltaTime;

            // Scroll both copies downward
            layer.transform1.position += Vector3.down * speed;
            layer.transform2.position += Vector3.down * speed;

            // When a copy scrolls completely off-screen bottom, reposition it above the other copy
            if (layer.transform1.position.y <= -layer.spriteHeight)
            {
                layer.transform1.position = new Vector3(
                    layer.transform1.position.x,
                    layer.transform2.position.y + layer.spriteHeight,
                    layer.transform1.position.z
                );
            }
            if (layer.transform2.position.y <= -layer.spriteHeight)
            {
                layer.transform2.position = new Vector3(
                    layer.transform2.position.x,
                    layer.transform1.position.y + layer.spriteHeight,
                    layer.transform2.position.z
                );
            }
        }
    }

    /// <summary>
    /// Automatically find child SpriteRenderers, duplicate them, and set up scrolling pairs.
    /// </summary>
    private void AutoSetupLayers()
    {
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();
        layers = new ParallaxLayer[childRenderers.Length];

        for (int i = 0; i < childRenderers.Length; i++)
        {
            SpriteRenderer sr = childRenderers[i];
            float height = sr.bounds.size.y;

            // The existing object is copy 1
            Transform t1 = sr.transform;

            // Create copy 2 positioned above copy 1
            GameObject copy = Instantiate(sr.gameObject, transform);
            copy.name = sr.gameObject.name + "_Copy";
            Transform t2 = copy.transform;
            t2.position = new Vector3(t1.position.x, t1.position.y + height, t1.position.z);

            layers[i] = new ParallaxLayer
            {
                transform1 = t1,
                transform2 = t2,
                speedMultiplier = 1f + i * 0.5f, // Deeper layers scroll slower
                spriteHeight = height
            };
        }
    }
}
