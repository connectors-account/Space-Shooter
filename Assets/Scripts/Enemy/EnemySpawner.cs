// ============================================================
//  EnemySpawner.cs  –  Formation helper called by WaveManager
//  Attach to any scene object; WaveManager calls SpawnFormation.
// ============================================================
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Spawns a V or grid formation at the top of the screen.
    /// </summary>
    public void SpawnFormation(GameObject prefab, int cols, int rows,
                               float colSpacing, float rowSpacing,
                               Vector3 topCentre,
                               EnemyController.Pattern     movePattern,
                               EnemyShooter.ShotPattern    shotPattern,
                               float speed)
    {
        float totalW = (cols - 1) * colSpacing;
        float startX = topCentre.x - totalW * 0.5f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = new Vector3(startX + c * colSpacing,
                                          topCentre.y - r * rowSpacing, 0f);
                var go = Instantiate(prefab, pos, Quaternion.identity);

                var ctrl = go.GetComponent<EnemyController>();
                if (ctrl) { ctrl.pattern = movePattern; ctrl.speed = speed; }

                var sh = go.GetComponent<EnemyShooter>();
                if (sh) sh.pattern = shotPattern;
            }
        }
    }
}
