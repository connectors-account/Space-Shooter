using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Destroys objects that exit the game boundaries
    /// </summary>
    public class Boundary : MonoBehaviour
    {
        [SerializeField] private bool destroyBullets = true;
        [SerializeField] private bool destroyEnemies = true;
        [SerializeField] private bool destroyPowerUps = true;
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (destroyBullets && (other.CompareTag("PlayerBullet") || other.CompareTag("EnemyBullet")))
            {
                Destroy(other.gameObject);
            }
            else if (destroyEnemies && other.CompareTag("Enemy"))
            {
                // Don't destroy, just let it go
            }
            else if (destroyPowerUps && other.CompareTag("PowerUp"))
            {
                Destroy(other.gameObject);
            }
        }
    }
}
