using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Projectiles;
using SpaceShooter.Utilities;
using SpaceShooter.Core;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Boss enemy. Enters from the top, then slides side to side near the top of the screen.
    /// Phase 1 (>50% HP): radial 8-bullet bursts. Phase 2 (&lt;=50% HP): rotating spiral plus
    /// aimed shots. Health 500, Score 2000. Displays a world-space health bar above itself.
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        [Header("Boss")]
        public float sideSpeed = 2f;
        public float entryTargetY = 3f;
        public int radialCount = 8;
        public float phase1Interval = 1.2f;
        public float phase2SpiralInterval = 0.12f;
        public float phase2AimedInterval = 1.5f;
        public float bossBulletSpeed = 5f;
        public int bossBulletDamage = 15;

        [Header("Difficulty scaling")]
        public bool finalBoss = false;

        private bool _entering = true;
        private float _dir = 1f;
        private float _startX;
        private float _range = 3.5f;
        private float _nextRadial;
        private float _spiralAngle;
        private float _nextSpiral;
        private float _nextAimed;

        // Health bar (world-space canvas built at runtime)
        private Image _healthFill;
        private Canvas _barCanvas;

        protected override void Start()
        {
            maxHealth = finalBoss ? 900 : 500;
            scoreValue = finalBoss ? 4000 : 2000;
            speed = sideSpeed;
            contactDamage = 40;
            bulletSpeed = bossBulletSpeed;
            bulletDamage = bossBulletDamage;
            base.Start();

            _startX = 0f;
            BuildHealthBar();
            NotifyHudBossActive(true);
        }

        protected override void Update()
        {
            base.Update();
            UpdateHealthBar();
        }

        protected override Sprite CreateSprite()
        {
            return SpriteGenerator.CreateBoss(finalBoss
                ? new Color(0.8f, 0.2f, 0.3f)
                : new Color(0.6f, 0.3f, 0.8f));
        }

        protected override void Move()
        {
            if (_entering)
            {
                transform.position += Vector3.down * 2f * Time.deltaTime;
                if (transform.position.y <= entryTargetY)
                {
                    _entering = false;
                    _startX = transform.position.x;
                }
                return;
            }

            float x = transform.position.x + _dir * sideSpeed * Time.deltaTime;
            if (x > _startX + _range) { x = _startX + _range; _dir = -1f; }
            else if (x < _startX - _range) { x = _startX - _range; _dir = 1f; }
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }

        protected override void FirePattern()
        {
            if (_entering) return;

            bool phase2 = CurrentHealth <= maxHealth * 0.5f;

            if (!phase2)
            {
                // Phase 1: radial bursts.
                if (Time.time >= _nextRadial)
                {
                    _nextRadial = Time.time + phase1Interval;
                    BulletPattern.FireCircle(transform.position, radialCount, enemyBulletPrefab,
                        bossBulletSpeed, bossBulletDamage, false);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("boss_shoot");
                }
            }
            else
            {
                // Phase 2: continuous rotating spiral.
                if (Time.time >= _nextSpiral)
                {
                    _nextSpiral = Time.time + phase2SpiralInterval;
                    _spiralAngle = (_spiralAngle + 17f) % 360f;
                    BulletPattern.FireSpiral(transform.position, finalBoss ? 4 : 3, _spiralAngle,
                        enemyBulletPrefab, bossBulletSpeed, bossBulletDamage, false);
                }
                // Plus periodic aimed shots.
                if (Time.time >= _nextAimed)
                {
                    _nextAimed = Time.time + phase2AimedInterval;
                    Vector2 target = PlayerTransform != null
                        ? (Vector2)PlayerTransform.position
                        : (Vector2)transform.position + Vector2.down;
                    BulletPattern.FireAimed(transform.position, target, enemyBulletPrefab,
                        bossBulletSpeed + 2f, bossBulletDamage, false);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("boss_shoot");
                }
            }
        }

        protected override void Die()
        {
            NotifyHudBossActive(false);
            base.Die();
        }

        private void OnDestroy()
        {
            NotifyHudBossActive(false);
        }

        // --- Health bar ----------------------------------------------------

        private void BuildHealthBar()
        {
            var canvasGo = new GameObject("BossHealthBar");
            canvasGo.transform.SetParent(transform);
            canvasGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            _barCanvas = canvasGo.AddComponent<Canvas>();
            _barCanvas.renderMode = RenderMode.WorldSpace;
            var rt = _barCanvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200f, 20f);
            rt.localScale = Vector3.one * 0.01f;

            var whiteSprite = SpriteGenerator.CreateRect(4, 4, Color.white);

            var bg = new GameObject("BG");
            bg.transform.SetParent(canvasGo.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.sprite = whiteSprite;
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(canvasGo.transform, false);
            _healthFill = fill.AddComponent<Image>();
            _healthFill.sprite = whiteSprite;
            _healthFill.color = new Color(1f, 0.2f, 0.2f, 1f);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(1f, 1f);
            fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
            _healthFill.type = Image.Type.Filled;
            _healthFill.fillMethod = Image.FillMethod.Horizontal;
            _healthFill.fillAmount = 1f;
        }

        private void UpdateHealthBar()
        {
            if (_healthFill != null)
                _healthFill.fillAmount = Mathf.Clamp01(CurrentHealth / (float)maxHealth);
            // Keep bar upright regardless of any boss rotation.
            if (_barCanvas != null)
                _barCanvas.transform.rotation = Quaternion.identity;
        }

        private void NotifyHudBossActive(bool active)
        {
            var hud = FindObjectOfType<UI.HUDController>();
            if (hud != null)
            {
                if (active) hud.SetBossTracked(this);
                else hud.ClearBossTracked(this);
            }
        }
    }
}
