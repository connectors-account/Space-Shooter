using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    public float spawnYPosition = 6f;
    public float minX = -7f;
    public float maxX = 7f;

    [Header("Enemy Prefabs")]
    public GameObject basicEnemyPrefab;
    public GameObject zigzagEnemyPrefab;
    public GameObject circularEnemyPrefab;
    public GameObject chargerEnemyPrefab;
    public GameObject bossEnemyPrefab;

    private bool isSpawning = false;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnEnemy(EnemyType type)
    {
        Vector3 spawnPos = GetRandomSpawnPosition();
        SpawnEnemyAt(type, spawnPos);
    }

    public void SpawnEnemyAt(EnemyType type, Vector3 position)
    {
        GameObject prefab = GetPrefabForType(type);
        if (prefab == null)
        {
            Debug.LogWarning($"No prefab found for enemy type: {type}");
            return;
        }

        string poolTag = type.ToString() + "Enemy";

        GameObject enemy;
        if (ObjectPooler.Instance != null && ObjectPooler.Instance.poolDictionary.ContainsKey(poolTag))
        {
            enemy = ObjectPooler.Instance.SpawnFromPool(poolTag, position, Quaternion.identity);
        }
        else
        {
            enemy = Instantiate(prefab, position, Quaternion.identity);
        }
    }

    public void SpawnWave(List<EnemySpawnInfo> spawnInfos)
    {
        StartCoroutine(SpawnWaveCoroutine(spawnInfos));
    }

    private IEnumerator SpawnWaveCoroutine(List<EnemySpawnInfo> spawnInfos)
    {
        isSpawning = true;

        foreach (var info in spawnInfos)
        {
            if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing)
            {
                isSpawning = false;
                yield break;
            }

            SpawnEnemyAt(info.type, info.position);
            yield return new WaitForSeconds(info.delay);
        }

        isSpawning = false;
    }

    public void SpawnFormation(EnemyType type, FormationType formation, int count)
    {
        List<Vector3> positions = GetFormationPositions(formation, count);
        StartCoroutine(SpawnFormationCoroutine(type, positions));
    }

    private IEnumerator SpawnFormationCoroutine(EnemyType type, List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            SpawnEnemyAt(type, pos);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private List<Vector3> GetFormationPositions(FormationType formation, int count)
    {
        List<Vector3> positions = new List<Vector3>();

        switch (formation)
        {
            case FormationType.Line:
                float spacing = (maxX - minX) / (count + 1);
                for (int i = 0; i < count; i++)
                {
                    float x = minX + spacing * (i + 1);
                    positions.Add(new Vector3(x, spawnYPosition, 0));
                }
                break;

            case FormationType.V:
                float vSpacing = 1f;
                for (int i = 0; i < count; i++)
                {
                    int side = i % 2 == 0 ? 1 : -1;
                    int row = i / 2;
                    float x = side * row * vSpacing;
                    float y = spawnYPosition - row * 0.5f;
                    positions.Add(new Vector3(x, y, 0));
                }
                break;

            case FormationType.Circle:
                float radius = 2f;
                float angleStep = 360f / count;
                for (int i = 0; i < count; i++)
                {
                    float angle = angleStep * i * Mathf.Deg2Rad;
                    float x = Mathf.Cos(angle) * radius;
                    float y = spawnYPosition + Mathf.Sin(angle) * radius * 0.5f;
                    positions.Add(new Vector3(x, y, 0));
                }
                break;

            case FormationType.Random:
                for (int i = 0; i < count; i++)
                {
                    positions.Add(GetRandomSpawnPosition());
                }
                break;
        }

        return positions;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(minX, maxX);
        return new Vector3(x, spawnYPosition, 0);
    }

    private GameObject GetPrefabForType(EnemyType type)
    {
        return type switch
        {
            EnemyType.Basic => basicEnemyPrefab,
            EnemyType.Zigzag => zigzagEnemyPrefab,
            EnemyType.Circular => circularEnemyPrefab,
            EnemyType.Charger => chargerEnemyPrefab,
            EnemyType.Boss => bossEnemyPrefab,
            _ => basicEnemyPrefab
        };
    }

    public void SpawnBoss()
    {
        Vector3 spawnPos = new Vector3(0, spawnYPosition + 2f, 0);
        SpawnEnemyAt(EnemyType.Boss, spawnPos);
    }

    public bool IsSpawning()
    {
        return isSpawning;
    }
}

[System.Serializable]
public class EnemySpawnInfo
{
    public EnemyType type;
    public Vector3 position;
    public float delay;

    public EnemySpawnInfo(EnemyType type, Vector3 position, float delay = 0.5f)
    {
        this.type = type;
        this.position = position;
        this.delay = delay;
    }
}

public enum FormationType
{
    Line,
    V,
    Circle,
    Random
}
