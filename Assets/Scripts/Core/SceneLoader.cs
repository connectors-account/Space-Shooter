using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>
    /// Handles asynchronous scene transitions with an optional fade-through-black
    /// loading screen. Persists across scenes so the fader survives the load.
    /// </summary>
    public class SceneLoader : Singleton<SceneLoader>
    {
        public const string MainMenuScene = "MainMenu";
        public const string GameScene = "Game";

        [Tooltip("Full-screen CanvasGroup used to fade to/from black during loads.")]
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private float fadeDuration = 0.4f;

        private bool _isLoading;

        protected override void Awake()
        {
            persistAcrossScenes = true;
            base.Awake();
        }

        public void LoadMainMenu() => LoadScene(MainMenuScene);

        public void LoadGame() => LoadScene(GameScene);

        public void ReloadGame() => LoadScene(GameScene);

        /// <summary>Loads a scene by name with a fade transition.</summary>
        public void LoadScene(string sceneName)
        {
            if (_isLoading) return;
            StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            _isLoading = true;
            Time.timeScale = 1f;

            yield return Fade(1f); // fade to black

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            if (op != null)
            {
                op.allowSceneActivation = true;
                while (!op.isDone)
                {
                    yield return null;
                }
            }

            yield return Fade(0f); // fade back in
            _isLoading = false;
        }

        private IEnumerator Fade(float targetAlpha)
        {
            if (fadeGroup == null)
            {
                yield break;
            }

            float startAlpha = fadeGroup.alpha;
            float elapsed = 0f;
            fadeGroup.blocksRaycasts = true;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }

            fadeGroup.alpha = targetAlpha;
            fadeGroup.blocksRaycasts = targetAlpha > 0.01f;
        }
    }
}
