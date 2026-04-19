using SpaceShooter.Gameplay;
using SpaceShooter.UI;
using UnityEngine;

namespace SpaceShooter.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (FindObjectOfType<GameBootstrap>() != null)
            {
                return;
            }

            GameObject bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<GameBootstrap>();
            DontDestroyOnLoad(bootstrap);
        }

        private void Awake()
        {
            EnsureMainCamera();

            EntityFactory.CreateParallaxBackground();

            AudioManager audioManager = gameObject.AddComponent<AudioManager>();
            audioManager.Initialize();

            SpawnManager spawnManager = gameObject.AddComponent<SpawnManager>();
            spawnManager.Initialize();

            UIManager uiManager = UIBuilder.CreateUI();

            GameManager gameManager = gameObject.AddComponent<GameManager>();
            gameManager.Initialize(spawnManager, uiManager, audioManager);

            spawnManager.Bind(gameManager);
            uiManager.BindButtons(gameManager);
        }

        private static void EnsureMainCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 5.2f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }
    }
}
