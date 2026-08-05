using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;
using SpaceShooter.Audio;
using SpaceShooter.Utilities;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Screen-clearing bomb. Attach to the player. Holds a stock of bombs;
    /// when detonated it flashes the screen white, destroys every active enemy
    /// on screen (awarding score for each) and clears all enemy bullets.
    ///
    /// Reads the bomb button from the player's input handler.
    /// </summary>
    public class Bomb : MonoBehaviour
    {
        [SerializeField] private int startingBombs = 1;
        [SerializeField] private int maxBombs = 5;
        [SerializeField] private float flashDuration = 0.35f;

        public int BombCount { get; private set; }

        public System.Action<int> OnBombCountChanged;

        private Player.PlayerInputHandler _input;
        private Image _flashImage;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _input = GetComponent<Player.PlayerInputHandler>();
            BombCount = startingBombs;
            BuildFlashOverlay();
        }

        private void Start()
        {
            OnBombCountChanged?.Invoke(BombCount);
        }

        private void BuildFlashOverlay()
        {
            var canvasGo = new GameObject("BombFlashCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;
            canvasGo.AddComponent<CanvasScaler>();

            var imgGo = new GameObject("Flash");
            imgGo.transform.SetParent(canvasGo.transform, false);
            _flashImage = imgGo.AddComponent<Image>();
            _flashImage.color = new Color(1f, 1f, 1f, 0f);
            _flashImage.raycastTarget = false;
            var rt = _flashImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            DontDestroyOnLoad(canvasGo);
            _flashCanvas = canvasGo;
        }

        private GameObject _flashCanvas;

        private void OnDestroy()
        {
            if (_flashCanvas != null) Destroy(_flashCanvas);
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
                return;

            if (_input != null && _input.BombPressed)
                Detonate();
        }

        public void AddBomb(int amount = 1)
        {
            BombCount = Mathf.Min(maxBombs, BombCount + amount);
            OnBombCountChanged?.Invoke(BombCount);
        }

        /// <summary>Use a bomb if any are available.</summary>
        public bool Detonate()
        {
            if (BombCount <= 0) return false;

            BombCount--;
            OnBombCountChanged?.Invoke(BombCount);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(Constants.SfxBomb);
            CameraShake.ShakeStatic(0.5f, 0.5f);
            if (_flashImage != null)
            {
                if (_flashRoutine != null) StopCoroutine(_flashRoutine);
                _flashRoutine = StartCoroutine(FlashRoutine());
            }

            ClearEnemies();
            ClearEnemyBullets();
            return true;
        }

        private void ClearEnemies()
        {
            var enemies = GameObject.FindObjectsOfType<Enemy.EnemyBase>();
            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy && !enemy.IsBoss)
                    enemy.KillByBomb();
            }
        }

        private void ClearEnemyBullets()
        {
            var bullets = GameObject.FindGameObjectsWithTag(Constants.TagEnemyBullet);
            foreach (var b in bullets)
            {
                if (b != null && b.activeInHierarchy && ObjectPool.Instance != null)
                    ObjectPool.Instance.Release(Constants.PoolEnemyBullet, b);
            }
        }

        private IEnumerator FlashRoutine()
        {
            float half = flashDuration * 0.5f;
            float t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                _flashImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 0.85f, t / half));
                yield return null;
            }
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                _flashImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.85f, 0f, t / half));
                yield return null;
            }
            _flashImage.color = new Color(1f, 1f, 1f, 0f);
            _flashRoutine = null;
        }
    }
}
