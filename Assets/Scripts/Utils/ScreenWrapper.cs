using UnityEngine;

/// <summary>
/// Optional screen wrapping behavior for objects.
/// </summary>
public class ScreenWrapper : MonoBehaviour
{
    [SerializeField] private bool wrapHorizontal = true;
    [SerializeField] private bool wrapVertical = false;
    [SerializeField] private float bufferX = 0.5f;
    [SerializeField] private float bufferY = 0.5f;

    private Camera mainCam;
    private float screenHalfWidth;
    private float screenHalfHeight;

    private void Start()
    {
        mainCam = Camera.main;
        if (mainCam != null)
        {
            screenHalfHeight = mainCam.orthographicSize;
            screenHalfWidth = screenHalfHeight * mainCam.aspect;
        }
    }

    private void LateUpdate()
    {
        if (mainCam == null) return;

        Vector3 pos = transform.position;

        if (wrapHorizontal)
        {
            if (pos.x > screenHalfWidth + bufferX)
                pos.x = -screenHalfWidth - bufferX;
            else if (pos.x < -screenHalfWidth - bufferX)
                pos.x = screenHalfWidth + bufferX;
        }

        if (wrapVertical)
        {
            if (pos.y > screenHalfHeight + bufferY)
                pos.y = -screenHalfHeight - bufferY;
            else if (pos.y < -screenHalfHeight - bufferY)
                pos.y = screenHalfHeight + bufferY;
        }

        transform.position = pos;
    }
}
