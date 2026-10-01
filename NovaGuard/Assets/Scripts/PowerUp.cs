using UnityEngine;

namespace NovaGuard
{
    public sealed class PowerUp : MonoBehaviour
    {
        public PickupKind kind;
        private float age;
        private bool collected;
        private Vector3 baseScale;
        private void Awake() { baseScale = transform.localScale; }
        private void Update()
        {
            if (!GameController.Instance.IsPlaying) return;
            age += Time.deltaTime;
            transform.position += Vector3.down * (1.6f * Time.deltaTime);
            transform.localScale = baseScale * (1 + Mathf.Sin(age * 5) * 0.08f);
            if (transform.position.y < -10 || age > 15) Destroy(gameObject);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || !GameController.Instance.IsPlaying) return;
            PlayerShip player = other.GetComponent<PlayerShip>();
            if (player == null) return;
            collected = true;
            player.Apply(kind);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
