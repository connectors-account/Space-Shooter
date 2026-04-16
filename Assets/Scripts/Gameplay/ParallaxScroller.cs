using UnityEngine;

namespace SpaceShooter.Gameplay
{
    public class ParallaxScroller : MonoBehaviour
    {
        private Transform _farA;
        private Transform _farB;
        private Transform _nearA;
        private Transform _nearB;

        private float _tileHeight = 12f;

        public void Setup(Sprite farSprite, Sprite nearSprite)
        {
            _farA = CreateTile("Far_A", farSprite, new Vector3(0f, 0f, 10f), new Color(0.08f, 0.08f, 0.2f, 1f));
            _farB = CreateTile("Far_B", farSprite, new Vector3(0f, _tileHeight, 10f), new Color(0.08f, 0.08f, 0.2f, 1f));
            _nearA = CreateTile("Near_A", nearSprite, new Vector3(0f, 0f, 9f), new Color(0.12f, 0.12f, 0.3f, 0.9f));
            _nearB = CreateTile("Near_B", nearSprite, new Vector3(0f, _tileHeight, 9f), new Color(0.12f, 0.12f, 0.3f, 0.9f));
        }

        private Transform CreateTile(string tileName, Sprite sprite, Vector3 position, Color tint)
        {
            var go = new GameObject(tileName);
            go.transform.SetParent(transform, false);
            go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = tint;
            renderer.sortingOrder = -20;
            go.transform.localScale = new Vector3(19f, 12f, 1f);
            return go.transform;
        }

        private void Update()
        {
            ScrollPair(_farA, _farB, 0.7f);
            ScrollPair(_nearA, _nearB, 1.4f);
        }

        private void ScrollPair(Transform a, Transform b, float speed)
        {
            if (a == null || b == null)
            {
                return;
            }

            a.position += Vector3.down * (speed * Time.deltaTime);
            b.position += Vector3.down * (speed * Time.deltaTime);

            if (a.position.y <= -_tileHeight)
            {
                a.position = b.position + Vector3.up * _tileHeight;
            }

            if (b.position.y <= -_tileHeight)
            {
                b.position = a.position + Vector3.up * _tileHeight;
            }
        }
    }
}
