### Asset provenance

| Files | Origin | Use |
|---|---|---|
| `Assets/Art/source-sheet.png` | AI-generated original sprite sheet created specifically for Nova Guard with GPT Image 2.5 | Source artwork, not a gameplay screenshot |
| `Assets/Art/Player.png`, `Scout.png`, `Gunner.png`, `Tank.png` | Cropped/resized from that original sheet | Four ship sprites |
| `Assets/Art/Repair.png`, `Rapid.png`, `Shield.png` | Cropped/resized from that original sheet | Three power-up sprites |
| `Assets/Art/PlayerBolt.png`, `EnemyBolt.png` | Cropped/resized from that original sheet | Two laser sprites |
| `Assets/Audio/*.wav` | Original deterministic sine/harmonic/noise synthesis in `Tools/prepare_assets.py` | Eight gameplay/UI effects |
| Starfield points and impact sparks | Tiny geometric marks created by `Starfield.cs` and `HitBurst.cs` | Moving background and feedback, not replacements for ship artwork |
| `LegacyRuntime.ttf` | Built-in Unity font resource | Runtime uGUI text; subject to Unity's license |

The generated image was downloaded and is bundled locally; the game never hot-links an asset URL. There are no third-party downloaded ship models, stock sprite packs, music tracks, or sound libraries. Generated-art use remains subject to the applicable generation-service terms; no exclusive copyright claim is made for AI output. Unity Editor/player and official Unity packages retain their own licenses.

`Tools/prepare_assets.py` reproduces the crops and all WAV files from the included source sheet without internet access. It does not regenerate the AI artwork. The game uses the already-supplied files, so neither Python nor image-generation access is needed to build it.
