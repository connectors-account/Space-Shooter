using UnityEngine;

namespace SpaceShooter.Utils
{
    public static class SpriteGenerator
    {
        public static Sprite CreateSquareSprite(int size, Color color)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite CreateCircleSprite(int size, Color color)
        {
            Texture2D texture = new Texture2D(size, size);
            float radius = size / 2f;
            Vector2 center = new Vector2(radius, radius);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance <= radius)
                    {
                        texture.SetPixel(x, y, color);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite CreateTriangleSprite(int size, Color color, bool pointUp = true)
        {
            Texture2D texture = new Texture2D(size, size);
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            for (int y = 0; y < size; y++)
            {
                int actualY = pointUp ? y : size - 1 - y;
                float ratio = (float)y / size;
                int halfWidth = Mathf.RoundToInt(ratio * size / 2f);
                int centerX = size / 2;

                for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
                {
                    if (x >= 0 && x < size)
                    {
                        texture.SetPixel(x, actualY, color);
                    }
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite CreatePlayerShipSprite()
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            Color mainColor = new Color(0.2f, 0.6f, 1f);
            Color accentColor = new Color(0.4f, 0.8f, 1f);
            Color cockpitColor = new Color(0.8f, 0.9f, 1f);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            int centerX = size / 2;
            for (int y = 10; y < size - 5; y++)
            {
                float progress = (float)(y - 10) / (size - 15);
                int halfWidth = Mathf.RoundToInt(Mathf.Lerp(12, 3, progress));

                for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
                {
                    if (x >= 0 && x < size)
                    {
                        Color pixelColor = mainColor;
                        if (Mathf.Abs(x - centerX) == halfWidth)
                            pixelColor = accentColor;
                        if (y > size - 20 && Mathf.Abs(x - centerX) < 3)
                            pixelColor = cockpitColor;
                        texture.SetPixel(x, y, pixelColor);
                    }
                }
            }

            for (int wing = -1; wing <= 1; wing += 2)
            {
                for (int y = 15; y < 35; y++)
                {
                    int wingX = centerX + wing * (8 + (35 - y) / 3);
                    if (wingX >= 0 && wingX < size)
                    {
                        texture.SetPixel(wingX, y, accentColor);
                        if (wingX + wing >= 0 && wingX + wing < size)
                            texture.SetPixel(wingX + wing, y, mainColor);
                    }
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite CreateEnemyShipSprite(Color baseColor)
        {
            int size = 48;
            Texture2D texture = new Texture2D(size, size);
            Color accentColor = baseColor * 1.3f;
            accentColor.a = 1f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            int centerX = size / 2;
            for (int y = 5; y < size - 5; y++)
            {
                float progress = (float)(y - 5) / (size - 10);
                int halfWidth = Mathf.RoundToInt(Mathf.Lerp(3, 10, progress));

                for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
                {
                    if (x >= 0 && x < size)
                    {
                        Color pixelColor = baseColor;
                        if (Mathf.Abs(x - centerX) == halfWidth)
                            pixelColor = accentColor;
                        texture.SetPixel(x, y, pixelColor);
                    }
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite CreateBulletSprite(Color color)
        {
            int width = 8;
            int height = 16;
            Texture2D texture = new Texture2D(width, height);
            Color glowColor = color * 1.5f;
            glowColor.a = 0.5f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            for (int x = 1; x < width - 1; x++)
            {
                texture.SetPixel(x, 1, glowColor);
                texture.SetPixel(x, height - 2, glowColor);
            }
            for (int y = 1; y < height - 1; y++)
            {
                texture.SetPixel(1, y, glowColor);
                texture.SetPixel(width - 2, y, glowColor);
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 16);
        }

        public static Sprite CreatePowerUpSprite(Color color)
        {
            int size = 32;
            Texture2D texture = new Texture2D(size, size);
            Color glowColor = color * 1.5f;
            glowColor.a = 1f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            float radius = size / 2f - 2;
            Vector2 center = new Vector2(size / 2f, size / 2f);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance <= radius)
                    {
                        float gradient = 1f - (distance / radius);
                        Color pixelColor = Color.Lerp(color, glowColor, gradient);
                        texture.SetPixel(x, y, pixelColor);
                    }
                    else if (distance <= radius + 2)
                    {
                        Color borderColor = glowColor;
                        borderColor.a = 0.5f;
                        texture.SetPixel(x, y, borderColor);
                    }
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
