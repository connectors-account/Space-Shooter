using UnityEngine;

namespace MinimalSpaceShooter
{
    // Keep a constant playfield when the desktop window is resized.
    public sealed class ArenaViewport : MonoBehaviour
    {
        private Camera arenaCamera;

        private void Awake()
        {
            arenaCamera = GetComponent<Camera>();
            Update();
        }

        private void Update()
        {
            float windowAspect = Screen.width / (float)Mathf.Max(1, Screen.height);
            const float targetAspect = 4f / 3f;
            if (windowAspect > targetAspect)
            {
                float width = targetAspect / windowAspect;
                arenaCamera.rect = new Rect((1f - width) / 2f, 0, width, 1);
            }
            else
            {
                float height = windowAspect / targetAspect;
                arenaCamera.rect = new Rect(0, (1f - height) / 2f, 1, height);
            }
            arenaCamera.aspect = targetAspect;
        }
    }
}
