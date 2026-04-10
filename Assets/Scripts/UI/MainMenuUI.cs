using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Handles main-menu panel visuals and button callbacks.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text highScoreText;

        public void Show(int highScore)
        {
            if (panel != null)
            {
                panel.SetActive(true);
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
