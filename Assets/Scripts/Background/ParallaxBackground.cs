using UnityEngine;

/// <summary>
/// Creates a smooth parallax scrolling background using multiple layers.
/// Each layer scrolls at a different speed for depth effect.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform transform;
        public float scrollSpeed = 1f;
        public bool tile = true;
        [HideInInspector] public float spriteHeight;
        [HideInInspector] public Vector3 startPosition;
    }

    [SerializeField] private ParallaxLayer[] layers;

    private void Start()
    {
        if (layers == null) return;

        foreach (var layer in layers)
        {
            if (layer.transform == null) continue;
            layer.startPosition = layer.transform.position;

            SpriteRenderer sr = layer.transform.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                layer.spriteHeight = sr.bounds.size.y;
            }
        }
    }

    private void Update()
    {
        if (layers == null) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        foreach (var layer in layers)
        {
            if (layer.transform == null) continue;

            // Scroll downward
            layer.transform.Translate(Vector3.down * layer.scrollSpeed * Time.deltaTime, Space.World);

            // Tile: reset position when scrolled past
            if (layer.tile && layer.spriteHeight > 0)
            {
                if (layer.transform.position.y <= layer.startPosition.y - layer.spriteHeight)
                {
                    Vector3 pos = layer.transform.position;
                    pos.y += layer.spriteHeight * 2f;
                    layer.transform.position = pos;
                }
            }
        }
    }
}
