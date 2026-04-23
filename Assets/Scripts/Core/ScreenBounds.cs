using UnityEngine;

namespace SpaceShooter.Core
{
    public static class ScreenBounds
    {
        public static Vector2 MinWorld(Camera cam)
        {
            return cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane));
        }

        public static Vector2 MaxWorld(Camera cam)
        {
            return cam.ViewportToWorldPoint(new Vector3(1f, 1f, cam.nearClipPlane));
        }

        public static bool IsOutside(Camera cam, Vector3 worldPosition, float padding = 0.5f)
        {
            Vector2 min = MinWorld(cam);
            Vector2 max = MaxWorld(cam);
            return worldPosition.x < min.x - padding || worldPosition.x > max.x + padding ||
                   worldPosition.y < min.y - padding || worldPosition.y > max.y + padding;
        }
    }
}
