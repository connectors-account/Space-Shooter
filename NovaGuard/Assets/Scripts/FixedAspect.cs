using UnityEngine;

namespace NovaGuard
{
    [RequireComponent(typeof(Camera))]
    public sealed class FixedAspect : MonoBehaviour
    {
        private Camera view;
        private int width, height;
        private void Awake() { view = GetComponent<Camera>(); Resize(); }
        private void Update() { if (Screen.width != width || Screen.height != height) Resize(); }
        private void Resize()
        {
            width = Screen.width;
            height = Mathf.Max(1, Screen.height);
            float ratio = (width / (float)height) / (16f / 9f);
            view.rect = ratio < 1 ? new Rect(0, (1 - ratio) * .5f, 1, ratio)
                : new Rect((1 - 1 / ratio) * .5f, 0, 1 / ratio, 1);
        }
    }
}
