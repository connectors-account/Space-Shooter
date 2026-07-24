// ============================================================
//  PlayerController.cs  –  WASD / Arrow movement + screen clamp
// ============================================================
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed   = 6f;
    public float boostSpeed  = 10f;   // used by SpeedBoost power-up

    [Header("Screen Bounds")]
    public float xMin = -4.3f;
    public float xMax =  4.3f;
    public float yMin = -4.5f;
    public float yMax =  4.5f;

    Rigidbody2D _rb;
    float       _currentSpeed;
    bool        _boosted;
    float       _boostTimer;

    void Awake()
    {
        _rb           = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _currentSpeed = moveSpeed;
    }

    void Update()
    {
        // Speed boost countdown
        if (_boosted)
        {
            _boostTimer -= Time.deltaTime;
            if (_boostTimer <= 0f)
            {
                _boosted      = false;
                _currentSpeed = moveSpeed;
            }
        }

        // Read axes (works with both WASD and Arrow keys by default in Unity)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 dir = new Vector2(h, v).normalized;
        _rb.linearVelocity = dir * _currentSpeed;

        // Clamp position inside screen
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, xMin, xMax);
        p.y = Mathf.Clamp(p.y, yMin, yMax);
        transform.position = p;
    }

    // ── Power-up callbacks ───────────────────────────────────

    public void ApplySpeedBoost(float duration)
    {
        _boosted      = true;
        _boostTimer   = duration;
        _currentSpeed = boostSpeed;
    }
}
