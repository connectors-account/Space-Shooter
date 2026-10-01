using System;
using NovaGuard;
using UnityEditor;
using UnityEngine;

/// <summary>Run inside Unity after generation. This is an asset-wiring audit, not a gameplay test.</summary>
public static class ProjectValidation
{
    [MenuItem("Tools/Nova Guard/Validate Generated Assets")]
    public static void Validate()
    {
        Check(AssetDatabase.LoadAssetAtPath<SceneAsset>(ProjectSetup.ScenePath) != null, "Generated scene exists");
        string[] names = { "Player", "Scout", "Gunner", "Tank", "PlayerBolt", "EnemyBolt", "Repair", "Rapid", "Shield" };
        foreach (string name in names)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Generated/Prefabs/" + name + ".prefab");
            Check(prefab != null, name + " prefab exists");
            SpriteRenderer renderer = prefab.GetComponent<SpriteRenderer>();
            Check(renderer != null && renderer.sprite != null, name + " has a sprite");
            Collider2D collider = prefab.GetComponent<Collider2D>();
            Check(collider != null && collider.isTrigger, name + " has a trigger collider");
            Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab) == 0, name + " has no missing scripts");
            if (prefab.GetComponent<PowerUp>() != null) continue;
            Rigidbody2D body = prefab.GetComponent<Rigidbody2D>();
            Check(body != null && body.gravityScale == 0, name + " has zero-gravity physics");
            Check(body.collisionDetectionMode == CollisionDetectionMode2D.Continuous, name + " uses continuous collision detection");
        }
        foreach (SoundCue cue in Enum.GetValues(typeof(SoundCue)))
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/" + cue + ".wav");
            Check(clip != null && clip.samples > 0, cue + " audio imports");
        }
        Check(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") != null, "Built-in UI font resolves");
        Check(EditorBuildSettings.scenes.Length > 0 && EditorBuildSettings.scenes[0].path == ProjectSetup.ScenePath
            && EditorBuildSettings.scenes[0].enabled, "Game scene is enabled for builds");
        GameController game = UnityEngine.Object.FindObjectOfType<GameController>();
        Check(game != null, "Game scene is open");
        Check(game.playerPrefab != null && game.playerBulletPrefab != null && game.enemyBulletPrefab != null, "Player and bullet references are wired");
        Check(game.enemyPrefabs.Length == 3 && game.powerUpPrefabs.Length == 3, "All enemy and pickup variants are wired");
        foreach (EnemyShip enemy in game.enemyPrefabs) Check(enemy != null, "Enemy reference resolves");
        foreach (PowerUp pickup in game.powerUpPrefabs) Check(pickup != null, "Pickup reference resolves");
        Check(game.ui != null && game.waves != null && game.sound != null && game.worldRoot != null, "Systems are wired");
        Check(game.sound.clips.Length == Enum.GetValues(typeof(SoundCue)).Length, "Audio bank length matches enum");
        foreach (AudioClip clip in game.sound.clips) Check(clip != null, "Audio reference resolves");
        Debug.Log("Nova Guard: generated asset validation passed. Play Mode and Windows runtime testing are still required.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Nova Guard validation: " + message);
    }
}
