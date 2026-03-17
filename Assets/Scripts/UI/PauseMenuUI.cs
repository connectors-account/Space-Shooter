using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause menu that appears when Escape is pressed during gameplay.
/// Attach to a panel inside the Game scene Canvas.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance { get; private set; }

    [Header("UI References")]
    public Text pauseText;
    public Button resumeButton;
    public Button mainMenuButton;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        gameObject.SetActive(false);
    }

    void Start()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    void Update()
    {
        // Toggle pause with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;

            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
                OnResumeClicked();
            else
                ShowPause();
        }
    }

    public void ShowPause()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        if (GameManager.Instance != null)
            GameManager.Instance.IsPaused = true;
    }

    public void OnResumeClicked()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.IsPaused = false;
    }

    void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.IsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}
