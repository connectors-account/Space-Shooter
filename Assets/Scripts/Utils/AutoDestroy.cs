using UnityEngine;

namespace SpaceShooter.Utils
{
    /// <summary>
    /// Simple auto-destroy component with configurable lifetime.
    /// Attach to particle effects, temporary objects, etc.
    /// </summary>
    public class AutoDestroy : MonoBehaviour
    {
        [SerializeField] private float lifetime = 2f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}
