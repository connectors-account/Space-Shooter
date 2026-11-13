using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds the whole game at runtime, so an empty scene is all that is needed.
/// Also provides small helpers for building objects out of coloured primitives.
/// </summary>
public static class GameBootstrap
{
    static readonly Dictionary<Color, Material> materialCache = new Dictionary<Color, Material>();
    static Material baseMaterial;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (GameManager.Instance != null) return;

        SetUpCamera();
        SetUpLighting();

        GameObject player = CreatePlayer();

        GameObject managerObject = new GameObject("GameManager");
        GameManager manager = managerObject.AddComponent<GameManager>();
        managerObject.AddComponent<EnemySpawner>();
        manager.Initialise(player.GetComponent<PlayerController>());
    }

    static void SetUpCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObject = new GameObject("Main Camera");
            camObject.tag = "MainCamera";
            cam = camObject.AddComponent<Camera>();
        }

        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 50f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
    }

    static void SetUpLighting()
    {
        // Remove any lights the scene already has so colours look the same in every scene.
#if UNITY_2023_1_OR_NEWER
        Light[] existing = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
        Light[] existing = Object.FindObjectsOfType<Light>();
#endif
        foreach (Light l in existing) l.enabled = false;

        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.6f);

        GameObject lightObject = new GameObject("Key Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.0f;
        light.shadows = LightShadows.None;
        lightObject.transform.rotation = Quaternion.Euler(30f, 20f, 0f);
    }

    static GameObject CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        BoxCollider col = player.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(1.2f, 1.0f, 0.6f);
        AddKinematicBody(player);

        Color hull = new Color(0.3f, 0.75f, 1f);
        Color wing = new Color(0.15f, 0.4f, 0.85f);
        CreatePart(PrimitiveType.Capsule, player.transform, new Vector3(0f, 0f, 0f), new Vector3(0.45f, 0.5f, 0.45f), hull);
        CreatePart(PrimitiveType.Cube, player.transform, new Vector3(0f, -0.2f, 0f), new Vector3(1.4f, 0.25f, 0.25f), wing);
        CreatePart(PrimitiveType.Sphere, player.transform, new Vector3(0f, 0.15f, -0.2f), new Vector3(0.25f, 0.25f, 0.25f), new Color(1f, 1f, 0.6f));
        CreatePart(PrimitiveType.Cube, player.transform, new Vector3(0f, -0.55f, 0f), new Vector3(0.25f, 0.15f, 0.25f), new Color(1f, 0.55f, 0.1f));

        player.AddComponent<PlayerController>();
        return player;
    }

    /// <summary>Adds a kinematic, gravity-free Rigidbody so trigger events fire.</summary>
    public static void AddKinematicBody(GameObject go)
    {
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    /// <summary>Creates a coloured primitive as a child, with its own collider removed.</summary>
    public static GameObject CreatePart(PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Color colour)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        Collider c = part.GetComponent<Collider>();
        if (c != null) Object.Destroy(c);

        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;

        Renderer r = part.GetComponent<Renderer>();
        r.sharedMaterial = GetMaterial(colour, r.sharedMaterial);
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        return part;
    }

    /// <summary>
    /// Returns a cached material of the given colour. It is copied from the primitive's
    /// built-in default material, so its shader is always present in Windows builds.
    /// </summary>
    static Material GetMaterial(Color colour, Material defaultMaterial)
    {
        if (materialCache.TryGetValue(colour, out Material cached) && cached != null) return cached;

        if (baseMaterial == null)
        {
            baseMaterial = defaultMaterial != null ? defaultMaterial : new Material(Shader.Find("Standard"));
        }

        Material m = new Material(baseMaterial);
        m.color = colour;
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.4f);
        materialCache[colour] = m;
        return m;
    }
}
