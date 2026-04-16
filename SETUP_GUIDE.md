# Setup Guide (Unity Space Shooter)

This project is now self-contained and auto-configured.

## 1) Open in Unity

- Use **Unity 2022.3 LTS**.
- Open the repo root folder in Unity Hub.

## 2) Auto Content Generation

On first import, the editor script automatically ensures:
- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/GamePlay.unity`
- `Assets/Scenes/GameOver.unity`
- `Assets/Prefabs/*`
- `Assets/Sprites/*`

If you need to regenerate manually:
- **Tools → Space Shooter → Regenerate Project Content**

## 3) Play

- Open `Assets/Scenes/MainMenu.unity`
- Press Play

## 4) Build Windows EXE

- File → Build Settings
- Add/verify scenes in this order:
  1. MainMenu
  2. GamePlay
  3. GameOver
- Platform: PC, Mac & Linux Standalone
- Target: Windows x86_64
- Build

## 5) Controls

- Move: WASD / Arrow keys
- Shoot: Space / Left Mouse Button
- Pause: Esc
