using UnityEngine;

/// <summary>
/// Procedurally generates a starfield background using particles.
/// Attach to an empty GameObject in the scene.
/// </summary>
public class StarfieldGenerator : MonoBehaviour
{
    [Header("Starfield Settings")]
    public int starCount = 200;
    public float fieldWidth = 20f;
    public float fieldHeight = 15f;
    public float minStarSize = 0.02f;
    public float maxStarSize = 0.08f;
    public float scrollSpeed = 0.5f;

    [Header("Star Layers (parallax)")]
    public int layerCount = 3;

    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null)
            ps = gameObject.AddComponent<ParticleSystem>();

        // Configure the particle system as a starfield
        var main = ps.main;
        main.maxParticles = starCount;
        main.startLifetime = 999f;
        main.startSpeed = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.loop = false;
        main.startColor = Color.white;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.enabled = false;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));

        // Emit stars manually
        EmitStars();
    }

    void EmitStars()
    {
        var emitParams = new ParticleSystem.EmitParams();

        for (int i = 0; i < starCount; i++)
        {
            float x = Random.Range(-fieldWidth / 2f, fieldWidth / 2f);
            float y = Random.Range(-fieldHeight / 2f, fieldHeight / 2f);
            float layer = Random.Range(0, layerCount);
            float layerFactor = (layer + 1f) / layerCount;

            emitParams.position = new Vector3(x, y, 10f + layer);
            emitParams.velocity = Vector3.down * scrollSpeed * layerFactor;
            emitParams.startSize = Mathf.Lerp(minStarSize, maxStarSize, layerFactor);
            emitParams.startLifetime = 999f;

            float brightness = Mathf.Lerp(0.3f, 1f, layerFactor);
            emitParams.startColor = new Color(brightness, brightness, brightness + Random.Range(0f, 0.1f), brightness);

            ps.Emit(emitParams, 1);
        }
    }

    void Update()
    {
        // Recycle stars that go below the screen
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        int count = ps.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            if (particles[i].position.y < -fieldHeight / 2f)
            {
                particles[i].position = new Vector3(
                    Random.Range(-fieldWidth / 2f, fieldWidth / 2f),
                    fieldHeight / 2f,
                    particles[i].position.z
                );
            }
        }

        ps.SetParticles(particles, count);
    }
}
