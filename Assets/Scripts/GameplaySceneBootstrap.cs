using UnityEngine;

/// <summary>
/// GameplaySceneBootstrap runs when the GamePlay scene loads.
/// It notifies the GameManager to kick off gameplay, creates required
/// runtime objects if they are missing, and starts the background music.
/// Attach this to an empty GameObject in the GamePlay scene.
/// </summary>
public class GameplaySceneBootstrap : MonoBehaviour
{
    private void Start()
    {
        // Start gameplay music
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameMusic();

        // Tell GameManager to begin
        if (GameManager.Instance != null)
            GameManager.Instance.BeginGameplay();
    }
}
