using UnityEngine;

/// <summary>
/// EnemyController moves an enemy ship downward. When it exits the screen, it is destroyed.
/// Attach this script to the Enemy prefab.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Tooltip("Speed at which the enemy drifts downward")]
    public float speed = 4f;

    [Tooltip("Optional slight horizontal wobble amplitude (0 = straight line)")]
    public float wobbleAmount = 0.5f;

    [Tooltip("Speed of the horizontal wobble")]
    public float wobbleSpeed = 2f;

    // Stores the starting X so the wobble oscillates around it
    private float startX;

    void Start()
    {
        startX = transform.position.x;

        // Randomize speed slightly so enemies don't all move at the same pace
        speed += Random.Range(-0.5f, 1.0f);
    }

    void Update()
    {
        // Move downward
        float newY = transform.position.y - speed * Time.deltaTime;

        // Horizontal wobble for visual variety
        float newX = startX + Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;

        transform.position = new Vector3(newX, newY, 0f);

        // Destroy the enemy if it goes off the bottom of the screen
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }
}
