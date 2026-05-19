using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to automatically set up required tags for the game.
/// Run from: Tools > Space Shooter > Setup Tags.
/// </summary>
public class TagSetup
{
    [MenuItem("Tools/Space Shooter/Setup Tags")]
    public static void SetupRequiredTags()
    {
        AddTag("Player");
        AddTag("PlayerBullet");
        AddTag("EnemyBullet");
        AddTag("Enemy");
        AddTag("PowerUp");

        Debug.Log("All required tags have been set up successfully!");
    }

    /// <summary>
    /// Adds a tag to the Tag Manager if it doesn't already exist.
    /// </summary>
    private static void AddTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Check if tag already exists
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue == tagName) return;
        }

        // Also check the built-in tag "Player" is already present
        if (tagName == "Player")
        {
            // "Player" is a built-in tag in Unity, no need to add
            return;
        }

        // Add new tag
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
        newTag.stringValue = tagName;

        tagManager.ApplyModifiedProperties();
        Debug.Log($"Tag '{tagName}' added successfully.");
    }
}
