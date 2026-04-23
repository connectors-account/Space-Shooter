using SpaceShooter.Enemy;
using UnityEngine;

namespace SpaceShooter.Core
{
    public class GameplayDirector : MonoBehaviour
    {
        [SerializeField] private EnemySpawner enemySpawner;

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            if (enemySpawner != null && enemySpawner.AllWavesCompleted)
            {
                GameManager.Instance.GameOver();
            }
        }
    }
}
