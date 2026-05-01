using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Visual
{
    public class ParallaxScroller : MonoBehaviour
    {
        [System.Serializable]
        private class Layer
        {
            public Transform Transform;
            public float Speed;
            public float ResetY;
            public float StartY;
        }

        private readonly List<Layer> _layers = new();

        public void AddLayer(Transform layerTransform, float speed, float startY, float resetY)
        {
            _layers.Add(new Layer
            {
                Transform = layerTransform,
                Speed = speed,
                StartY = startY,
                ResetY = resetY
            });
        }

        private void Update()
        {
            for (var i = 0; i < _layers.Count; i++)
            {
                var layer = _layers[i];
                var pos = layer.Transform.position;
                pos.y -= layer.Speed * Time.deltaTime;
                if (pos.y < layer.ResetY)
                {
                    pos.y = layer.StartY;
                }
                layer.Transform.position = pos;
            }
        }
    }
}
