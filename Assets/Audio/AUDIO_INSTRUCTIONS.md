# Audio Instructions

The game ships with **procedurally synthesised sound effects and music** (see
`Assets/Scripts/Managers/AudioManager.cs`), so it is audible without any imported audio. To use your own
audio, import clips here and register them — a registered clip overrides the synthesised default with the
same key.

## Sound keys used by the game
| Key           | When it plays                  | Suggested type            |
|---------------|--------------------------------|---------------------------|
| `player_shot` | Player fires                   | Short laser/“pew”         |
| `enemy_shot`  | Enemy fires                    | Lower-pitched shot        |
| `explosion`   | Enemy/player destroyed         | Noise burst               |
| `player_hit`  | Player takes damage            | Low thud / alarm          |
| `powerup`     | Power-up collected             | Rising chime              |
| `ui_click`    | UI button pressed              | Soft click                |
| `wave`        | New wave begins                | Short fanfare             |
| `music`       | Looping background music       | Ambient loop              |

## Import settings
- **player_shot / enemy_shot / explosion / player_hit / powerup / ui_click / wave**
  - Load Type: `Decompress On Load`
  - Preload Audio Data: on
  - Format: PCM or Vorbis
- **music**
  - Load Type: `Streaming`
  - Loop the asset itself, or rely on the looping `AudioSource` (already configured).

## Registering custom clips
After the managers initialise (e.g. from any script that runs after the bootstrap), call:

```csharp
using SpaceShooter.Managers;

AudioManager.Instance.RegisterClip("explosion", myExplosionClip);
AudioManager.Instance.RegisterClip("music", myMusicClip);
```

A simple integration approach:
1. Put your clips in `Assets/Resources/Audio/`.
2. In a small `MonoBehaviour` (added to the GamePlay scene, or extend `GameBootstrap`), load and register them:

```csharp
AudioManager.Instance.RegisterClip("player_shot",
    Resources.Load<AudioClip>("Audio/player_shot"));
```

Volume can be tuned at runtime via `AudioManager.Instance.SetSfxVolume(0..1)` and `SetMusicVolume(0..1)`.
