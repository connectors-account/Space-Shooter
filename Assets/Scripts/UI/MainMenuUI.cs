using SpaceShooter.Audio;
using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public void OnStartClicked()
        {
            AudioManager.Instance?.PlayUIClick();
            GameManager.Instance.StartNewGame();
        }

        public void OnQuitClicked()
        {
            AudioManager.Instance?.PlayUIClick();
            Application.Quit();
        }
    }
}
