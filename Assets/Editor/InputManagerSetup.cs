#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SpaceShooter.EditorTools
{
    public static class InputManagerSetup
    {
        [MenuItem("Tools/Space Shooter/Configure Legacy Input")]
        public static void ConfigureLegacyInput()
        {
            SerializedObject inputManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProperty = inputManager.FindProperty("m_Axes");

            AddAxisIfMissing(axesProperty, "Horizontal", "a", "d", "left", "right", 3f, 3f, 0.001f, 1f);
            AddAxisIfMissing(axesProperty, "Vertical", "s", "w", "down", "up", 3f, 3f, 0.001f, 1f);
            AddAxisIfMissing(axesProperty, "Fire1", "", "", "", "", 1000f, 1000f, 0.001f, 1f, "mouse 0", "joystick button 0");

            inputManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("Legacy input axes configured: Horizontal, Vertical, Fire1");
        }

        private static void AddAxisIfMissing(
            SerializedProperty axesProperty,
            string name,
            string negativeButton,
            string positiveButton,
            string altNegative,
            string altPositive,
            float gravity,
            float sensitivity,
            float dead,
            float axis,
            string button = "",
            string altButton = "")
        {
            for (int i = 0; i < axesProperty.arraySize; i++)
            {
                SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(i);
                if (axisProperty.FindPropertyRelative("m_Name").stringValue == name)
                {
                    return;
                }
            }

            axesProperty.arraySize++;
            SerializedProperty newAxis = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);
            newAxis.FindPropertyRelative("m_Name").stringValue = name;
            newAxis.FindPropertyRelative("negativeButton").stringValue = negativeButton;
            newAxis.FindPropertyRelative("positiveButton").stringValue = positiveButton;
            newAxis.FindPropertyRelative("altNegativeButton").stringValue = altNegative;
            newAxis.FindPropertyRelative("altPositiveButton").stringValue = altPositive;
            newAxis.FindPropertyRelative("gravity").floatValue = gravity;
            newAxis.FindPropertyRelative("dead").floatValue = dead;
            newAxis.FindPropertyRelative("sensitivity").floatValue = sensitivity;
            newAxis.FindPropertyRelative("snap").boolValue = false;
            newAxis.FindPropertyRelative("invert").boolValue = false;
            newAxis.FindPropertyRelative("type").intValue = 0;
            newAxis.FindPropertyRelative("axis").intValue = (int)axis;
            newAxis.FindPropertyRelative("joyNum").intValue = 0;
            newAxis.FindPropertyRelative("descriptiveName").stringValue = button;
            newAxis.FindPropertyRelative("descriptiveNegativeName").stringValue = altButton;
        }
    }
}
#endif
