using UnityEngine;

namespace SpaceShooter.Environment
{
    public class ParallaxScroller : MonoBehaviour
    {
        [SerializeField] private float scrollSpeed = 1.5f;
        [SerializeField] private float resetY = -20f;
        [SerializeField] private float startY = 20f;

        private void Update()
        {
            transform.Translate(Vector3.down * (scrollSpeed * Time.deltaTime), Space.World);

            if (transform.position.y <= resetY)
            {
                Vector3 pos = transform.position;
                pos.y = startY;
                transform.position = pos;
            }
        }
    }
}
