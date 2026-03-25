// =============================================================================
// StarfieldGenerator.cs — Procedural starfield particle background
// =============================================================================
using UnityEngine;

namespace SpaceShooter.Effects
{
    /// <summary>
    /// Generates a scrolling starfield using Unity's particle system.
    /// Creates depth by having multiple star sizes and speeds.
    /// </summary>
    public class StarfieldGenerator : MonoBehaviour
    {
        [Header("Starfield Settings")]
        [SerializeField] private int starCount = 200;
        [SerializeField] private float areaWidth = 20f;
        [SerializeField] private float areaHeight = 15f;
        [SerializeField] private float minSpeed = 0.5f;
        [SerializeField] private float maxSpeed = 3f;
        [SerializeField] private float minSize = 0.02f;
        [SerializeField] private float maxSize = 0.08f;

        private ParticleSystem ps;
        private ParticleSystem.Particle[] stars;
        private float[] starSpeeds;

        private void Start()
        {
            ps = GetComponent<ParticleSystem>();
            if (ps == null)
            {
                ps = gameObject.AddComponent<ParticleSystem>();
            }

            // Configure particle system
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.maxParticles = starCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = 9999f;
            main.startSpeed = 0f;

            // Disable emission (manual control)
            var emission = ps.emission;
            emission.enabled = false;

            // Set renderer
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = Color.white;

            // Create stars
            InitializeStars();
        }

        /// <summary>
        /// Creates stars with random positions, sizes, and scroll speeds.
        /// </summary>
        private void InitializeStars()
        {
            stars = new ParticleSystem.Particle[starCount];
            starSpeeds = new float[starCount];

            for (int i = 0; i < starCount; i++)
            {
                float speed = Random.Range(minSpeed, maxSpeed);
                float size = Mathf.Lerp(minSize, maxSize, (speed - minSpeed) / (maxSpeed - minSpeed));
                float brightness = Mathf.Lerp(0.3f, 1f, (speed - minSpeed) / (maxSpeed - minSpeed));

                stars[i].position = new Vector3(
                    Random.Range(-areaWidth / 2f, areaWidth / 2f),
                    Random.Range(-areaHeight / 2f, areaHeight / 2f),
                    10f
                );
                stars[i].startSize = size;
                stars[i].startColor = new Color(brightness, brightness, brightness * 1.1f, 1f);
                stars[i].remainingLifetime = 9999f;
                starSpeeds[i] = speed;
            }

            ps.SetParticles(stars, starCount);
        }

        private void Update()
        {
            if (stars == null) return;

            int count = ps.GetParticles(stars);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = stars[i].position;
                pos.y -= starSpeeds[i] * Time.deltaTime;

                // Wrap around
                if (pos.y < -areaHeight / 2f)
                {
                    pos.y = areaHeight / 2f;
                    pos.x = Random.Range(-areaWidth / 2f, areaWidth / 2f);
                }

                stars[i].position = pos;
            }

            ps.SetParticles(stars, count);
        }
    }
}
