using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Infinite scrolling parallax layer.
    /// Use on each background sprite layer with different scroll speeds.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [SerializeField] private float scrollSpeed = 1f;
        [SerializeField] private float resetY = -12f;
        [SerializeField] private float startY = 12f;

        private void Update()
        {
            transform.position += Vector3.down * (scrollSpeed * Time.deltaTime);

            if (transform.position.y <= resetY)
            {
                transform.position = new Vector3(transform.position.x, startY, transform.position.z);
            }
        }
    }
}
