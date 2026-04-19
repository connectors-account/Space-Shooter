using UnityEngine;

namespace SpaceShooter.Gameplay
{
    public class PowerUpController : MonoBehaviour
    {
        public enum PowerUpType
        {
            Health,
            RapidFire,
            Shield
        }

        [SerializeField] private float fallSpeed = 2f;
        [SerializeField] private float bobSpeed = 5f;
        [SerializeField] private float bobAmplitude = 0.08f;

        private float startX;

        public PowerUpType Type { get; private set; }

        public void SetPowerUpType(PowerUpType powerUpType)
        {
            Type = powerUpType;
            startX = transform.position.x;
        }

        private void Update()
        {
            transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
            float xOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.position = new Vector3(startX + xOffset, transform.position.y, 0f);

            if (transform.position.y < -6.8f)
            {
                Destroy(gameObject);
            }
        }
    }
}
