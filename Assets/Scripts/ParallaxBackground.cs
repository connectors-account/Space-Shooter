using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layerTransform;
        public float scrollSpeed = 0.5f;
        public float resetY = -18f;
        public float startY = 18f;
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
            ParallaxLayer layer = layers[i];
            if (layer.layerTransform == null)
            {
                continue;
            }

            Vector3 position = layer.layerTransform.position;
            position.y -= layer.scrollSpeed * Time.deltaTime;

            if (position.y <= layer.resetY)
            {
                position.y = layer.startY;
            }

            layer.layerTransform.position = position;
        }
    }
}
