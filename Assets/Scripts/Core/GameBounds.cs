using UnityEngine;

/// <summary>
/// Defines the playable area boundaries based on camera viewport.
/// Provides helper methods for boundary checks and clamping.
/// </summary>
public class GameBounds : MonoBehaviour
{
    public static GameBounds Instance { get; private set; }

    public float MinX { get; private set; }
    public float MaxX { get; private set; }
    public float MinY { get; private set; }
    public float MaxY { get; private set; }

    [Tooltip("Padding inside the screen edges for player movement")]
    public float padding = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CalculateBounds();
    }

    private void CalculateBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        MinX = bottomLeft.x + padding;
        MaxX = topRight.x - padding;
        MinY = bottomLeft.y + padding;
        MaxY = topRight.y - padding;
    }

    /// <summary>
    /// Clamp a position within the game bounds.
    /// </summary>
    public Vector3 ClampPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, MinX, MaxX);
        position.y = Mathf.Clamp(position.y, MinY, MaxY);
        return position;
    }

    /// <summary>
    /// Check if a position is outside the bounds (with optional extra margin).
    /// </summary>
    public bool IsOutOfBounds(Vector3 position, float margin = 1f)
    {
        return position.x < MinX - margin || position.x > MaxX + margin ||
               position.y < MinY - margin || position.y > MaxY + margin;
    }
}
