using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.Environment
{
    /// <summary>
    /// Procedurally scatters ~200 tiny star quads across the screen, each scrolling
    /// downward at a random parallax speed and wrapping to the top when off-screen.
    /// </summary>
    public class StarField : MonoBehaviour
    {
        #region Fields
        [SerializeField] private int _starCount = 200;
        [SerializeField] private float _minScale = 0.02f;
        [SerializeField] private float _maxScale = 0.08f;
        [SerializeField] private float _minSpeed = 0.3f;
        [SerializeField] private float _maxSpeed = 2.5f;
        [SerializeField] private int _sortingOrder = -50;

        private readonly List<Transform> _stars = new List<Transform>();
        private readonly List<float> _speeds = new List<float>();
        private Sprite _starSprite;

        private static readonly Color[] StarColors =
        {
            Color.white,
            new Color(0.7f, 0.8f, 1f),   // blue-ish
            new Color(1f, 0.95f, 0.7f),  // yellow-ish
        };
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _starSprite = SpriteGenerator.GenerateStar();
            CreateStars();
        }

        private void Update()
        {
            float top = GameConstants.CAMERA_TOP + 0.5f;
            float bottom = GameConstants.CAMERA_BOTTOM - 0.5f;
            float range = top - bottom;

            for (int i = 0; i < _stars.Count; i++)
            {
                Transform star = _stars[i];
                if (star == null) continue;
                Vector3 pos = star.position;
                pos.y -= _speeds[i] * Time.deltaTime;
                if (pos.y < bottom)
                {
                    pos.y += range;
                    pos.x = Random.Range(GameConstants.CAMERA_LEFT, GameConstants.CAMERA_RIGHT);
                }
                star.position = pos;
            }
        }
        #endregion

        #region Creation
        private void CreateStars()
        {
            for (int i = 0; i < _starCount; i++)
            {
                GameObject go = new GameObject($"Star_{i}");
                go.transform.SetParent(transform, false);

                float x = Random.Range(GameConstants.CAMERA_LEFT, GameConstants.CAMERA_RIGHT);
                float y = Random.Range(GameConstants.CAMERA_BOTTOM, GameConstants.CAMERA_TOP);
                go.transform.position = new Vector3(x, y, 10f);

                float scale = Random.Range(_minScale, _maxScale);
                go.transform.localScale = Vector3.one * scale;

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _starSprite;
                sr.color = StarColors[Random.Range(0, StarColors.Length)];
                sr.sortingOrder = _sortingOrder;

                _stars.Add(go.transform);
                // Larger stars scroll faster (closer parallax).
                float speed = Mathf.Lerp(_minSpeed, _maxSpeed, Mathf.InverseLerp(_minScale, _maxScale, scale));
                _speeds.Add(speed);
            }
        }
        #endregion
    }
}
