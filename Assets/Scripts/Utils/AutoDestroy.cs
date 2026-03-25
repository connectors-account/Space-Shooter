// =============================================================================
// AutoDestroy.cs — Self-destroying component with timer
// =============================================================================
using UnityEngine;

namespace SpaceShooter.Utils
{
    /// <summary>
    /// Destroys the attached GameObject after a set duration.
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
}
