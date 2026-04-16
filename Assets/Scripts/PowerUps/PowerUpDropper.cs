using UnityEngine;

namespace SpaceShooter.PowerUps
{
    public class PowerUpDropper : MonoBehaviour
    {
        [System.Serializable]
        private struct DropEntry
        {
            public PowerUpType type;
            [Range(0f, 1f)] public float weight;
            public GameObject prefab;
        }

        [SerializeField] private DropEntry[] dropTable;
        [Range(0f, 1f)]
        [SerializeField] private float dropChance = 0.25f;

        public void TryDrop(Vector3 worldPosition)
        {
            if (dropTable == null || dropTable.Length == 0)
            {
                return;
            }

            if (Random.value > dropChance)
            {
                return;
            }

            float totalWeight = 0f;
            for (int i = 0; i < dropTable.Length; i++)
            {
                totalWeight += Mathf.Max(0f, dropTable[i].weight);
            }

            if (totalWeight <= 0f)
            {
                return;
            }

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < dropTable.Length; i++)
            {
                DropEntry entry = dropTable[i];
                cumulative += Mathf.Max(0f, entry.weight);

                if (roll <= cumulative && entry.prefab != null)
                {
                    Instantiate(entry.prefab, worldPosition, Quaternion.identity);
                    return;
                }
            }
        }
    }
}
