using UnityEngine;

namespace OrbitBreaker
{
    [RequireComponent(typeof(Camera))]
    public sealed class FixedAspectCamera : MonoBehaviour
    {
        private Camera targetCamera;
        private int lastWidth, lastHeight;
        private void Awake() { targetCamera = GetComponent<Camera>(); Apply(); }
        private void Update() { if (Screen.width != lastWidth || Screen.height != lastHeight) Apply(); }
        private void Apply()
        {
            lastWidth = Screen.width; lastHeight = Screen.height;
            float relative = ((float)lastWidth / Mathf.Max(1, lastHeight)) / (16f / 9f);
            targetCamera.rect = relative > 1f
                ? new Rect((1f - 1f / relative) * .5f, 0, 1f / relative, 1f)
                : new Rect(0, (1f - relative) * .5f, 1f, relative);
            targetCamera.aspect = 16f / 9f;
        }
    }
}
