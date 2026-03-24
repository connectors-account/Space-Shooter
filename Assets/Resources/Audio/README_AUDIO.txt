=== AUDIO PLACEHOLDER INFORMATION ===

The game runs perfectly without audio files.
The AudioManager will gracefully skip any missing clips.

To add sound effects and music:

1. Place audio files (.wav, .ogg, or .mp3) in this folder
2. Recommended files:
   - menu_music.ogg     (looping background music for main menu)
   - game_music.ogg     (looping background music for gameplay)
   - player_shoot.wav   (short laser/blaster sound)
   - explosion.wav      (enemy/player destruction)
   - player_hit.wav     (player takes damage)
   - shield_hit.wav     (shield absorbs a hit)
   - powerup.wav        (collecting a power-up)
   - button_click.wav   (UI button press)
   - wave_start.wav     (new wave beginning)

3. After importing, assign clips to the AudioManager component:
   - Select the AudioManager GameObject
   - Drag each clip into the matching slot in the Inspector

Free sound effect resources:
- https://freesound.org
- https://opengameart.org
- https://kenney.nl/assets (CC0 game assets)
