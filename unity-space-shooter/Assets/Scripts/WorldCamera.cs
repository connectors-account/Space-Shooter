using UnityEngine;

namespace Starward
{
    [RequireComponent(typeof(Camera))]
    public sealed class WorldCamera : MonoBehaviour
    {
        Camera view;
        void Awake() { view = GetComponent<Camera>(); }
        void LateUpdate()
        {
            const float desired = 16f / 9f;
            float actual = Screen.width / (float)Mathf.Max(1, Screen.height);
            if (actual > desired)
            {
                float width = desired / actual;
                view.rect = new Rect((1 - width) * .5f, 0, width, 1);
            }
            else
            {
                float height = actual / desired;
                view.rect = new Rect(0, (1 - height) * .5f, 1, height);
            }
            view.aspect = desired;
        }
    }
}
