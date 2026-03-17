using UnityEngine;
using System.Collections.Generic;

namespace SpaceShooter.Effects
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layerTransform;
        public float scrollSpeed = 1f;
        public bool tileVertically = true;
        public float tileHeight = 10f;
    }
    
    /// <summary>
    /// Creates parallax scrolling background effect
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [SerializeField] private List<ParallaxLayer> layers = new List<ParallaxLayer>();
        [SerializeField] private float baseScrollSpeed = 2f;
        [SerializeField] private bool autoScroll = true;
        
        [Header("Star Field")]
        [SerializeField] private bool generateStars = true;
        [SerializeField] private GameObject starPrefab;
        [SerializeField] private int starCount = 50;
        [SerializeField] private float starFieldWidth = 20f;
        [SerializeField] private float starFieldHeight = 15f;
        [SerializeField] private float minStarSpeed = 0.5f;
        [SerializeField] private float maxStarSpeed = 3f;
        
        private List<StarParticle> stars = new List<StarParticle>();
        private Camera mainCamera;
        
        private class StarParticle
        {
            public Transform transform;
            public float speed;
            public float originalY;
        }
        
        private void Start()
        {
            mainCamera = Camera.main;
            
            if (generateStars)
            {
                GenerateStarField();
            }
        }
        
        private void Update()
        {
            if (!autoScroll) return;
            if (SpaceShooter.Core.GameManager.Instance != null && 
                SpaceShooter.Core.GameManager.Instance.CurrentState != SpaceShooter.Core.GameState.Playing)
                return;
            
            UpdateParallaxLayers();
            UpdateStarField();
        }
        
        private void UpdateParallaxLayers()
        {
            foreach (var layer in layers)
            {
                if (layer.layerTransform == null) continue;
                
                // Scroll the layer
                Vector3 newPosition = layer.layerTransform.position;
                newPosition.y -= layer.scrollSpeed * baseScrollSpeed * Time.deltaTime;
                
                // Tile vertically if needed
                if (layer.tileVertically)
                {
                    if (newPosition.y <= -layer.tileHeight)
                    {
                        newPosition.y += layer.tileHeight * 2f;
                    }
                }
                
                layer.layerTransform.position = newPosition;
            }
        }
        
        private void GenerateStarField()
        {
            for (int i = 0; i < starCount; i++)
            {
                CreateStar(true);
            }
        }
        
        private void CreateStar(bool randomY)
        {
            float x = Random.Range(-starFieldWidth / 2f, starFieldWidth / 2f);
            float y = randomY ? Random.Range(-starFieldHeight / 2f, starFieldHeight / 2f) : starFieldHeight / 2f;
            float speed = Random.Range(minStarSpeed, maxStarSpeed);
            
            GameObject starObj;
            
            if (starPrefab != null)
            {
                starObj = Instantiate(starPrefab, transform);
            }
            else
            {
                // Create a simple star sprite
                starObj = new GameObject("Star");
                starObj.transform.SetParent(transform);
                
                SpriteRenderer sr = starObj.AddComponent<SpriteRenderer>();
                sr.sprite = CreateStarSprite();
                sr.color = new Color(1f, 1f, 1f, Random.Range(0.5f, 1f));
                sr.sortingLayerName = "Background";
                sr.sortingOrder = -100;
                
                // Scale based on speed (further = smaller/slower)
                float scale = Mathf.Lerp(0.02f, 0.08f, (speed - minStarSpeed) / (maxStarSpeed - minStarSpeed));
                starObj.transform.localScale = Vector3.one * scale;
            }
            
            starObj.transform.position = new Vector3(x, y, 10f);
            
            stars.Add(new StarParticle
            {
                transform = starObj.transform,
                speed = speed,
                originalY = y
            });
        }
        
        private Sprite CreateStarSprite()
        {
            Texture2D texture = new Texture2D(4, 4);
            Color[] colors = new Color[16];
            for (int i = 0; i < 16; i++)
            {
                colors[i] = Color.white;
            }
            texture.SetPixels(colors);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            
            return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }
        
        private void UpdateStarField()
        {
            for (int i = stars.Count - 1; i >= 0; i--)
            {
                var star = stars[i];
                if (star.transform == null)
                {
                    stars.RemoveAt(i);
                    continue;
                }
                
                // Move star down
                Vector3 pos = star.transform.position;
                pos.y -= star.speed * baseScrollSpeed * Time.deltaTime;
                
                // Wrap around
                if (pos.y < -starFieldHeight / 2f)
                {
                    pos.y = starFieldHeight / 2f;
                    pos.x = Random.Range(-starFieldWidth / 2f, starFieldWidth / 2f);
                }
                
                star.transform.position = pos;
            }
        }
        
        public void SetScrollSpeed(float speed)
        {
            baseScrollSpeed = speed;
        }
    }
}
