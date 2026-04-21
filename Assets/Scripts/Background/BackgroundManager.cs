using UnityEngine;

namespace SpaceShooter.Background
{
    public class BackgroundManager : MonoBehaviour
    {
        [SerializeField] private ParallaxLayer[] layers;

        private void Awake()
        {
            if (layers == null || layers.Length == 0)
            {
                layers = GetComponentsInChildren<ParallaxLayer>();
            }
        }
    }
}
