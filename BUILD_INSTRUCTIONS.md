# Build Instructions — Windows Standalone (.exe)

Step-by-step guide to produce a runnable `Space Shooter.exe`. Assumes you have already opened
the project and run **Space Shooter ▸ Setup Game** at least once (see `README.md`).

---

## A. Install Windows Build Support (one time)

If you installed Unity through **Unity Hub**:

1. Open **Unity Hub ▸ Installs**.
2. Click the **⚙ (gear) ▸ Add modules** on your editor version.
3. Tick **Windows Build Support (Mono)** — and optionally **(IL2CPP)** for a faster, harder-to-
   decompile build.
4. Click **Install** and wait for it to finish.

> If you are already on Windows with the standard editor install, this module is usually present.

---

## B. Generate scenes & prefabs (if not done yet)

1. Open the project in Unity.
2. Menu bar ▸ **Space Shooter ▸ Setup Game**.
3. Wait for the **"Setup complete!"** dialog.

This guarantees `Assets/Scenes/MainMenu.unity` and `Assets/Scenes/GameScene.unity` exist and are
already added to Build Settings.

---

## C. Configure Build Settings

1. Menu bar ▸ **File ▸ Build Settings…**
2. Under **Scenes In Build**, confirm the list is exactly:
   - `Scenes/MainMenu`   → index **0**
   - `Scenes/GameScene`  → index **1**

   The **MainMenu must be index 0** so the game boots into the menu.
   - Missing scenes? Open each scene, then click **Add Open Scenes**, or re-run
     *Space Shooter ▸ Setup Game*.
   - Wrong order? Drag `MainMenu` to the top of the list.
3. In the **Platform** list on the left, select **Windows, Mac, Linux**.
4. On the right set:
   - **Target Platform:** `Windows`
   - **Architecture:** `Intel 64-bit (x86_64)`
5. If the platform is not already active, click **Switch Platform** (this may take a minute).

---

## D. (Optional) Player Settings

1. In **Build Settings**, click **Player Settings…** (bottom-left).
2. Recommended values (the setup script already sets several of these):
   - **Company Name:** `IndieDev`
   - **Product Name:** `Space Shooter`
   - **Resolution and Presentation ▸ Fullscreen Mode:** `Windowed` or `Fullscreen Window`
   - **Default Screen Width / Height:** `1920 × 1080`
3. (Optional) **Other Settings ▸ Configuration ▸ Scripting Backend:** choose `IL2CPP` for a
   more optimized build, or leave `Mono` for faster build times.

---

## E. Build

1. Back in **File ▸ Build Settings…**, click **Build** (not *Build And Run* the first time, so
   you can inspect the output).
2. Choose/create an output folder, e.g. `Builds/Windows`.
3. Wait for the build to complete. Unity opens the folder when done.

You will get:

```
Builds/Windows/
├── Space Shooter.exe          ← run this
├── UnityPlayer.dll
├── Space Shooter_Data/        ← game data (keep next to the .exe)
└── MonoBleedingEdge/          (Mono backend only)
```

---

## F. Run & distribute

1. Double-click **`Space Shooter.exe`**.
2. To share the game, zip the **entire** `Builds/Windows` folder. The `.exe` will not run
   without its accompanying `_Data` folder and `UnityPlayer.dll`.

---

## Troubleshooting

| Symptom | Fix |
|---|---|
| "Scene couldn't be loaded … not been added to the build settings" | Add both scenes in **File ▸ Build Settings** (section C). |
| Menu shows but **Play** does nothing | Ensure `GameScene` is in Build Settings at index 1. |
| Everything is black / no sprites | Run **Space Shooter ▸ Setup Game**; sprites are generated at runtime, so make sure scripts compiled with no errors. |
| No sound | Sound is procedural; check the OS volume and the in-game Pause menu volume sliders. |
| Buttons not clickable | Confirm an **EventSystem** exists in the scene (created automatically by the setup script). |
| Compile errors after import | Use Unity **2021.3 LTS or newer**; older versions lack some C# 8/9 syntax used here. |

---

Enjoy building and shipping **Space Shooter**! 🚀
