using UnityEngine;

public class SpriteGenerator : MonoBehaviour
{
    public static Sprite CreatePlayerSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        Color shipColor = new Color(0.2f, 0.6f, 1f);
        Color cockpitColor = new Color(0.8f, 0.9f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int cx = x - size / 2;
                int cy = y - size / 2;

                if (cy > -12 && cy < 14)
                {
                    int maxWidth = (14 - Mathf.Abs(cy)) / 2 + 2;
                    if (Mathf.Abs(cx) <= maxWidth)
                    {
                        pixels[y * size + x] = shipColor;
                    }
                }

                if (cy > 2 && cy < 10 && Mathf.Abs(cx) < 3)
                {
                    pixels[y * size + x] = cockpitColor;
                }

                if (cy < -4 && cy > -12)
                {
                    if ((cx > 4 && cx < 8) || (cx < -4 && cx > -8))
                    {
                        int wingWidth = (-4 - cy) / 2;
                        if (Mathf.Abs(Mathf.Abs(cx) - 6) <= wingWidth)
                        {
                            pixels[y * size + x] = shipColor;
                        }
                    }
                }
            }
        }

        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
    }

    public static Sprite CreateEnemySprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        Color enemyColor = Color.red;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int cx = x - size / 2;
                int cy = y - size / 2;

                float dist = Mathf.Sqrt(cx * cx + cy * cy);
                if (dist < 10)
                {
                    pixels[y * size + x] = enemyColor;
                }

                if (cy < 6 && cy > -6)
                {
                    if ((cx > 8 && cx < 14) || (cx < -8 && cx > -14))
                    {
                        pixels[y * size + x] = enemyColor;
                    }
                }
            }
        }

        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
    }

    public static Sprite CreateBulletSprite()
    {
        int width = 4;
        int height = 12;
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float brightness = 1f - (float)y / height * 0.5f;
                pixels[y * width + x] = new Color(brightness, brightness, 1f);
            }
        }

        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 12);
    }

    public static Sprite CreatePowerUpSprite()
    {
        int size = 24;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        Color powerUpColor = Color.yellow;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int cx = x - size / 2;
                int cy = y - size / 2;

                float dist = Mathf.Sqrt(cx * cx + cy * cy);
                if (dist < 10 && dist > 6)
                {
                    pixels[y * size + x] = powerUpColor;
                }

                if (Mathf.Abs(cx) < 3 && Mathf.Abs(cy) < 8)
                {
                    pixels[y * size + x] = powerUpColor;
                }
                if (Mathf.Abs(cy) < 3 && Mathf.Abs(cx) < 8)
                {
                    pixels[y * size + x] = powerUpColor;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24);
    }

    public static Sprite CreateSquareSprite(Color color, int size = 16)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;

        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
