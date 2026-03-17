# Audio Assets Guide

## Required Sound Effects

Place your audio files in the appropriate folders:

### SFX Folder (Assets/Audio/SFX/)
- `player_shoot.wav` - Player weapon firing sound
- `enemy_shoot.wav` - Enemy weapon firing sound  
- `explosion.wav` - Explosion sound for enemy/player destruction
- `player_hit.wav` - Sound when player takes damage
- `powerup_collect.wav` - Sound when collecting power-ups
- `button_click.wav` - UI button click sound

### Music Folder (Assets/Audio/Music/)
- `menu_music.mp3` - Main menu background music (looping)
- `game_music.mp3` - In-game background music (looping)
- `boss_music.mp3` - Boss battle music (looping)
- `gameover_music.mp3` - Game over screen music

## Recommended Audio Settings

### For Sound Effects:
- Format: WAV or OGG
- Sample Rate: 44100 Hz
- Channels: Mono
- Load Type: Decompress On Load

### For Music:
- Format: MP3 or OGG
- Sample Rate: 44100 Hz  
- Channels: Stereo
- Load Type: Streaming

## Free Audio Resources

You can find free game audio at:
- [Freesound.org](https://freesound.org)
- [OpenGameArt.org](https://opengameart.org)
- [Kenney Assets](https://kenney.nl/assets)
- [ZapSplat](https://www.zapsplat.com)

## Importing Audio in Unity

1. Drag audio files into the appropriate folder
2. Select the audio file in Project window
3. In Inspector, set:
   - Load Type: Based on usage (see above)
   - Compression Format: Vorbis for music, PCM for short SFX
   - Quality: 70-100% based on needs
4. Click Apply
