using UnityEngine;

/// <summary>
/// Ensures the camera is set up correctly for a 2D space shooter.
/// Attach this to the Main Camera.
/// </summary>
public class CameraSetup : MonoBehaviour
{
    void Awake()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f); // Dark space blue
        }
    }
}
