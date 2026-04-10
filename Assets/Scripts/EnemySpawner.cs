using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float fallbackSpawnY = 6.2f;
    [SerializeField] private float horizontalRange = 8f;

    private int _enemiesAlive;
    private bool _waveSpawning;

    public int EnemiesAlive => _enemiesAlive;
    public bool WaveSpawning => _waveSpawning;

    public IEnumerator SpawnWave(WaveData waveData)
    {
        _waveSpawning = true;

        for (int i = 0; i < waveData.enemyCount; i++)
        {
            EnemyType type = ChooseEnemyType(waveData);
            SpawnEnemy(type);
            yield return new WaitForSeconds(waveData.spawnInterval);
        }

        _waveSpawning = false;
    }

    public void SpawnEnemy(EnemyType enemyType)
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        ObjectPool pool = GameManager.Instance.GetEnemyPool(enemyType);
        EnemyController enemy = pool != null ? pool.Get<EnemyController>() : null;
        if (enemy == null)
        {
            return;
        }

        enemy.transform.position = GetSpawnPosition();
        enemy.Initialize(pool, this);
        _enemiesAlive++;
    }

    public void NotifyEnemyDestroyed(EnemyController enemy)
    {
        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
    }

    private EnemyType ChooseEnemyType(WaveData waveData)
    {
        float roll = Random.value;
        if (roll < waveData.tankChance)
        {
            return EnemyType.Tank;
        }

        if (roll < waveData.tankChance + waveData.fastChance)
        {
            return EnemyType.Fast;
        }

        return EnemyType.Basic;
    }

    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }

        return new Vector3(Random.Range(-horizontalRange, horizontalRange), fallbackSpawnY, 0f);
    }
}
