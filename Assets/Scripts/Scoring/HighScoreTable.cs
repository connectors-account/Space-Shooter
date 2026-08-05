using System;
using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Scoring
{
    /// <summary>
    /// Serializable wrapper so a list of ints can be stored via JsonUtility.
    /// </summary>
    [Serializable]
    public class HighScoreData
    {
        public List<int> scores = new List<int>();
    }

    /// <summary>
    /// Static helper that persists the top-N scores as a JSON array inside
    /// PlayerPrefs. Used by <see cref="ScoreManager"/> and the UI displays.
    /// </summary>
    public static class HighScoreTable
    {
        /// <summary>Load the stored high scores (sorted high to low).</summary>
        public static List<int> Load()
        {
            string json = PlayerPrefs.GetString(Constants.PrefsHighScores, string.Empty);
            if (string.IsNullOrEmpty(json))
                return new List<int>();

            try
            {
                var data = JsonUtility.FromJson<HighScoreData>(json);
                if (data?.scores == null) return new List<int>();
                data.scores.Sort((a, b) => b.CompareTo(a));
                return data.scores;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[HighScoreTable] Failed to parse scores: {e.Message}");
                return new List<int>();
            }
        }

        /// <summary>Insert a score, keep the top <see cref="Constants.MaxHighScoreEntries"/>, and persist.</summary>
        public static void AddScore(int score)
        {
            if (score <= 0) return;

            var scores = Load();
            scores.Add(score);
            scores.Sort((a, b) => b.CompareTo(a));
            if (scores.Count > Constants.MaxHighScoreEntries)
                scores.RemoveRange(Constants.MaxHighScoreEntries, scores.Count - Constants.MaxHighScoreEntries);

            var data = new HighScoreData { scores = scores };
            PlayerPrefs.SetString(Constants.PrefsHighScores, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        /// <summary>Highest single stored score, or 0 if none.</summary>
        public static int Best()
        {
            var scores = Load();
            return scores.Count > 0 ? scores[0] : 0;
        }

        /// <summary>Erase all stored high scores.</summary>
        public static void Clear()
        {
            PlayerPrefs.DeleteKey(Constants.PrefsHighScores);
            PlayerPrefs.SetInt(Constants.PrefsHighScore, 0);
            PlayerPrefs.Save();
        }
    }
}
