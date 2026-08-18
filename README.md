# SpaceShooter — Unity 2D Space Shooter

A complete, top-down arcade space-shooter built in **C# for Unity**, developed
**test-first** with the Unity Test Framework. Every gameplay system ships with a
matching test file written *before* its implementation.

> **Platform target:** Windows standalone (`.exe`). The project also runs in the
> editor Play mode and can be built for macOS/Linux/WebGL with the same scripts.

---

## 1. Requirements

| Tool | Version |
|------|---------|
| Unity Editor | **2022.3 LTS** or newer (tested against 2022.3 / 2023.x) |
| Unity module | *Windows Build Support (IL2CPP)* — install via Unity Hub |
| Test Framework | `com.unity.test-framework` (bundled with Unity; adds NUnit) |
| UI package | `com.unity.ugui` (bundled) — used by all UI scripts |

Everything the scripts depend on (UnityEngine, UnityEngine.UI, physics 2D,
PlayerPrefs, coroutines) is included with a default Unity install. No third-party
packages are required.

---

## 2. Opening the project

1. Install **Unity Hub** and a **2022.3 LTS** (or newer) editor with the
   **Windows Build Support** module.
2. In Unity Hub choose **Add → Add project from disk** and select the
   `SpaceShooter/` folder (the one containing the `Assets/` directory).
3. Open the project. Unity will import assets and generate `Library/`,
   `Packages/manifest.json` and `.meta` files automatically on first import.
4. If the Test Framework is not already present, open
   **Window → Package Manager → Unity Registry**, find **Test Framework** and
   install it. (It is included by default in 2022.3.)

The scripts are organised behind two assembly definitions:

* `Assets/Scripts/SpaceShooterGame.asmdef` — all gameplay code (namespace `SpaceShooter`).
* `Assets/Tests/SpaceShooterTests.asmdef` — all tests (namespace `SpaceShooter.Tests`),
  referencing the game assembly + NUnit + the Test Framework.

---

## 3. Setting up the scenes

The repository ships the **scripts and `.meta` descriptors**; you create the two
scenes inside the editor (Unity stores scenes as machine-generated YAML that must
be authored in the editor). Full step-by-step hierarchies are in
[`Assets/Scenes/SceneList.md`](Assets/Scenes/SceneList.md). In short:

### MainMenu scene (`Assets/Scenes/MainMenu.unity`)
```
Main Camera (Orthographic, size 5, background black)
Managers (empty)
 ├── GameManager      (GameManager.cs)
 ├── ScoreManager     (ScoreManager.cs)
 ├── AudioManager     (AudioManager.cs)
 └── SceneLoader      (SceneLoader.cs)   ← assign the fade CanvasGroup
Background            (ParallaxBackground.cs ×2–3 layers)
Canvas (Screen Space - Overlay)
 ├── Title (Text)                        ← assign to MainMenuController.titleTransform
 ├── PlayButton (Button)
 ├── QuitButton (Button)
 ├── HighScoreText (Text)
 └── FadeOverlay (Image + CanvasGroup)   ← assign to SceneLoader.fadeGroup
MainMenuController (MainMenuController.cs) ← wire the buttons/labels above
```

### Game scene (`Assets/Scenes/Game.unity`)
```
Main Camera (Orthographic, size 5, background black)
Managers (empty; or let the DontDestroyOnLoad managers persist from MainMenu)
 ├── GameManager, ScoreManager, AudioManager, SceneLoader
 ├── BulletPool        (BulletPool.cs)   ← assign PlayerBullet & EnemyBullet prefabs
 ├── EnemySpawner      (EnemySpawner.cs) ← assign Drone/Fighter/Boss prefabs
 └── WaveManager       (WaveManager.cs)  ← assign the EnemySpawner
Background             (ParallaxBackground.cs ×2–3 layers)
Player                 (Player prefab)   ← tag "Player"
Canvas (Screen Space - Overlay)
 ├── HUD               (HUDController.cs) ← hearts, score, wave, shield, boss bars
 ├── PauseMenu         (PauseMenuController.cs)
 └── GameOver          (GameOverController.cs)
```

Add both scenes to the build list under **File → Build Settings → Scenes in
Build** (MainMenu at index 0, Game at index 1).

Prefab, sprite and audio wiring instructions live in:
* [`Assets/Prefabs/PrefabSetup.md`](Assets/Prefabs/PrefabSetup.md)
* [`Assets/Sprites/SpriteSetup.md`](Assets/Sprites/SpriteSetup.md)
* [`Assets/Audio/AudioSetup.md`](Assets/Audio/AudioSetup.md)

