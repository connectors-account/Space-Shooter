using UnityEngine;

/// <summary>
/// ParallaxBackground creates an infinite scrolling background effect.
/// Works with multiple layers for depth effect.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("Base scroll speed")]
    public float scrollSpeed = 2f;
    
    [Tooltip("Parallax multiplier (lower = slower/farther)")]
    public float parallaxMultiplier = 1f;

    [Header("Tiling Settings")]
    [Tooltip("Height of the background tile")]
    public float tileHeight = 20f;
    
    [Tooltip("Starting Y position")]
    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        // Don't scroll if game is paused
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused())
            return;
        
        // Calculate new position
        float newY = transform.position.y - (scrollSpeed * parallaxMultiplier * Time.deltaTime);
        
        // Reset position when tile scrolls past the camera
        if (newY <= startY - tileHeight)
        {
            newY += tileHeight;
        }
        
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
