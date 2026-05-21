using UnityEngine;

/// <summary>
/// DestroyOffScreen - Destroys the attached GameObject when it leaves the screen bounds.
/// Attach to any object that should be cleaned up when off-screen (bullets, debris, etc.).
/// </summary>
public class DestroyOffScreen : MonoBehaviour
{
    [Header("Bounds")]
    public float topBound = 8f;
    public float bottomBound = -8f;
    public float leftBound = -11f;
    public float rightBound = 11f;

    private void Update()
    {
        Vector3 pos = transform.position;
        if (pos.y > topBound || pos.y < bottomBound || pos.x < leftBound || pos.x > rightBound)
        {
            Destroy(gameObject);
        }
    }
}
