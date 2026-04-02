using UnityEngine;

/// <summary>
/// Automatically destroys a GameObject after a set time.
/// Useful for temporary effects like explosions.
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
