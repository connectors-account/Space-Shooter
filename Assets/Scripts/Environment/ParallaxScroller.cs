using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Environment
{
    public class ParallaxScroller : MonoBehaviour
    {
        private Transform farA;
        private Transform farB;
        private Transform nearA;
        private Transform nearB;

        private const float TileHeight = 20f;

        public void Initialize()
        {
            Sprite farSprite = SpriteFactory.CreateStarTile();
            Sprite nearSprite = SpriteFactory.CreateStarTile();

            farA = CreateLayer("Far_A", farSprite, new Color(1f, 1f, 1f, 0.6f), -20, new Vector3(0f, 0f, 0f));
            farB = CreateLayer("Far_B", farSprite, new Color(1f, 1f, 1f, 0.6f), -20, new Vector3(0f, TileHeight, 0f));

            nearA = CreateLayer("Near_A", nearSprite, new Color(1f, 1f, 1f, 0.9f), -19, new Vector3(0f, 0f, 0f));
            nearB = CreateLayer("Near_B", nearSprite, new Color(1f, 1f, 1f, 0.9f), -19, new Vector3(0f, TileHeight, 0f));
        }

        private Transform CreateLayer(string name, Sprite sprite, Color tint, int sortingOrder, Vector3 localPosition)
        {
            GameObject layer = new GameObject(name);
            layer.transform.SetParent(transform, false);
            layer.transform.localPosition = localPosition;

            SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = tint;
            renderer.sortingOrder = sortingOrder;

            layer.transform.localScale = new Vector3(15f, 15f, 1f);
            return layer.transform;
        }

        private void Update()
        {
            ScrollPair(farA, farB, 0.8f);
            ScrollPair(nearA, nearB, 1.5f);
        }

        private void ScrollPair(Transform first, Transform second, float speed)
        {
            if (first == null || second == null)
            {
                return;
            }

            float delta = speed * Time.deltaTime;
            first.position += Vector3.down * delta;
            second.position += Vector3.down * delta;

            if (first.position.y <= -TileHeight)
            {
                first.position = new Vector3(first.position.x, second.position.y + TileHeight, first.position.z);
            }

            if (second.position.y <= -TileHeight)
            {
                second.position = new Vector3(second.position.x, first.position.y + TileHeight, second.position.z);
            }
        }
    }
}
