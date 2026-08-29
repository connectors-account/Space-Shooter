using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SpaceShooter.Utilities;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Wraps UnityEngine.SceneManagement with convenience methods and an optional
    /// async load with a fade-in/out loading screen.
    /// </summary>
    public class SceneLoader : Singleton<SceneLoader>
    {
        [Header("Loading Screen (optional)")]
        [SerializeField] private CanvasGroup loadingScreen;
        [SerializeField] private Slider progressBar;
        [SerializeField] private float fadeDuration = 0.35f;

        private bool _isLoading;

        protected override void OnAwakeInitialize()
        {
            if (loadingScreen != null)
            {
                loadingScreen.alpha = 0f;
                loadingScreen.gameObject.SetActive(false);
            }
        }

        // ------------------------------------------------------------------
        // Convenience wrappers
        // ------------------------------------------------------------------
        public void LoadMainMenu(bool async = true) => LoadScene(Constants.Scenes.MainMenu, async);
        public void LoadGameScene(bool async = true) => LoadScene(Constants.Scenes.Game, async);
        public void LoadGameOver(bool async = true) => LoadScene(Constants.Scenes.GameOver, async);

        /// <summary>
        /// Loads a scene either synchronously or asynchronously with an optional loading fade.
        /// </summary>
        public void LoadScene(string sceneName, bool async = true)
        {
            if (_isLoading)
            {
                return;
            }

            // Always reset the timescale so the next scene runs normally.
            Time.timeScale = 1f;

            if (async && loadingScreen != null)
            {
                StartCoroutine(LoadSceneAsyncRoutine(sceneName));
            }
            else if (async)
            {
                StartCoroutine(LoadSceneAsyncSimple(sceneName));
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ------------------------------------------------------------------
        // Async routines
        // ------------------------------------------------------------------
        private IEnumerator LoadSceneAsyncSimple(string sceneName)
        {
            _isLoading = true;
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone)
            {
                yield return null;
            }
            _isLoading = false;
        }

        private IEnumerator LoadSceneAsyncRoutine(string sceneName)
        {
            _isLoading = true;

            // Fade in the loading screen.
            loadingScreen.gameObject.SetActive(true);
            yield return StartCoroutine(Fade(0f, 1f));

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            // Unity reports progress up to 0.9 before activation.
            while (op.progress < 0.9f)
            {
                if (progressBar != null)
                {
                    progressBar.value = Mathf.Clamp01(op.progress / 0.9f);
                }
                yield return null;
            }

            if (progressBar != null)
            {
                progressBar.value = 1f;
            }

            // Small pause so the full bar is visible.
            yield return new WaitForSecondsRealtime(0.15f);

            op.allowSceneActivation = true;
            while (!op.isDone)
            {
                yield return null;
            }

            // Fade out the loading screen.
            yield return StartCoroutine(Fade(1f, 0f));
            loadingScreen.gameObject.SetActive(false);

            _isLoading = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            float elapsed = 0f;
            loadingScreen.alpha = from;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                loadingScreen.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                yield return null;
            }

            loadingScreen.alpha = to;
        }
    }
}
