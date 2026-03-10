using UnityEngine;
using SpaceShooter.Player;
using SpaceShooter.UI;

namespace SpaceShooter.Managers
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private GameHUD gameHUD;

        [Header("Managers")]
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private EffectsManager effectsManager;

        private PlayerController playerInstance;

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            SpawnPlayer();
            AudioManager.Instance?.PlayGameMusic();
        }

        private void SpawnPlayer()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("Player prefab not assigned!");
                return;
            }

            Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : new Vector3(0, -3f, 0);
            GameObject playerObj = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            playerInstance = playerObj.GetComponent<PlayerController>();

            if (gameHUD != null && playerInstance != null)
            {
                gameHUD.SetupPlayerEvents(playerInstance);
            }
        }

        public void RestartLevel()
        {
            if (waveManager != null)
            {
                waveManager.ResetWaves();
            }

            ClearEnemies();
            ClearBullets();
            ClearPowerUps();

            SpawnPlayer();
        }

        private void ClearEnemies()
        {
            var enemies = FindObjectsOfType<Enemy.EnemyBase>();
            foreach (var enemy in enemies)
            {
                Destroy(enemy.gameObject);
            }
        }

        private void ClearBullets()
        {
            var bullets = FindObjectsOfType<Combat.Bullet>();
            foreach (var bullet in bullets)
            {
                Destroy(bullet.gameObject);
            }
        }

        private void ClearPowerUps()
        {
            var powerUps = FindObjectsOfType<PowerUps.PowerUpBase>();
            foreach (var powerUp in powerUps)
            {
                Destroy(powerUp.gameObject);
            }
        }
    }
}
