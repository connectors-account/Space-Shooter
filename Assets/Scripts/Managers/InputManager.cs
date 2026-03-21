using UnityEngine;

/// <summary>
/// Centralized input handling for keyboard controls.
/// Provides clean input state that other scripts can query.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // Movement input
    public float HorizontalInput { get; private set; }
    public float VerticalInput { get; private set; }
    public Vector2 MoveInput => new Vector2(HorizontalInput, VerticalInput).normalized;

    // Action input
    public bool FireHeld { get; private set; }
    public bool FirePressed { get; private set; }
    public bool PausePressed { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // Movement: WASD or Arrow Keys
        HorizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        VerticalInput = Input.GetAxisRaw("Vertical");      // W/S or Up/Down

        // Shooting: Space bar
        FireHeld = Input.GetKey(KeyCode.Space);
        FirePressed = Input.GetKeyDown(KeyCode.Space);

        // Pause: Escape
        PausePressed = Input.GetKeyDown(KeyCode.Escape);
    }
}
