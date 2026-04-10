using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text comboText;

    private void Update()
    {
        if (scoreManager == null)
        {
            return;
        }

        if (scoreText != null)
        {
            scoreText.text = $"Score: {scoreManager.Score}";
        }

        if (comboText != null)
        {
            comboText.text = $"Combo x{scoreManager.ComboMultiplier}";
        }
    }
}
