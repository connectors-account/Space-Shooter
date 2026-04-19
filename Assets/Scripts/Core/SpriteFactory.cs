using UnityEngine;

namespace SpaceShooter.Core
{
    public static class SpriteFactory
    {
        public static Sprite CreateTriangle(Color color, int width = 48, int height = 48)
        {
            Texture2D texture = NewTexture(width, height);
            float center = width / 2f;
            float slope = center / height;

            for (int y = 0; y < height; y++)
            {
                float halfWidth = y * slope;
                int minX = Mathf.FloorToInt(center - halfWidth);
                int maxX = Mathf.CeilToInt(center + halfWidth);
                for (int x = minX; x <= maxX; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return ToSprite(texture, 100f);
        }

        public static Sprite CreateDiamond(Color color, int size = 42)
        {
            Texture2D texture = NewTexture(size, size);
            float center = (size - 1) / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Mathf.Abs(x - center) + Mathf.Abs(y - center);
                    if (distance <= center)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }

            texture.Apply();
            return ToSprite(texture, 100f);
        }

        public static Sprite CreateRect(Color color, int width = 12, int height = 30)
        {
            Texture2D texture = NewTexture(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return ToSprite(texture, 100f);
        }

        public static Sprite CreateCircle(Color color, int size = 36)
        {
            Texture2D texture = NewTexture(size, size);
            float center = (size - 1) / 2f;
            float radius = center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (distance <= radius)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }

            texture.Apply();
            return ToSprite(texture, 100f);
        }

        public static Sprite CreateStarTile(int width = 256, int height = 256)
        {
            Texture2D texture = NewTexture(width, height);
            Color background = new Color(0.02f, 0.03f, 0.08f, 1f);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, background);
                }
            }

            int stars = 220;
            for (int i = 0; i < stars; i++)
            {
                int x = Random.Range(0, width);
                int y = Random.Range(0, height);
                float brightness = Random.Range(0.5f, 1f);
                texture.SetPixel(x, y, new Color(brightness, brightness, brightness, 1f));
            }

            texture.Apply();
            return ToSprite(texture, 64f);
        }

        private static Texture2D NewTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = clear;
            }

            texture.SetPixels(pixels);
            return texture;
        }

        private static Sprite ToSprite(Texture2D texture, float pixelsPerUnit)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
    }
}
