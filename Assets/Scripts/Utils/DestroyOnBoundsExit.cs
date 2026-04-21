using UnityEngine;

namespace SpaceShooter.Utils
{
    public class DestroyOnBoundsExit : MonoBehaviour
    {
        [SerializeField] private float lowerY = -8f;
        [SerializeField] private float upperY = 8f;

        private void Update()
        {
            if (transform.position.y < lowerY || transform.position.y > upperY)
            {
                Destroy(gameObject);
            }
        }
    }
}
