using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Destroys the GameObject after a delay. Used for explosion / hit effects.
    /// If a ParticleSystem is present it waits for its duration automatically.
    /// </summary>
    public class AutoDestroy : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 1f;

        private void Start()
        {
            ParticleSystem ps = GetComponent<ParticleSystem>();
            if (ps != null)
            {
                lifeTime = ps.main.duration + ps.main.startLifetime.constantMax;
            }
            Destroy(gameObject, lifeTime);
        }
    }
}
