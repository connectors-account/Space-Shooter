using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SpaceShooter;

namespace SpaceShooter.Tests
{
    /// <summary>
    /// TDD tests for <see cref="BulletPool"/>.
    /// </summary>
    public class BulletPoolTests
    {
        /// <summary>
        /// Builds a fully wired BulletPool with in-memory prefabs so tests (and the
        /// PlayerShooter tests) do not depend on project asset prefabs.
        /// </summary>
        public static BulletPool CreateTestPool(int size = 30)
        {
            var playerPrefab = new GameObject("PlayerBulletPrefab");
            var pRb = playerPrefab.AddComponent<Rigidbody2D>();
            pRb.gravityScale = 0f;
            playerPrefab.AddComponent<BoxCollider2D>().isTrigger = true;
            var pb = playerPrefab.AddComponent<PlayerBullet>();
            playerPrefab.SetActive(false);

            var enemyPrefab = new GameObject("EnemyBulletPrefab");
            var eRb = enemyPrefab.AddComponent<Rigidbody2D>();
            eRb.gravityScale = 0f;
            enemyPrefab.AddComponent<BoxCollider2D>().isTrigger = true;
            var eb = enemyPrefab.AddComponent<EnemyBullet>();
            enemyPrefab.SetActive(false);

            var go = new GameObject("BulletPool");
            var pool = go.AddComponent<BulletPool>();
            pool.playerBulletPrefab = pb;
            pool.enemyBulletPrefab = eb;
            pool.initialPoolSize = size;
            pool.InitializePools();
            return pool;
        }

        [Test]
        public void Pool_InitializesWithCorrectCount()
        {
            var pool = CreateTestPool(30);
            Assert.AreEqual(30, pool.GetInactiveCount(BulletType.Player));
            Assert.AreEqual(30, pool.GetInactiveCount(BulletType.Enemy));
            Object.DestroyImmediate(pool.gameObject);
        }

        [Test]
        public void GetBullet_ReturnsActivatedBullet()
        {
            var pool = CreateTestPool(30);
            var bullet = pool.GetBullet(BulletType.Player, Vector3.zero, Vector2.up);
            Assert.IsNotNull(bullet);
            Assert.IsTrue(bullet.gameObject.activeSelf);
            Assert.AreEqual(29, pool.GetInactiveCount(BulletType.Player));
            Object.DestroyImmediate(pool.gameObject);
        }

        [Test]
        public void ReturnBullet_DeactivatesBullet()
        {
            var pool = CreateTestPool(30);
            var bullet = pool.GetBullet(BulletType.Player, Vector3.zero, Vector2.up);
            pool.ReturnBullet(bullet);
            Assert.IsFalse(bullet.gameObject.activeSelf);
            Assert.AreEqual(30, pool.GetInactiveCount(BulletType.Player));
            Object.DestroyImmediate(pool.gameObject);
        }

        [Test]
        public void GetBullet_PositionsBulletCorrectly()
        {
            var pool = CreateTestPool(30);
            var pos = new Vector3(3f, 4f, 0f);
            var bullet = pool.GetBullet(BulletType.Player, pos, Vector2.up);
            Assert.AreEqual(pos, bullet.transform.position);
            Object.DestroyImmediate(pool.gameObject);
        }

        [Test]
        public void Pool_ExpandsWhenEmpty()
        {
            var pool = CreateTestPool(2);
            var b1 = pool.GetBullet(BulletType.Player, Vector3.zero, Vector2.up);
            var b2 = pool.GetBullet(BulletType.Player, Vector3.zero, Vector2.up);
            Assert.AreEqual(0, pool.GetInactiveCount(BulletType.Player));
            // Pool is empty; requesting again must expand it, not return null.
            var b3 = pool.GetBullet(BulletType.Player, Vector3.zero, Vector2.up);
            Assert.IsNotNull(b3);
            Assert.IsTrue(b3.gameObject.activeSelf);
            Assert.AreEqual(3, pool.GetActiveCount(BulletType.Player));
            Object.DestroyImmediate(pool.gameObject);
        }
    }
}
