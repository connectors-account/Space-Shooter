// ============================================================================
// GameBounds.cs - Defines the playable area and provides boundary checks
// Attached to an empty GameObject; other scripts query it for screen limits.
// ============================================================================
using UnityEngine;

/// <summary>
/// Calculates and exposes the world-space boundaries of the camera viewport.
/// Other scripts use these bounds for clamping player position, spawning enemies
/// off-screen, and despawning objects that leave the play area.
/// </summary>
public class GameBounds : MonoBehaviour
{
    public static GameBounds Instance { get; private set; }

    /// <summary>Minimum world-space position visible on screen.</summary>
    public Vector2 Min { get; private set; }
    /// <summary>Maximum world-space position visible on screen.</summary>
    public Vector2 Max { get; private set; }
    /// <summary>Buffer in world units beyond the screen edge before an object is considered out of bounds.</summary>
    public float DespawnBuffer { get; private set; } = 2f;

    private Camera mainCam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        mainCam = Camera.main;
        CalculateBounds();
    }

    /// <summary>
    /// Recalculates bounds from the main camera. Call this if resolution changes.
    /// </summary>
    public void CalculateBounds()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        // Convert viewport corners to world space.
        Vector3 bottomLeft = mainCam.ViewportToWorldPoint(new Vector3(0f, 0f, mainCam.nearClipPlane));
        Vector3 topRight   = mainCam.ViewportToWorldPoint(new Vector3(1f, 1f, mainCam.nearClipPlane));

        Min = new Vector2(bottomLeft.x, bottomLeft.y);
        Max = new Vector2(topRight.x, topRight.y);
    }

    /// <summary>
    /// Returns true if the position is outside the visible area plus the despawn buffer.
    /// </summary>
    public bool IsOutOfBounds(Vector3 position)
    {
        return position.x < Min.x - DespawnBuffer ||
               position.x > Max.x + DespawnBuffer ||
               position.y < Min.y - DespawnBuffer ||
               position.y > Max.y + DespawnBuffer;
    }

    /// <summary>
    /// Clamps a position to the visible screen area with an optional inset padding.
    /// </summary>
    public Vector3 ClampToScreen(Vector3 position, float padding = 0.5f)
    {
        position.x = Mathf.Clamp(position.x, Min.x + padding, Max.x - padding);
        position.y = Mathf.Clamp(position.y, Min.y + padding, Max.y - padding);
        return position;
    }

    /// <summary>
    /// Returns a random X position along the top edge of the screen (for top-down spawning).
    /// </summary>
    public float RandomTopX()
    {
        return Random.Range(Min.x + 1f, Max.x - 1f);
    }

    /// <summary>
    /// Returns the Y position just above the visible top (spawn point for enemies).
    /// </summary>
    public float TopSpawnY()
    {
        return Max.y + DespawnBuffer * 0.5f;
    }
}
