# Detailed Scene Setup Guide

This guide walks you through manually setting up the Unity scenes.

## Setting Up the Main Menu Scene

### Step 1: Create New Scene
1. Open Unity
2. Go to **File** → **New Scene** → **Empty Scene**
3. Save immediately as `Assets/Scenes/MainMenu.unity`

### Step 2: Create Camera
1. Right-click in Hierarchy → **Camera**
2. Name it "Main Camera"
3. Tag it as "MainCamera"
4. Set these properties:
   - Clear Flags: Solid Color
   - Background: Dark blue (R:13, G:13, B:38)
   - Projection: Orthographic
   - Size: 5

### Step 3: Create Scene Manager
1. Right-click in Hierarchy → **Create Empty**
2. Name it "SceneManager"
3. Click **Add Component** → search "MainMenuSetup"
4. Enable "Setup On Start" checkbox

### Step 4: Create GameManager (Persistent)
1. Right-click in Hierarchy → **Create Empty**
2. Name it "GameManager"
3. Click **Add Component** → search "GameManager"

### Step 5: Save Scene
- Press **Ctrl+S** to save

---

## Setting Up the Game Scene

### Step 1: Create New Scene
1. Go to **File** → **New Scene** → **Empty Scene**
2. Save immediately as `Assets/Scenes/Game.unity`

### Step 2: Create Camera
1. Right-click in Hierarchy → **Camera**
2. Name it "Main Camera"
3. Tag it as "MainCamera"
4. Set properties:
   - Clear Flags: Solid Color
   - Background: Dark blue (R:13, G:13, B:38)
   - Projection: Orthographic
   - Size: 5

### Step 3: Create Scene Setup
1. Right-click in Hierarchy → **Create Empty**
2. Name it "SceneSetup"
3. Add Component → "SceneSetup"
4. Check all boxes:
   - ✅ Setup On Start
   - ✅ Create Player
   - ✅ Create UI
   - ✅ Create Background
   - ✅ Create Spawners

### Step 4: Create Prefab Generator
1. Right-click in Hierarchy → **Create Empty**
2. Name it "PrefabGenerator"
3. Add Component → "PrefabGenerator"

### Step 5: Save Scene
- Press **Ctrl+S** to save

---

## Alternative: Manual Object Setup

If you prefer to set up objects manually instead of using the auto-setup scripts:

### Create Player Manually

1. **Create Player Object**
   - Right-click Hierarchy → **2D Object** → **Sprite** → **Square**
   - Name it "Player"
   - Tag it as "Player"
   - Position: (0, -3.5, 0)

2. **Add Components**
   - Add Component → **Box Collider 2D**
     - Is Trigger: ✅
     - Size: (0.8, 0.8)
   - Add Component → **Rigidbody 2D**
     - Gravity Scale: 0
     - Constraints → Freeze Rotation: ✅
   - Add Component → **PlayerController**

3. **Create Fire Point**
   - Right-click on Player → **Create Empty**
   - Name it "FirePoint"
   - Position: (0, 0.5, 0)

4. **Create Shield Visual**
   - Right-click on Player → **2D Object** → **Sprite** → **Circle**
   - Name it "ShieldVisual"
   - Scale: (2, 2, 1)
   - Color: Cyan with low alpha
   - Disable it (uncheck the checkbox)

### Create Bullet Prefab

1. **Create Bullet Object**
   - Right-click Hierarchy → **2D Object** → **Sprite** → **Square**
   - Name it "PlayerBullet"
   - Tag it as "PlayerBullet"
   - Scale: (0.15, 0.3, 1)
   - Color: Cyan

2. **Add Components**
   - Add Component → **Box Collider 2D**
     - Is Trigger: ✅
   - Add Component → **Rigidbody 2D**
     - Gravity Scale: 0
   - Add Component → **Bullet**

3. **Create Prefab**
   - Drag from Hierarchy into `Assets/Prefabs/Bullets/`
   - Delete from scene

### Create Enemy Prefab

1. **Create Enemy Object**
   - Right-click Hierarchy → **2D Object** → **Sprite** → **Square**
   - Name it "BasicEnemy"
   - Tag it as "Enemy"
   - Color: Red

2. **Add Components**
   - Add Component → **Box Collider 2D** (Is Trigger: ✅)
   - Add Component → **Rigidbody 2D** (Gravity Scale: 0)
   - Add Component → **BasicEnemy**

3. **Create Prefab**
   - Drag into `Assets/Prefabs/Enemies/`
   - Delete from scene

### Wire Up References

After creating prefabs, connect them:

1. Select **PlayerController** on Player
2. Drag **PlayerBullet** prefab to "Bullet Prefab" field
3. Drag **FirePoint** to "Fire Points" array

4. Select **EnemySpawner**
5. Drag enemy prefabs to their respective fields

---

## Build Settings Configuration

1. Go to **File** → **Build Settings**

2. Click **Add Open Scenes** or drag scenes:
   - MainMenu (should be index 0)
   - Game (should be index 1)

3. Platform Settings:
   - Platform: **PC, Mac & Linux Standalone**
   - Target Platform: **Windows**
   - Architecture: **x86_64**

4. Click **Switch Platform** if needed

5. Click **Player Settings** and configure:
   - Product Name: Space Shooter
   - Resolution: 1920 x 1080
   - Fullscreen Mode: Windowed (or Fullscreen Window)

---

## Testing the Game

1. Open the MainMenu scene
2. Press **Play** button
3. Click "Start Game" to begin
4. Test controls:
   - WASD/Arrows to move
   - Space to shoot
   - ESC to pause

## Common Setup Issues

**"Script not found" error:**
- Make sure scripts are in Assets/Scripts folder
- Check script names match class names
- Wait for Unity to finish compiling

**Player doesn't move:**
- Verify the PlayerController component is attached
- Check Edit → Project Settings → Input Manager
- Make sure Rigidbody2D has Freeze Rotation enabled

**Bullets don't appear:**
- Verify bullet prefab is assigned to PlayerController
- Check that bullet prefab has Bullet script attached
- Ensure FirePoint exists as child of Player

**Enemies don't spawn:**
- Check EnemySpawner has enemy prefabs assigned
- Verify GameManager state is "Playing"
- Make sure enemy prefabs have correct Enemy scripts
