using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private Renderer[] backgroundLayers;
    [SerializeField] private float[] layerScrollSpeeds;
    [SerializeField] private bool scrollVertically = true;

    private Vector2[] offsets;

    private void Start()
    {
        if (backgroundLayers == null || backgroundLayers.Length == 0)
        {
            return;
        }

        offsets = new Vector2[backgroundLayers.Length];
        if (layerScrollSpeeds == null || layerScrollSpeeds.Length != backgroundLayers.Length)
        {
            layerScrollSpeeds = new float[backgroundLayers.Length];
            for (int i = 0; i < layerScrollSpeeds.Length; i++)
            {
                layerScrollSpeeds[i] = 0.05f + i * 0.07f;
            }
        }
    }

    private void Update()
    {
        if (backgroundLayers == null || offsets == null)
        {
            return;
        }

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            Renderer layer = backgroundLayers[i];
            if (layer == null || layer.material == null)
            {
                continue;
            }

            float scrollAmount = layerScrollSpeeds[i] * Time.deltaTime;
            offsets[i] += scrollVertically ? new Vector2(0f, scrollAmount) : new Vector2(scrollAmount, 0f);
            layer.material.mainTextureOffset = offsets[i];
        }
    }
}
