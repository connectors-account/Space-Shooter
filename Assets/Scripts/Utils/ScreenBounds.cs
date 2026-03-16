using UnityEngine;

/// <summary>
/// ScreenBounds provides utility methods for screen boundary calculations.
/// </summary>
public class ScreenBounds : MonoBehaviour
{
    public static ScreenBounds Instance { get; private set; }

    [Header("Boundary Settings")]
    [SerializeField] private float padding = 0.5f;

    // Calculated bounds
    public float Left { get; private set; }
    public float Right { get; private set; }
    public float Top { get; private set; }
    public float Bottom { get; private set; }

    private Camera mainCamera;

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

    /// <summary>
    /// Calculate screen bounds based on camera
    /// </summary>
    private void CalculateBounds()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) return;

        float height = mainCamera.orthographicSize;
        float width = height * mainCamera.aspect;

        Left = -width + padding;
        Right = width - padding;
        Top = height - padding;
        Bottom = -height + padding;
    }

    /// <summary>
    /// Check if a position is within screen bounds
    /// </summary>
    public bool IsWithinBounds(Vector3 position)
    {
        return position.x >= Left && position.x <= Right &&
               position.y >= Bottom && position.y <= Top;
    }

    /// <summary>
    /// Clamp a position to screen bounds
    /// </summary>
    public Vector3 ClampToBounds(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, Left, Right);
        position.y = Mathf.Clamp(position.y, Bottom, Top);
        return position;
    }

    /// <summary>
    /// Get a random position within screen bounds
    /// </summary>
    public Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(Left, Right),
            Random.Range(Bottom, Top),
            0f
        );
    }

    /// <summary>
    /// Get spawn position at top of screen
    /// </summary>
    public Vector3 GetTopSpawnPosition(float yOffset = 1f)
    {
        return new Vector3(
            Random.Range(Left, Right),
            Top + yOffset,
            0f
        );
    }
}
