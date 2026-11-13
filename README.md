# Space Shooter (Unity, Windows desktop)

A minimal space shooter built entirely from Unity's built-in primitive shapes and colours. No sprites, no imported assets, no sound, and no extra packages. Everything (camera, light, player, enemies, UI) is created by script at runtime, so an empty scene is all you need.

Target: Unity 2022.3 LTS or Unity 6, using the **3D (Built-in Render Pipeline)** template.

## Controls

| Action | Keys |
|---|---|
| Move | Arrow keys or WASD |
| Shoot | Space or left mouse button (hold for continuous fire) |
| Restart (game over screen) | R or the **Restart** button |
| Quit (game over screen) | **Quit** button |

Shoot the red enemies for 10 points each. If an enemy touches your ship, it's game over.

## Build steps

1. **Create the project.** In Unity Hub, create a new project from the **3D (Built-in Render Pipeline)** template (called "3D" in 2022.3, "3D (Built-In Render Pipeline)" in Unity 6) and name it `space_shooter`.
2. **Add the scripts.** Copy this repository's `Assets/Scripts` folder into the project's `Assets` folder. Wait for Unity to finish compiling.
3. **Set the input handling.** Go to **Edit > Project Settings > Player > Other Settings > Active Input Handling** and choose **Both** or **Input Manager (Old)**. Unity 6 projects can default to the new Input System, which would stop the legacy input calls from working. Let Unity restart if it asks.
4. **Save a scene.** Use the default `SampleScene`, or **File > New Scene** and then **File > Save As** (for example, `Assets/Scenes/Main.unity`). You don't need to add anything to it. Any existing camera is reused and any existing lights are switched off at runtime.
5. **Try it (optional).** Press **Play** in the editor.
6. **Add the scene to the build.** Open **File > Build Settings** (Unity 6: **File > Build Profiles**) and click **Add Open Scenes**. Make sure your scene is the only one ticked, or that it is first.
7. **Build for Windows.** Choose **Windows** as the platform, set **Architecture** to **x86_64**, click **Switch Platform** if needed, and then click **Build**. Pick an empty output folder, for example `Build/`.
8. **Run it.** Open the output folder and double-click `space_shooter.exe`.

## Notes

- **Colours in builds.** Materials are copied from the primitives' own built-in default material and then recoloured. Their shader (Standard) is therefore always included in Windows builds, so you don't need to change **Always Included Shaders**. If shapes ever appear pink in a build, add **Standard** under **Project Settings > Graphics > Always Included Shaders**.
- **Window size.** The play area adapts to the window's aspect ratio. To change the default window, go to **Project Settings > Player > Resolution and Presentation**.
- **Tuning.** You can adjust speeds and timings with the public fields at the top of `PlayerController.cs`, `EnemySpawner.cs`, `Enemy.cs` and `Bullet.cs`.

## Scripts

- `GameBootstrap.cs` builds the scene at runtime (camera, light, player, manager) and contains the primitive/material helpers.
- `GameManager.cs` handles the score, game state, IMGUI score display, the game over screen, restart and quit.
- `PlayerController.cs` handles clamped movement and shooting with a fire-rate cooldown.
- `Bullet.cs` makes bullets move upwards and removes them off-screen.
- `Enemy.cs` makes enemies move downwards, removes them off-screen and handles collisions with bullets and the player.
- `EnemySpawner.cs` spawns enemies at a random X position above the screen at fixed intervals.
