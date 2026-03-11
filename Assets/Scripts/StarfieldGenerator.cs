using UnityEngine;

/// <summary>
/// Creates a simple procedural starfield using Unity's Particle System.
/// Attach this to an empty GameObject; it will configure a ParticleSystem at runtime.
/// This is an OPTIONAL alternative to the BackgroundScroller.
/// </summary>
public class StarfieldGenerator : MonoBehaviour
{
    [Header("Starfield Settings")]
    [Tooltip("Number of star particles visible at a time.")]
    public int maxStars = 200;

    [Tooltip("Speed stars drift downward.")]
    public float starSpeed = 3f;

    [Tooltip("Minimum star size.")]
    public float minSize = 0.02f;

    [Tooltip("Maximum star size.")]
    public float maxSize = 0.08f;

    // =========================================================================
    // Unity Lifecycle
    // =========================================================================

    private void Start()
    {
        // Add a ParticleSystem if one isn't already attached
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps == null)
        {
            ps = gameObject.AddComponent<ParticleSystem>();
        }

        // Stop the default playback so we can configure it
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // -- Main module --
        var main = ps.main;
        main.maxParticles = maxStars;
        main.startLifetime = 8f;
        main.startSpeed = starSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = Color.white;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.playOnAwake = true;

        // -- Emission module --
        var emission = ps.emission;
        emission.rateOverTime = maxStars / 4f;

        // -- Shape module: emit from a line across the top --
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(18f, 0.1f, 1f);
        shape.rotation = new Vector3(0f, 0f, 0f);

        // -- Velocity over Lifetime: stars fall downward --
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = 0f;
        velocity.y = -starSpeed;
        velocity.z = 0f;

        // -- Renderer: use default particle material --
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = Color.white;

        // Position the emitter above the camera
        transform.position = new Vector3(0f, 8f, 0f);

        ps.Play();
    }
}
