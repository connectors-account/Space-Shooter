using UnityEngine;

namespace Starfall
{
    // The committed .unity scene references this script via its committed GUID.
    // Runtime creation uses normal Unity components and generated prefab assets.
    public sealed class SceneEntry : MonoBehaviour
    {
        void Awake()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 0, -10);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.015f, .025f, .065f);
            camera.nearClipPlane = .1f; camera.farClipPlane = 100;
            cameraObject.AddComponent<AudioListener>();
            new GameObject("Starfall").AddComponent<StarGame>();
        }
    }
}
