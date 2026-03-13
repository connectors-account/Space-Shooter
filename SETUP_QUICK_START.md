# Quick Start Guide

This is a condensed setup guide to get the game running as fast as possible.

## 5-Minute Setup

### 1. Install Unity (if not installed)
- Download [Unity Hub](https://unity.com/download)
- Install Unity **2022.3 LTS** with **Windows Build Support**

### 2. Create Project
- Unity Hub → New Project → **2D (Built-in Render Pipeline)**
- Name: `SpaceShooter`
- Create project

### 3. Copy Scripts
- Copy all `.cs` files from `Assets/Scripts/` to your Unity project's `Assets/Scripts/`

### 4. Add Tags
In Unity: **Edit → Project Settings → Tags and Layers → Tags**

Add these tags:
- `Player`
- `Enemy`
- `Bullet`
- `PowerUp`

### 5. Auto-Setup
1. In Unity: **GameObject → Create Empty**
2. Name it `GameSetup`
3. Add Component → **GameSetup** script
4. Press **Play**
5. Game auto-configures!

### 6. Build for Windows
1. **File → Build Settings**
2. Platform: **Windows, Mac, Linux**
3. Click **Add Open Scenes**
4. Click **Build**
5. Select output folder
6. Done! Run the `.exe` file

## Controls
- **WASD/Arrows**: Move
- **Space**: Shoot
- **Enter**: Start game
- **Escape**: Pause
- **R**: Restart (after game over)

## That's it!

Enjoy your space shooter game!
