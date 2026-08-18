using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SpaceShooter;

namespace SpaceShooter.Tests
{
    /// <summary>
    /// TDD tests for the power-up hierarchy.
    /// </summary>
    public class PowerUpTests
    {
        private GameObject CreatePlayer(out PlayerController pc, out PlayerHealth ph, out PlayerShooter ps)
        {
            var go = new GameObject("Player");
            pc = go.AddComponent<PlayerController>();
            ph = go.AddComponent<PlayerHealth>();
            ph.invincibilityDuration = 0f;
            ph.ResetHealth();
            ps = go.AddComponent<PlayerShooter>();
            pc.moveSpeed = 8f;
            ps.fireMode = FireMode.Single;
            return go;
        }

        private T CreatePowerUp<T>() where T : PowerUpBase
        {
            var go = new GameObject(typeof(T).Name);
            return go.AddComponent<T>();
        }

        [Test]
        public void SpeedPowerUp_IncreasesMoveSpeed()
        {
            var player = CreatePlayer(out var pc, out _, out _);
            var pu = CreatePowerUp<PowerUpSpeed>();
            pu.Apply(player);
            Assert.AreEqual(12f, pc.moveSpeed, 0.0001f); // 8 + 4
            pu.Expire(player);
            Assert.AreEqual(8f, pc.moveSpeed, 0.0001f);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(pu.gameObject);
        }

        [Test]
        public void ShieldPowerUp_GrantsShield()
        {
            var player = CreatePlayer(out _, out var ph, out _);
            var pu = CreatePowerUp<PowerUpShield>();
            pu.Apply(player);
            Assert.IsTrue(ph.HasShield);
            Assert.AreEqual(3, ph.ShieldHP);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(pu.gameObject);
        }

        [Test]
        public void TripleShotPowerUp_ChangesFireMode()
        {
            var player = CreatePlayer(out _, out _, out var ps);
            var pu = CreatePowerUp<PowerUpTripleShot>();
            pu.Apply(player);
            Assert.AreEqual(FireMode.Triple, ps.fireMode);
            pu.Expire(player);
            Assert.AreEqual(FireMode.Single, ps.fireMode);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(pu.gameObject);
        }

        [UnityTest]
        public IEnumerator BombPowerUp_ClearsAllEnemies()
        {
            var sm = new GameObject("ScoreManager").AddComponent<ScoreManager>();
            sm.Initialize();
            sm.ResetScore();

            var e1 = new GameObject("Enemy1").AddComponent<EnemyHealth>();
            e1.Configure(3, 100);
            var e2 = new GameObject("Enemy2").AddComponent<EnemyHealth>();
            e2.Configure(5, 250);
            yield return null;

            var player = CreatePlayer(out _, out _, out _);
            var pu = CreatePowerUp<PowerUpBomb>();
            pu.Apply(player);
            yield return null; // allow Destroy to process

            Assert.IsTrue(e1 == null);
            Assert.IsTrue(e2 == null);
            Assert.AreEqual(100, ScoreManager.Instance.GetScore()); // 50 per enemy * 2

            Object.DestroyImmediate(player);
            Object.DestroyImmediate(pu.gameObject);
            Object.DestroyImmediate(sm.gameObject);
        }

        [UnityTest]
        public IEnumerator SpeedPowerUp_ExpiresAfterDuration()
        {
            var player = CreatePlayer(out var pc, out _, out _);
            var pu = CreatePowerUp<PowerUpSpeed>();
            pu.duration = 0.3f;
            pu.ApplyWithTimer(player);
            Assert.AreEqual(12f, pc.moveSpeed, 0.0001f);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(8f, pc.moveSpeed, 0.0001f);
            Object.DestroyImmediate(player);
        }
    }
}
