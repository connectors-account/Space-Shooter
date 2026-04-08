using UnityEngine;

/// <summary>
/// Utility script to destroy objects when they move off screen.
/// Attach to any object that should be cleaned up when leaving the play area.
/// </summary>
public class DestroyOffScreen : MonoBehaviour
{
    public float boundaryX = 12f;
    public float boundaryY = 8f;

    void Update()
    {
        if (Mathf.Abs(transform.position.x) > boundaryX ||
            Mathf.Abs(transform.position.y) > boundaryY)
        {
            Destroy(gameObject);
        }
    }
}
