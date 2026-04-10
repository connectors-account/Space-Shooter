using UnityEngine;

namespace SpaceShooter.Effects
{
    /// <summary>
    /// Runtime-created particle explosion effect.
    /// Spawn with ExplosionParticles.Spawn(position).
    /// </summary>
    public class ExplosionParticles : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.55f;

        public static void Spawn(Vector3 position)
        {
            GameObject fxObject = new GameObject("ExplosionParticles");
            fxObject.transform.position = position;
            fxObject.AddComponent<ExplosionParticles>();
        }

        private void Awake()
        {
            ParticleSystem particles = gameObject.AddComponent<ParticleSystem>();
            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            var main = particles.main;
            main.loop = false;
            main.playOnAwake = true;
            main.startLifetime = 0.35f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.8f, 5.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.95f, 0.45f, 1f),
                new Color(1f, 0.45f, 0.05f, 0.9f)
            );
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.duration = 0.25f;

            var emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[]
            {
                new ParticleSystem.Burst(0f, 32)
            });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var velocityOverLifetime = particles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-0.25f, 0.35f);

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.95f, 0.5f), 0f),
                    new GradientColorKey(new Color(1f, 0.35f, 0.1f), 0.55f),
                    new GradientColorKey(new Color(0.25f, 0.1f, 0.1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.75f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            particles.Play();
            Destroy(gameObject, lifetime);
        }
    }
}
