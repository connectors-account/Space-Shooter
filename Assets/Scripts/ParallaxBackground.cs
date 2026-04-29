using UnityEngine;

/// <summary>
/// Scrolls layered background sprites for a simple parallax effect.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform layerTransform;
        public float scrollSpeed = 0.3f;
        public float resetY = -12f;
        public float startY = 12f;
    }

    [SerializeField] private Layer[] layers;

    private void Update()
    {
        if (GameManager.Instance != null && (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver))
        {
            return;
        }

        for (int i = 0; i < layers.Length; i++)
        {
            Layer layer = layers[i];
            if (layer.layerTransform == null)
            {
                continue;
            }

            Vector3 pos = layer.layerTransform.position;
            pos.y -= layer.scrollSpeed * Time.deltaTime;

            if (pos.y <= layer.resetY)
            {
                pos.y = layer.startY;
            }

            layer.layerTransform.position = pos;
        }
    }
}
