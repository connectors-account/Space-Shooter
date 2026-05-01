using SpaceShooter.Core;
using SpaceShooter.Projectiles;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SpaceShooter.Player
{
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerController : MonoBehaviour
    {
        private GameManager _gameManager;
        private ObjectPoolManager _pool;
        private GameConfig _config;
        private PlayerHealth _health;

        private int _weaponLevel;
        private float _nextShotTime;

        public void Initialize(GameManager gameManager, ObjectPoolManager pool, GameConfig config)
        {
            _gameManager = gameManager;
            _pool = pool;
            _config = config;
            _health = GetComponent<PlayerHealth>();
            _health.Initialize(gameManager, config.PlayerMaxHealth);
            _weaponLevel = 1;
            _nextShotTime = 0f;
        }

        public void UpgradeWeapon()
        {
            _weaponLevel = Mathf.Min(_weaponLevel + 1, _config.MaxWeaponLevel);
            Sound.SoundManager.Instance?.PlaySfx("powerup");
        }

        private void Update()
        {
            if (_gameManager.CurrentState != GameState.Playing) return;

            HandleMovement();
            HandleShooting();
        }

        private void HandleMovement()
        {
            var input = GetMoveInput();
            var position = transform.position + (Vector3)(input * (_config.PlayerMoveSpeed * Time.deltaTime));

            position.x = Mathf.Clamp(position.x, -_config.PlayAreaHalfWidth + 0.8f, _config.PlayAreaHalfWidth - 0.8f);
            position.y = Mathf.Clamp(position.y, -_config.PlayAreaHalfHeight + 0.8f, _config.PlayAreaHalfHeight - 0.8f);
            transform.position = position;
        }

        private void HandleShooting()
        {
            if (!IsFirePressed() || Time.time < _nextShotTime) return;
            _nextShotTime = Time.time + (_config.PlayerFireInterval * Mathf.Lerp(1f, 0.55f, (_weaponLevel - 1f) / (_config.MaxWeaponLevel - 1f)));

            SpawnPlayerBullet(Vector2.up, Vector3.zero);

            if (_weaponLevel >= 2)
            {
                SpawnPlayerBullet((Vector2.up + Vector2.left * 0.18f).normalized, Vector3.left * 0.25f);
            }

            if (_weaponLevel >= 3)
            {
                SpawnPlayerBullet((Vector2.up + Vector2.right * 0.18f).normalized, Vector3.right * 0.25f);
            }

            if (_weaponLevel >= 4)
            {
                SpawnPlayerBullet(Vector2.up, Vector3.left * 0.5f);
                SpawnPlayerBullet(Vector2.up, Vector3.right * 0.5f);
            }

            Sound.SoundManager.Instance?.PlaySfx("player_shoot");
        }

        private void SpawnPlayerBullet(Vector2 direction, Vector3 offset)
        {
            var bullet = _pool.Get("bullet_player", transform.position + offset + Vector3.up * 0.7f, Quaternion.identity);
            if (bullet == null) return;
            bullet.GetComponent<Projectile>().Initialize(_pool, Faction.Player, direction, _config.PlayerBulletSpeed, _config.PlayerBulletDamage);
        }

        private static Vector2 GetMoveInput()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                var x = (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed ? -1 : 0) +
                        (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed ? 1 : 0);
                var y = (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed ? -1 : 0) +
                        (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed ? 1 : 0);
                return new Vector2(x, y).normalized;
            }
#endif
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        }

        private static bool IsFirePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && (Keyboard.current.spaceKey.isPressed || Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);
#else
            return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#endif
        }
    }
}
