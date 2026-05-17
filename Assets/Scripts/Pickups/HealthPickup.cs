using UnityEngine;

/// <summary>
/// A health pickup that drifts downward. Player collects it via trigger.
/// </summary>
public class HealthPickup : MonoBehaviour
{
    public float fallSpeed = 2f;
    public float rotateSpeed = 90f;
    public float lifetime = 10f;

    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        if (Time.time - spawnTime > lifetime || transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }
}
