using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SpaceShooter.Core
{
    [System.Serializable]
    public class PowerUpSpawnEntry
    {
        public GameObject prefab;
        public float weight = 1f;
    }
    
    /// <summary>
    /// Spawns power-ups at intervals or from enemy drops
    /// </summary>
    public class PowerUpSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private List<PowerUpSpawnEntry> powerUps = new List<PowerUpSpawnEntry>();
        [SerializeField] private float spawnInterval = 15f;
        [SerializeField] private float spawnIntervalVariance = 5f;
        [SerializeField] private float enemyDropChance = 0.15f;
        
        [Header("Spawn Area")]
        [SerializeField] private float spawnYPosition = 6f;
        [SerializeField] private float spawnXMin = -6f;
        [SerializeField] private float spawnXMax = 6f;
        
        private float totalWeight;
        private Coroutine spawnCoroutine;
        
        private void Start()
        {
            CalculateTotalWeight();
            StartSpawning();
            
            // Subscribe to enemy death for drops
            // This would be connected via events in a full implementation
        }
        
        private void CalculateTotalWeight()
        {
            totalWeight = 0f;
            foreach (var entry in powerUps)
            {
                totalWeight += entry.weight;
            }
        }
        
        public void StartSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
        
        public void StopSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }
        
        private IEnumerator SpawnRoutine()
        {
            yield return new WaitForSeconds(5f); // Initial delay
            
            while (true)
            {
                if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }
                
                float waitTime = spawnInterval + Random.Range(-spawnIntervalVariance, spawnIntervalVariance);
                yield return new WaitForSeconds(waitTime);
                
                SpawnRandomPowerUp();
            }
        }
        
        public void SpawnRandomPowerUp()
        {
            if (powerUps.Count == 0 || totalWeight <= 0) return;
            
            float x = Random.Range(spawnXMin, spawnXMax);
            Vector3 spawnPosition = new Vector3(x, spawnYPosition, 0f);
            
            GameObject powerUp = SelectRandomPowerUp();
            if (powerUp != null)
            {
                Instantiate(powerUp, spawnPosition, Quaternion.identity);
            }
        }
        
        public void TrySpawnFromEnemy(Vector3 position)
        {
            if (Random.value <= enemyDropChance)
            {
                SpawnPowerUpAt(position);
            }
        }
        
        public void SpawnPowerUpAt(Vector3 position)
        {
            if (powerUps.Count == 0) return;
            
            GameObject powerUp = SelectRandomPowerUp();
            if (powerUp != null)
            {
                Instantiate(powerUp, position, Quaternion.identity);
            }
        }
        
        private GameObject SelectRandomPowerUp()
        {
            float random = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var entry in powerUps)
            {
                currentWeight += entry.weight;
                if (random <= currentWeight)
                {
                    return entry.prefab;
                }
            }
            
            return powerUps[powerUps.Count - 1].prefab;
        }
    }
}
