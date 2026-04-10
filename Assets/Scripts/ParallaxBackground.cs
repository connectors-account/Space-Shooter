using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Renderer renderer;
        public float speed = 0.1f;
        public Vector2 direction = new Vector2(0f, -1f);
    }

    [SerializeField] private Layer[] layers;

    private void Update()
    {
        if (layers == null)
        {
            return;
        }

        foreach (Layer layer in layers)
        {
            if (layer.renderer == null)
            {
                continue;
            }

            Vector2 offsetDelta = layer.direction.normalized * (layer.speed * Time.deltaTime);
            Material mat = layer.renderer.material;
            mat.mainTextureOffset += offsetDelta;
        }
    }
}
