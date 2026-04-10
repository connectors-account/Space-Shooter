using System.Collections;
using UnityEngine;

[System.Serializable]
public struct WaveData
{
    public int waveNumber;
    public int enemyCount;
    public float spawnInterval;
    public float fastChance;
    public float tankChance;
}

public class WaveManager : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private float timeBetweenWaves = 2.5f;
    [SerializeField] private int baseEnemyCount = 8;
    [SerializeField] private float minSpawnInterval = 0.2f;

    public int CurrentWave { get; private set; } = 0;

    private void Start()
    {
        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        while (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            CurrentWave++;
            WaveData data = BuildWave(CurrentWave);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx(AudioCue.WaveStart);
            }

            yield return enemySpawner.StartCoroutine(enemySpawner.SpawnWave(data));

            while (enemySpawner.EnemiesAlive > 0 && !GameManager.Instance.IsGameOver)
            {
                yield return null;
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private WaveData BuildWave(int wave)
    {
        float difficulty = 1f + wave * 0.12f;
        return new WaveData
        {
            waveNumber = wave,
            enemyCount = Mathf.RoundToInt(baseEnemyCount * difficulty),
            spawnInterval = Mathf.Max(minSpawnInterval, 0.85f - (wave * 0.05f)),
            fastChance = Mathf.Clamp01(0.15f + wave * 0.03f),
            tankChance = Mathf.Clamp01(0.05f + wave * 0.02f)
        };
    }
}
