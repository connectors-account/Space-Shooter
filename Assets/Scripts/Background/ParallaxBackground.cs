// ============================================================================
// ParallaxBackground.cs — Multi-layer parallax scrolling background
// ============================================================================
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Scrolling")]
    [SerializeField] private float scrollSpeed = 0.5f;
    [SerializeField] private float parallaxFactor = 1f; // 1 = foreground speed, <1 = slower (background)

    [Header("Looping")]
    [SerializeField] private float spriteHeight = 10f; // height of the sprite for looping
    [SerializeField] private bool autoDetectHeight = true;

    private Vector3 startPosition;
    private float totalScroll;

    // =========================================================================
    private void Start()
    {
        startPosition = transform.position;

        if (autoDetectHeight)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                spriteHeight = sr.bounds.size.y;
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        float delta = scrollSpeed * parallaxFactor * Time.deltaTime;
        totalScroll += delta;

        transform.Translate(Vector3.down * delta, Space.World);

        // Loop: when scrolled past one full height, reset
        if (totalScroll >= spriteHeight)
        {
            totalScroll -= spriteHeight;
            transform.position = startPosition;
        }
    }
}
