# Star Defender — Space Shooter

Single-file Python/Pygame desktop game. No external assets required — everything is drawn in code.

---

## Run instantly

```bash
pip install pygame
python space_shooter.py
```

---

## Controls

| Key | Action |
|-----|--------|
| **WASD** / **Arrow Keys** | Move ship |
| **Space** / **Z** | Fire |
| **ESC** | Pause / Resume |
| **R** (Game Over/Victory) | Retry |
| **M** (Game Over/Victory) | Main Menu |

---

## Features

- **3 enemy types** — Basic (straight), Fighter (sine-wave + aimed), Boss (3 phases)
- **4 waves** — escalating difficulty, final wave is the Boss
- **6 power-ups** — Health, Shield, Rapid Fire, Triple Shot, Speed Boost, Bomb
- **Combo scoring** — chain kills for bonus points
- **Parallax starfield** — 3-layer scrolling background
- **Boss fight** — 3 attack phases, health bar, multi-drop power-ups
- **Full HUD** — score, hi-score, lives, wave, active buffs + timers, boss HP
- **Particle effects** — explosions, hit sparks, floating score text
- **Invincibility frames** — flicker after taking damage
- **Hi-score** — saved in session (add `pickle`/`json` to persist to disk)

---

## Package as Windows .exe (PyInstaller)

```bash
pip install pyinstaller
pyinstaller --onefile --windowed space_shooter.py
```

The executable will be in the `dist/` folder as `space_shooter.exe`.  
Double-click to run — no Python installation needed on the target machine.

---

## Project structure

```
SpaceShooterPy/
├── space_shooter.py   ← entire game (single file, ~750 lines)
├── requirements.txt
└── README.md
```
