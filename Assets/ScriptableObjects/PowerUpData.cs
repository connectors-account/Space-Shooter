using UnityEngine;

namespace SpaceShooter.Data
{
    /// <summary>
    /// The kind of effect a power-up applies when collected.
    /// </summary>
    public enum PowerUpType
    {
        WeaponUpgrade,
        Shield,
        HealthPack,
        SpeedBoost,
        BombClear
    }

    /// <summary>
    /// Data-driven definition of a power-up. Create assets via
    /// Assets > Create > Space Shooter > PowerUp Data.
    /// </summary>
    [CreateAssetMenu(fileName = "PowerUpData", menuName = "Space Shooter/PowerUp Data", order = 2)]
    public class PowerUpData : ScriptableObject
    {
        [Header("Type")]
        public PowerUpType type = PowerUpType.WeaponUpgrade;

        [Header("Presentation")]
        public Sprite icon;
        public Color glowColor = Color.cyan;

        [Header("Behaviour")]
        [Tooltip("Effect duration in seconds. 0 means an instant / one-shot effect.")]
        public float duration = 0f;

        [Tooltip("Generic magnitude used by some effects (e.g. heal amount, speed multiplier).")]
        public float magnitude = 1f;

        [Header("Score")]
        public int scoreValue = 50;
    }
}
