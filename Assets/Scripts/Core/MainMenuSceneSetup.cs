using UnityEngine;

/// <summary>
/// Bootstraps the MainMenu scene. Creates camera, managers, background, and menu UI.
/// Attach this to an empty GameObject in the MainMenuScene.
/// </summary>
public class MainMenuSceneSetup : MonoBehaviour
{
    private void Awake()
    {
        SetupCamera();
        SetupManagers();
        SetupBackground();
        SetupMenuUI();
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
        }
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.01f, 0.01f, 0.05f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0, 0, -10);
    }

    private void SetupManagers()
    {
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }

        if (AudioManager.Instance == null)
        {
            GameObject audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();
        }
    }

    private void SetupBackground()
    {
        // Simple star background for menu
        Camera cam = Camera.main;
        float boundsX = cam.orthographicSize * cam.aspect;
        float boundsY = cam.orthographicSize;

        for (int i = 0; i < 80; i++)
        {
            GameObject star = new GameObject($"MenuStar_{i}");
            float brightness = Random.Range(0.2f, 0.8f);
            float size = Random.Range(0.02f, 0.06f);

            star.transform.position = new Vector3(
                Random.Range(-boundsX, boundsX),
                Random.Range(-boundsY, boundsY),
                10f
            );
            star.transform.localScale = Vector3.one * size;

            SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.CreateStar(4, new Color(brightness, brightness, brightness + 0.1f));
            sr.sortingOrder = -10;
        }
    }

    private void SetupMenuUI()
    {
        GameObject menuObj = new GameObject("MainMenuUI");
        menuObj.AddComponent<MainMenuUI>();
    }
}
