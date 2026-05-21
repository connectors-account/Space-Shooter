using UnityEngine;

namespace SpaceShooter.Effects
{
    /// <summary>
    /// Procedurally generates a scrolling star field using a particle system.
    /// Creates a convincing space background without needing sprite assets.
    /// Attach to an empty GameObject in the scene.
    /// </summary>
    public class StarfieldGenerator : MonoBehaviour
    {
        [Header("Star Layers")]
        [SerializeField] private int farStarCount = 80;
        [SerializeField] private float farStarSpeed = 0.5f;
        [SerializeField] private float farStarSize = 0.03f;

        [SerializeField] private int midStarCount = 40;
        [SerializeField] private float midStarSpeed = 1.2f;
        [SerializeField] private float midStarSize = 0.06f;

        [SerializeField] private int nearStarCount = 15;
        [SerializeField] private float nearStarSpeed = 2.5f;
        [SerializeField] private float nearStarSize = 0.1f;

        private void Start()
        {
            CreateStarLayer("FarStars", farStarCount, farStarSpeed, farStarSize, 0.4f, -5);
            CreateStarLayer("MidStars", midStarCount, midStarSpeed, midStarSize, 0.7f, -4);
            CreateStarLayer("NearStars", nearStarCount, nearStarSpeed, nearStarSize, 1f, -3);
        }

        private void CreateStarLayer(string name, int count, float speed, float size, float brightness, int sortOrder)
        {
            GameObject layer = new GameObject(name);
            layer.transform.SetParent(transform);
            layer.transform.localPosition = Vector3.zero;

            ParticleSystem ps = layer.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = 15f;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = new Color(brightness, brightness, brightness * 1.1f, brightness);
            main.maxParticles = count * 3;
            main.loop = true;
            main.playOnAwake = true;
            main.gravityModifier = 0f;
            main.startRotation = 0f;

            var emission = ps.emission;
            emission.rateOverTime = count * 0.5f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;

            float screenWidth = Camera.main != null ? Camera.main.orthographicSize * Camera.main.aspect * 2.2f : 20f;
            shape.scale = new Vector3(screenWidth, 0.1f, 0.1f);
            shape.position = new Vector3(0f, Camera.main != null ? Camera.main.orthographicSize + 2f : 7f, 0f);
            shape.rotation = new Vector3(90f, 0f, 0f);

            // Velocity - move stars downward
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.y = -speed;
            vel.x = 0f;

            // Set main speed to 0 since we use velocity over lifetime
            main.startSpeed = 0f;

            var renderer = layer.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = sortOrder;

            // Pre-warm so stars are already visible at start
            ps.Simulate(10f, true, true);
            ps.Play();
        }
    }
}
