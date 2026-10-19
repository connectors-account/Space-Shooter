using UnityEngine;

namespace MinimalSpaceShooter
{
    public sealed class PlayerShip : MonoBehaviour
    {
        private const float Speed = 6f;
        private const float HalfSize = 0.4f;
        private const float ShotInterval = 0.18f;
        private SpaceShooter game;
        private Rigidbody2D body;
        private Vector2 movement;
        private float shotCooldown;

        public void Initialize(SpaceShooter owner)
        {
            game = owner;
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (!game.Running) return;
            float x = 0f;
            float y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x--;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x++;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y--;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y++;
            movement = Vector2.ClampMagnitude(new Vector2(x, y), 1f);

            shotCooldown -= Time.deltaTime;
            if (Input.GetKey(KeyCode.Space) && shotCooldown <= 0f)
            {
                game.Shoot(body.position + Vector2.up * 0.7f);
                shotCooldown = ShotInterval;
            }
        }

        private void FixedUpdate()
        {
            if (!game.Running) return;
            Vector2 next = body.position + movement * Speed * Time.fixedDeltaTime;
            next.x = Mathf.Clamp(next.x, -SpaceShooter.HalfWidth + HalfSize,
                SpaceShooter.HalfWidth - HalfSize);
            next.y = Mathf.Clamp(next.y, -SpaceShooter.HalfHeight + HalfSize,
                SpaceShooter.HalfHeight - HalfSize);
            body.MovePosition(next);
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) movement = Vector2.zero;
        }
    }
}
