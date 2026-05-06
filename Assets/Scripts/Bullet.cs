using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 4f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Boundary"))
        {
            Destroy(gameObject);
        }
    }
}
