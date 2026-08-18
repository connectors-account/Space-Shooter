using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SpaceShooter;

namespace SpaceShooter.Tests
{
    /// <summary>
    /// TDD tests for <see cref="WaveManager"/>.
    /// Wave progression logic is validated independently of the live enemy spawner.
    /// </summary>
    public class WaveManagerTests
    {
        private WaveManager CreateManager()
        {
            var go = new GameObject("WaveManager");
            var wm = go.AddComponent<WaveManager>();
            wm.Initialize();
            wm.autoSpawn = false; // do not touch the live spawner during logic tests
            return wm;
        }

        [Test]
        public void Wave1_HasFiveEnemies()
        {
            var wm = CreateManager();
            WaveData data = wm.GetWaveData(1);
            Assert.AreEqual(5, wm.GetTotalEnemies(data));
            Object.DestroyImmediate(wm.gameObject);
        }

        [Test]
        public void Wave1_IsAllDrones()
        {
            var wm = CreateManager();
            WaveData data = wm.GetWaveData(1);
            Assert.AreEqual(1, data.enemyTypes.Length);
            Assert.AreEqual(EnemyType.Drone, data.enemyTypes[0]);
            Assert.AreEqual(5, data.counts[0]);
            Object.DestroyImmediate(wm.gameObject);
        }

        [Test]
        public void Wave5_IsBossWave()
        {
            var wm = CreateManager();
            WaveData data = wm.GetWaveData(5);
            Assert.IsTrue(data.isBossWave);
            Assert.AreEqual(EnemyType.Boss, data.enemyTypes[0]);
            Object.DestroyImmediate(wm.gameObject);
        }

        [Test]
        public void TotalWaves_IsTen()
        {
            var wm = CreateManager();
            Assert.AreEqual(10, wm.TotalWaves);
            Object.DestroyImmediate(wm.gameObject);
        }

        [Test]
        public void BeginWave_SetsRemainingCount()
        {
            var wm = CreateManager();
            wm.BeginWave(1);
            Assert.AreEqual(1, wm.CurrentWave);
            Assert.AreEqual(5, wm.EnemiesRemaining);
            Object.DestroyImmediate(wm.gameObject);
        }

        [Test]
        public void WaveAdvances_AfterAllEnemiesKilled()
        {
            var wm = CreateManager();
            wm.BeginWave(1);
            for (int i = 0; i < 5; i++) wm.NotifyEnemyKilled();
            Assert.AreEqual(2, wm.CurrentWave);
            Object.DestroyImmediate(wm.gameObject);
        }

        [Test]
        public void OnWaveComplete_FiresWhenWaveCleared()
        {
            var wm = CreateManager();
            int completed = -1;
            wm.OnWaveComplete += w => completed = w;
            wm.BeginWave(1);
            for (int i = 0; i < 5; i++) wm.NotifyEnemyKilled();
            Assert.AreEqual(1, completed);
            Object.DestroyImmediate(wm.gameObject);
        }

        [Test]
        public void OnBossSpawn_FiresOnBossWave()
        {
            var wm = CreateManager();
            bool bossSpawned = false;
            wm.OnBossSpawn += () => bossSpawned = true;
            wm.BeginWave(5);
            Assert.IsTrue(bossSpawned);
            Object.DestroyImmediate(wm.gameObject);
        }

        [Test]
        public void Wave3_MixesDronesAndFighters()
        {
            var wm = CreateManager();
            WaveData data = wm.GetWaveData(3);
            Assert.AreEqual(2, data.enemyTypes.Length);
            Assert.AreEqual(8, wm.GetTotalEnemies(data)); // 5 drones + 3 fighters
            Object.DestroyImmediate(wm.gameObject);
        }
    }
}
