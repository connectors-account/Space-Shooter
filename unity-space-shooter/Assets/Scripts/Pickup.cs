using UnityEngine;

namespace Starward
{
    public enum PickupKind { Repair, Weapon }
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Pickup : MonoBehaviour
    {
        public PickupKind kind;
        GameManager game;
        Rigidbody2D body;
        float born;
        bool collected;
        void Awake() { body = GetComponent<Rigidbody2D>(); }
        public void Initialize(GameManager owner) { game = owner; born = Time.time; }
        void FixedUpdate()
        {
            if (game == null || !game.IsPlaying) return;
            body.MovePosition(body.position + Vector2.down * 1.3f * Time.fixedDeltaTime);
            if (body.position.y < -6.3f || Time.time - born > 14) Destroy(gameObject);
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || game == null || !game.IsPlaying) return;
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;
            collected = true;
            if (kind == PickupKind.Repair) player.Health.Heal(2); else player.Upgrade();
            game.audioBank.Play(SoundId.Pickup);
            game.SpawnBurst(transform.position, kind == PickupKind.Repair ? Color.green : Color.yellow);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
