using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Handles asynchronous scene loading with an optional loading screen overlay.
    /// Persists across scenes as a singleton.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        public const string MainMenuScene = "MainMenu";
        public const string GameScene = "GameScene";

        [Header("Loading Screen (optional)")]
        [SerializeField] private CanvasGroup loadingCanvas;
        [SerializeField] private Slider progressBar;

        private bool _isLoading;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (loadingCanvas != null)
            {
                loadingCanvas.gameObject.SetActive(false);
            }
        }

        public void LoadMainMenu()
        {
            LoadScene(MainMenuScene);
        }

        public void LoadGameScene()
        {
            LoadScene(GameScene);
        }

        public void LoadScene(string sceneName)
        {
            if (_isLoading) return;
            Time.timeScale = 1f;
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            _isLoading = true;

            if (loadingCanvas != null)
            {
                loadingCanvas.gameObject.SetActive(true);
                loadingCanvas.alpha = 1f;
            }
            if (progressBar != null)
            {
                progressBar.value = 0f;
            }

            yield return null;

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (!op.isDone)
            {
                // Unity progress caps at 0.9 until activation is allowed.
                float progress = Mathf.Clamp01(op.progress / 0.9f);
                if (progressBar != null)
                {
                    progressBar.value = progress;
                }

                if (op.progress >= 0.9f)
                {
                    if (progressBar != null)
                    {
                        progressBar.value = 1f;
                    }
                    op.allowSceneActivation = true;
                }
                yield return null;
            }

            if (loadingCanvas != null)
            {
                loadingCanvas.gameObject.SetActive(false);
            }
            _isLoading = false;
        }
    }
}
