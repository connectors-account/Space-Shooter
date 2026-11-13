using UnityEngine;

/// <summary>
/// Moves the player's ship within the screen and fires bullets.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float fireCooldown = 0.18f;
    public float edgeMargin = 0.7f;

    float nextFireTime;

    public void ResetPlayer()
    {
        Vector2 half = GameManager.Instance != null ? GameManager.Instance.HalfScreenSize : new Vector2(10f, 6f);
        transform.position = new Vector3(0f, -half.y + 1.5f, 0f);
        // Short delay so the click on the Restart button does not fire a shot.
        nextFireTime = Time.time + 0.25f;
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.IsGameOver) return;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1f) input.Normalize();

        Vector2 half = gm.HalfScreenSize;
        Vector3 pos = transform.position + (Vector3)(input * moveSpeed * Time.deltaTime);
        pos.x = Mathf.Clamp(pos.x, -half.x + edgeMargin, half.x - edgeMargin);
        pos.y = Mathf.Clamp(pos.y, -half.y + edgeMargin, half.y - edgeMargin);
        pos.z = 0f;
        transform.position = pos;

        bool firePressed = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        if (firePressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            Bullet.Spawn(transform.position + new Vector3(0f, 0.8f, 0f));
        }
    }
}
