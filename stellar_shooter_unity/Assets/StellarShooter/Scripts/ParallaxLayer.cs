using UnityEngine;

namespace StellarShooter
{
    public sealed class ParallaxLayer : MonoBehaviour
    {
        public Transform first;
        public Transform second;
        public float speed = .25f;
        public float tileHeight = 18;
        private void Update()
        {
            float offset = speed * Time.deltaTime;
            first.position += Vector3.down * offset;
            second.position += Vector3.down * offset;
            if (first.position.y <= -tileHeight) first.position += Vector3.up * (tileHeight * 2);
            if (second.position.y <= -tileHeight) second.position += Vector3.up * (tileHeight * 2);
        }
    }
}
