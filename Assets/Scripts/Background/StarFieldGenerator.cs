// ============================================================================
// StarFieldGenerator.cs — Procedurally generates scrolling star particles
// Creates a retro star-field effect using Unity's built-in Particle System.
// Attach to an empty GameObject; it configures the ParticleSystem at runtime.
// ============================================================================
using UnityEngine;

namespace SpaceShooter.Background
{
    [RequireComponent(typeof(ParticleSystem))]
    public class StarFieldGenerator : MonoBehaviour
    {
        [Header("Stars")]
        [SerializeField] private int starCount = 200;
        [SerializeField] private float fieldWidth = 20f;
        [SerializeField] private float fieldHeight = 14f;
        [SerializeField] private float minSpeed = 1f;
        [SerializeField] private float maxSpeed = 4f;
        [SerializeField] private float minSize = 0.02f;
        [SerializeField] private float maxSize = 0.08f;

        private void Start()
        {
            var ps = GetComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = fieldHeight / minSpeed + 2f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
            main.startColor = new Color(1f, 1f, 1f, 0.8f);
            main.maxParticles = starCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;
            // Rotate the system so particles move downward
            transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

            var emission = ps.emission;
            emission.rateOverTime = starCount / main.startLifetime.constantMax;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(fieldWidth, 1f, 0.1f);
            shape.position = new Vector3(0f, 0f, fieldHeight / 2f);

            // Use default particle material
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = Color.white;

            ps.Play();
        }
    }
}
