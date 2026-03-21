using UnityEngine;

/// <summary>
/// Manages multiple parallax background layers for a space environment.
/// Creates seamless scrolling by duplicating and repositioning background tiles.
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    [System.Serializable]
    public class BackgroundLayer
    {
        public string name;
        public SpriteRenderer spriteRenderer;
        public float scrollSpeed = 1f;
        [HideInInspector] public Vector3 startPos;
        [HideInInspector] public float spriteHeight;
    }

    public BackgroundLayer[] layers;

    private void Start()
    {
        foreach (var layer in layers)
        {
            if (layer.spriteRenderer != null)
            {
                layer.startPos = layer.spriteRenderer.transform.position;
                layer.spriteHeight = layer.spriteRenderer.bounds.size.y;
            }
        }
    }

    private void Update()
    {
        foreach (var layer in layers)
        {
            if (layer.spriteRenderer == null) continue;

            float offset = Mathf.Repeat(Time.time * layer.scrollSpeed, layer.spriteHeight);
            layer.spriteRenderer.transform.position = layer.startPos + Vector3.down * offset;
        }
    }
}
