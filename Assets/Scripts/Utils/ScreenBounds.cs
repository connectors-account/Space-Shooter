using UnityEngine;

namespace SpaceShooter.Utils
{
    public class ScreenBounds : MonoBehaviour
    {
        public static ScreenBounds Instance { get; private set; }

        [SerializeField] private Camera mainCamera;
        
        public float MinX { get; private set; }
        public float MaxX { get; private set; }
        public float MinY { get; private set; }
        public float MaxY { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                CalculateBounds();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            CalculateBounds();
        }

        private void CalculateBounds()
        {
            if (mainCamera == null) return;

            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;

            MinX = mainCamera.transform.position.x - cameraWidth / 2f;
            MaxX = mainCamera.transform.position.x + cameraWidth / 2f;
            MinY = mainCamera.transform.position.y - cameraHeight / 2f;
            MaxY = mainCamera.transform.position.y + cameraHeight / 2f;
        }

        public bool IsWithinBounds(Vector3 position, float padding = 0f)
        {
            return position.x >= MinX - padding && position.x <= MaxX + padding &&
                   position.y >= MinY - padding && position.y <= MaxY + padding;
        }

        public Vector3 ClampToBounds(Vector3 position, float padding = 0f)
        {
            position.x = Mathf.Clamp(position.x, MinX + padding, MaxX - padding);
            position.y = Mathf.Clamp(position.y, MinY + padding, MaxY - padding);
            return position;
        }
    }
}
