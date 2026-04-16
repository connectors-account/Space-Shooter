using System;
using UnityEngine;

namespace SpaceShooter.Enemies
{
    [Serializable]
    public class WaveEnemyEntry
    {
        public GameObject enemyPrefab;
        public int count = 4;
        public float spawnInterval = 0.4f;
    }

    [Serializable]
    public class WaveDefinition
    {
        public string waveName = "Wave";
        public float startDelay = 1f;
        public WaveEnemyEntry[] enemyEntries;
    }
}
