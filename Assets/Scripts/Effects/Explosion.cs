using UnityEngine;

/// <summary>
/// Simple explosion effect that auto-destroys after its lifetime.
/// Attach to explosion prefab with particle system or animated sprite.
/// </summary>
public class Explosion : MonoBehaviour
{
    public float lifetime = 1f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
