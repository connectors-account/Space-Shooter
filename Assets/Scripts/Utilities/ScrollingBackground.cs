using UnityEngine;

/// <summary>
/// ScrollingBackground provides a simple infinite vertical scrolling effect.
/// Uses two copies of the background that swap positions.
/// </summary>
public class ScrollingBackground : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Scroll speed in units per second")]
    public float scrollSpeed = 2f;
    
    [Tooltip("The height at which to reset the position")]
    public float resetHeight = -20f;
    
    [Tooltip("The offset to apply when resetting (usually double the camera height)")]
    public float resetOffset = 40f;

    void Update()
    {
        // Don't scroll if game is paused
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused())
        {
            // Still scroll in main menu for visual effect
            if (GameManager.Instance.GetCurrentState() != GameManager.GameState.MainMenu)
                return;
        }
        
        // Move downward
        transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
        
        // Reset position when below threshold
        if (transform.position.y <= resetHeight)
        {
            Vector3 newPos = transform.position;
            newPos.y += resetOffset;
            transform.position = newPos;
        }
    }
}
