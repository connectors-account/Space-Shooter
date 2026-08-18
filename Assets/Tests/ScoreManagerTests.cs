using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SpaceShooter;

namespace SpaceShooter.Tests
{
    /// <summary>
    /// TDD tests for <see cref="ScoreManager"/>.
    /// </summary>
    public class ScoreManagerTests
    {
        private ScoreManager CreateManager()
        {
            var go = new GameObject("ScoreManager");
            var sm = go.AddComponent<ScoreManager>();
            sm.Initialize();
            sm.ResetScore();
            return sm;
        }

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(ScoreManager.HighScoreKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void Score_StartsAtZero()
        {
            var sm = CreateManager();
            Assert.AreEqual(0, sm.GetScore());
            Object.DestroyImmediate(sm.gameObject);
        }

        [Test]
        public void AddScore_IncreasesScore()
        {
            var sm = CreateManager();
            sm.AddScore(100);
            Assert.AreEqual(100, sm.GetScore());
            sm.AddScore(50);
            Assert.AreEqual(150, sm.GetScore());
            Object.DestroyImmediate(sm.gameObject);
        }

        [Test]
        public void HighScore_PersistsAcrossSessions()
        {
            var sm = CreateManager();
            sm.AddScore(500);
            Assert.AreEqual(500, sm.GetHighScore());
            Object.DestroyImmediate(sm.gameObject);

            // Simulate a brand new session: a new manager should read PlayerPrefs.
            var sm2 = new GameObject("ScoreManager2").AddComponent<ScoreManager>();
            sm2.Initialize();
            Assert.AreEqual(500, sm2.GetHighScore());
            Object.DestroyImmediate(sm2.gameObject);
        }

        [Test]
        public void Multiplier_IncreasesWithConsecutiveKills()
        {
            var sm = CreateManager();
            Assert.AreEqual(1, sm.Multiplier);
            for (int i = 0; i < 5; i++) sm.RegisterKill();
            Assert.AreEqual(2, sm.Multiplier);
            for (int i = 0; i < 5; i++) sm.RegisterKill();
            Assert.AreEqual(3, sm.Multiplier);
            Object.DestroyImmediate(sm.gameObject);
        }

        [Test]
        public void Multiplier_AppliesToAddedScore()
        {
            var sm = CreateManager();
            for (int i = 0; i < 5; i++) sm.RegisterKill(); // multiplier now 2
            sm.AddScore(100);
            Assert.AreEqual(200, sm.GetScore());
            Object.DestroyImmediate(sm.gameObject);
        }

        [Test]
        public void Multiplier_ResetsOnPlayerDamage()
        {
            var sm = CreateManager();
            for (int i = 0; i < 10; i++) sm.RegisterKill(); // multiplier 3
            Assert.AreEqual(3, sm.Multiplier);
            sm.OnPlayerDamaged();
            Assert.AreEqual(1, sm.Multiplier);
            Object.DestroyImmediate(sm.gameObject);
        }

        [Test]
        public void OnScoreChanged_FiresOnAdd()
        {
            var sm = CreateManager();
            int reported = -1;
            sm.OnScoreChanged += s => reported = s;
            sm.AddScore(42);
            Assert.AreEqual(42, reported);
            Object.DestroyImmediate(sm.gameObject);
        }
    }
}
