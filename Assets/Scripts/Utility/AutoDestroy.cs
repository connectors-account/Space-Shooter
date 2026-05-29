using UnityEngine;

/// <summary>
/// Destroys the attached GameObject after a specified lifetime.
/// Useful for particles, temporary effects, etc.
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
