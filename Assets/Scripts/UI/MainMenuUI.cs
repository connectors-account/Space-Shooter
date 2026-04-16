using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public void OnStartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }

        public void OnQuitClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }
    }
}
