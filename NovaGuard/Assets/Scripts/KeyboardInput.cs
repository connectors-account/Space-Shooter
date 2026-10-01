using UnityEngine;

namespace NovaGuard
{
    /// <summary>Built-in Input Manager backend; no configured axes or external input package required.</summary>
    public static class KeyboardInput
    {
        public static Vector2 Movement
        {
            get
            {
                float x = (Held(KeyCode.D, KeyCode.RightArrow) ? 1 : 0)
                        - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1 : 0);
                float y = (Held(KeyCode.W, KeyCode.UpArrow) ? 1 : 0)
                        - (Held(KeyCode.S, KeyCode.DownArrow) ? 1 : 0);
                return Vector2.ClampMagnitude(new Vector2(x, y), 1);
            }
        }
        public static bool FireHeld => Held(KeyCode.Space, KeyCode.Z);
        public static bool PausePressed => Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
        public static bool RestartPressed => Input.GetKeyDown(KeyCode.R);
        public static bool MutePressed => Input.GetKeyDown(KeyCode.M);
        private static bool Held(KeyCode a, KeyCode b) => Input.GetKey(a) || Input.GetKey(b);
    }
}
