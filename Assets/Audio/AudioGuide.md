# Audio Placeholder Setup Guide

Add your temporary or final audio clips into `Assets/Audio/`.

Recommended files:
- `music_gameplay_loop.wav`
- `sfx_player_shoot.wav`
- `sfx_enemy_shoot.wav`
- `sfx_player_hit.wav`
- `sfx_player_death.wav`
- `sfx_enemy_death.wav`
- `sfx_powerup.wav`

## Setup in Unity

1. Select `AudioManager` object in `GamePlay` scene.
2. Ensure it has 2 AudioSources:
   - `SFX Source` (Play On Awake OFF)
   - `Music Source` (Play On Awake OFF)
3. In `AudioManager` inspector, assign clips:
   - Gameplay Music Clip
   - Player Shoot Clip
   - Enemy Shoot Clip
   - Player Hit Clip
   - Player Death Clip
   - Enemy Death Clip
   - PowerUp Clip

## Import Settings Tips

- Load Type:
  - SFX: `Decompress On Load`
  - Music: `Streaming`
- Compression Format:
  - WAV or OGG Vorbis both work

If no clips are assigned, gameplay still functions without audio errors.
