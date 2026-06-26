# Build Instructions — Windows Standalone

This guide walks through compiling **Space Shooter** into a standalone Windows executable (`.exe`).

---

## 1. Prerequisites
- **Unity Hub** installed.
- **Unity 2022.3 LTS** editor installed (developed on `2022.3.40f1`).
- During installation (or via *Unity Hub → Installs → ⚙ → Add Modules*) make sure
  **"Windows Build Support (Mono)"** and/or **"Windows Build Support (IL2CPP)"** are checked.
  - *Mono* builds faster and is fine for testing.
  - *IL2CPP* produces a faster, harder-to-decompile build (recommended for release; requires
    *Visual Studio with the "Desktop development with C++" workload*).

---

## 2. Open the project
1. Launch **Unity Hub → Open → Add project from disk**.
2. Select the `SpaceShooter/` folder (the one containing `Assets/`, `Packages/`, `ProjectSettings/`).
3. Wait for the initial import and script compilation to finish. Confirm there are **no compiler errors**
   in the **Console** (red messages). Warnings are fine.

---

## 3. Verify Build Settings scenes
1. Open **File → Build Settings…**
2. Under **Scenes In Build** you should see, in this order:
   - `Assets/Scenes/MainMenu.unity`  → index **0**
   - `Assets/Scenes/GamePlay.unity`  → index **1**
3. If they are missing:
   - Open `MainMenu.unity`, then click **Add Open Scenes**.
   - Open `GamePlay.unity`, then click **Add Open Scenes**.
   - Drag **MainMenu** above **GamePlay** so MainMenu is index 0.
   - Tick the checkbox next to each so both are **enabled**.

> The project ships with `ProjectSettings/EditorBuildSettings.asset` already listing both scenes, so this is
> usually pre-configured.

---

## 4. Select the target platform
1. Still in **Build Settings**, select **Windows, Mac, Linux** in the platform list.
2. If it is not already the active platform, click **Switch Platform** (this may take a moment).
3. Set:
   - **Target Platform**: `Windows`
   - **Architecture**: `Intel 64-bit (x86_64)`

---

## 5. (Optional) Player Settings
Open **Edit → Project Settings → Player** to customise:
- **Company Name** / **Product Name** (the product name becomes the `.exe` name).
- **Resolution and Presentation**: default windowed 1280×720 works well; enable *Run In Background* if desired.
- **Icon**: assign a custom icon if you have one.
- **Scripting Backend** (under *Configuration*): `Mono` (default) or `IL2CPP` (release).

---

## 6. Build
1. In **Build Settings**, click **Build** (or **Build And Run** to launch immediately).
2. Choose/create an **empty output folder**, e.g. `SpaceShooter/Build/Windows/`.
3. Unity compiles and writes the player. When finished you'll have:
   ```
   Build/Windows/
   ├── SpaceShooter.exe          # the game executable
   ├── UnityPlayer.dll
   ├── SpaceShooter_Data/        # all game data
   └── MonoBleedingEdge/ (Mono builds)
   ```

---

## 7. Run
- Double-click **`SpaceShooter.exe`**.
- To distribute, zip the **entire** output folder — the `.exe` requires the sibling `*_Data` folder and DLLs.

---

## 8. Command-line build (CI / automation, optional)
You can build headlessly with the Unity CLI. Example (adjust the Unity path and project path):

```bat
"C:\Program Files\Unity\Hub\Editor\2022.3.40f1\Editor\Unity.exe" ^
  -quit -batchmode -nographics ^
  -projectPath "C:\path\to\SpaceShooter" ^
  -buildWindows64Player "C:\path\to\SpaceShooter\Build\Windows\SpaceShooter.exe" ^
  -logFile "C:\path\to\SpaceShooter\build.log"
```

For full control (scenes, options) create an editor build script under `Assets/Editor/` that calls
`BuildPipeline.BuildPlayer(...)` and invoke it with `-executeMethod`. This is optional — the GUI build in
steps 4–6 is sufficient for most users.

---

## 9. Common issues
| Symptom | Fix |
|---------|-----|
| "No scenes in build" / black screen | Re-add both scenes in Build Settings (step 3) and ensure they're enabled. |
| Compiler errors on import | Confirm you're on Unity **2022.3 LTS**; let the package manager finish resolving built-in modules. |
| Build button greyed out | Switch the platform to **Windows** first (step 4). |
| IL2CPP build fails | Install **Visual Studio** with the *Desktop development with C++* workload, then rebuild. |
| `.exe` won't start on another PC | Ship the whole output folder (the `_Data` folder + DLLs must accompany the exe). |
