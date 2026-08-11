using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Destroys the GameObject after a set lifetime. Handy for the optional explosion
    /// effect prefab so temporary effects clean themselves up.
    /// </summary>
    public class SelfDestruct : MonoBehaviour
    {
        [Tooltip("Seconds before this object is destroyed.")]
        [SerializeField] private float lifetime = 1f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}
