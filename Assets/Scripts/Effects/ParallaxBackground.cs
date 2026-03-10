using UnityEngine;

namespace SpaceShooter.Effects
{
    public class ParallaxBackground : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public Transform transform;
            public float scrollSpeed = 1f;
            public bool tile = true;
            public float resetHeight = -20f;
            public float startHeight = 20f;
        }

        [Header("Parallax Layers")]
        [SerializeField] private ParallaxLayer[] layers;

        [Header("Global Settings")]
        [SerializeField] private float baseScrollSpeed = 2f;
        [SerializeField] private bool autoScroll = true;

        private void Update()
        {
            if (!autoScroll) return;

            foreach (var layer in layers)
            {
                if (layer.transform == null) continue;

                Vector3 newPos = layer.transform.position;
                newPos.y -= layer.scrollSpeed * baseScrollSpeed * Time.deltaTime;

                if (layer.tile && newPos.y <= layer.resetHeight)
                {
                    newPos.y = layer.startHeight;
                }

                layer.transform.position = newPos;
            }
        }

        public void SetScrollSpeed(float speed)
        {
            baseScrollSpeed = speed;
        }

        public void SetAutoScroll(bool enabled)
        {
            autoScroll = enabled;
        }
    }
}
