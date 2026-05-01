using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnY = 6f;
    [SerializeField] private float minSpawnX = -7f;
    [SerializeField] private float maxSpawnX = 7f;
    [SerializeField] private float enemySpawnInterval = 0.5f;
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private int enemiesPerWaveIncrement = 2;

    [Header("Power-Up Spawning")]
    [SerializeField] private GameObject powerUpPrefab;
    [SerializeField] private float minPowerUpInterval = 8f;
    [SerializeField] private float maxPowerUpInterval = 14f;

    private Coroutine waveRoutine;

    private void Start()
    {
        StartCoroutine(PowerUpSpawnLoop());
    }

    public void SpawnWave(int waveNumber)
    {
        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
        }

        waveRoutine = StartCoroutine(SpawnWaveRoutine(waveNumber));
    }

    private IEnumerator SpawnWaveRoutine(int waveNumber)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab is not assigned in SpawnManager.");
            yield break;
        }

        int enemiesToSpawn = baseEnemiesPerWave + (waveNumber - 1) * enemiesPerWaveIncrement;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                yield break;
            }

            Vector3 spawnPos = new Vector3(Random.Range(minSpawnX, maxSpawnX), spawnY, 0f);
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            EnemyController enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.ConfigureForWave(waveNumber);
            }

            GameManager.Instance.NotifyEnemySpawned();
            yield return new WaitForSeconds(enemySpawnInterval);
        }

        GameManager.Instance?.NotifyWaveSpawnComplete();
    }

    private IEnumerator PowerUpSpawnLoop()
    {
        while (true)
        {
            float wait = Random.Range(minPowerUpInterval, maxPowerUpInterval);
            yield return new WaitForSeconds(wait);

            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                continue;
            }

            SpawnPowerUp();
        }
    }

    private void SpawnPowerUp()
    {
        if (powerUpPrefab == null)
        {
            return;
        }

        Vector3 spawnPos = new Vector3(Random.Range(minSpawnX, maxSpawnX), spawnY, 0f);
        GameObject powerObj = Instantiate(powerUpPrefab, spawnPos, Quaternion.identity);
        PowerUp powerUp = powerObj.GetComponent<PowerUp>();
        if (powerUp != null)
        {
            powerUp.AssignRandomType();
        }
    }
}
