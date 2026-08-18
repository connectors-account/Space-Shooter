using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SpaceShooter;

namespace SpaceShooter.Tests
{
    /// <summary>
    /// TDD tests for <see cref="EnemyHealth"/>.
    /// </summary>
    public class EnemyHealthTests
    {
        private EnemyHealth CreateHealth(int max = 3, int score = 100)
        {
            var go = new GameObject("Enemy");
            var eh = go.AddComponent<EnemyHealth>();
            eh.Configure(max, score);
            return eh;
        }

        [Test]
        public void Enemy_StartsWithCorrectHealth()
        {
            var eh = CreateHealth(3);
            Assert.AreEqual(3, eh.CurrentHealth);
            Assert.AreEqual(3, eh.maxHealth);
            Object.DestroyImmediate(eh.gameObject);
        }

        [Test]
        public void TakeDamage_ReducesHealth()
        {
            var eh = CreateHealth(3);
            eh.TakeDamage(1);
            Assert.AreEqual(2, eh.CurrentHealth);
            Object.DestroyImmediate(eh.gameObject);
        }

        [Test]
        public void Health_CannotGoBelowZero()
        {
            var eh = CreateHealth(3);
            eh.TakeDamage(10);
            Assert.AreEqual(0, eh.CurrentHealth);
            Object.DestroyImmediate(eh.gameObject);
        }

        [Test]
        public void Death_FiresOnDeathEvent()
        {
            var eh = CreateHealth(1);
            bool died = false;
            eh.OnDeath += () => died = true;
            eh.TakeDamage(1);
            Assert.IsTrue(died);
            Object.DestroyImmediate(eh.gameObject);
        }

        [UnityTest]
        public IEnumerator Death_DestroysGameObject()
        {
            var eh = CreateHealth(1);
            var go = eh.gameObject;
            eh.TakeDamage(1);
            // Destroy happens end of frame.
            yield return null;
            Assert.IsTrue(go == null);
        }

        [UnityTest]
        public IEnumerator Death_AwardsScore()
        {
            var sm = new GameObject("ScoreManager").AddComponent<ScoreManager>();
            sm.Initialize();
            sm.ResetScore();
            yield return null; // ensure singleton Instance assigned

            var eh = CreateHealth(1, 250);
            eh.TakeDamage(1);
            Assert.AreEqual(250, ScoreManager.Instance.GetScore());
            yield return null;
            Object.DestroyImmediate(sm.gameObject);
        }
    }
}
