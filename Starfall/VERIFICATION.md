### Verification record

**Confirmed on the Linux authoring VM:**

- Tree-sitter parsed all **14 C# files / 911 C# lines** without syntax errors. This includes the standalone test harness; the Python validator is one additional code file.
- Source inspection found no TODO/FIXME or unimplemented-exception markers.
- All **18 committed Unity metadata GUIDs** are unique. The bootstrap scene's script GUID resolves to `SceneEntry.cs.meta`, and the enabled build-scene GUID resolves to the committed scene.
- Scene and project-settings YAML parsed successfully after stripping Unity-specific tag directives; package JSON parsed successfully.
- The package manifest contains only the four required built-in Unity modules, and the project explicitly selects legacy input.
- `GameRules.cs` and `Tools/GameRulesTests.cs` compiled with Mono `mcs`; the resulting test executable passed **5,140 assertions**, including wave caps, unlocks, enemy health bounds, repairs, and fire intervals. These execute the actual rules source, not a port of the rules.
- `git diff --check` reported no whitespace errors.

**Not performed:** Unity Editor import, Unity API/type compilation, editor content generation, `.unity` deserialization by Unity, prefab serialization/import, Play mode, gameplay collision tests, rendered visual inspection, audible sound checks, Windows build, or Windows executable execution. No Unity Editor or Windows runtime is installed on this VM. Parsing valid YAML does not prove Unity will load it; parsing C# does not prove Unity APIs will compile. No rendered screenshot or executable is claimed.

### Reproduce the lightweight checks

These optional commands run from the project root **on a machine with Python and Mono installed**. They do not launch Unity:

```text
python -m pip install tree-sitter tree-sitter-c-sharp pyyaml
python Tools/validate.py
mcs -out:GameRulesTests.exe Assets/Starfall/Scripts/GameRules.cs Tools/GameRulesTests.cs
mono GameRulesTests.exe
```

Keep the standalone test binary outside `Assets`. For Windows gameplay/build instructions, use `README.md` instead.

### Windows / Unity smoke-test checklist — still pending

1. **Fresh import:** use the documented Unity version; wait for compilation and automatic generation. Confirm no Console errors and run Starfall → Validate Generated Content. Check that 9 prefabs, 11 sprite PNGs, 6 WAVs, and the catalog exist.
2. **Scene and start:** open the committed scene and enter Play mode. Confirm the menu, three-depth starfield, and Enter/click launch behavior. Check the Console for missing scripts/materials/resources.
3. **Movement/fire:** test both movement key sets, diagonal speed, arena clamps, Shift precision, Space and Z continuous fire, and player-bullet collisions.
4. **Patterns/waves:** reach waves 2 and 4; verify fan and radial patterns, enemy-bullet collisions, increasing waves, and no stuck wave after the last enemy leaves or dies.
5. **Health/scoring:** verify five starting hull points, one damage per invulnerability window, no score for ramming/escaping enemies, score for shooting kills, and game over at zero hull.
6. **Pickups:** shooting kills 5/10/15 should drop spread/repair/rapid respectively. Verify two-point repair capped at five, timed expiration, combined rapid+spread, and fresh timers/hull after retry.
7. **Pause/focus:** Escape must freeze actors, starfield, wave delays, and power timers; resume must restore them. Alt-Tab should pause. Test mouse menu transitions while paused.
8. **Restart:** retry after death, return to menu, launch again, and repeat. Confirm no old enemies/projectiles or accelerated duplicate spawners remain. Close/reopen the game to check best score and mute persistence.
9. **Presentation/audio:** inspect sprite readability and hitboxes; listen for all six effects, test M and sound buttons, resize to 4:3 and ultrawide, and check the HUD stays inside the letterboxed arena.
10. **Native build:** build Windows x86_64, run the output on Windows outside Unity, test keyboard focus and Quit, and verify a copy of the complete build folder runs from a different directory.

### Likely integration risks to check first

- Unity may normalize the hand-authored minimal project settings and bootstrap scene. If scene import fails, retain the original for diagnosis, remove only that scene from a copy of the project, and run the builder to recreate it through Unity's own API.
- Missing Windows Build Support or a mismatched input backend must be corrected in Hub/Player settings as described in the README.
- Procedural texture/audio import, prefab saving, and IMGUI sizing are implemented but need the real Editor to confirm. The builder logs generation progress and throws explicit errors for missing generated references or a failed build.
