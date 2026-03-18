using UnityEngine;

/// <summary>
/// Automatically destroys a GameObject after a set time.
/// Useful for particle effects, temporary objects, etc.
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
