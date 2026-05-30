using UnityEngine;

/// <summary>
/// Simple helper — destroys the GameObject after a set time.
/// Used for explosion effects and other temporary objects.
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 1f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
