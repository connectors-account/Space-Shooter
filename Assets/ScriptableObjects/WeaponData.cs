using UnityEngine;

namespace SpaceShooter.Data
{
    /// <summary>
    /// Data-driven definition of a weapon / firing configuration.
    /// Create assets via Assets > Create > Space Shooter > Weapon Data.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Space Shooter/Weapon Data", order = 3)]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        public string weaponName = "Blaster";

        [Header("Firing")]
        [Tooltip("Seconds between shots.")]
        public float fireRate = 0.15f;
        public float bulletSpeed = 15f;
        public int bulletDamage = 10;

        [Header("Spread")]
        [Tooltip("Number of bullets fired per shot.")]
        public int bulletsPerShot = 1;
        [Tooltip("Total spread angle in degrees across all bullets.")]
        public float spreadAngle = 0f;

        [Header("Visuals")]
        public Sprite bulletSprite;
        public GameObject muzzleFlashPrefab;
    }
}
