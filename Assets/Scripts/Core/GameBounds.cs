using UnityEngine;

/// <summary>
/// Defines the playable game area boundaries based on the camera viewport.
/// Used by player movement clamping and enemy spawn positions.
/// </summary>
public class GameBounds : MonoBehaviour
{
    public static GameBounds Instance { get; private set; }

    public float minX { get; private set; }
    public float maxX { get; private set; }
    public float minY { get; private set; }
    public float maxY { get; private set; }

    [Header("Padding")]
    public float horizontalPadding = 0.5f;
    public float verticalPadding = 0.5f;

    private void Awake()
    {
        Instance = this;
        CalculateBounds();
    }

    private void CalculateBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        minX = cam.transform.position.x - camWidth / 2f + horizontalPadding;
        maxX = cam.transform.position.x + camWidth / 2f - horizontalPadding;
        minY = cam.transform.position.y - camHeight / 2f + verticalPadding;
        maxY = cam.transform.position.y + camHeight / 2f - verticalPadding;
    }

    /// <summary>
    /// Returns a random position along the top edge of the screen for spawning enemies.
    /// </summary>
    public Vector3 GetRandomTopSpawnPosition()
    {
        float x = Random.Range(minX, maxX);
        float y = maxY + 1f;
        return new Vector3(x, y, 0f);
    }

    /// <summary>
    /// Returns a random position along the right edge for side-spawning enemies.
    /// </summary>
    public Vector3 GetRandomRightSpawnPosition()
    {
        float x = maxX + 1f;
        float y = Random.Range(minY, maxY);
        return new Vector3(x, y, 0f);
    }

    /// <summary>
    /// Check if a position is within the visible screen area (with some margin).
    /// </summary>
    public bool IsOutOfBounds(Vector3 position, float margin = 2f)
    {
        return position.x < minX - margin || position.x > maxX + margin ||
               position.y < minY - margin || position.y > maxY + margin;
    }
}
