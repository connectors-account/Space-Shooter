using UnityEngine;

namespace SpaceShooter.Environment
{
    /// <summary>
    /// Simple 2-layer parallax background scroller.
    /// </summary>
    public class ParallaxScroller : MonoBehaviour
    {
        [SerializeField] private Transform farLayer;
        [SerializeField] private Transform nearLayer;
        [SerializeField] private float farSpeed = 0.4f;
        [SerializeField] private float nearSpeed = 1.1f;
        [SerializeField] private float resetY = -12f;
        [SerializeField] private float startY = 12f;

        private void Update()
        {
            ScrollLayer(farLayer, farSpeed);
            ScrollLayer(nearLayer, nearSpeed);
        }

        private void ScrollLayer(Transform layer, float speed)
        {
            if (layer == null)
            {
                return;
            }

            layer.position += Vector3.down * (speed * Time.deltaTime);
            if (layer.position.y <= resetY)
            {
                layer.position = new Vector3(layer.position.x, startY, layer.position.z);
            }
        }
    }
}
