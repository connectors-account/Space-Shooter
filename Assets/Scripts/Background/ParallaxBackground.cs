using UnityEngine;

namespace SpaceShooter.Background
{
    /// <summary>
    /// Coordinates a set of ParallaxLayers, assigning each a scroll speed to create a
    /// depth effect: far stars scroll slowly, near asteroids scroll quickly.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Layers (ordered far -> near)")]
        [SerializeField] private ParallaxLayer[] layers;

        [Header("Default Speeds")]
        [Tooltip("Applied by index when 'applyDefaultSpeeds' is enabled: 0.5 far stars, 1.5 mid nebula, 3 near asteroids.")]
        [SerializeField] private float[] defaultSpeeds = { 0.5f, 1.5f, 3f };

        [SerializeField] private bool applyDefaultSpeeds = true;

        [Header("Global Multiplier")]
        [Tooltip("Multiplies every layer's speed. Useful to speed the whole field up during boss fights.")]
        [SerializeField] private float globalSpeedMultiplier = 1f;

        private void Start()
        {
            if (layers == null)
            {
                return;
            }

            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i] == null)
                {
                    continue;
                }

                if (applyDefaultSpeeds && defaultSpeeds != null && i < defaultSpeeds.Length)
                {
                    layers[i].ScrollSpeed = defaultSpeeds[i] * globalSpeedMultiplier;
                }
                else
                {
                    layers[i].ScrollSpeed *= globalSpeedMultiplier;
                }
            }
        }

        /// <summary>
        /// Adjusts the global speed multiplier at runtime and re-applies it to all layers.
        /// </summary>
        public void SetGlobalSpeedMultiplier(float multiplier)
        {
            if (layers == null)
            {
                return;
            }

            float ratio = Mathf.Approximately(globalSpeedMultiplier, 0f) ? 1f : multiplier / globalSpeedMultiplier;
            globalSpeedMultiplier = multiplier;

            foreach (var layer in layers)
            {
                if (layer != null)
                {
                    layer.ScrollSpeed *= ratio;
                }
            }
        }
    }
}
