using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Scoring;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Reads the persisted top-5 high scores from PlayerPrefs (stored as a JSON
    /// array by <see cref="HighScoreTable"/>) and formats them into a text
    /// element. Can drive an assigned Text, or one attached at runtime.
    /// </summary>
    public class HighScoreDisplay : MonoBehaviour
    {
        [SerializeField] private Text targetText;
        [SerializeField] private string header = "TOP SCORES";
        [SerializeField] private bool refreshOnEnable = true;

        public void AttachText(Text text)
        {
            targetText = text;
            Refresh();
        }

        private void OnEnable()
        {
            if (refreshOnEnable) Refresh();
        }

        private void Start()
        {
            if (targetText == null) targetText = GetComponent<Text>();
            Refresh();
        }

        /// <summary>Rebuild the formatted score list from PlayerPrefs.</summary>
        public void Refresh()
        {
            if (targetText == null) targetText = GetComponent<Text>();
            if (targetText == null) return;
            targetText.text = Format();
        }

        /// <summary>Return the formatted top-5 list (public so other UI can reuse it).</summary>
        public string Format()
        {
            var scores = HighScoreTable.Load();
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(header)) sb.AppendLine(header);

            if (scores.Count == 0)
            {
                sb.AppendLine("—");
                return sb.ToString();
            }

            for (int i = 0; i < scores.Count; i++)
                sb.AppendLine($"{i + 1}.  {scores[i]:N0}");

            return sb.ToString();
        }
    }
}
