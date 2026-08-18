# Scene Setup

Two scenes are required. Create them via **File → New Scene** and save into
`Assets/Scenes/` with the exact names below, then add both to
**File → Build Settings → Scenes in Build** (MainMenu = 0, Game = 1).

Scenes are saved by Unity as generated YAML, so they must be authored in the
editor. The `.unity.meta` files in this folder reserve stable asset GUIDs; Unity
creates/links the matching `.unity` files on first save.

---

## MainMenu.unity

| GameObject | Components | Notes |
|------------|-----------|-------|
| `Main Camera` | Camera (Orthographic, size 5), AudioListener | Background = solid black |
| `Managers` | — (empty parent) | |
| `Managers/GameManager` | `GameManager` | `persistAcrossScenes` auto-set |
| `Managers/ScoreManager` | `ScoreManager` | persists |
| `Managers/AudioManager` | `AudioManager` | assign clips (see AudioSetup.md) |
| `Managers/SceneLoader` | `SceneLoader` | assign `fadeGroup` = FadeOverlay CanvasGroup |
| `Background` | 2–3 × `ParallaxBackground` children | scroll speeds 0.5 / 1.0 / 2.0 |
| `Canvas` | Canvas (Screen Space - Overlay), CanvasScaler, GraphicRaycaster | |
| `Canvas/Title` | Text (RectTransform) | assign to `MainMenuController.titleTransform` |
| `Canvas/PlayButton` | Button + Text | → `MainMenuController.playButton` |
| `Canvas/QuitButton` | Button + Text | → `MainMenuController.quitButton` |
| `Canvas/HighScoreText` | Text | → `MainMenuController.highScoreText` |
| `Canvas/FadeOverlay` | Image (black, stretch full) + CanvasGroup | alpha 0; → `SceneLoader.fadeGroup` |
| `MainMenuController` | `MainMenuController` | wire the fields above |
| `EventSystem` | EventSystem + StandaloneInputModule | required for UI buttons |

## Game.unity

| GameObject | Components | Notes |
|------------|-----------|-------|
| `Main Camera` | Camera (Orthographic, size 5), AudioListener, tag **MainCamera** | black background |
| `Managers/GameManager` | `GameManager` | (may persist from MainMenu) |
| `Managers/ScoreManager` | `ScoreManager` | |
| `Managers/AudioManager` | `AudioManager` | |
| `Managers/SceneLoader` | `SceneLoader` | |
| `Managers/BulletPool` | `BulletPool` | assign `playerBulletPrefab`, `enemyBulletPrefab`, size 30 |
| `Managers/EnemySpawner` | `EnemySpawner` | assign Drone/Fighter/Boss prefabs |
| `Managers/WaveManager` | `WaveManager` | assign `spawner` = EnemySpawner |
| `Background` | 2–3 × `ParallaxBackground` | |
| `Player` | Player prefab | **tag "Player"** |
| `Canvas` | Canvas + CanvasScaler + GraphicRaycaster | |
| `Canvas/HUD` | `HUDController` | assign hearts[3], score, wave, multiplier, shield Slider, boss Slider |
| `Canvas/PauseMenu` | `PauseMenuController` | panel + Resume/Restart/MainMenu + 2 volume Sliders |
| `Canvas/GameOver` | `GameOverController` | panel + final/high score Texts + NEW HIGH label + Retry/MainMenu |
| `EventSystem` | EventSystem + StandaloneInputModule | |

### Start-up flow
1. `MainMenuController.OnPlay` → `SceneLoader.LoadGame()` + `GameManager.NewGame()`.
2. `GameManager.NewGame()` resets the score, sets state **Playing**, starts
   `WaveManager.StartWaves()` and plays `bgMusic`.
3. Player death → `PlayerHealth.Die()` → `GameManager.GameOver()` → the
   `GameOverController` panel appears.
4. **Escape** toggles pause via `GameManager` (freezes `Time.timeScale`).
