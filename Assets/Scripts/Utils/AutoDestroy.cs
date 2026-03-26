// ============================================================================
// AutoDestroy.cs — Destroys GameObject after a delay
// ============================================================================
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
