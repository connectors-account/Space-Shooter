using UnityEngine;

/// <summary>
/// Lightweight bootstrap script placed in GameScene.
/// Ensures GameManager.StartGame() is called once the scene is loaded
/// and kicks off the EnemySpawner.
/// </summary>
public class GameSceneBootstrap : MonoBehaviour
{
    public EnemySpawner enemySpawner;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }

        if (enemySpawner != null)
        {
            enemySpawner.BeginSpawning();
        }
    }
}
