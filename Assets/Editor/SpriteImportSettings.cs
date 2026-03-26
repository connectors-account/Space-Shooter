// ============================================================================
// SpriteImportSettings.cs — Automatically configures imported PNGs as
// pixel-art sprites with correct settings
// ============================================================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SpriteImportSettings : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        // Only auto-configure images inside our Sprites folder
        if (!assetPath.Contains("Assets/Sprites/")) return;

        TextureImporter importer = (TextureImporter)assetImporter;

        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 64;
        importer.filterMode = FilterMode.Point;           // crisp pixel art
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.wrapMode = TextureWrapMode.Repeat;       // for tiling backgrounds
        importer.maxTextureSize = 512;

        // Sprite packing
        importer.spritePackingTag = "SpaceShooterAtlas";

        Debug.Log($"[SpriteImport] Configured pixel-art settings for: {assetPath}");
    }
}
#endif
