# Audio Assets Guide

## Required Audio Files

Place the following audio files in this folder (`Assets/Audio/`):

### Music
- **bgm_gameplay.ogg** — Looping background music (electronic/synthwave style)
  - Format: OGG Vorbis or MP3
  - Suggested length: 60-120 seconds, seamlessly loopable

### Sound Effects
- **sfx_player_shoot.wav** — Player laser/bullet fire (short, ~0.1s)
- **sfx_enemy_shoot.wav** — Enemy bullet fire (slightly different tone)
- **sfx_explosion.wav** — Explosion when enemy dies (~0.3s)
- **sfx_player_hit.wav** — Player takes damage (~0.2s)
- **sfx_powerup.wav** — Power-up collected (positive chime, ~0.3s)
- **sfx_game_over.wav** — Game over jingle (~1-2s)
- **sfx_wave_start.wav** — New wave announcement (~0.5s)

## Free Audio Sources
- **Kenney.nl** → "Impact Sounds" and "UI Audio" (CC0)
- **Freesound.org** → Search "laser", "explosion", "8-bit" (check licenses)
- **OpenGameArt.org** → Search "space music", "sci-fi sfx"
- **BFXR** (bfxr.net) → Generate retro sound effects in browser

## Import Settings
1. Select audio clip in Unity Inspector
2. For **SFX**: Load Type = Decompress On Load, Compression = PCM or Vorbis
3. For **Music**: Load Type = Streaming, Compression = Vorbis, Quality = 70%
4. Click **Apply**
