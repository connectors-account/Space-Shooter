using UnityEngine;

namespace SpaceShooter.Background
{
    public class ParallaxScroller : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public Transform layerTransform;
            public float speedMultiplier = 0.2f;
            public float loopHeight = 20f;
        }

        [SerializeField] private float baseScrollSpeed = 1.2f;
        [SerializeField] private ParallaxLayer[] layers;

        private void Update()
        {
            float dt = Time.deltaTime;
            foreach (ParallaxLayer layer in layers)
            {
                if (layer.layerTransform == null) continue;

                Vector3 pos = layer.layerTransform.position;
                pos.y -= baseScrollSpeed * layer.speedMultiplier * dt;

                if (pos.y <= -layer.loopHeight)
                {
                    pos.y += layer.loopHeight * 2f;
                }

                layer.layerTransform.position = pos;
            }
        }
    }
}
