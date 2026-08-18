using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SpaceShooter;

namespace SpaceShooter.Tests
{
    /// <summary>
    /// TDD tests for <see cref="PlayerShooter"/>.
    /// Shot geometry is validated through the pure <c>GetShotDirections</c> method,
    /// while spawning is validated through <c>Fire</c> using a live bullet pool.
    /// </summary>
    public class PlayerShooterTests
    {
        private PlayerShooter CreateShooter(out BulletPool pool)
        {
            pool = BulletPoolTests.CreateTestPool();

            var go = new GameObject("Player");
            var shooter = go.AddComponent<PlayerShooter>();
            shooter.fireRate = 0.2f;
            shooter.fireMode = FireMode.Single;
            go.transform.position = Vector3.zero;
            return shooter;
        }

        [Test]
        public void SingleMode_ReturnsOneDirectionStraightUp()
        {
            var go = new GameObject("Player");
            var shooter = go.AddComponent<PlayerShooter>();
            Vector2[] dirs = shooter.GetShotDirections(FireMode.Single);
            Assert.AreEqual(1, dirs.Length);
            Assert.AreEqual(0f, dirs[0].x, 0.0001f);
            Assert.AreEqual(1f, dirs[0].y, 0.0001f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void TripleMode_ReturnsThreeDirectionsAtCorrectAngles()
        {
            var go = new GameObject("Player");
            var shooter = go.AddComponent<PlayerShooter>();
            Vector2[] dirs = shooter.GetShotDirections(FireMode.Triple);
            Assert.AreEqual(3, dirs.Length);
            // Angles measured from the "up" vector: -15, 0, +15 degrees.
            Assert.AreEqual(-15f, SignedAngleFromUp(dirs[0]), 0.01f);
            Assert.AreEqual(0f, SignedAngleFromUp(dirs[1]), 0.01f);
            Assert.AreEqual(15f, SignedAngleFromUp(dirs[2]), 0.01f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Spread5Mode_ReturnsFiveDirections()
        {
            var go = new GameObject("Player");
            var shooter = go.AddComponent<PlayerShooter>();
            Vector2[] dirs = shooter.GetShotDirections(FireMode.Spread5);
            Assert.AreEqual(5, dirs.Length);
            Assert.AreEqual(-30f, SignedAngleFromUp(dirs[0]), 0.01f);
            Assert.AreEqual(30f, SignedAngleFromUp(dirs[4]), 0.01f);
            Object.DestroyImmediate(go);
        }

        [UnityTest]
        public IEnumerator Fire_Single_SpawnsOneBullet()
        {
            var shooter = CreateShooter(out var pool);
            yield return null;
            var bullets = shooter.Fire();
            Assert.AreEqual(1, bullets.Count);
            Object.DestroyImmediate(shooter.gameObject);
            Object.DestroyImmediate(pool.gameObject);
        }

        [UnityTest]
        public IEnumerator Fire_Triple_SpawnsThreeBullets()
        {
            var shooter = CreateShooter(out var pool);
            shooter.fireMode = FireMode.Triple;
            yield return null;
            var bullets = shooter.Fire();
            Assert.AreEqual(3, bullets.Count);
            Object.DestroyImmediate(shooter.gameObject);
            Object.DestroyImmediate(pool.gameObject);
        }

        [UnityTest]
        public IEnumerator Fire_SpawnsBulletAtShooterPosition()
        {
            var shooter = CreateShooter(out var pool);
            shooter.transform.position = new Vector3(2f, 3f, 0f);
            yield return null;
            var bullets = shooter.Fire();
            Assert.AreEqual(2f, bullets[0].transform.position.x, 0.5f);
            Assert.GreaterOrEqual(bullets[0].transform.position.y, 3f);
            Object.DestroyImmediate(shooter.gameObject);
            Object.DestroyImmediate(pool.gameObject);
        }

        [UnityTest]
        public IEnumerator FireRate_CooldownPreventsRapidFire()
        {
            var shooter = CreateShooter(out var pool);
            shooter.fireRate = 0.3f;
            yield return null;
            var first = shooter.Fire();
            Assert.AreEqual(1, first.Count);
            var second = shooter.Fire(); // immediate, must be blocked
            Assert.AreEqual(0, second.Count);
            yield return new WaitForSeconds(0.4f);
            var third = shooter.Fire(); // cooldown elapsed
            Assert.AreEqual(1, third.Count);
            Object.DestroyImmediate(shooter.gameObject);
            Object.DestroyImmediate(pool.gameObject);
        }

        private static float SignedAngleFromUp(Vector2 dir)
        {
            return Vector2.SignedAngle(Vector2.up, dir);
        }
    }
}
