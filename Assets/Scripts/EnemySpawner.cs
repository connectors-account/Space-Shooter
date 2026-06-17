using UnityEngine;

/// <summary>
/// Spawns enemy ships at the top of the screen on a timer. The spawn interval
/// gradually decreases over time so the game ramps up in difficulty. Each
/// spawned enemy is given a random movement pattern and horizontal position.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    [Tooltip("Enemy prefab to spawn. Must contain an EnemyController.")]
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("Seconds between spawns at the start of the game.")]
    [SerializeField] private float startInterval = 1.8f;

    [Tooltip("Smallest allowed interval as difficulty ramps up.")]
    [SerializeField] private float minInterval = 0.5f;

    [Tooltip("How much the interval shrinks per second of play.")]
    [SerializeField] private float difficultyRamp = 0.02f;

    [Tooltip("Vertical offset above the top edge where enemies appear.")]
    [SerializeField] private float spawnHeightPadding = 0.6f;

    [Tooltip("Horizontal margin kept away from the screen edges (viewport 0-1).")]
    [Range(0f, 0.4f)]
    [SerializeField] private float horizontalMargin = 0.08f;

    private float currentInterval;
    private float timer;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        currentInterval = startInterval;
        timer = currentInterval;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (enemyPrefab == null || mainCamera == null) return;

        // Ramp difficulty: slowly shrink the spawn interval.
        currentInterval = Mathf.Max(minInterval, currentInterval - difficultyRamp * Time.deltaTime);

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = currentInterval;
        }
    }

    private void SpawnEnemy()
    {
        // Pick a random horizontal position within the visible area.
        float vx = Random.Range(horizontalMargin, 1f - horizontalMargin);
        Vector3 viewportPoint = new Vector3(vx, 1f, Mathf.Abs(mainCamera.transform.position.z));
        Vector3 worldPoint = mainCamera.ViewportToWorldPoint(viewportPoint);
        worldPoint.z = 0f;
        worldPoint.y += spawnHeightPadding;

        GameObject enemy = Instantiate(enemyPrefab, worldPoint, Quaternion.identity);

        // Make sure it carries the Enemy tag for collision logic.
        if (!enemy.CompareTag("Enemy"))
        {
            enemy.tag = "Enemy";
        }
    }
}
