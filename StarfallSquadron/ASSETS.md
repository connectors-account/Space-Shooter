### Asset provenance

- `Assets/Starfall/Art/atlas.png`: AI-generated for this project using GPT Image 2.5. Original generated download: https://cdn.abacus.ai/images/8c840e91-1a75-49a3-abd3-c789e7c72d41.png . The full PNG is bundled; nothing is hot-linked at runtime.
- `player`, `scout`, `fighter`, `gunship`, `boss`, `repair`, `spread`, `shield` PNGs: cropped, alpha-cleaned, resized versions of that atlas. They are the actual shipped illustrations, not stand-in assets.
- `bolt.png`, `particle.png`: neutral geometric rendering primitives produced by the preparation script, tinted in the game for projectiles, stars, and sparks.
- `Assets/Starfall/Audio/*.wav`: original procedural PCM synthesis from `Tools/prepare_assets.py`. Nine one-shot effects plus one four-second seamless ambient drone. Mono, 16-bit, 22,050 Hz. No sampled commercial audio.
- UI font: Unity's built-in GUI font, supplied with Unity, not redistributed separately.
- `Docs/AssetPreview.png`: contact sheet of the bundled assets, not a screenshot of the Unity game.

Source code and original synthesized audio/geometric assets are provided under the included MIT license. Generated illustrations are provided for use, modification and redistribution with this project; no exclusivity or copyright-registration claim is made for AI-generated artwork. No third-party model or asset-store content is included. Unity itself remains subject to Unity's license.
