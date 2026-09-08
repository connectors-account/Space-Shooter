# Space Shooter — Desktop Game

A complete 2D space shooter built with **Python + Pygame**.  
No Unity, no extra assets — everything is drawn in code.  
Compiles to a standalone **Windows .exe** with one command.

---

## Requirements

- Python 3.9 or newer  
- pip

---

## Run directly (dev mode)

```bash
pip install pygame numpy
python game.py
```

---

## Build Windows .exe

```bash
pip install pygame numpy pyinstaller
pyinstaller --onefile --noconsole game.py
```

The executable is created at:

```
dist/game.exe
```

Double-click `dist/game.exe` — no Python install needed on the target machine.

---

## Controls

| Key | Action |
|-----|--------|
| Arrow Keys / WASD | Move |
| SPACE | Shoot |
| P / ESC | Pause |
| ENTER | Confirm menus |
| M | Return to Main Menu |
| R | Restart (from pause) |

---

## Features

- **10 waves** of escalating enemies
- **3 enemy types** (Grunt, Zigzagger, Circler) + **Boss** (2-phase)
- **Bullet patterns**: single, triple-shot, 5-way spread, circle burst, aimed
- **4 power-ups**: Shield, Triple Shot, Speed Boost, Health Pack
- **Parallax starfield** (3 layers, procedural, no image files)
- **Screen shake** on boss phase transitions and hits
- **Synthesised sound effects** (no audio files needed — generated with numpy)
- **HUD**: HP bar, score, wave number, buff timers, boss HP bar
- **Main Menu**, **Pause**, **Game Over**, **Victory** screens
- **Animated score count-up** on Game Over
- **High score** saved to `highscore.txt` next to the executable

---

## File structure

```
SpaceShooterPygame/
├── game.py           ← entire game (single file, ~500 lines)
├── requirements.txt
└── README.md
```
