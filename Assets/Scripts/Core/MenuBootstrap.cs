using UnityEngine;
using SpaceShooter.Background;
using SpaceShooter.UI;
using SpaceShooter.Utilities;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Builds the Main Menu scene at runtime: an orthographic camera, the
    /// scrolling parallax background and the main-menu UI. Drop a single empty
    /// GameObject with this component into the MainMenu scene (see README).
    /// </summary>
    public class MenuBootstrap : MonoBehaviour
    {
        [SerializeField] private float orthographicSize = 6f;
        [SerializeField] private Color backgroundColour = new Color(0.02f, 0.02f, 0.06f);

        private void Awake()
        {
            SetupCamera();
        }

        private void Start()
        {
            SetupBackground();
            SetupMenu();
        }

        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
            cam.backgroundColor = backgroundColour;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0f, 0f, -10f);

            if (cam.GetComponent<CameraShake>() == null)
                cam.gameObject.AddComponent<CameraShake>();
            if (cam.GetComponent<AudioListener>() == null)
                cam.gameObject.AddComponent<AudioListener>();
        }

        private void SetupBackground()
        {
            var go = new GameObject("ParallaxBackground");
            go.AddComponent<ParallaxBackground>();
        }

        private void SetupMenu()
        {
            new GameObject("MainMenu").AddComponent<MainMenuController>();
        }
    }
}
