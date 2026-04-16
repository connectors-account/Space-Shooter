using System.Collections;
using UnityEngine;

namespace SpaceShooter.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private float spawnXMin = -8f;
        [SerializeField] private float spawnXMax = 8f;
        [SerializeField] private float spawnY = 6f;

        public IEnumerator SpawnWave(WaveDefinition wave)
        {
            if (wave == null)
            {
                yield break;
            }

            if (wave.startDelay > 0f)
            {
                yield return new WaitForSeconds(wave.startDelay);
            }

            foreach (WaveEnemyEntry entry in wave.enemyEntries)
            {
                if (entry.enemyPrefab == null || entry.count <= 0)
                {
                    continue;
                }

                for (int i = 0; i < entry.count; i++)
                {
                    Vector3 spawnPosition = new Vector3(Random.Range(spawnXMin, spawnXMax), spawnY, 0f);
                    Instantiate(entry.enemyPrefab, spawnPosition, Quaternion.identity);

                    float delay = Mathf.Max(0.02f, entry.spawnInterval);
                    yield return new WaitForSeconds(delay);
                }
            }
        }
    }
}
