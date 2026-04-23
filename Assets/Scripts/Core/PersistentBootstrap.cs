using UnityEngine;

namespace SpaceShooter.Core
{
    public class PersistentBootstrap : MonoBehaviour
    {
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject soundManagerPrefab;

        private void Awake()
        {
            if (GameManager.Instance == null && gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }

            if (Audio.SoundManager.Instance == null && soundManagerPrefab != null)
            {
                Instantiate(soundManagerPrefab);
            }
        }
    }
}
