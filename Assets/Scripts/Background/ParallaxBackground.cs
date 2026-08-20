using UnityEngine;

namespace SpaceShooter.Background
{
    /// <summary>
    /// Three-layer scrolling starfield. Each layer is a pair of stacked SpriteRenderers using a
    /// procedurally generated star texture; the pair leapfrogs to create a seamless vertical wrap.
    /// Far layer = slow/small, near layer = fast/large.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [System.Serializable]
        public struct Layer
        {
            public float speed;
            public int starCount;
            public float starScale;
            public int sortingOrder;
            [HideInInspector] public Transform tileA;
            [HideInInspector] public Transform tileB;
            [HideInInspector] public float height;
        }

        public Layer[] layers = new Layer[3]
        {
            new Layer { speed = 0.5f, starCount = 40, starScale = 1f, sortingOrder = -30 },
            new Layer { speed = 1.2f, starCount = 30, starScale = 1.6f, sortingOrder = -20 },
            new Layer { speed = 2.4f, starCount = 20, starScale = 2.4f, sortingOrder = -10 },
        };

        public Color starColor = Color.white;

        private Camera _cam;
        private float _worldHeight;
        private float _worldWidth;

        private void Start()
        {
            _cam = Camera.main;
            RecalcSize();
            BuildLayers();
        }

        private void RecalcSize()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;
            _worldHeight = _cam.orthographicSize * 2f;
            _worldWidth = _worldHeight * _cam.aspect;
        }

        private void BuildLayers()
        {
            for (int i = 0; i < layers.Length; i++)
            {
                var tex = GenerateStarTexture(256, 256, layers[i].starCount, layers[i].starScale, starColor);
                var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100f);

                layers[i].tileA = CreateTile($"Layer{i}_A", sprite, layers[i].sortingOrder);
                layers[i].tileB = CreateTile($"Layer{i}_B", sprite, layers[i].sortingOrder);

                // Scale each tile to cover the full screen width and height.
                float spriteW = sprite.bounds.size.x;
                float spriteH = sprite.bounds.size.y;
                float sx = _worldWidth / spriteW;
                float sy = _worldHeight / spriteH;
                layers[i].tileA.localScale = new Vector3(sx, sy, 1f);
                layers[i].tileB.localScale = new Vector3(sx, sy, 1f);
                layers[i].height = _worldHeight;

                layers[i].tileA.position = new Vector3(_cam.transform.position.x, _cam.transform.position.y, 0f);
                layers[i].tileB.position = new Vector3(_cam.transform.position.x, _cam.transform.position.y + _worldHeight, 0f);
            }
        }

        private Transform CreateTile(string name, Sprite sprite, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return go.transform;
        }

        private void Update()
        {
            if (_cam == null) return;
            float camBottom = _cam.transform.position.y - _worldHeight * 0.5f;

            for (int i = 0; i < layers.Length; i++)
            {
                var L = layers[i];
                if (L.tileA == null || L.tileB == null) continue;

                float dy = L.speed * Time.deltaTime;
                L.tileA.position += Vector3.down * dy;
                L.tileB.position += Vector3.down * dy;

                // When a tile scrolls fully below the camera, jump it above the other one.
                WrapTile(L.tileA, L.tileB, camBottom, L.height);
                WrapTile(L.tileB, L.tileA, camBottom, L.height);
            }
        }

        private void WrapTile(Transform tile, Transform other, float camBottom, float height)
        {
            if (tile.position.y + height * 0.5f < camBottom)
            {
                tile.position = new Vector3(tile.position.x, other.position.y + height, tile.position.z);
            }
        }

        /// <summary>
        /// Generate a tileable star texture with random white dots of varying brightness.
        /// Static so menus can reuse it for a starfield RawImage.
        /// </summary>
        public static Texture2D GenerateStarTexture(int w, int h, int starCount, float sizeScale)
        {
            return GenerateStarTexture(w, h, starCount, sizeScale, Color.white);
        }

        public static Texture2D GenerateStarTexture(int w, int h, int starCount, float sizeScale, Color color)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            var clear = new Color(0, 0, 0, 0);
            var px = new Color[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = clear;

            var rng = new System.Random(w * 7919 + h * 104729 + starCount);
            for (int s = 0; s < starCount; s++)
            {
                int cx = rng.Next(0, w);
                int cy = rng.Next(0, h);
                float bright = 0.4f + (float)rng.NextDouble() * 0.6f;
                int radius = Mathf.Max(1, Mathf.RoundToInt(sizeScale * (0.5f + (float)rng.NextDouble())));

                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        if (dx * dx + dy * dy > radius * radius) continue;
                        int x = ((cx + dx) % w + w) % w;
                        int y = ((cy + dy) % h + h) % h;
                        var c = color;
                        c.a = bright;
                        px[y * w + x] = c;
                    }
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }
    }
}
