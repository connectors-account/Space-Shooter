using UnityEngine;

/// <summary>
/// PauseController listens for the Escape key to toggle pause.
/// Attach to any persistent object in the GamePlay scene.
/// </summary>
public class PauseController : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
            }
        }
    }
}
