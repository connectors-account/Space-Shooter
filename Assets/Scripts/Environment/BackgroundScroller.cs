using System.Collections.Generic;
using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Environment
{
    /// <summary>
    /// Creates and scrolls a three-layer parallax star-field background. Each layer scrolls at a
    /// different speed to create a sense of depth, and stars wrap from the bottom to the top so the
    /// field is endless. Everything is generated procedurally so no art assets are required.
    /// </summary>
    public class BackgroundScroller : MonoBehaviour
    {
        private GameConfig _config;

        private class Layer
        {
            public Transform Root;
            public float Speed;
            public readonly List<Transform> Stars = new List<Transform>();
        }

        private readonly List<Layer> _layers = new List<Layer>();
        private float _topY;
        private float _bottomY;

        /// <summary>
        /// Builds the parallax layers. Called once by the bootstrap.
        /// </summary>
        /// <param name="config">Shared configuration (defines world bounds).</param>
        public void Initialize(GameConfig config)
        {
            _config = config;
            _topY = config.HalfHeight + 1f;
            _bottomY = -config.HalfHeight - 1f;

            // Layer definitions: (speed, starCount, size, brightness).
            CreateLayer(0.6f, 30, 0.05f, 0.35f, -20);
            CreateLayer(1.4f, 22, 0.09f, 0.6f, -19);
            CreateLayer(2.6f, 14, 0.14f, 0.95f, -18);
        }

        private void CreateLayer(float speed, int count, float size, float brightness, int sortingOrder)
        {
            var layerRoot = new GameObject($"BG_Layer_{speed}").transform;
            layerRoot.SetParent(transform, false);

            var layer = new Layer { Root = layerRoot, Speed = speed };
            Color color = new Color(brightness, brightness, Mathf.Min(1f, brightness + 0.15f));
            Sprite star = SpriteFactory.CreateStarSprite(Color.white, 8);

            for (int i = 0; i < count; i++)
            {
                var starGo = new GameObject("Star");
                starGo.transform.SetParent(layerRoot, false);
                starGo.transform.position = RandomPosition();
                starGo.transform.localScale = Vector3.one * (size / 0.08f);

                var sr = starGo.AddComponent<SpriteRenderer>();
                sr.sprite = star;
                sr.color = color;
                sr.sortingOrder = sortingOrder;

                layer.Stars.Add(starGo.transform);
            }
            _layers.Add(layer);
        }

        private Vector3 RandomPosition()
        {
            float x = Random.Range(-_config.HalfWidth - 1f, _config.HalfWidth + 1f);
            float y = Random.Range(_bottomY, _topY);
            return new Vector3(x, y, 10f);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            float height = _topY - _bottomY;

            foreach (Layer layer in _layers)
            {
                float delta = layer.Speed * dt;
                foreach (Transform star in layer.Stars)
                {
                    Vector3 p = star.position;
                    p.y -= delta;
                    if (p.y < _bottomY)
                    {
                        // Wrap to the top, randomising X for variety.
                        p.y += height;
                        p.x = Random.Range(-_config.HalfWidth - 1f, _config.HalfWidth + 1f);
                    }
                    star.position = p;
                }
            }
        }
    }
}
