### Stellar Shooter — Unity C# Windows desktop game

A simple **2D Windows desktop space shooter**, not a web app. This folder is the one Unity project to open: it directly contains `Assets`, `Packages`, and `ProjectSettings`. Open this `stellar_shooter_unity` subfolder, not the repository root. The older root project is preserved separately and is not part of this Unity project.

**Editor:** Unity **6000.0.62f1** (Unity 6.0 LTS). **Rendering:** built-in pipeline. **UI:** UGUI 2.0.0. **Input:** built-in Input Manager (Old). **Build:** Windows x64, Mono. Use the pinned editor rather than importing into an older Unity version.

### Open and play — on your Windows PC

1. Install Unity Hub and Unity **6000.0.62f1** from the [official release page](https://unity.com/releases/editor/whats-new/6000.0.62f1). Activate an eligible Unity license. Include Windows player support if offered; the Mono build does not require IL2CPP/C++ tools.
2. Download/extract this full project. In Hub, choose **Add project from disk** and select the folder directly containing `Assets`, `Packages`, and `ProjectSettings`.
3. Open with the pinned editor. Let Unity import the assets and resolve packages.
4. Choose **Stellar Shooter → 1 - Generate Project**. It creates all prefabs, the playable scene, camera, colliders, audio wiring, UI, EventSystem, layers, and build-list entry. **No manual Inspector wiring is required.** Wait for the Console message ending in “Press Play.”
5. Press **Play**, focus the Game tab, and click **START FLIGHT** or press Enter.

Scenes/prefabs are deliberately generated in Unity by the included complete `Editor/ProjectBuilder.cs`; they are **not shipped as pre-generated serialized files**. Run **Stellar Shooter → 2 - Validate Generated Project** outside Play mode to check the generated references. If the menu is missing, check the first red Console error and confirm the editor/package versions; do not create a replacement template or install URP/the new Input System.

### Controls and gameplay

| Action | Controls |
|---|---|
| Move | WASD or arrows (normalized diagonal movement) |
| Shoot | Hold Space |
| Pause/resume | Escape or P |
| Navigate menus | Arrows or W/S, then Enter; mouse also supported |
| Mute | M or sound button |
| Restart after game over | R or FLY AGAIN |
| Restart / main menu / quit | Menu buttons |

- Start with **5 hull**. Enemy shots, contact, and escaped enemies cost one hull unless protected. Hits grant **1.15 seconds** of invulnerability. Zero hull ends the run.
- **Scout:** aimed single shot, wave 1 onward. **Gunner:** three-shot fan, wave 2 onward. **Bomber:** eight-shot radial pattern, wave 3 onward.
- Endless waves increase enemy count, hull, movement speed, and firing rate with bounded difficulty. Clear all enemies to progress. There is no final victory wave.
- Kills score **100 / 150 / 200** for scout/gunner/bomber; ramming gives no points.
- Every fourth scored kill drops a pickup, cycling **Spread → Repair → Shield**. Spread gives triple shots for **12 seconds**; Repair restores up to **2 hull**; Shield protects for **8 seconds**. Timed upgrades refresh rather than stack.
- HUD shows score, wave, hull, and power-up timers. Two scrolling background layers provide parallax; eight bundled WAV cues cover actions and menus.
- Pause freezes movement, waves and power-up timers. Losing window focus pauses an active run. Restart clears all actors and resets the run. Best score is saved at game over; mute preference is saved immediately via `PlayerPrefs`.

### Full source map

```text
Assets/StellarShooter/
  Scripts/
    GameRules.cs         Balance values and bullet-pattern rules (pure C#)
    GameSession.cs       Lifecycle, scoring, drops, pause/restart/game over
    PlayerController.cs  Input, movement, shooting, hull, upgrades
    Enemy.cs             Enemy movement, patterns, collision and death
    Projectile.cs        Shot physics, ownership, collision and expiry
    Pickup.cs            Pickup drift and collection
    WaveDirector.cs      Deterministic spawn sequence and wave progression
    GameUI.cs            Menu actions, HUD and focus
    SoundBank.cs         WAV playback, throttling and mute persistence
    ParallaxLayer.cs     Two-tile background scrolling
    ImpactEffect.cs      Sprite-burst hit/explosion feedback
    FixedAspectCamera.cs Stable 16:9 arena in resized windows
  Editor/ProjectBuilder.cs  Complete setup, validation and Windows build helper
  Art/                       Nine sprite PNGs and two background PNGs
  Audio/                     Eight PCM WAV effects
  Generated/                 Created by Generate Project:
    Prefabs/                 Player, two shots, three enemies, three pickups, impact
    Scenes/StellarShooter.unity
Packages/manifest.json
ProjectSettings/             Version, player, input and layer settings
SourceArt/sprite-sheet.png
Tools/                       Optional asset tools and offline tests
BUILD_INSTRUCTIONS.md        Windows executable build and distribution
ASSETS.md                    Asset provenance
```

All game art and audio are bundled locally; no Python or asset downloads are needed in Unity. Input deliberately uses `Input.GetKey/GetKeyDown`; the supplied legacy axes serve menu navigation. Keep **Active Input Handling = Input Manager (Old)**. Change balance in `GameRules.cs`; spawning in `WaveDirector.cs`; UI/setup in `ProjectBuilder.cs`. Retain `.meta` files when copying assets.

Regeneration replaces only generated prefabs and the generated scene, with a menu confirmation. Save custom generated-asset edits elsewhere first. The build helper preserves an existing generated scene. Layers 8–12 are reserved for player, enemy, friendly shot, hostile shot, and pickup; the runtime collision matrix enables only intended pairs. Simple instantiate/destroy is used rather than an unnecessary pooling framework.

### Verification boundary

**Previously checked during source preparation on Linux (not rerun for this publication):** C# syntax parsing; unique type declarations/GUIDs; project, input and package pins; image transparency/dimensions and PNG decoding; WAV format/non-silence/clipping; asset metadata; generator references and setup/build operations. This run passed **250 offline assertions** across **14 C# files**, **11 PNGs**, and **8 WAVs**. `GameRules.cs` and `Tools/RulesTests.cs` were actually compiled with Mono and passed **82 rule assertions**. All 20 bundled artwork/audio/source-image files were checked byte-for-byte against the existing implementation; none were regenerated.

**Not checked here:** Unity package import, Unity assembly/API compilation, execution of the scene/prefab generator, Play mode, UI/physics/audio behavior, or a Windows executable build/runtime. Unity Editor is not installed in this environment. Syntax checks and pure-C# tests do not establish those results. **No prebuilt executable is included.**

On your Windows PC, generate and validate, then test: movement/bounds and shooting; all three patterns; score and pickups; damage/invulnerability/game over; R restart and pause restart twice; pause/timer freezing; mouse/keyboard menus; focus loss, resizing and mute. Build and repeat in the executable; reopen it to check saved best score and mute preference.

Optional offline checks (Python with Pillow, tree-sitter, tree-sitter-c-sharp; Mono):

```text
python Tools/validate_source.py
mcs -warnaserror -out:RulesTests.exe Assets/StellarShooter/Scripts/GameRules.cs Tools/RulesTests.cs
mono RulesTests.exe
```
