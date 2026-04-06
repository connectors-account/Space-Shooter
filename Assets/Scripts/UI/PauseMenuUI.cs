using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause menu overlay. Toggled with Escape key during gameplay.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button mainMenuButton;

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += OnStateChanged;

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnStateChanged;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                GameManager.Instance.PauseGame();
            }
            else if (GameManager.Instance.CurrentState == GameState.Paused)
            {
                GameManager.Instance.ResumeGame();
            }
        }
    }

    private void OnResumeClicked()
    {
        GameManager.Instance?.ResumeGame();
    }

    private void OnMainMenuClicked()
    {
        GameManager.Instance?.ResumeGame(); // Restore timeScale
        ReturnAllEnemiesToPool();
        ReturnAllBulletsToPool();
        GameManager.Instance?.ReturnToMainMenu();
    }

    private void ReturnAllEnemiesToPool()
    {
        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        foreach (var enemy in enemies)
        {
            if (enemy.gameObject.activeInHierarchy)
            {
                if (ObjectPool.Instance != null)
                    ObjectPool.Instance.ReturnToPool(enemy.poolTag, enemy.gameObject);
                else
                    enemy.gameObject.SetActive(false);
            }
        }

        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null) spawner.StopSpawning();
    }

    private void ReturnAllBulletsToPool()
    {
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (var bullet in bullets)
        {
            if (bullet.gameObject.activeInHierarchy)
            {
                if (ObjectPool.Instance != null)
                    ObjectPool.Instance.ReturnToPool(bullet.poolTag, bullet.gameObject);
                else
                    bullet.gameObject.SetActive(false);
            }
        }
    }

    private void OnStateChanged(GameState state)
    {
        if (pausePanel != null)
            pausePanel.SetActive(state == GameState.Paused);
    }
}
