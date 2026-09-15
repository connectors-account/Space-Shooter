# Space Shooter — Desktop Game

Single-file Python + Pygame game. No Unity, no IDE, no assets needed.

## Requirements
```
Python 3.8+  (https://python.org)
pygame       pip install pygame
```

## Run
```
python space_shooter.py
```

## Build a standalone Windows .exe (optional)
```
pip install pyinstaller
pyinstaller --onefile --noconsole space_shooter.py
# Output: dist/space_shooter.exe
```

## Controls
| Key | Action |
|-----|--------|
| Arrow Keys / WASD | Move ship |
| Space / Z | Fire |
| P | Pause |
| Esc | Quit |

## Features
- 3 enemy types: Basic, Fast (zigzag), Tank (high HP)
- 5 weapon upgrade levels
- Power-ups: Weapon Up, Shield, Extra Life, Bomb (screen clear)
- Infinite waves with scaling difficulty
- Combo scoring
- Star-field parallax background
- Explosions, invincibility frames, shield mechanic
- High-score persistence across sessions
