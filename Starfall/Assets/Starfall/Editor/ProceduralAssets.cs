using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Starfall.Editor
{
    // Original analytic/geometric icons and synthesized audio, no external asset downloads.
    public static class ProceduralAssets
    {
        public const string Folder = "Assets/Starfall/Generated";
        static readonly Vector2[] Hull = { new Vector2(0, .88f), new Vector2(.24f, .2f), new Vector2(.78f, -.56f), new Vector2(.2f, -.34f), new Vector2(0, -.62f), new Vector2(-.2f, -.34f), new Vector2(-.78f, -.56f), new Vector2(-.24f, .2f) };
        static bool Polygon(float x, float y, Vector2[] points)
        {
            bool inside = false;
            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
                if ((points[i].y > y) != (points[j].y > y) && x < (points[j].x - points[i].x) * (y - points[i].y) / (points[j].y - points[i].y) + points[i].x) inside = !inside;
            return inside;
        }
        public static Sprite Sprite(string name, int kind, Color color, float pixelsPerUnit)
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float px = (x + .5f) / size * 2 - 1, py = (y + .5f) / size * 2 - 1;
                float ax = Mathf.Abs(px), ay = Mathf.Abs(py), r = Mathf.Sqrt(px * px + py * py);
                bool solid = false, detail = false;
                switch (kind)
                {
                    case 0: // Upward player hull, luminous cockpit and exhaust.
                        solid = Polygon(px, py, Hull);
                        detail = ax < .07f && py > -.2f && py < .55f;
                        if (ax < .105f && py < -.52f && py > -.91f) { solid = true; detail = true; }
                        break;
                    case 1: // Downward scout with two cut-out wing tips.
                        solid = Polygon(px, -py, Hull); detail = ax < .11f && py < .2f && py > -.5f; break;
                    case 2: // Armored fan ship.
                        solid = ax < .77f && ay < .42f && ax + ay < .94f || ax < .18f && py < -.3f && py > -.78f;
                        detail = (Mathf.Abs(ax - .4f) < .055f && ay < .23f) || r < .13f; break;
                    case 3: // Radial turret with eight spokes.
                        solid = r < .56f || r < .84f && Mathf.Cos(Mathf.Atan2(py, px) * 8) > .72f;
                        detail = r < .18f || Mathf.Abs(r - .39f) < .035f; break;
                    case 4: solid = ax < .16f && ay < .83f; detail = ax < .065f && ay < .7f; break;
                    case 5: solid = r < .6f; detail = r < .27f; break;
                    case 6: case 7: case 8:
                        solid = ax + ay < .9f;
                        if (kind == 6) detail = ax < .085f && ay < .38f || ay < .085f && ax < .38f;
                        if (kind == 7) detail = Polygon(px, py, new[] { new Vector2(.06f,.45f), new Vector2(-.27f,-.03f), new Vector2(-.02f,-.03f), new Vector2(-.08f,-.43f), new Vector2(.29f,.08f), new Vector2(.04f,.08f) });
                        if (kind == 8) detail = ax < .055f && ay < .37f || Mathf.Abs(ax - (py + .35f) * .43f) < .055f && py > -.28f && py < .32f;
                        break;
                    case 9: solid = r < .24f || ax < .045f && ay < .8f || ay < .045f && ax < .8f; detail = r < .13f; break;
                    case 10: solid = ax + ay < .62f; detail = r < .19f; break;
                }
                Color c = Color.clear;
                if (solid)
                {
                    float lighting = Mathf.Clamp01(.76f + py * .22f - px * .12f);
                    c = detail ? new Color(.91f, .98f, 1, 1) : new Color(color.r * lighting, color.g * lighting, color.b * lighting, 1);
                }
                pixels[y * size + x] = c;
            }
            texture.SetPixels(pixels); texture.Apply();
            string path = Folder + "/" + name + ".png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        public static AudioClip Sound(string name, int kind, float seconds)
        {
            const int rate = 22050;
            int count = Mathf.CeilToInt(rate * seconds);
            string path = Folder + "/" + name + ".wav";
            var rng = new System.Random(817 + kind);
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
                writer.Write((short)1); writer.Write((short)1); writer.Write(rate); writer.Write(rate * 2);
                writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
                double phase = 0;
                for (int i = 0; i < count; i++)
                {
                    float t = (float)i / rate, u = (float)i / count;
                    float frequency = kind == 0 ? Mathf.Lerp(1300, 350, u) : kind == 1 ? Mathf.Lerp(370, 150, u) : kind == 2 ? 65 : kind == 3 ? Mathf.Lerp(200, 60, u) : kind == 4 ? (u < .33f ? 523 : u < .66f ? 659 : 784) : (u < .5f ? 330 : 494);
                    phase += 2 * Math.PI * frequency / rate;
                    double signal = Math.Sin(phase);
                    if (kind == 2 || kind == 3) signal = signal * .35 + (rng.NextDouble() * 2 - 1) * .65;
                    double envelope = Math.Min(1, t / .007) * Math.Pow(1 - u, kind == 2 ? 2 : 1.3);
                    writer.Write((short)(signal * envelope * 14500));
                }
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
