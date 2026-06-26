using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Environment
{
    /// <summary>
    /// Spawns pooled <see cref="ExplosionEffect"/> instances. Provides a single entry point so any
    /// system can request an explosion at a world position without managing particle systems.
    /// </summary>
    public class ExplosionManager : MonoBehaviour
    {
        /// <summary>Global access point.</summary>
        public static ExplosionManager Instance { get; private set; }

        private ObjectPool _pool;
        private Transform _container;

        /// <summary>
        /// Builds the explosion pool. Called once by the bootstrap.
        /// </summary>
        public void Initialize()
        {
            Instance = this;
            _container = new GameObject("Explosions").transform;
            _container.SetParent(transform, false);

            GameObject template = CreateTemplate();
            _pool = new ObjectPool(template, _container, prewarm: 12);
            template.SetActive(false);
        }

        private GameObject CreateTemplate()
        {
            var go = new GameObject("Explosion");
            go.SetActive(false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = 0.5f;
            main.startSpeed = 4f;
            main.startSize = 0.3f;
            main.maxParticles = 64;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)24) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingOrder = 8;

            go.AddComponent<ExplosionEffect>();
            return go;
        }

        /// <summary>
        /// Spawns and plays an explosion at the given position.
        /// </summary>
        /// <param name="position">World position of the burst.</param>
        /// <param name="color">Base particle colour.</param>
        /// <param name="scale">Relative size of the burst.</param>
        public void Spawn(Vector3 position, Color color, float scale = 1f)
        {
            if (_pool == null)
            {
                return;
            }

            GameObject go = _pool.Get(position, Quaternion.identity);
            go.GetComponent<ExplosionEffect>().Play(color, scale);
        }

        /// <summary>Returns an explosion instance to the pool.</summary>
        public void Release(GameObject go)
        {
            _pool?.Release(go);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
