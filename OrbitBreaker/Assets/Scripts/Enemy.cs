using UnityEngine;

namespace OrbitBreaker
{
    public enum EnemyPattern { Aimed, Fan, Radial }

    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class Enemy : MonoBehaviour
    {
        public EnemyPattern pattern;
        public int Points => pattern == EnemyPattern.Aimed ? 100 : pattern == EnemyPattern.Fan ? 160 : 220;
        public bool IsAlive => !dead;
        private GameController game;
        private Rigidbody2D body;
        private int health;
        private bool dead;
        private float age, originX, speed, nextShot, fireInterval, phase, bulletSpeed;

        private void Awake() { body = GetComponent<Rigidbody2D>(); }

        public void Initialize(GameController controller, int wave, float offset)
        {
            game = controller;
            game.Register(this);
            health = (pattern == EnemyPattern.Aimed ? 2 : 4) + Mathf.Min(3, (wave - 1) / 4);
            speed = Mathf.Min(2.6f, .85f + wave * .10f);
            bulletSpeed = Mathf.Min(5f, 2.5f + wave * .13f);
            fireInterval = Mathf.Max(.85f, 2f - wave * .06f);
            nextShot = 1.5f;
            originX = body.position.x;
            phase = offset;
        }

        private void FixedUpdate()
        {
            if (game == null || dead || game.Phase != GamePhase.Playing) { body.velocity = Vector2.zero; return; }
            age += Time.fixedDeltaTime;
            float amplitude = pattern == EnemyPattern.Aimed ? .35f : .8f;
            Vector2 next = new Vector2(originX + Mathf.Sin(age * 1.8f + phase) * amplitude,
                body.position.y - speed * Time.fixedDeltaTime);
            next.x = Mathf.Clamp(next.x, -7.8f, 7.8f);
            body.MovePosition(next);
            if (age >= nextShot && next.y < 4.1f && next.y > -2.8f)
            {
                nextShot = age + fireInterval;
                Shoot();
            }
            if (next.y < -5.7f)
            {
                if (game.Player != null) game.Player.Hit();
                Despawn();
            }
        }

        private void Shoot()
        {
            if (game.Player == null) return;
            Vector2 muzzle = body.position + Vector2.down * .5f;
            Vector2 aim = ((Vector2)game.Player.transform.position - muzzle).normalized;
            if (aim.sqrMagnitude < .01f) aim = Vector2.down;
            if (pattern == EnemyPattern.Aimed) game.Fire(muzzle, aim, false, bulletSpeed);
            else if (pattern == EnemyPattern.Fan)
            {
                for (int i = -1; i <= 1; i++)
                    game.Fire(muzzle, Rotate(aim, i * 20f), false, bulletSpeed);
            }
            else
            {
                for (int i = 0; i < 8; i++)
                    game.Fire(muzzle, Rotate(Vector2.down, i * 45f + age * 12f), false, bulletSpeed * .85f);
            }
            game.EnemyFireSound();
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad;
            return new Vector2(v.x * Mathf.Cos(r) - v.y * Mathf.Sin(r), v.x * Mathf.Sin(r) + v.y * Mathf.Cos(r));
        }

        public void Hit()
        {
            if (dead || game == null || game.Phase != GamePhase.Playing) return;
            health--;
            if (health > 0) return;
            game.EnemyKilled(this);
            Despawn();
        }

        private void Despawn()
        {
            if (dead) return;
            dead = true;
            GetComponent<Collider2D>().enabled = false;
            game.Unregister(this);
            Destroy(gameObject);
        }

        private void OnDestroy() { if (game != null) game.Unregister(this); }
    }
}
