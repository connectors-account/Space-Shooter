Audio Placeholder Instructions
============================

The game currently generates simple procedural beep sounds in AudioManager.cs.

To replace with custom SFX:
1. Add your audio files to this Assets/Audio folder (e.g., shoot.wav, explosion.wav, powerup.wav).
2. Update AudioManager.cs to load clips from Resources or serialized fields.
3. Optional: create an AudioMixer and route effects for volume control.

Suggested replacements:
- Shoot: short, high-pitch laser
- Explosion: low thump burst
- Power-up: bright ascending chime
