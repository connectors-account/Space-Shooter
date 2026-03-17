using UnityEngine;

/// <summary>
/// Automatically sets up a tiling scrolling background at runtime.
/// Attach to an empty GameObject named "Background".
/// Creates two child sprites that tile vertically for seamless scrolling.
/// </summary>
public class BackgroundSetup : MonoBehaviour
{
    public Sprite backgroundSprite;
    public float scrollSpeed = 1f;
    public int sortingOrder = -10;
    public Color backgroundColor = new Color(0.05f, 0.02f, 0.15f); // dark space purple

    void Start()
    {
        // Set camera background color
        Camera.main.backgroundColor = backgroundColor;

        if (backgroundSprite == null)
        {
            // Create a simple starfield procedurally
            CreateProceduralBackground();
            return;
        }

        // Create two tiled sprites
        for (int i = 0; i < 2; i++)
        {
            GameObject bg = new GameObject($"BG_Layer_{i}");
            bg.transform.SetParent(transform);

            SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = backgroundSprite;
            sr.sortingOrder = sortingOrder;

            float height = sr.bounds.size.y;
            bg.transform.position = new Vector3(0, i * height, 10);

            ParallaxBackground parallax = bg.AddComponent<ParallaxBackground>();
            parallax.scrollSpeed = scrollSpeed;
        }
    }

    void CreateProceduralBackground()
    {
        // Create a simple dark background with stars as child objects
        Camera.main.backgroundColor = backgroundColor;

        // Create star particle system
        GameObject starsObj = new GameObject("Stars");
        starsObj.transform.SetParent(transform);
        starsObj.transform.localPosition = new Vector3(0, 5, 10);

        ParticleSystem ps = starsObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSpeed = scrollSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
        main.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(0.8f, 0.8f, 1f));
        main.startLifetime = 15f;
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0;

        var emission = ps.emission;
        emission.rateOverTime = 30;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(20, 0.1f, 1);
        shape.rotation = new Vector3(0, 0, 0);

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = -scrollSpeed;
        velocity.x = 0;

        // Disable default renderer module emission direction
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = sortingOrder;
        renderer.material = new Material(Shader.Find("Sprites/Default"));
    }
}
