# Space Shooter — Python / Pygame Desktop Edition

A simple, self-contained **desktop** space-shooter game in a single Python file.
No external art or audio assets required — every visual is drawn with Pygame primitives.

## Requirements
- Python 3.8+
- [pygame](https://www.pygame.org/) (`pip install pygame`)

## Run
```bash
pip install pygame
python space_shooter.py
```

## Controls
| Key | Action |
|-----|--------|
| `WASD` / Arrow keys | Move ship |
| `SPACE` | Shoot |
| `P` | Pause / Resume |
| `ESC` / `ENTER` | Menu navigation / Quit |

## Features
- 3-layer parallax star-field background
- Player ship with tilt, shield ring, invincibility frames and 3 lives
- Three enemy types — Basic (triangle), Fast (zigzag diamond), Tank (spread-firing hexagon)
- Boss battle every 5th wave with 3 attack phases
- Procedurally generated **infinite** waves that scale in difficulty
- 5 power-ups — Health, Shield, Rapid Fire, Triple Shot, Bomb
- Particle explosions, on-screen HUD, score, high-score and game-over screens

## Build a standalone Windows .exe (optional)
```bash
pip install pyinstaller
pyinstaller --onefile --windowed space_shooter.py
# Output: dist/space_shooter.exe
```
