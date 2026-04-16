using System.Collections;
using SpaceShooter.Combat;
using SpaceShooter.Player;
using UnityEngine;

namespace SpaceShooter.PowerUps
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerWeapon))]
    public class PlayerPowerUpController : MonoBehaviour
    {
        [Header("Rapid Fire")]
        [SerializeField] private float rapidFireMultiplier = 2.5f;
        [SerializeField] private float rapidFireDuration = 8f;

        [Header("Shield")]
        [SerializeField] private int shieldHitCount = 3;
        [SerializeField] private float shieldDuration = 10f;
        [SerializeField] private GameObject shieldVisual;

        [Header("Health Restore")]
        [SerializeField] private int healthRestoreAmount = 30;

        private Health health;
        private PlayerWeapon weapon;

        private Coroutine rapidFireRoutine;
        private Coroutine shieldRoutine;
        private int remainingShieldHits;
        private bool shieldActive;

        private void Awake()
        {
            health = GetComponent<Health>();
            weapon = GetComponent<PlayerWeapon>();
            SetShieldVisual(false);
        }

        public void ApplyPowerUp(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.RapidFire:
                    ActivateRapidFire();
                    break;
                case PowerUpType.Shield:
                    ActivateShield();
                    break;
                case PowerUpType.HealthRestore:
                    health.Heal(healthRestoreAmount);
                    break;
            }
        }

        public bool TryAbsorbIncomingDamage(int incomingDamage)
        {
            if (!shieldActive || incomingDamage <= 0)
            {
                return false;
            }

            remainingShieldHits--;
            if (remainingShieldHits <= 0)
            {
                DeactivateShield();
            }

            return true;
        }

        private void ActivateRapidFire()
        {
            if (rapidFireRoutine != null)
            {
                StopCoroutine(rapidFireRoutine);
            }

            rapidFireRoutine = StartCoroutine(RapidFireCoroutine());
        }

        private IEnumerator RapidFireCoroutine()
        {
            weapon.SetFireRateMultiplier(rapidFireMultiplier);
            yield return new WaitForSeconds(rapidFireDuration);
            weapon.ResetFireRateMultiplier();
            rapidFireRoutine = null;
        }

        private void ActivateShield()
        {
            remainingShieldHits = shieldHitCount;
            shieldActive = true;
            SetShieldVisual(true);

            if (shieldRoutine != null)
            {
                StopCoroutine(shieldRoutine);
            }

            shieldRoutine = StartCoroutine(ShieldCoroutine());
        }

        private IEnumerator ShieldCoroutine()
        {
            yield return new WaitForSeconds(shieldDuration);
            DeactivateShield();
            shieldRoutine = null;
        }

        private void DeactivateShield()
        {
            shieldActive = false;
            remainingShieldHits = 0;
            SetShieldVisual(false);
        }

        private void SetShieldVisual(bool isVisible)
        {
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(isVisible);
            }
        }
    }
}
