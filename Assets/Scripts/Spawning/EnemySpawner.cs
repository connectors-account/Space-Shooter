using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>The kinds of enemy the spawner can create.</summary>
    public enum EnemyType
    {
        Drone,
        Fighter,
        Boss
    }

    /// <summary>Preset multi-enemy formations.</summary>
    public enum FormationType
    {
        None,
        Line,
        VShape,
        Diamond
    }

    /// <summary>
    /// Spawns enemies at the top of the screen, individually or in preset formations.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        public EnemyDrone dronePrefab;
        public EnemyFighter fighterPrefab;
        public EnemyBoss bossPrefab;

        [Header("Spawn area (world units)")]
        [Tooltip("World Y just above the top of the screen where enemies appear.")]
        public float spawnY = 5.5f;

        [Tooltip("Half-width of the horizontal spawn range.")]
        public float xRange = 7f;

        /// <summary>Spawns a single enemy of <paramref name="type"/> at <paramref name="position"/>.</summary>
        public GameObject SpawnEnemy(EnemyType type, Vector3 position)
        {
            EnemyBase prefab = PrefabFor(type);
            if (prefab == null) return null;

            EnemyBase enemy = Instantiate(prefab, position, Quaternion.identity);
            return enemy.gameObject;
        }

        /// <summary>Spawns a single enemy of <paramref name="type"/> at a random X along the top edge.</summary>
        public GameObject SpawnEnemyRandom(EnemyType type)
        {
            float x = Random.Range(-xRange, xRange);
            return SpawnEnemy(type, new Vector3(x, spawnY, 0f));
        }

        /// <summary>
        /// Spawns one of the preset formations:
        /// Line = 5 drones, V-shape = 7 fighters, Diamond = 9 mixed enemies.
        /// </summary>
        public List<GameObject> SpawnFormation(FormationType type)
        {
            var spawned = new List<GameObject>();
            switch (type)
            {
                case FormationType.Line:
                    for (int i = 0; i < 5; i++)
                    {
                        float x = Mathf.Lerp(-xRange, xRange, i / 4f);
                        spawned.Add(SpawnEnemy(EnemyType.Drone, new Vector3(x, spawnY, 0f)));
                    }
                    break;

                case FormationType.VShape:
                    for (int i = 0; i < 7; i++)
                    {
                        int offset = i - 3;                       // -3..3
                        float x = offset * 1.4f;
                        float y = spawnY + Mathf.Abs(offset) * 0.6f; // V opening downward
                        spawned.Add(SpawnEnemy(EnemyType.Fighter, new Vector3(x, y, 0f)));
                    }
                    break;

                case FormationType.Diamond:
                    // 9 enemies arranged as a diamond; corners are fighters, rest drones.
                    Vector2[] offsets =
                    {
                        new Vector2(0f, 2f),
                        new Vector2(-1.5f, 1f), new Vector2(1.5f, 1f),
                        new Vector2(-3f, 0f), new Vector2(0f, 0f), new Vector2(3f, 0f),
                        new Vector2(-1.5f, -1f), new Vector2(1.5f, -1f),
                        new Vector2(0f, -2f)
                    };
                    for (int i = 0; i < offsets.Length; i++)
                    {
                        EnemyType t = (i == 0 || i == offsets.Length - 1 || i == 3 || i == 5)
                            ? EnemyType.Fighter
                            : EnemyType.Drone;
                        var pos = new Vector3(offsets[i].x, spawnY + offsets[i].y, 0f);
                        spawned.Add(SpawnEnemy(t, pos));
                    }
                    break;
            }

            spawned.RemoveAll(go => go == null);
            return spawned;
        }

        private EnemyBase PrefabFor(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Fighter: return fighterPrefab;
                case EnemyType.Boss: return bossPrefab;
                case EnemyType.Drone:
                default: return dronePrefab;
            }
        }
    }
}
