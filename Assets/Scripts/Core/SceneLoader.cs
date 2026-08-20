using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Handles asynchronous scene loading with an optional loading overlay.
    /// Scene names must match those added to Build Settings ("MainMenu", "GameScene").
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        public const string MainMenuScene = "MainMenu";
        public const string GameScene = "GameScene";

        public float LoadProgress { get; private set; }
        public bool IsLoading { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadMainMenu()
        {
            LoadScene(MainMenuScene, GameState.MainMenu);
        }

        public void LoadGame()
        {
            LoadScene(GameScene, GameState.Playing);
        }

        public void ReloadGame()
        {
            LoadScene(GameScene, GameState.Playing);
        }

        private void LoadScene(string sceneName, GameState targetState)
        {
            if (IsLoading) return;
            StartCoroutine(LoadRoutine(sceneName, targetState));
        }

        private IEnumerator LoadRoutine(string sceneName, GameState targetState)
        {
            IsLoading = true;
            LoadProgress = 0f;
            Time.timeScale = 1f;

            // Guard: only load scenes that actually exist in build settings.
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"[SceneLoader] Scene '{sceneName}' is not in Build Settings. " +
                                 "Add it via File > Build Settings.");
                IsLoading = false;
                yield break;
            }

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                LoadProgress = Mathf.Clamp01(op.progress / 0.9f);
                yield return null;
            }

            LoadProgress = 1f;
            yield return new WaitForSecondsRealtime(0.2f);
            op.allowSceneActivation = true;

            while (!op.isDone) yield return null;

            IsLoading = false;

            if (GameManager.Instance != null)
            {
                if (targetState == GameState.Playing)
                    GameManager.Instance.StartNewGame();
                else
                    GameManager.Instance.SetState(targetState);
            }
        }
    }
}
