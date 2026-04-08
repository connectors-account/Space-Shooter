using UnityEngine;

/// <summary>
/// Generates a simple starfield using Unity's ParticleSystem at runtime.
/// Attach to an empty GameObject in the scene.
/// </summary>
public class BackgroundStarfield : MonoBehaviour
{
    public int starCount = 100;
    public float fieldWidth = 12f;
    public float fieldHeight = 10f;
    public float minSpeed = 0.5f;
    public float maxSpeed = 3f;
    public float minSize = 0.02f;
    public float maxSize = 0.08f;

    private ParticleSystem ps;

    void Start()
    {
        ps = gameObject.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 20f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = new Color(0.8f, 0.85f, 1f, 0.8f);
        main.maxParticles = starCount * 2;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = starCount / 5f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(fieldWidth, 0.1f, 1f);
        shape.position = new Vector3(0, fieldHeight / 2f, 0);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.y = new ParticleSystem.MinMaxCurve(-maxSpeed, -minSpeed);

        // Remove default renderer material issues
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = Color.white;

        ps.Play();
    }
}
