using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SpaceShooter;

namespace SpaceShooter.Tests
{
    /// <summary>
    /// TDD tests for <see cref="PlayerHealth"/>.
    /// </summary>
    public class PlayerHealthTests
    {
        private PlayerHealth CreateHealth()
        {
            var go = new GameObject("Player");
            var ph = go.AddComponent<PlayerHealth>();
            ph.invincibilityDuration = 0f; // disable i-frames for deterministic tests
            ph.ResetHealth();
            return ph;
        }

        [Test]
        public void Player_StartsWithMaxHealth()
        {
            var ph = CreateHealth();
            Assert.AreEqual(ph.maxHealth, ph.CurrentHealth);
            Assert.AreEqual(3, ph.maxHealth);
            Object.DestroyImmediate(ph.gameObject);
        }

        [Test]
        public void TakeDamage_ReducesHealth()
        {
            var ph = CreateHealth();
            ph.TakeDamage(1);
            Assert.AreEqual(2, ph.CurrentHealth);
            Object.DestroyImmediate(ph.gameObject);
        }

        [Test]
        public void Health_CannotGoBelowZero()
        {
            var ph = CreateHealth();
            ph.TakeDamage(100);
            Assert.AreEqual(0, ph.CurrentHealth);
            Object.DestroyImmediate(ph.gameObject);
        }

        [Test]
        public void DeathEvent_FiresAtZeroHealth()
        {
            var ph = CreateHealth();
            bool died = false;
            ph.OnDeath += () => died = true;
            ph.TakeDamage(ph.maxHealth);
            Assert.IsTrue(died);
            Object.DestroyImmediate(ph.gameObject);
        }

        [Test]
        public void Shield_AbsorbsDamage()
        {
            var ph = CreateHealth();
            ph.ActivateShield(2);
            Assert.IsTrue(ph.HasShield);
            ph.TakeDamage(1);
            Assert.AreEqual(3, ph.CurrentHealth, "Health should be untouched while shield holds");
            Assert.AreEqual(1, ph.ShieldHP);
            Object.DestroyImmediate(ph.gameObject);
        }

        [Test]
        public void Shield_DepletesAndOverflowsToHealth()
        {
            var ph = CreateHealth();
            ph.ActivateShield(2);
            ph.TakeDamage(3); // 2 absorbed by shield, 1 overflows to health
            Assert.IsFalse(ph.HasShield);
            Assert.AreEqual(0, ph.ShieldHP);
            Assert.AreEqual(2, ph.CurrentHealth);
            Object.DestroyImmediate(ph.gameObject);
        }

        [Test]
        public void Heal_RestoresHealthUpToMax()
        {
            var ph = CreateHealth();
            ph.TakeDamage(2);
            ph.Heal(1);
            Assert.AreEqual(2, ph.CurrentHealth);
            ph.Heal(10);
            Assert.AreEqual(ph.maxHealth, ph.CurrentHealth);
            Object.DestroyImmediate(ph.gameObject);
        }

        [UnityTest]
        public IEnumerator Invincibility_BlocksDamageDuringWindow()
        {
            var go = new GameObject("Player");
            var ph = go.AddComponent<PlayerHealth>();
            ph.invincibilityDuration = 0.3f;
            ph.ResetHealth();
            yield return null;
            ph.TakeDamage(1);
            Assert.AreEqual(2, ph.CurrentHealth);
            ph.TakeDamage(1); // should be ignored, still invincible
            Assert.AreEqual(2, ph.CurrentHealth);
            yield return new WaitForSeconds(0.4f);
            ph.TakeDamage(1); // i-frames elapsed, damage applies
            Assert.AreEqual(1, ph.CurrentHealth);
            Object.DestroyImmediate(go);
        }
    }
}
