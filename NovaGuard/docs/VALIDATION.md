### Verification boundary

This deliverable is **Unity C# source for a 2D Windows desktop application**. It is not a browser game, WebGL build, 3D scene, or verified Windows binary.

#### Executed on the Linux authoring machine

`Tools/validate_source.py` checks:

- Every C# file parses without syntax-tree errors using the tree-sitter C# grammar.
- MonoBehaviour class/file naming, menu callback source references, and absence of unimplemented markers.
- Manifest JSON, Unity project-settings YAML, pinned Editor version, Windows target, and legacy-input configuration.
- Four menu navigation axes, documented input keys, layer assignments, and the intended collision-matrix source contract.
- Nine bundled game sprites decode as RGBA images, contain transparent pixels and near-opaque content, and remain within the import size cap.
- Eight bundled WAVs match the sound enum, decode as non-silent 44.1 kHz mono 16-bit PCM, and do not clip.
- Asset metadata has complete, unique GUIDs.
- Source-level pause, focus, restart-cleanup and generation safeguards are present.

The machine-readable result is `SOURCE_CHECKS.json`. Python scripts can also be checked using `python -m py_compile Tools/*.py`.

An initial sprite assertion incorrectly required the generated luminous sprite cores to have alpha exactly 255. Some supplied cores are intentionally near-opaque at 253/254. The check was corrected to require transparency plus a near-opaque core of at least 240, without modifying the artwork or discarding the alpha check.

#### Not executed

- Unity C# compilation, assembly/type checking, official package import, or scene/prefab generation.
- `ProjectValidation.Validate` inside the Unity Editor.
- Play Mode rendering, keyboard/UI interaction, audio playback, physics/collision behavior, wave balance, or memory/performance tests.
- Windows x86-64 compilation, executable launch, distribution tests or the `.cmd` helper.

No suitable Unity Editor or C# compiler was found on PATH; neither Unity nor a large toolchain was installed. Tree-sitter syntax parsing is **not compilation**. Source-token checks are **not behavioral tests**. The generated scene and prefabs are not pre-baked into the source archive: they are created by Unity's public Editor APIs on first import as documented in the README.

#### Remaining risks to resolve in Unity

The key unverified integration points are the pinned Editor's package/import defaults, first-import callback ordering, imported sprite/audio references, runtime uGUI construction and font loading, and physics/UI behavior. The generator's asset audit helps detect missing scene/prefab references on the user's Editor, but only a real Play Mode/Windows test can confirm gameplay and build correctness. See `WINDOWS_TEST_CHECKLIST.md`.

The project uses familiar 2022.3 public APIs; official documentation was checked for delayed Editor callbacks, prefab saving, Windows builds, and the `.NET Standard` API-compatibility enum. No unsupported promise of a successful build is made.

### Requested-feature source map

| Requested feature | Implementing source / asset |
|---|---|
| Windows desktop, complete project | ProjectSettings, manifest, ProjectSetup.BuildWindows, BuildWindows.cmd |
| Player movement / fire / keyboard input | PlayerShip.cs, KeyboardInput.cs |
| Multiple enemies / spawn logic / bullet patterns | EnemyShip.cs, WaveDirector.cs |
| Collision / damage / health / invulnerability | PhysicsRules.cs, Bullet.cs, PlayerShip.cs, EnemyShip.cs |
| Score / persistent best | GameController.cs, GameUI.cs, PlayerPrefs |
| Power-ups | PowerUp.cs, PlayerShip.Apply, supplied pickup sprites |
| Wave progression | WaveDirector.cs, EnemyShip.Configure |
| Background parallax | Starfield.cs, three scroll-speed layers |
| Sound effects | SoundBank.cs, eight supplied WAV files |
| Main menu / HUD / pause / game over / restart / quit | GameUI.cs, GameController.cs |
| Scenes / prefabs | Deterministic Editor generator in ProjectSetup.cs |
| Original sprites | Assets/Art; source sheet and nine individual sprites |
| Full code / build instructions | Assets/Scripts, Assets/Editor, README.md |

All listed features have source implementations. Their runtime acceptance remains unverified, not presumed successful.