---

## 4. Running the tests

1. Open **Window → General → Test Runner**.
2. The window has two tabs:
   * **EditMode** — pure-logic tests (scores, health math, shot geometry, pooling).
   * **PlayMode** — tests that need the MonoBehaviour lifecycle / coroutines
     (fire-rate cooldown, invincibility window, enemy death, power-up expiry).
3. Click **Run All**. All tests in `Assets/Tests/` should pass.

The suite covers: `PlayerController`, `PlayerHealth`, `PlayerShooter`,
`EnemyHealth`, `BulletPool`, `WaveManager`, `ScoreManager`, the power-ups and
`AudioManager`.

You can also run tests headless from the command line:
```bash
"C:\Program Files\Unity\Hub\Editor\2022.3.x\Editor\Unity.exe" ^
  -runTests -batchmode -projectPath "C:\path\to\SpaceShooter" ^
  -testPlatform PlayMode -testResults "C:\path\to\results.xml"
```
(Repeat with `-testPlatform EditMode` for the edit-mode suite.)

---

## 5. Building for Windows

1. **File → Build Settings**.
2. Set **Platform** to **Windows, Mac & Linux Standalone** and click
   **Switch Platform** if it is not already active.
3. **Target Platform:** Windows. **Architecture:** x86_64.
4. Ensure **Scenes in Build** lists `MainMenu` (0) and `Game` (1).
5. (Optional) **Player Settings → Other Settings → Scripting Backend:** IL2CPP for
   a faster, more protected build; Mono for quicker iteration.
6. Click **Build**, choose an output folder (e.g. `Build/Windows/`).

---

## 6. Creating a standalone `.exe`

After a successful build you will get:
```
Build/Windows/
├── SpaceShooter.exe          ← double-click to play
├── UnityPlayer.dll
├── SpaceShooter_Data/        ← all game data (do not separate from the .exe)
└── MonoBleedingEdge/ (Mono builds)
```
Ship the **entire folder** (zip it) — the `.exe` needs the sibling
`*_Data` folder and `UnityPlayer.dll`. Use **Build And Run** to build and launch
in one step.

---

## 7. Troubleshooting

| Symptom | Fix |
|---------|-----|
| `The type or namespace 'NUnit' could not be found` | Install **Test Framework** via Package Manager; confirm `SpaceShooterTests.asmdef` lists `UnityEngine.TestRunner` + `UnityEditor.TestRunner` and precompiled `nunit.framework.dll`. |
| `UnityEngine.UI` not found in the game assembly | Confirm `SpaceShooterGame.asmdef` has `"references": ["UnityEngine.UI"]` and that `com.unity.ugui` is installed. |
| Bullets do nothing / `BulletPool.Instance` is null | Add a **BulletPool** to the Game scene and assign the PlayerBullet & EnemyBullet prefabs. |
| Enemies never shoot at the player / dive | Tag the player GameObject **"Player"** (used by `GameObject.FindWithTag`). |
| Player can move off screen | The bounds come from `Camera.main`; ensure the camera is **Orthographic** and tagged **MainCamera**, or call `PlayerController.SetBounds(...)`. |
| Pause does not freeze the game | Ensure a **GameManager** exists; it sets `Time.timeScale = 0` on pause. |
| High score never updates | `ScoreManager` writes to `PlayerPrefs`; on Windows this lives in the registry under `HKCU\Software\<Company>\<Product>`. Set Company/Product in Player Settings. |
| PlayMode tests warn about audio | Expected in batch mode; the tests assert clip/loop/volume state, not actual audio output. |

---

## 8. Project layout

```
SpaceShooter/
├── README.md
├── SpaceShooter.asmdef            (root descriptor)
└── Assets/
    ├── Scenes/       MainMenu/Game .meta descriptors + SceneList.md
    ├── Scripts/      SpaceShooterGame.asmdef + all gameplay code
    ├── Tests/        SpaceShooterTests.asmdef + all test files
    ├── Prefabs/      PrefabSetup.md
    ├── Sprites/      SpriteSetup.md
    └── Audio/        AudioSetup.md
```

Gameplay code is grouped by domain: `Core/`, `Player/`, `Enemies/`, `Bullets/`,
`Spawning/`, `PowerUps/`, `Background/`, `Audio/`, `UI/`, `Scoring/`.
