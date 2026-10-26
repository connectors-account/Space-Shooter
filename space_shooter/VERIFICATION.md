### Verification

#### Checks completed on the Linux development machine

- Compiled `Assets/Scripts/GameRules.cs` and `Tests/GameRulesTests.cs` with Mono C# (`mcs -warnaserror`). Compilation succeeded without warnings.
- Executed that test program using Mono: **20 checks passed**. These cover idle and directional movement, normalized diagonals, arena bounds, zero timestep, overlap/separation, fast swept collisions, misses, boundary contact, collision time, reverse motion, and initial overlap.
- Parsed all **4 C# files** using the tree-sitter C# grammar: no syntax errors found. This is syntax validation, not Unity API/type validation.
- Checked the package manifest, asset metadata existence, unique asset GUIDs, the scene GUID/build-list reference, legacy input setting, and shader resource existence: passed.
- Reviewed source paths for spawning, bullet removal and scoring, enemy/player collision, game-over input gating, restart cleanup, and runtime bootstrap. These paths were not exercised inside Unity.

#### Not verified here

Unity is not installed on this machine. The following remain unverified: Unity project import/deserialization, Unity-specific C# API compilation, shader compilation, runtime rendering, keyboard/HUD behavior, complete game flow, and Windows player build/launch. No Windows executable is included. The standalone math tests do not claim to verify these areas.

Open the supplied project in Unity on your Windows computer, follow `README.md`, run the gameplay checklist there, and build the Windows x86_64 player before distributing it.
