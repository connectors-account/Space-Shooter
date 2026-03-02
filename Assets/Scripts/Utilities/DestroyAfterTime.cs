using UnityEngine;

/// <summary>
/// DestroyAfterTime automatically destroys a GameObject after a specified time.
/// Useful for effects, particles, and temporary objects.
/// </summary>
public class DestroyAfterTime : MonoBehaviour
{
    [Tooltip("Time in seconds before destruction")]
    public float lifetime = 2f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
