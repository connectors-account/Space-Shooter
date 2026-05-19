using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the Main Menu UI with Start Game and Quit buttons.
/// Also displays an animated title and background stars.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    [Header("Title")]
    [SerializeField] private Text titleText;

    private float titleBobTime;

    private void Start()
    {
        Time.timeScale = 1f;

        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void Update()
    {
        // Animate title text with a gentle bob
        if (titleText != null)
        {
            titleBobTime += Time.deltaTime;
            float offsetY = Mathf.Sin(titleBobTime * 2f) * 5f;
            titleText.transform.localPosition = new Vector3(
                titleText.transform.localPosition.x,
                offsetY + 100f,
                0
            );
        }
    }

    private void OnStartClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void OnQuitClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
