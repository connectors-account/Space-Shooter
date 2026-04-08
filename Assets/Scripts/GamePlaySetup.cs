using UnityEngine;

/// <summary>
/// Placed in the GamePlay scene. Wires references to GameManager on scene load.
/// </summary>
public class GamePlaySetup : MonoBehaviour
{
    public EnemySpawner enemySpawner;
    public PlayerController player;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGamePlaySceneLoaded(enemySpawner, player);
        }
        else
        {
            Debug.LogWarning("GamePlaySetup: No GameManager found. " +
                "Make sure to start from MainMenu scene or add a GameManager to this scene.");
            // Fallback: create a temporary GameManager so the game still works
            GameObject go = new GameObject("GameManager");
            GameManager gm = go.AddComponent<GameManager>();
            gm.OnGamePlaySceneLoaded(enemySpawner, player);
        }
    }
}
