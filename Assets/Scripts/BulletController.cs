using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BulletController : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 6f;

    private Vector2 direction = Vector2.up;
    private bool enemyBullet;
    private int damage = 1;

    public void Initialize(Vector2 travelDirection, bool isEnemyBullet, int bulletDamage)
    {
        direction = travelDirection.normalized;
        enemyBullet = isEnemyBullet;
        damage = Mathf.Max(1, bulletDamage);
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        transform.Translate(direction * (speed * Time.deltaTime));
        CheckOutOfBounds();
    }

    private void CheckOutOfBounds()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        if (viewportPos.x < -0.1f || viewportPos.x > 1.1f || viewportPos.y < -0.1f || viewportPos.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enemyBullet)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }

            return;
        }

        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
