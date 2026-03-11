using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float scrollSpeed = 0.5f;
    public bool infiniteScrolling = true;

    [Header("References")]
    public Transform[] backgroundLayers;
    public float[] layerSpeeds;

    private float[] layerHeights;
    private Vector3[] startPositions;

    private void Start()
    {
        if (backgroundLayers == null || backgroundLayers.Length == 0)
        {
            // Auto-find child layers
            backgroundLayers = new Transform[transform.childCount];
            layerSpeeds = new float[transform.childCount];

            for (int i = 0; i < transform.childCount; i++)
            {
                backgroundLayers[i] = transform.GetChild(i);
                layerSpeeds[i] = scrollSpeed * (1f + i * 0.5f);
            }
        }

        InitializeLayers();
    }

    private void InitializeLayers()
    {
        layerHeights = new float[backgroundLayers.Length];
        startPositions = new Vector3[backgroundLayers.Length];

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            if (backgroundLayers[i] == null) continue;

            startPositions[i] = backgroundLayers[i].position;

            // Get height from sprite renderer
            SpriteRenderer sr = backgroundLayers[i].GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                layerHeights[i] = sr.sprite.bounds.size.y * backgroundLayers[i].localScale.y;
            }
            else
            {
                layerHeights[i] = 10f; // Default height
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing)
            return;

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            if (backgroundLayers[i] == null) continue;

            float speed = i < layerSpeeds.Length ? layerSpeeds[i] : scrollSpeed;
            Vector3 movement = Vector3.down * speed * Time.deltaTime;
            backgroundLayers[i].Translate(movement);

            if (infiniteScrolling)
            {
                // Reset position when layer scrolls off screen
                if (backgroundLayers[i].position.y <= -layerHeights[i])
                {
                    Vector3 newPos = backgroundLayers[i].position;
                    newPos.y += layerHeights[i] * 2f;
                    backgroundLayers[i].position = newPos;
                }
            }
        }
    }

    public void SetScrollSpeed(float newSpeed)
    {
        scrollSpeed = newSpeed;
        for (int i = 0; i < layerSpeeds.Length; i++)
        {
            layerSpeeds[i] = scrollSpeed * (1f + i * 0.5f);
        }
    }

    public void ResetPositions()
    {
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            if (backgroundLayers[i] != null && i < startPositions.Length)
            {
                backgroundLayers[i].position = startPositions[i];
            }
        }
    }
}
