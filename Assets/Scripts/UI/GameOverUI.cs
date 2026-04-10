using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Controls game-over panel and final score data.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text finalWaveText;
        [SerializeField] private Text highScoreText;

        public void Show(int finalScore, int finalWave, int highScore)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }

            if (finalScoreText != null)
            {
                finalScoreText.text = $"Score: {finalScore}";
            }

            if (finalWaveText != null)
            {
                finalWaveText.text = $"Wave Reached: {finalWave}";
            }

            if (highScoreText != null)
            {
                highScoreText.text = $"High Score: {highScore}";
            }
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
