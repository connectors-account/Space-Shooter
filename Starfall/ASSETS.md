### Asset provenance

The two source sheets were newly generated for this project through the image-generation workflow and supplied as actual PNG files. They were **not** replaced with procedural ship drawings or stock asset placeholders. No further image generation was performed during final integration. The earlier `Tools/generate_art.py` helper is not the source of proof for the final media's model/version; the actual delivered PNGs listed below are authoritative.

| Source | Content | SHA-256 |
|---|---|---|
| `ArtSource/ships-original.png` | Up-facing cyan player; down-facing red scout, orange gunship and purple commander | `13867ede1459b50e282d260c77eb075a31f39c145e5bd86dae20cbaec37d2127` |
| `ArtSource/icons-original.png` | Cyan laser; green repair plus; gold rapid-fire lightning; blue shield | `03ad6f80b99babeac230de7df56626f909b67da44cb155b8c1eab1f79632062c` |

Both sheets are 1024 × 1024 pixels with black backgrounds. They are included to preserve the actual source material and allow offline reprocessing. “Original generated” describes their creation for this task, not a guarantee of copyright eligibility, exclusivity, or trademark clearance. Generated-media use remains subject to the generating service's applicable terms; no unsupported CC0 or third-party license claim is made.

### Prepared sprites

`Tools/prepare_assets.py` produces ten 256 × 256 RGBA PNGs in `Assets/SpaceShooter/Art`:

- **Player, Scout, Gunship, Commander:** extracted from the ship sheet. The actual horizontal gutter is at source y=400; a naïve y=512 split would cut lower-ship exhausts. Exterior-black flood removal preserves dark internal panels. Aspect ratio is preserved and content is centered with transparent padding.
- **Bolt, Repair, RapidFire, Shield:** extracted from the icon sheet. Black-matte removal recovers smooth transparency so the original luminous artwork does not have opaque dark borders on the game's background.
- **EnemyBolt:** recolored derivative of the original generated cyan laser, preserving its alpha silhouette.
- **Star:** extraction from that same generated laser's luminous core, resized and feathered with a radial alpha mask. This is a derivative media crop, not new image generation. Its edges and corners are transparent; it is not an opaque square patch.

`Documentation/AssetPreview.png` is a contact sheet of the final files composited on a dark background. It is **not a Unity screenshot or evidence of gameplay rendering**. The sprite shapes, retained exhausts, icon transparency and feathered star were visually inspected there.

### Original synthesized audio

`Tools/prepare_assets.py` also synthesizes five effects from mathematical waveforms and deterministic noise using Python's standard library. There are **no sampled recordings, downloaded sound packs, external music tracks or third-party sound licenses** in these effects. All output files are mono, 16-bit PCM WAV at 22,050 Hz:

| File in `Assets/SpaceShooter/Audio` | Synthesis | Nominal duration |
|---|---|---|
| `Laser.wav` | Falling sine sweep with harmonic | 0.13 s |
| `Hit.wav` | Filtered noise and descending low tone | 0.21 s |
| `Explosion.wav` | Longer filtered-noise / low-tone decay | 0.50 s |
| `Pickup.wav` | Four-note rising synthesized chime | 0.45 s |
| `GameOver.wav` | Four-note descending synthesized cue | 1.05 s |

Durations are quantized to whole samples. A fixed random seed, short attack and decay envelopes make the output reproducible and keep the endpoints near zero. Offline checks verified file encoding, expected frame counts, non-silence and unclipped PCM peaks. Playback, game mix and subjective loudness still require Unity/Windows testing; no listening test is claimed.

### Other dependencies

Unity Editor/runtime and built-in package modules remain governed by Unity's terms. No paid package or external font is included. Pillow is used only by the optional offline image-maintenance tools and is not bundled. The game itself uses Unity's built-in sprite materials, fonts, audio and IMGUI.
