using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SpaceShooter;

namespace SpaceShooter.Tests
{
    /// <summary>
    /// TDD tests for <see cref="PlayerController"/>.
    /// Movement is validated through the deterministic <c>Move</c> method so the
    /// tests do not depend on live keyboard input.
    /// </summary>
    public class PlayerControllerTests
    {
        private PlayerController CreateController()
        {
            var go = new GameObject("Player");
            var pc = go.AddComponent<PlayerController>();
            pc.moveSpeed = 8f;
            // Explicit bounds so tests do not depend on Camera.main.
            pc.SetBounds(-8f, 8f, -4.5f, 4.5f);
            go.transform.position = Vector3.zero;
            return pc;
        }

        [Test]
        public void Move_Right_IncreasesX()
        {
            var pc = CreateController();
            pc.Move(Vector2.right, 0.5f);
            Assert.AreEqual(4f, pc.transform.position.x, 0.0001f); // 8 * 0.5
            Object.DestroyImmediate(pc.gameObject);
        }

        [Test]
        public void Move_Left_DecreasesX()
        {
            var pc = CreateController();
            pc.Move(Vector2.left, 0.25f);
            Assert.AreEqual(-2f, pc.transform.position.x, 0.0001f); // -8 * 0.25
            Object.DestroyImmediate(pc.gameObject);
        }

        [Test]
        public void Move_CannotExceedRightBound()
        {
            var pc = CreateController();
            pc.transform.position = new Vector3(8f, 0f, 0f);
            pc.Move(Vector2.right, 1f);
            Assert.AreEqual(8f, pc.transform.position.x, 0.0001f);
            Object.DestroyImmediate(pc.gameObject);
        }

        [Test]
        public void Move_CannotExceedLeftBound()
        {
            var pc = CreateController();
            pc.transform.position = new Vector3(-8f, 0f, 0f);
            pc.Move(Vector2.left, 1f);
            Assert.AreEqual(-8f, pc.transform.position.x, 0.0001f);
            Object.DestroyImmediate(pc.gameObject);
        }

        [Test]
        public void Move_SpeedIsAppliedCorrectly()
        {
            var pc = CreateController();
            pc.moveSpeed = 4f;
            pc.Move(Vector2.up, 1f);
            Assert.AreEqual(4f, pc.transform.position.y, 0.0001f);
            Object.DestroyImmediate(pc.gameObject);
        }

        [Test]
        public void Move_NoInput_StopsMovement()
        {
            var pc = CreateController();
            pc.transform.position = new Vector3(1f, 1f, 0f);
            pc.Move(Vector2.zero, 1f);
            Assert.AreEqual(1f, pc.transform.position.x, 0.0001f);
            Assert.AreEqual(1f, pc.transform.position.y, 0.0001f);
            Object.DestroyImmediate(pc.gameObject);
        }

        [Test]
        public void Move_WhenPaused_DoesNothing()
        {
            var pc = CreateController();
            pc.SetPaused(true);
            pc.Move(Vector2.right, 1f);
            Assert.AreEqual(0f, pc.transform.position.x, 0.0001f);
            Object.DestroyImmediate(pc.gameObject);
        }
    }
}
