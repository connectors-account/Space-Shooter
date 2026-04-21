using System.Collections;
using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Player
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float respawnDelay = 1.5f;

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLivesChanged += HandleLivesChanged;
            }
        }

        private void Start()
        {
            if (FindObjectOfType<PlayerController>() == null)
            {
                SpawnPlayer();
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLivesChanged -= HandleLivesChanged;
            }
        }

        private void HandleLivesChanged(int lives)
        {
            if (lives > 0)
            {
                StartCoroutine(SpawnAfterDelay());
            }
        }

        private IEnumerator SpawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnDelay);

            if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.GameOver)
            {
                yield break;
            }

            if (FindObjectOfType<PlayerController>() == null)
            {
                SpawnPlayer();
            }
        }

        private void SpawnPlayer()
        {
            if (playerPrefab == null)
            {
                return;
            }

            Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Instantiate(playerPrefab, position, Quaternion.identity);
        }
    }
}
