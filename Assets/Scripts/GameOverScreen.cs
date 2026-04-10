using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text highScoreText;

    public void Show(int score, int highScore)
    {
        if (root != null)
        {
            root.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {score}";
        }

        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore}";
        }
    }

    public void OnRestartPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    public void OnMainMenuPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
}
