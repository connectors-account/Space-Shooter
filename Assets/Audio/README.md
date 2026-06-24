# Audio Assets

The `AudioManager` is fully wired and ready to play sounds. It is designed so
the game runs perfectly **even with no audio files** — any unassigned clip is
simply skipped.

## Adding your own sounds

1. Drop your audio files (`.wav`, `.ogg`, or `.mp3`) into this folder.
2. In the scene, select the **Managers** GameObject.
3. On the **Audio Manager** component, drag each clip into the matching slot:

| Slot                | Suggested sound                     |
|---------------------|-------------------------------------|
| Player Shoot Clip   | short laser "pew"                   |
| Enemy Shoot Clip    | lower-pitched laser                 |
| Explosion Clip      | small explosion / boom              |
| Power Up Clip       | pleasant chime / pickup             |
| Wave Start Clip     | alert / whoosh                      |
| Game Over Clip      | descending tone                     |
| Button Click Clip   | UI click                            |
| Background Music    | looping space ambient track         |

## Free sound resources
- https://freesound.org
- https://opengameart.org
- https://sfxr.me  (generate retro 8-bit SFX in the browser)
