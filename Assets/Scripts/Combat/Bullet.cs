using UnityEngine;

namespace SpaceShooter.Combat
{
    public enum Faction
    {
        Player,
        Enemy,
        Neutral
    }

    /// <summary>
    /// Generic bullet logic for both player and enemies.
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private int damage = 10;
        [SerializeField] private float speed = 14f;
        [SerializeField] private float lifetime = 3.2f;
        [SerializeField] private Faction ownerFaction = Faction.Neutral;

        private Vector2 direction = Vector2.up;
        private float elapsed;

        public int Damage => damage;
        public Faction OwnerFaction => ownerFaction;

        public void Initialize(Vector2 moveDirection, Faction faction)
        {
            direction = moveDirection.normalized;
            ownerFaction = faction;
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            elapsed += Time.deltaTime;

            if (elapsed > lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || other.gameObject == gameObject)
            {
                return;
            }

            var affiliation = other.GetComponent<FactionAffiliation>();
            if (affiliation != null && affiliation.Faction == ownerFaction)
            {
                return;
            }

            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.ReceiveDamage(damage, gameObject);
                Destroy(gameObject);
                return;
            }

            if (other.CompareTag("Boundary"))
            {
                Destroy(gameObject);
            }
        }
    }
}
