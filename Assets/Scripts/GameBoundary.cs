using UnityEngine;

/// <summary>
/// Keeps the GameObject (typically the player ship) inside the camera's
/// visible area. Attach this to the player.
///
/// It computes the world-space screen edges from the main orthographic camera
/// and clamps the object's position every frame after movement is applied.
/// A padding value keeps the ship fully on-screen rather than half off the edge.
/// </summary>
public class GameBoundary : MonoBehaviour
{
    [Tooltip("Padding (world units) to keep the object away from the exact edge.")]
    public float padding = 0.5f;

    // Cached camera and computed boundary limits.
    private Camera cam;
    private float minX, maxX, minY, maxY;

    /// <summary>
    /// Start caches the camera and calculates the boundaries once.
    /// </summary>
    private void Start()
    {
        cam = Camera.main;
        CalculateBounds();
    }

    /// <summary>
    /// Computes world-space min/max X and Y from the orthographic camera.
    /// </summary>
    private void CalculateBounds()
    {
        if (cam == null) return;

        // Orthographic size is half the camera's vertical height in world units.
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Vector3 camPos = cam.transform.position;

        minX = camPos.x - halfWidth + padding;
        maxX = camPos.x + halfWidth - padding;
        minY = camPos.y - halfHeight + padding;
        maxY = camPos.y + halfHeight - padding;
    }

    /// <summary>
    /// LateUpdate runs after all Update/FixedUpdate movement, so we clamp the
    /// final position to ensure the object never sits outside the screen.
    /// </summary>
    private void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;
            CalculateBounds();
            if (cam == null) return;
        }

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
}
