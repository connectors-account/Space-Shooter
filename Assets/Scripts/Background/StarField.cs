// ============================================================
//  StarField.cs  –  Procedural star particles (no assets needed)
//
//  Attach to any GameObject in the Game scene.
//  Creates a ParticleSystem at runtime and configures it as a
//  vertically scrolling star field.
// ============================================================
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class StarField : MonoBehaviour
{
    [Header("Star Settings")]
    public int   starCount   = 120;
    public float spawnWidth  = 10f;
    public float spawnHeight = 14f;
    public float minSpeed    = 1.5f;
    public float maxSpeed    = 4.5f;
    public float minSize     = 0.02f;
    public float maxSize     = 0.12f;

    ParticleSystem _ps;

    void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
        ConfigureParticleSystem();
    }

    void ConfigureParticleSystem()
    {
        // Main module
        var main = _ps.main;
        main.loop          = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(spawnHeight / minSpeed,
                                                             spawnHeight / maxSpeed);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize     = new ParticleSystem.MinMaxCurve(minSize,  maxSize);
        main.startColor    = new ParticleSystem.MinMaxGradient(Color.white,
                                                                new Color(0.7f, 0.85f, 1f));
        main.maxParticles  = starCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Emission
        var em = _ps.emission;
        em.rateOverTime = starCount / (spawnHeight / ((minSpeed + maxSpeed) * 0.5f));

        // Shape: spawn at top of the screen
        var sh = _ps.shape;
        sh.shapeType   = ParticleSystemShapeType.Box;
        sh.enabled     = true;
        sh.scale       = new Vector3(spawnWidth, 0.1f, 1f);
        sh.position    = new Vector3(0f, spawnHeight * 0.5f, 0f);

        // Velocity (move downward)
        var vel = _ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;
        vel.y       = new ParticleSystem.MinMaxCurve(-1f);

        // Renderer: use the default particle material (additive)
        var renderer      = GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = Color.white;
        renderer.sortingOrder   = -10;

        _ps.Play();
    }
}
