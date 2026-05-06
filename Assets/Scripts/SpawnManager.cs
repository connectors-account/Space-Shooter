using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject chaserEnemyPrefab;
    [SerializeField] private GameObject zigZagEnemyPrefab;
    [SerializeField] private GameObject shooterEnemyPrefab;

    [Header("Power Up Prefabs")]
    [SerializeField] private GameObject rapidFirePowerUpPrefab;
    [SerializeField] private GameObject shieldPowerUpPrefab;
    [SerializeField] private GameObject healthPowerUpPrefab;

    [Header("Spawn Area")]
    [SerializeField] private float spawnXMin = -8f;
    [SerializeField] private float spawnXMax = 8f;
    [SerializeField] private float enemySpawnY = 6f;
    [SerializeField] private float powerUpSpawnY = 6.5f;

    [Header("Timing")]
    [SerializeField] private float baseSpawnDelay = 0.7f;
    [SerializeField] private float waveStartDelay = 1.2f;
    [SerializeField] private float waveClearCheckDelay = 1.5f;

    private Coroutine waveRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartWave(int waveNumber)
    {
        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
        }

        waveRoutine = StartCoroutine(SpawnWaveRoutine(waveNumber));
    }

    private IEnumerator SpawnWaveRoutine(int waveNumber)
    {
        yield return new WaitForSeconds(waveStartDelay);

        int enemyCount = GameManager.Instance.GetEnemyCountForWave(waveNumber);
        float spawnDelay = Mathf.Max(0.2f, baseSpawnDelay - (waveNumber - 1) * 0.04f);

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemyByWeightedType(waveNumber);
            yield return new WaitForSeconds(spawnDelay);
        }

        TrySpawnPowerUp(waveNumber);

        // Wait for all enemies to be cleared before starting next wave.
        while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
        {
            yield return new WaitForSeconds(waveClearCheckDelay);
        }

        GameManager.Instance.OnWaveCleared();
    }

    private void SpawnEnemyByWeightedType(int waveNumber)
    {
        float randomValue = Random.value;
        GameObject enemyPrefab;

        // More complex enemies become more common on later waves.
        float chaserWeight = Mathf.Max(0.2f, 0.65f - waveNumber * 0.03f);
        float zigZagWeight = Mathf.Clamp01(0.2f + waveNumber * 0.015f);

        if (randomValue < chaserWeight)
        {
            enemyPrefab = chaserEnemyPrefab;
        }
        else if (randomValue < chaserWeight + zigZagWeight)
        {
            enemyPrefab = zigZagEnemyPrefab;
        }
        else
        {
            enemyPrefab = shooterEnemyPrefab;
        }

        if (enemyPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = new Vector3(Random.Range(spawnXMin, spawnXMax), enemySpawnY, 0f);
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }

    private void TrySpawnPowerUp(int waveNumber)
    {
        float chance = Mathf.Clamp01(0.35f + waveNumber * 0.02f);
        if (Random.value > chance)
        {
            return;
        }

        GameObject[] powerUps = { rapidFirePowerUpPrefab, shieldPowerUpPrefab, healthPowerUpPrefab };
        GameObject selected = powerUps[Random.Range(0, powerUps.Length)];

        if (selected == null)
        {
            return;
        }

        Vector3 spawnPosition = new Vector3(Random.Range(spawnXMin, spawnXMax), powerUpSpawnY, 0f);
        Instantiate(selected, spawnPosition, Quaternion.identity);
    }
}
