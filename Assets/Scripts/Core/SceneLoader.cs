using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Handles asynchronous scene loading with a simple full-screen fade
    /// (and optional loading-progress bar). Persists across scenes.
    ///
    /// If no CanvasGroup is assigned in the Inspector, one is created at
    /// runtime so the loader works with zero manual setup.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [Header("Fade")]
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Optional progress bar")]
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

            if (fadeGroup == null)
                CreateRuntimeFadeCanvas();

            // Start fully transparent.
            if (fadeGroup != null)
            {
                fadeGroup.alpha = 0f;
                fadeGroup.blocksRaycasts = false;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void CreateRuntimeFadeCanvas()
        {
            var canvasGo = new GameObject("SceneLoaderCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // On top of everything.
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var panelGo = new GameObject("FadePanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            var image = panelGo.AddComponent<Image>();
            image.color = Color.black;
            var rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            fadeGroup = panelGo.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }

        /// <summary>Load a scene by name with a fade-out / fade-in transition.</summary>
        public void LoadScene(string sceneName)
        {
            if (_isLoading) return;
            StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            _isLoading = true;

            yield return StartCoroutine(Fade(1f)); // Fade to black.

            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(true);
                progressBar.value = 0f;
            }

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            // Unity reports 0.9 when ready to activate.
            while (op.progress < 0.9f)
            {
                if (progressBar != null)
                    progressBar.value = Mathf.Clamp01(op.progress / 0.9f);
                yield return null;
            }

            if (progressBar != null) progressBar.value = 1f;

            // Small beat so the fade feels intentional.
            yield return new WaitForSecondsRealtime(0.1f);
            op.allowSceneActivation = true;

            while (!op.isDone)
                yield return null;

            if (progressBar != null)
                progressBar.gameObject.SetActive(false);

            yield return StartCoroutine(Fade(0f)); // Fade back in.

            _isLoading = false;
        }

        private IEnumerator Fade(float targetAlpha)
        {
            if (fadeGroup == null) yield break;

            fadeGroup.blocksRaycasts = true;
            float start = fadeGroup.alpha;
            float elapsed = 0f;
            float dur = Mathf.Max(0.0001f, fadeDuration);

            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(start, targetAlpha, elapsed / dur);
                yield return null;
            }
            fadeGroup.alpha = targetAlpha;
            fadeGroup.blocksRaycasts = targetAlpha > 0.01f;
        }
    }
}
