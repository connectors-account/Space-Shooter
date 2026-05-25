# 🚀 Space Shooter — Unity 2D Arcade Game

A classic arcade-style space shooter built with Unity. Defend against waves of enemies, collect power-ups, and chase high scores!

![Genre](https://img.shields.io/badge/Genre-Arcade%20Shooter-blue)
![Engine](https://img.shields.io/badge/Engine-Unity%202021.3+-green)
![Platform](https://img.shields.io/badge/Platform-Windows-orange)
![License](https://img.shields.io/badge/License-MIT-yellow)

---

## 🎮 Gameplay

- **Pilot your ship** through endless waves of enemies
- **Shoot down** aliens to score points
- **Collect power-ups** to gain the advantage
- **Survive** as long as possible — waves get harder over time!

### Controls

| Action | Keys |
|--------|------|
| Move | Arrow Keys / WASD |
| Shoot | Space |
| Menu Navigation | Mouse Click |

---

## ✨ Features

### Core Gameplay
- ⬆️ **Player Ship** — Smooth 8-directional movement with screen clamping
- 🔫 **Shooting System** — Hold Space for continuous fire
- ❤️ **Health System** — 5 hit points with invincibility frames after damage
- 💥 **Collision Detection** — Bullets, enemies, and power-ups all interact

### Enemies
- 👾 **4 Movement Patterns** — Straight, Zigzag, Sine wave, Dive-bomb
- 📈 **Progressive Difficulty** — More enemies, tougher health, faster fire rates
- 🌊 **Wave System** — Named waves with brief pauses between them
- 🎯 **Enemy Shooting** — Enemies fire back at you!

### Power-Ups
- 🛡️ **Shield** — Absorbs one hit (cyan diamond)
- ⚡ **Rapid Fire** — Triple fire rate for 8 seconds (gold diamond)
- 🔱 **Multi-Shot** — Three-way spread for 8 seconds (purple diamond)
- 💚 **Health** — Restores one health point (green diamond)

### Visual & Audio
- 🌌 **Parallax Starfield** — Multi-layer scrolling star background
- 💫 **Explosion Effects** — Expanding/fading burst on enemy death
- 🔊 **Procedural Audio** — All sound effects generated at runtime (no audio files needed!)
- ✨ **Visual Feedback** — Damage flash, shield glow, power-up notifications

### UI & Game Flow
- 📋 **Main Menu** — Play and Quit buttons, high score display
- 📊 **In-Game HUD** — Score, wave counter, health hearts
- 💀 **Game Over Screen** — Final score, high score, restart/menu options
- 💾 **Persistent High Score** — Saved between sessions via PlayerPrefs

---

## 🏗️ Architecture

The game uses a **self-bootstrapping** design — a single `GameSetup` component creates all game objects at runtime. No prefab configuration or complex scene setup needed.

### Scripts Overview

| Script | Responsibility |
|--------|---------------|
| `GameSetup.cs` | Master bootstrap — creates all game objects at runtime |
| `PlayerController.cs` | Player movement, shooting, health, power-up effects |
| `EnemyController.cs` | Enemy AI with 4 movement patterns and shooting |
| `BulletController.cs` | Universal bullet behavior (player & enemy) |
| `EnemySpawner.cs` | Wave-based spawning with difficulty progression |
| `PowerUpController.cs` | 4 power-up types with pickup mechanics |
| `GameManager.cs` | Central game state, scoring, and flow control |
| `UIManager.cs` | Programmatic UI creation (menus, HUD, notifications) |
| `ParallaxBackground.cs` | Multi-layer scrolling starfield |
| `AudioManager.cs` | Procedural sound effect synthesis |
| `SpriteFactory.cs` | Runtime geometric sprite generation |
| `ExplosionEffect.cs` | Animated explosion visual |

### Design Principles
- **Zero external assets** — All sprites, audio, and UI created in code
- **Singleton managers** — GameManager and AudioManager use singleton pattern
- **Component-based** — Each behavior is a focused MonoBehaviour
- **Runtime prefabs** — Enemy/bullet/power-up templates created programmatically

---

## 🔧 Building

See **[BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md)** for the complete step-by-step guide.

### Quick Start
1. Install Unity 2021.3+ with Windows Build Support
2. Open this folder as a Unity project
3. Create a scene, add an empty GameObject, attach `GameSetup.cs`
4. Press Play to test, then File → Build Settings → Build

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/          ← All 12 C# game scripts
│   ├── Scenes/           ← Game scene (created in Unity)
│   ├── Prefabs/          ← (Generated at runtime)
│   ├── Materials/        ← (Placeholder)
│   ├── Sprites/          ← (Generated at runtime via SpriteFactory)
│   ├── Audio/            ← (Generated at runtime via AudioManager)
│   └── Resources/        ← (Placeholder)
├── ProjectSettings/      ← Unity project configuration
│   ├── TagManager.asset  ← Custom tags
│   ├── InputManager.asset← Control bindings
│   └── ...
├── Packages/             ← Unity package manifest
├── BUILD_INSTRUCTIONS.md ← Complete build guide
└── README.md             ← This file
```

---

## 🎯 Game Balance

| Wave | Enemies | Enemy HP | Fire Rate | New Patterns |
|------|---------|----------|-----------|--------------|
| 1 | 4 | 1 | Slow | Straight, Zigzag |
| 3 | 6 | 1-2 | Medium | + Sine |
| 5 | 8 | 2 | Fast | + Dive |
| 8+ | 11+ | 2-3 | Very Fast | All patterns, aggressive |

---

## 📝 License

This project is provided as-is for educational and personal use. Feel free to modify and distribute.
