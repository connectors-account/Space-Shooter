using SpaceShooter.Core;
using SpaceShooter.Environment;
using SpaceShooter.Managers;
using SpaceShooter.Player;
using SpaceShooter.PowerUps;
using SpaceShooter.UI;
using SpaceShooter.Weapons;
using UnityEngine;

namespace SpaceShooter.Bootstrap
{
    /// <summary>
    /// Single entry point for the GamePlay scene. On <see cref="Start"/> it constructs the entire
    /// gameplay world — camera, managers, pools, player, background and UI — wires every system
    /// together and starts the first wave. This removes the need for fragile manual scene wiring and
    /// guarantees the scene is always assembled in a valid, deterministic order.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField]
        private GameConfig _config = new GameConfig();

        private void Start()
        {
            // Guarantee a valid config even if scene serialization stripped the inspector values.
            if (_config == null || _config.PlayerMaxHealth <= 0)
            {
                _config = new GameConfig();
            }

            ConfigureCamera();

            // Root container for all spawned managers/pools (kept tidy in the hierarchy).
            var systems = new GameObject("Systems").transform;

            // 1. Foundational, dependency-free managers.
            AudioManager audio = systems.gameObject.AddComponent<AudioManager>();
            audio.Initialize();

            ExplosionManager explosions = CreateChild<ExplosionManager>(systems, "ExplosionManager");
            explosions.Initialize();

            BulletManager bullets = CreateChild<BulletManager>(systems, "BulletManager");
            bullets.Initialize(_config);

            PowerUpManager powerUps = CreateChild<PowerUpManager>(systems, "PowerUpManager");
            powerUps.Initialize(_config);

            // 2. Background.
            var bgGo = new GameObject("Background");
            bgGo.AddComponent<BackgroundScroller>().Initialize(_config);

            // 3. Player.
            var playerGo = new GameObject("Player");
            PlayerController player = playerGo.AddComponent<PlayerController>();
            player.Initialize(_config);

            // 4. Spawn manager (needs the player transform for aimed fire).
            SpawnManager spawner = CreateChild<SpawnManager>(systems, "SpawnManager");
            spawner.Initialize(_config, player.transform);

            // 5. Game manager (coordinates player + spawner).
            GameManager game = CreateChild<GameManager>(systems, "GameManager");
            game.Initialize(_config, player, spawner);

            // 6. UI (subscribes to game + player events).
            var uiGo = new GameObject("UIManager");
            uiGo.AddComponent<UIManager>().Initialize(game, player);

            // 7. Go!
            game.StartGame();
        }

        private void ConfigureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = _config.HalfHeight;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.06f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        private static T CreateChild<T>(Transform parent, string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
        }
    }
}
