using UnityEngine;

namespace SpaceShooter.Background
{
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private float scrollSpeed = 1f;
        [SerializeField] private float resetY = -12f;
        [SerializeField] private float startY = 12f;

        private void Update()
        {
            transform.Translate(Vector3.down * (scrollSpeed * Time.deltaTime), Space.World);
            if (transform.position.y <= resetY)
            {
                transform.position = new Vector3(transform.position.x, startY, transform.position.z);
            }
        }
    }
}
