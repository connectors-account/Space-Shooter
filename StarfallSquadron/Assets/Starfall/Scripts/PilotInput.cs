using UnityEngine;

namespace Starfall
{
    // Built-in Input Manager, intentionally no Input System package dependency.
    public static class PilotInput
    {
        public static Vector2 Movement
        {
            get
            {
                float x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0)
                        - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
                float y = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0)
                        - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
                return Vector2.ClampMagnitude(new Vector2(x + Input.GetAxisRaw("PadX"), y + Input.GetAxisRaw("PadY")), 1);
            }
        }
        public static bool Fire { get { return Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0) || Input.GetKey(KeyCode.JoystickButton0); } }
        public static bool Confirm { get { return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0); } }
        public static bool Pause { get { return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.JoystickButton7); } }
        public static bool Restart { get { return Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton3); } }
        public static bool Mute { get { return Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.JoystickButton2); } }
        public static bool Menu { get { return Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.JoystickButton1); } }
    }
}
