using UnityEngine;

namespace SpaceShooter.Core
{
    public class BoundaryCleaner : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                Destroy(other.gameObject);
            }
        }
    }
}
