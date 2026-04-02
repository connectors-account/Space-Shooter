// =============================================================================
// TagSetup.cs (Editor Script)
// Ensures all required tags exist in the Unity project.
// Run from: Tools > Space Shooter > Setup Tags
// Also runs automatically when the game setup is triggered.
// =============================================================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class TagSetup : MonoBehaviour
{
    /// <summary>
    /// Creates all tags required by the Space Shooter game.
    /// </summary>
    [MenuItem("Tools/Space Shooter/Setup Tags")]
    public static void SetupTags()
    {
        AddTag("Player");
        AddTag("Enemy");
        AddTag("PlayerBullet");
        AddTag("EnemyBullet");
        AddTag("PowerUp");

        Debug.Log("TagSetup: All required tags have been created.");
    }

    /// <summary>
    /// Adds a tag to the project if it doesn't already exist.
    /// Uses SerializedObject to modify the TagManager asset directly.
    /// </summary>
    private static void AddTag(string tag)
    {
        // Check if tag already exists
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i] == tag)
                return; // Tag already exists
        }

        // Access the TagManager asset
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Add the new tag
        int index = tagsProp.arraySize;
        tagsProp.InsertArrayElementAtIndex(index);
        SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(index);
        newTag.stringValue = tag;

        tagManager.ApplyModifiedProperties();
        Debug.Log("TagSetup: Added tag '" + tag + "'");
    }
}
#endif
