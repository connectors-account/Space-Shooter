using UnityEngine;

/// <summary>
/// Utility to destroy any object that leaves the screen bounds.
/// Attach to objects that should be cleaned up when off-screen.
/// </summary>
public class ScreenBounds : MonoBehaviour
{
    public float margin = 2f;
    public float checkInterval = 0.5f;

    private float nextCheck;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Time.time < nextCheck) return;
        nextCheck = Time.time + checkInterval;

        if (mainCamera == null) return;

        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
        if (viewPos.x < -margin || viewPos.x > 1f + margin ||
            viewPos.y < -margin || viewPos.y > 1f + margin)
        {
            Destroy(gameObject);
        }
    }
}
