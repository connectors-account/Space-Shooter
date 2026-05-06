using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 1.2f;
    [SerializeField] private float spawnYViewport = 1.1f;
    [SerializeField] private float minXViewport = 0.1f;
    [SerializeField] private float maxXViewport = 0.9f;

    private Camera mainCamera;
    private float spawnTimer;

    private void Awake()
    {
        mainCamera = Camera.main;
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (enemyPrefab == null || mainCamera == null)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        float randomX = Random.Range(minXViewport, maxXViewport);
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(new Vector3(randomX, spawnYViewport, 0f));
        worldPosition.z = 0f;

        Instantiate(enemyPrefab, worldPosition, Quaternion.identity);
    }
}
