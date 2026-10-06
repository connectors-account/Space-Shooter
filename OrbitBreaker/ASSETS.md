### Asset provenance

All game image and audio files are included locally. Nothing is fetched at runtime.

| Files | Origin and usage |
|---|---|
| `Assets/Art/Source/ships-atlas.png` | Original AI-generated four-ship atlas created for this project. Not a copied commercial/third-party sprite pack. |
| `player.png`, `scout.png`, `fan.png`, `spinner.png` | Cropped, alpha-cleaned, and resized from that atlas by `Tools/make_assets.py`; 256×256 RGBA each. |
| `repair.png`, `rapid.png`, `triple.png` | Original procedural geometric UI-like pickup symbols, 64×64 RGBA. |
| `bolt.png`, `orb.png`, `spark.png` | Original procedural geometric projectile / fading spark textures. |
| `stars-far.png`, `stars-near.png` | Original seeded procedural star textures, 512×512 RGBA. |
| `Assets/Audio/*.wav` | Original synthesized chirps, sweeps, and noise bursts from `Tools/make_assets.py`. Mono, 44.1 kHz, 16-bit PCM. No sampled recordings. |
| `LegacyRuntime.ttf` | Unity's built-in runtime font, loaded through Unity at runtime. Not redistributed as a separate font file in this source bundle. Covered by Unity's licensing terms. |

Ship-atlas generation source: https://cdn.abacus.ai/images/8e8628fd-7047-4e2e-98f0-2795535e2e55.png . The local atlas is the authoritative copy; the project never hot-links this URL.

SFX roles: `shoot`, `enemy-shot`, `hit`, `explode`, `pickup`, `wave`, `game-over`, `ui`. Pausing stops audio via AudioListener; the mute setting affects the game's AudioSource.

The included source and original project assets are supplied under the accompanying MIT license to the extent rights can be granted. No claim of exclusive copyright or trademark clearance is made for AI-generated artwork. Unity packages and Unity's built-in resources retain their own licenses; no Unity engine binary is redistributed here.
