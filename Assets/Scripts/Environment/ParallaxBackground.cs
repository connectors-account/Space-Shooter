using System.Collections.Generic;
using SpaceShooter.Utils;
using UnityEngine;

namespace SpaceShooter.Environment
{
    public class ParallaxBackground : MonoBehaviour
    {
        private readonly List<Transform> _farStars = new List<Transform>();
        private readonly List<Transform> _midStars = new List<Transform>();

        private const int FarCount = 45;
        private const int MidCount = 25;

        private void Start()
        {
            BuildLayer(_farStars, FarCount, 0.8f, new Color(0.55f, 0.65f, 0.85f, 0.8f));
            BuildLayer(_midStars, MidCount, 1.6f, new Color(0.8f, 0.85f, 1f, 0.95f));
        }

        private void Update()
        {
            ScrollLayer(_farStars, 0.8f);
            ScrollLayer(_midStars, 1.6f);
        }

        private void BuildLayer(List<Transform> targetList, int count, float scale, Color color)
        {
            for (var i = 0; i < count; i++)
            {
                var star = new GameObject("Star").transform;
                star.SetParent(transform);
                star.position = new Vector3(Random.Range(-9f, 9f), Random.Range(-5f, 5f), 0f);
                star.localScale = Vector3.one * scale;

                var renderer = star.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = SpriteFactory.GetSprite(color, ShapeType.Square, 4);
                renderer.sortingOrder = -100;

                targetList.Add(star);
            }
        }

        private static void ScrollLayer(List<Transform> layer, float speed)
        {
            foreach (var star in layer)
            {
                star.position += Vector3.down * (speed * Time.deltaTime);
                if (star.position.y < -5.5f)
                {
                    star.position = new Vector3(Random.Range(-9f, 9f), 5.6f, 0f);
                }
            }
        }
    }
}
