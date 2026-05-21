using UnityEngine;

namespace SpaceShooter.Effects
{
    public enum ExplosionType
    {
        Small,
        Medium,
        Large
    }

    /// <summary>
    /// Singleton that creates explosion particle effects at the given position.
    /// Uses Unity's particle system with programmatic configuration.
    /// </summary>
    public class ExplosionManager : MonoBehaviour
    {
        public static ExplosionManager Instance { get; private set; }

        [Header("Explosion Prefabs (Optional)")]
        [SerializeField] private GameObject smallExplosionPrefab;
        [SerializeField] private GameObject mediumExplosionPrefab;
        [SerializeField] private GameObject largeExplosionPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // If no prefabs are assigned, create them procedurally
            if (smallExplosionPrefab == null)
                smallExplosionPrefab = CreateExplosionPrefab("SmallExplosion", 10, 0.3f, 0.5f);
            if (mediumExplosionPrefab == null)
                mediumExplosionPrefab = CreateExplosionPrefab("MediumExplosion", 20, 0.5f, 0.8f);
            if (largeExplosionPrefab == null)
                largeExplosionPrefab = CreateExplosionPrefab("LargeExplosion", 35, 0.7f, 1.2f);
        }

        private GameObject CreateExplosionPrefab(string name, int maxParticles, float lifetime, float size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform);
            go.SetActive(false);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            // Main module
            var main = ps.main;
            main.duration = 0.5f;
            main.startLifetime = lifetime;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.3f, size);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.6f, 0f), // orange
                new Color(1f, 0.2f, 0f)  // red-orange
            );
            main.maxParticles = maxParticles;
            main.loop = false;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Disable;
            main.gravityModifier = 0.3f;

            // Emission – one burst
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, (short)maxParticles)
            });

            // Shape – sphere
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;

            // Size over lifetime – shrink
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            // Color over lifetime – fade out
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(1f, 0.4f, 0f), 0.5f),
                    new GradientColorKey(new Color(0.5f, 0f, 0f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            col.color = grad;

            // Renderer – use default particle material
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            return go;
        }

        public void SpawnExplosion(Vector3 position, ExplosionType type)
        {
            GameObject prefab = type switch
            {
                ExplosionType.Small => smallExplosionPrefab,
                ExplosionType.Medium => mediumExplosionPrefab,
                ExplosionType.Large => largeExplosionPrefab,
                _ => mediumExplosionPrefab
            };

            if (prefab == null) return;

            GameObject explosion = Instantiate(prefab, position, Quaternion.identity);
            explosion.SetActive(true);

            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                // Auto-destroy after particle system finishes
                Destroy(explosion, ps.main.duration + ps.main.startLifetime.constantMax + 0.1f);
            }
            else
            {
                Destroy(explosion, 1f);
            }
        }
    }
}
