using UnityEngine;

namespace SpaceShooter.Combat
{
    public interface IDamageable
    {
        void ReceiveDamage(int amount, GameObject source);
    }

    /// <summary>
    /// Generic damage receiver for enemies/destructible objects.
    /// </summary>
    public class Damageable : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 20;

        private int currentHealth;

        public event System.Action<GameObject> OnDied;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void SetMaxHealth(int hp)
        {
            maxHealth = Mathf.Max(1, hp);
            currentHealth = maxHealth;
        }

        public void ReceiveDamage(int amount, GameObject source)
        {
            currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(0, amount));
            if (currentHealth <= 0)
            {
                OnDied?.Invoke(source);
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Attach to objects to identify which side they belong to.
    /// </summary>
    public class FactionAffiliation : MonoBehaviour
    {
        [SerializeField] private Faction faction = Faction.Neutral;
        public Faction Faction => faction;
    }
}
