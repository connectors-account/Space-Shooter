using UnityEngine;

/// <summary>
/// Destroys this GameObject once it leaves the visible screen area (plus a margin).
/// Attach to bullets and enemies so they are cleaned up after exiting the view,
/// preventing an ever-growing number of objects from hurting performance.
///
/// Works by converting the object's world position to viewport coordinates
/// (where 0..1 is on-screen) and checking whether it has drifted past a margin.
/// </summary>
public class DestroyOffScreen : MonoBehaviour
{
    [Tooltip("Extra viewport margin before destroying (0.1 = 10% past the edge).")]
    public float margin = 0.1f;

    // Cached reference to the main camera.
    private Camera cam;

    /// <summary>
    /// Start caches the main camera reference.
    /// </summary>
    private void Start()
    {
        cam = Camera.main;
    }

    /// <summary>
    /// Update checks the object's viewport position each frame.
    /// </summary>
    private void Update()
    {
        if (cam == null)
        {
            // Camera may have been re-created (e.g., scene reload); try again.
            cam = Camera.main;
            if (cam == null) return;
        }

        // Convert world position to viewport space (0..1 within the camera view).
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);

        // Destroy when the object moves beyond the screen edges plus the margin.
        if (viewportPos.x < -margin || viewportPos.x > 1f + margin ||
            viewportPos.y < -margin || viewportPos.y > 1f + margin)
        {
            Destroy(gameObject);
        }
    }
}
