using SpaceShooter.Combat;
using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float baseFireRate = 0.25f;
        [SerializeField] private float projectileSpeed = 14f;
        [SerializeField] private int projectileDamage = 20;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shootSfx;

        private float lastShotTime = -99f;
        private float fireRateMultiplier = 1f;

        public void SetFireRateMultiplier(float multiplier)
        {
            fireRateMultiplier = Mathf.Max(0.1f, multiplier);
        }

        public void ResetFireRateMultiplier()
        {
            fireRateMultiplier = 1f;
        }

        public void TryFire()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            if (projectilePrefab == null || firePoint == null)
            {
                return;
            }

            float effectiveFireRate = baseFireRate / fireRateMultiplier;
            if (Time.time < lastShotTime + effectiveFireRate)
            {
                return;
            }

            lastShotTime = Time.time;

            GameObject projectileGo = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            if (projectileGo.TryGetComponent(out Projectile projectile))
            {
                projectile.Initialize(Vector2.up, ProjectileOwner.Player, projectileDamage, projectileSpeed);
            }

            if (audioSource != null && shootSfx != null)
            {
                audioSource.PlayOneShot(shootSfx);
            }
        }
    }
}
