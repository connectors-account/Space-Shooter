using UnityEngine;

/// <summary>
/// Spawns enemies at a random X position just above the top of the screen.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public float spawnInterval = 1.0f;
    public float minSpeed = 2.5f;
    public float maxSpeed = 4.5f;
    public float edgeMargin = 0.7f;

    float timer;

    public void ResetTimer()
    {
        timer = spawnInterval;
    }

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.IsGameOver) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = spawnInterval;

        Vector2 half = gm.HalfScreenSize;
        float x = Random.Range(-half.x + edgeMargin, half.x - edgeMargin);
        Enemy.Spawn(new Vector3(x, half.y + 1f, 0f), Random.Range(minSpeed, maxSpeed));
    }
}
