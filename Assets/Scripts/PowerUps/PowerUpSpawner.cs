using UnityEngine;

namespace SpaceShooter.PowerUps
{
    public class PowerUpSpawner : MonoBehaviour
    {
        [SerializeField] private PowerUp[] powerUpPrefabs;
        [SerializeField] private float spawnInterval = 12f;
        [SerializeField, Range(0f, 1f)] private float spawnChance = 0.8f;
        [SerializeField] private float spawnY = 6f;
        [SerializeField] private float minX = -8f;
        [SerializeField] private float maxX = 8f;

        private float timer;

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0f)
            {
                return;
            }

            timer = spawnInterval;
            if (Random.value > spawnChance || powerUpPrefabs == null || powerUpPrefabs.Length == 0)
            {
                return;
            }

            int index = Random.Range(0, powerUpPrefabs.Length);
            float spawnX = Random.Range(minX, maxX);
            Instantiate(powerUpPrefabs[index], new Vector3(spawnX, spawnY, 0f), Quaternion.identity);
        }
    }
}
