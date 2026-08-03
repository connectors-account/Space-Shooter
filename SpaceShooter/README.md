# Space Shooter — C# Windows Desktop Game

A fully self-contained, zero-dependency space shooter built with
**C# (.NET 8) + Windows Forms + GDI+**.
No Unity, no third-party packages — just the .NET SDK.

---

## Project files

```
SpaceShooter/
├── SpaceShooter.csproj   ← project definition
├── Program.cs            ← entry point
├── GameManager.cs        ← score, lives, wave, high-score, game state
├── Player.cs             ← movement, shooting, invincibility frames
├── Enemy.cs              ← 3 enemy types with different bullet patterns
├── Bullet.cs             ← projectile logic (player & enemy)
├── EnemySpawner.cs       ← wave-based spawn scheduler
└── GameForm.cs           ← game loop, rendering, input, HUD, menus
```

---

## Requirements

| Tool | Version |
|------|---------|
| .NET SDK | 8.0 or later |
| OS | Windows 10 / 11 |

Install the SDK from: https://dotnet.microsoft.com/download

---

## Run in development

```bash
cd SpaceShooter
dotnet run
```

---

## Build a standalone Windows .exe

```bash
cd SpaceShooter
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./dist
```

The single executable is created at:

```
SpaceShooter/dist/SpaceShooter.exe
```

Double-click it — no runtime installation needed on the target machine.

---

## Controls

| Key | Action |
|-----|--------|
| W / ↑ | Move up |
| S / ↓ | Move down |
| A / ← | Move left |
| D / → | Move right |
| SPACE | Shoot |
| ENTER | Start / Restart |
| ESC | Quit |

---

## Gameplay

- **3 enemy types:**
  - 🔴 **Basic** — straight down, single shot.
  - 🟡 **Zigzag** — sine-wave drift, faster fire rate.
  - 🟣 **Tank** — slow, high HP, three-bullet spread shot.
- **Waves** — each wave spawns more enemies that are faster and shoot more often.
- **Score** — Basic 100 pts · Zigzag 150 pts · Tank 350 pts.
- **Lives** — 3 lives; your ship flashes after taking a hit (invincibility window).
- **Health bar** — shown bottom-left; replenishes on next life.
- **Hi-Score** — tracked across rounds in the same session.
- **Explosion particles** — coloured sparks on every kill.
- **Parallax starfield** — multi-speed scrolling star layers for depth.

---

## Architecture notes

- **Double-buffered GDI+** rendering (`Bitmap` back-buffer → `Panel.Paint`).
- **Fixed-interval `Timer`** at ~62 fps; delta-time capped at 50 ms to prevent physics spikes.
- All shapes are drawn **procedurally** — no sprite files are needed.
- Each class is self-contained and easy to extend (add power-ups, sounds, etc.).
