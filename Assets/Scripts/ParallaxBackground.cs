using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform tileA;
        public Transform tileB;
        public float scrollSpeed = 1f;
        public float tileHeight = 12f;
    }

    [SerializeField] private Layer[] layers;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Paused)
        {
            return;
        }

        for (int i = 0; i < layers.Length; i++)
        {
            ScrollLayer(layers[i]);
        }
    }

    private void ScrollLayer(Layer layer)
    {
        if (layer.tileA == null || layer.tileB == null)
        {
            return;
        }

        float delta = layer.scrollSpeed * Time.deltaTime;
        layer.tileA.position += Vector3.down * delta;
        layer.tileB.position += Vector3.down * delta;

        float resetThreshold = -layer.tileHeight;

        if (layer.tileA.position.y <= resetThreshold)
        {
            layer.tileA.position = layer.tileB.position + Vector3.up * layer.tileHeight;
        }

        if (layer.tileB.position.y <= resetThreshold)
        {
            layer.tileB.position = layer.tileA.position + Vector3.up * layer.tileHeight;
        }
    }
}
