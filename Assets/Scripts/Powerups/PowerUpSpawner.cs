using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Powerups
{
    public class PowerUpSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class PowerUpPrefabMap
        {
            public PowerUpType type;
            public GameObject prefab;
        }

        public static PowerUpSpawner Instance { get; private set; }

        [SerializeField] private float spawnChance = 0.22f;
        [SerializeField] private Transform powerUpParent;
        [SerializeField] private PowerUpPrefabMap[] powerUpPrefabs;

        private readonly Dictionary<PowerUpType, GameObject> prefabByType = new Dictionary<PowerUpType, GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            foreach (PowerUpPrefabMap map in powerUpPrefabs)
            {
                if (map != null && map.prefab != null && !prefabByType.ContainsKey(map.type))
                {
                    prefabByType.Add(map.type, map.prefab);
                }
            }
        }

        public void TrySpawn(Vector3 worldPosition)
        {
            if (Random.value > spawnChance) return;

            PowerUpType type = (PowerUpType)Random.Range(0, 3);
            if (!prefabByType.TryGetValue(type, out GameObject prefab) || prefab == null)
            {
                return;
            }

            Instantiate(prefab, worldPosition, Quaternion.identity, powerUpParent);
        }
    }
}
