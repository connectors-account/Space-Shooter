// ============================================================================
// StarfieldGenerator.cs — Procedural star particle background
// ============================================================================
using UnityEngine;

public class StarfieldGenerator : MonoBehaviour
{
    [Header("Star Layers")]
    [SerializeField] private int layerCount = 3;
    [SerializeField] private int starsPerLayer = 50;

    [Header("Star Settings")]
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float minSize = 0.02f;
    [SerializeField] private float maxSize = 0.08f;
    [SerializeField] private Color dimColor = new Color(0.4f, 0.4f, 0.6f, 0.5f);
    [SerializeField] private Color brightColor = new Color(1f, 1f, 1f, 1f);

    [Header("Bounds")]
    [SerializeField] private float xMin = -5.5f;
    [SerializeField] private float xMax = 5.5f;
    [SerializeField] private float yMin = -6f;
    [SerializeField] private float yMax = 6f;

    private StarData[] stars;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private struct StarData
    {
        public Vector3 position;
        public float speed;
        public float size;
        public Color color;
    }

    // =========================================================================
    private void Start()
    {
        // Use a simple approach: spawn small sprite objects as stars
        // For better performance, we'll use a particle system approach
        CreateStarParticles();
    }

    private void CreateStarParticles()
    {
        ParticleSystem ps = gameObject.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 20f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = new ParticleSystem.MinMaxGradient(dimColor, brightColor);
        main.maxParticles = layerCount * starsPerLayer;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, layerCount * starsPerLayer)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(xMax - xMin, yMax - yMin, 0);

        // Velocity over lifetime for scrolling
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.y = new ParticleSystem.MinMaxCurve(-maxSpeed, -minSpeed);

        // Renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = Color.white;
        renderer.sortingOrder = -10; // behind everything

        // Make stars twinkle
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0),
                new GradientColorKey(Color.white, 1)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.3f, 0),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0.3f, 1f)
            }
        );
        colorOverLifetime.color = grad;
    }
}
