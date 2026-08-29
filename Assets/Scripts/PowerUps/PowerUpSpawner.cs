using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.PowerUps
{
    /// <summary>
    /// Spawns random power-up prefabs at a given position based on a drop chance.
    /// Called by EnemyBase.OnDeath.
    /// </summary>
    public class PowerUpSpawner : Singleton<PowerUpSpawner>
    {
        [Header("Power-Up Prefabs")]
        [Tooltip("Each entry should have a PowerUpBase component with its PowerUpData assigned.")]
        [SerializeField] private GameObject[] powerUpPrefabs;

        [Header("Weighting")]
        [Tooltip("Relative spawn weights, matched by index to powerUpPrefabs. Leave empty for equal weights.")]
        [SerializeField] private float[] weights;

        protected override bool PersistAcrossScenes => false;

        /// <summary>
        /// Rolls against the given chance and, if successful, spawns a random power-up.
        /// </summary>
        public void TrySpawn(Vector3 position, float chance)
        {
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
            {
                return;
            }

            if (Random.value > Mathf.Clamp01(chance))
            {
                return;
            }

            Spawn(position);
        }

        /// <summary>
        /// Immediately spawns a random (weighted) power-up at the position.
        /// </summary>
        public GameObject Spawn(Vector3 position)
        {
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
            {
                return null;
            }

            int index = SelectIndex();
            GameObject prefab = powerUpPrefabs[index];
            if (prefab == null)
            {
                return null;
            }

            return Instantiate(prefab, position, Quaternion.identity);
        }

        private int SelectIndex()
        {
            if (weights == null || weights.Length != powerUpPrefabs.Length)
            {
                return Random.Range(0, powerUpPrefabs.Length);
            }

            float total = 0f;
            foreach (float w in weights)
            {
                total += Mathf.Max(0f, w);
            }

            if (total <= 0f)
            {
                return Random.Range(0, powerUpPrefabs.Length);
            }

            float roll = Random.value * total;
            float cumulative = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += Mathf.Max(0f, weights[i]);
                if (roll <= cumulative)
                {
                    return i;
                }
            }

            return powerUpPrefabs.Length - 1;
        }
    }
}
