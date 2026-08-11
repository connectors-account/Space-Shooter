using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Spawns enemies at fixed intervals along the top edge of the screen at random
    /// horizontal positions. Fixed difficulty (no progressive wave scaling), per the
    /// simplified scope.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawning")]
        [Tooltip("Enemy prefab to spawn.")]
        [SerializeField] private GameObject enemyPrefab;

        [Tooltip("Seconds between enemy spawns.")]
        [SerializeField] private float spawnInterval = 1.5f;

        [Tooltip("Horizontal padding from the screen edges when picking a spawn X.")]
        [SerializeField] private float horizontalPadding = 0.6f;

        [Tooltip("How far above the top edge enemies appear before entering view.")]
        [SerializeField] private float spawnHeightOffset = 0.5f;

        private Camera mainCamera;
        private float nextSpawnTime;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Start()
        {
            nextSpawnTime = Time.time + spawnInterval;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }

            if (enemyPrefab != null && Time.time >= nextSpawnTime)
            {
                SpawnEnemy();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }

        private void SpawnEnemy()
        {
            if (mainCamera == null)
            {
                return;
            }

            Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 1f, 0f));
            Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

            float spawnX = Random.Range(min.x + horizontalPadding, max.x - horizontalPadding);
            float spawnY = max.y + spawnHeightOffset;

            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
    }
}
