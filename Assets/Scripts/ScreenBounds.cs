using UnityEngine;

/// <summary>
/// Utility class that calculates screen boundaries in world-space coordinates.
/// Attach to any persistent GameObject (e.g. Main Camera) or use the static helper.
/// Other scripts reference this to clamp positions and destroy off-screen objects.
/// </summary>
public class ScreenBounds : MonoBehaviour
{
    public static ScreenBounds Instance { get; private set; }

    /// <summary>World-space bounds with a small padding.</summary>
    public float Left   { get; private set; }
    public float Right  { get; private set; }
    public float Top    { get; private set; }
    public float Bottom { get; private set; }

    [Tooltip("Extra world units added outside visible area before bullets are destroyed.")]
    public float destroyPadding = 1.0f;

    private Camera mainCam;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        mainCam = Camera.main;
        RecalculateBounds();
    }

    /// <summary>Call if the camera ortho size or aspect changes at runtime.</summary>
    public void RecalculateBounds()
    {
        if (mainCam == null) mainCam = Camera.main;
        float orthoHeight = mainCam.orthographicSize;
        float orthoWidth  = orthoHeight * mainCam.aspect;

        Left   = mainCam.transform.position.x - orthoWidth;
        Right  = mainCam.transform.position.x + orthoWidth;
        Top    = mainCam.transform.position.y + orthoHeight;
        Bottom = mainCam.transform.position.y - orthoHeight;
    }

    /// <summary>Returns true when a position is outside visible area + padding.</summary>
    public bool IsOffScreen(Vector3 pos)
    {
        return pos.x < Left   - destroyPadding ||
               pos.x > Right  + destroyPadding ||
               pos.y < Bottom - destroyPadding ||
               pos.y > Top    + destroyPadding;
    }

    /// <summary>Clamps a position inside the visible screen (no padding).</summary>
    public Vector3 ClampToScreen(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, Left,  Right);
        pos.y = Mathf.Clamp(pos.y, Bottom, Top);
        return pos;
    }

    /// <summary>Clamps with an inset so the player sprite stays fully visible.</summary>
    public Vector3 ClampToScreen(Vector3 pos, float inset)
    {
        pos.x = Mathf.Clamp(pos.x, Left  + inset, Right - inset);
        pos.y = Mathf.Clamp(pos.y, Bottom + inset, Top   - inset);
        return pos;
    }
}
