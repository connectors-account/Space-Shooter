using UnityEngine;

namespace StellarShooter
{
    [RequireComponent(typeof(Camera))]
    public sealed class FixedAspectCamera : MonoBehaviour
    {
        private Camera view;
        private int width, height;
        private void Awake() { view = GetComponent<Camera>(); Resize(); }
        private void Update() { if (Screen.width != width || Screen.height != height) Resize(); }
        private void Resize()
        {
            width = Screen.width;
            height = Screen.height;
            float scale = ((float)width / Mathf.Max(1, height)) / (16f / 9f);
            view.rect = scale < 1 ? new Rect(0, (1-scale)*.5f, 1, scale)
                : new Rect((1-1/scale)*.5f, 0, 1/scale, 1);
            view.aspect = 16f / 9f;
        }
    }
}
