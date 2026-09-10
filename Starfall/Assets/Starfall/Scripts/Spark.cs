using UnityEngine;

namespace Starfall
{
    public sealed class Spark : MonoBehaviour
    {
        Vector2 velocity;
        float remaining, duration;
        SpriteRenderer image;
        public void Initialize(Vector2 speed, float lifetime)
        {
            velocity = speed; remaining = duration = lifetime;
            image = GetComponent<SpriteRenderer>();
            transform.localScale = Vector3.one * 0.7f;
        }
        void Update()
        {
            remaining -= Time.deltaTime;
            if (remaining <= 0) { Destroy(gameObject); return; }
            transform.position += (Vector3)(velocity * Time.deltaTime);
            Color c = image.color; c.a = remaining / duration; image.color = c;
            transform.localScale = Vector3.one * (remaining / duration);
        }
    }
}
