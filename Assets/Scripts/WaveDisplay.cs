using UnityEngine;
using UnityEngine.UI;

public class WaveDisplay : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private Text waveText;

    private void Update()
    {
        if (waveManager != null && waveText != null)
        {
            waveText.text = $"Wave: {waveManager.CurrentWave}";
        }
    }
}
