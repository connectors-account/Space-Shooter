using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layerA;
        public Transform layerB;
        public float speed = 0.5f;
        public float tileHeight = 20f;
    }

    [SerializeField] private ParallaxLayer[] layers;

    private void Update()
    {
        if (layers == null)
        {
            return;
        }

        for (int i = 0; i < layers.Length; i++)
        {
            MoveLayer(layers[i]);
        }
    }

    private void MoveLayer(ParallaxLayer layer)
    {
        if (layer.layerA == null || layer.layerB == null)
        {
            return;
        }

        float move = layer.speed * Time.deltaTime;
        layer.layerA.position += Vector3.down * move;
        layer.layerB.position += Vector3.down * move;

        WrapIfNeeded(layer.layerA, layer.layerB, layer.tileHeight);
        WrapIfNeeded(layer.layerB, layer.layerA, layer.tileHeight);
    }

    private void WrapIfNeeded(Transform current, Transform other, float tileHeight)
    {
        if (current.position.y <= -tileHeight)
        {
            current.position = new Vector3(current.position.x, other.position.y + tileHeight, current.position.z);
        }
    }
}
